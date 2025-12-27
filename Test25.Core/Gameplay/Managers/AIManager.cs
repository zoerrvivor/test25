using Microsoft.Xna.Framework;
using Test25.Core.Gameplay.Entities;
using Test25.Core.Gameplay.World;
using Test25.Core.Utilities;

namespace Test25.Core.Gameplay.Managers
{
    public class AiManager
    {
        private float _aiTimer;
        private bool _aiHasFired;
        private bool _aiAiming;
        private float _aiTargetAngle;
        private float _aiTargetPower;

        public void ResetTurn()
        {
            _aiTimer = 0;
            _aiHasFired = false;
            _aiAiming = false;
        }

        public void UpdateAi(GameTime gameTime, List<Tank> players, int currentPlayerIndex, bool isProjectileInAir,
            bool isGameOver, float wind, WallType wallType, int terrainWidth, Action fireAction, Terrain terrain)
        {
            if (isProjectileInAir || isGameOver) return;

            var activeTank = players[currentPlayerIndex];
            if (!activeTank.IsActive) return;

            _aiTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_aiTimer < 1.0f) return; // Delay before acting

            if (!_aiAiming)
            {
                // 1. Find target based on Personality (Moved before weapon select so we know WHO blocking us)
                Tank target = FindTarget(activeTank, players);

                // 2. Select Weapon based on Personality AND Situation
                SelectWeaponForAi(activeTank, target, terrain);

                // 3. Calculate firing solution
                if (target != null)
                {
                    Vector2 targetPos = target.Position;

                    // --- Wall Type Logic (Wrap) ---
                    if (wallType == WallType.Wrap)
                    {
                        // Check if it is shorter to wrap around
                        float directDist = Math.Abs(targetPos.X - activeTank.Position.X);
                        float wrappedDistLeft = Math.Abs((targetPos.X - terrainWidth) - activeTank.Position.X);
                        float wrappedDistRight = Math.Abs((targetPos.X + terrainWidth) - activeTank.Position.X);

                        if (wrappedDistLeft < directDist)
                        {
                            targetPos.X -= terrainWidth; // Ghost target to left
                        }
                        else if (wrappedDistRight < directDist)
                        {
                            targetPos.X += terrainWidth; // Ghost target to right
                        }
                    }

                    if (activeTank.CurrentWeapon.Type == ProjectileType.Laser)
                    {
                        // Direct straight line towards target (ignoring terrain)
                        Vector2 turretPos = activeTank.Position + activeTank.TurretOffset;
                        Vector2 diff = targetPos - turretPos;
                        _aiTargetAngle = (float)Math.Atan2(-diff.Y, diff.X);


                        // Limit angle to upper hemisphere for sanity, though lasers can technically aim down if we wanted
                        // Let's keep it consistent with normal turret movement (0 to Pi)
                        _aiTargetAngle = MathHelper.Clamp(_aiTargetAngle, 0, MathHelper.Pi);

                        _aiTargetPower = 50f; // Fixed power for state machine
                    }
                    else
                    {
                        // --- Wind Compensation ---
                        float windCompensationFactor = 2.0f; // Tunable constant
                        targetPos.X -= wind * windCompensationFactor;

                        Vector2 diff = targetPos - activeTank.Position;
                        bool targetIsRight = targetPos.X > activeTank.Position.X;
                        _aiTargetAngle = targetIsRight ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;

                        float g = Constants.Gravity;
                        float range = Math.Abs(diff.X);

                        float v = (float)Math.Sqrt(range * g);
                        _aiTargetPower = v / Constants.PowerMultiplier;
                    }

                    // Apply Personality Errors
                    float aimError = activeTank.Personality.AimError;
                    float powerError = activeTank.Personality.PowerError;

                    _aiTargetAngle += (float)(Rng.Instance.NextDouble() * aimError * 2 - aimError);
                    _aiTargetPower += (float)(Rng.Instance.NextDouble() * powerError * 2 - powerError);

                    _aiTargetPower = MathHelper.Clamp(_aiTargetPower, 0, activeTank.Health);

                    _aiAiming = true;
                }
            }
            else
            {
                // 4. Move turret and power towards target
                float aimSpeed = 2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float powerSpeed = 50f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                bool aimed = false;
                bool powered = false;

                if (Math.Abs(activeTank.TurretAngle - _aiTargetAngle) < aimSpeed)
                {
                    activeTank.TurretAngle = _aiTargetAngle;
                    aimed = true;
                }
                else
                {
                    if (activeTank.TurretAngle < _aiTargetAngle) activeTank.TurretAngle += aimSpeed;
                    else activeTank.TurretAngle -= aimSpeed;
                }

                if (Math.Abs(activeTank.Power - _aiTargetPower) < powerSpeed)
                {
                    activeTank.Power = _aiTargetPower;
                    powered = true;
                }
                else
                {
                    if (activeTank.Power < _aiTargetPower) activeTank.Power += powerSpeed;
                    else activeTank.Power -= powerSpeed;
                }

                // 5. Fire when ready
                if (aimed && powered && !_aiHasFired)
                {
                    fireAction?.Invoke();
                    _aiHasFired = true;
                }
            }
        }

        private void SelectWeaponForAi(Tank aiTank, Tank target, Terrain terrain)
        {
            var p = aiTank.Personality;
            // Filter available weapons (Count > 0 or Infinite)
            var available = new List<Weapon>();
            foreach (var w in aiTank.Inventory)
            {
                if (w is Weapon weapon) // it is
                {
                    if (weapon.Count > 0 || weapon.IsInfinite) available.Add(weapon);
                }
            }

            Weapon choice = null;

            // --- Situation Analysis ---
            bool lineOfSightBlocked = false;
            if (target != null && terrain != null)
            {
                lineOfSightBlocked = IsLineBlocked(aiTank.Position, target.Position, terrain);
            }

            // High Priority: If blocked, use weapons that ignore/destroy terrain
            if (lineOfSightBlocked)
            {
                // Prefer Laser (cuts through)
                choice = available.Find(w => w.Type == ProjectileType.Laser);

                // Or Drone (homing, might go around)
                if (choice == null) choice = available.Find(w => w.Type == ProjectileType.Drone);

                // Or Roller (rolls over terrain)
                if (choice == null) choice = available.Find(w => w.Type == ProjectileType.Roller);

                // Or Mirv (splits mid-air, might get lucky over wall)
                if (choice == null) choice = available.Find(w => w.Type == ProjectileType.Mirv);
            }

            // If already picked, or not blocked (or no special weapon found), check personality
            if (choice == null)
            {
                if (p.WeaponPreference == WeaponPreference.Aggressive)
                {
                    // Try to find Nuke or high damage
                    choice = available.Find(w => w.Name == "Nuke");
                    if (choice == null) choice = available.Find(w => w.Damage > 50);
                }
                else if (p.WeaponPreference == WeaponPreference.Chaos)
                {
                    // Try MIRV, Roller, Dirt, Drone
                    choice = available.Find(w => w.Type == ProjectileType.Mirv ||
                                                 w.Type == ProjectileType.Roller ||
                                                 w.Type == ProjectileType.Drone);
                }

                // Special: If we have a homing missile (Drone) and we are a Sniper (Weakest target pref), use it!
                if (choice == null && p.TargetPreference == TargetPreference.Weakest)
                {
                    choice = available.Find(w => w.Type == ProjectileType.Drone);
                }
                // else Balanced/Conservative default to logic below

                // Fallback or Balanced: Randomize slightly but prefer better weapons if available
                if (choice == null)
                {
                    // 50% chance to just pick random available to spice it up
                    if (Rng.Instance.NextDouble() < 0.5)
                    {
                        choice = available[Rng.Instance.Next(available.Count)];
                    }
                    else
                    {
                        // Default to standard or whatever is currently selected if we don't want to switch too much
                        // actually, let's just pick Standard if nothing else
                        choice = available.Find(w => w.IsInfinite);
                    }
                }
            }

            if (choice != null)
            {
                aiTank.SetWeapon(choice);
            }
        }

        private Tank FindTarget(Tank activeTank, List<Tank> players)
        {
            var others = new List<Tank>();
            foreach (var p in players)
            {
                if (p != activeTank && p.IsActive) others.Add(p);
            }

            if (others.Count == 0) return null;

            TargetPreference pref = activeTank.Personality.TargetPreference;

            // Random
            if (pref == TargetPreference.Random)
            {
                return others[Rng.Instance.Next(others.Count)];
            }

            Tank bestTarget = null;
            float bestValue;
            // For closest, we want Smallest Value (Distance). 
            // For Weakest, we want Smallest Value (Health).
            // For Strongest, Largest Health.
            // Re-init for Closest/Weakest logic (finding Min)
            if (pref == TargetPreference.Closest || pref == TargetPreference.Weakest)
                bestValue = float.MaxValue;
            else
                bestValue = float.MinValue; // Strongest

            foreach (var target in others)
            {
                float val;
                switch (pref)
                {
                    case TargetPreference.Closest:
                        val = Vector2.Distance(activeTank.Position, target.Position);
                        if (val < bestValue)
                        {
                            bestValue = val;
                            bestTarget = target;
                        }

                        break;
                    case TargetPreference.Weakest:
                        val = target.Health;
                        if (val < bestValue)
                        {
                            bestValue = val;
                            bestTarget = target;
                        }

                        break;
                    case TargetPreference.Strongest:
                        val = target.Health;
                        if (val > bestValue)
                        {
                            bestValue = val;
                            bestTarget = target;
                        }

                        break;
                }
            }

            return bestTarget ?? others[0];
        }

        private bool IsLineBlocked(Vector2 start, Vector2 end, Terrain terrain)
        {
            Vector2 dir = end - start;
            float dist = dir.Length();
            dir.Normalize();

            // Check points along the line
            // Skip first few pixels to avoid self-collision
            // Step size 10 is enough for terrain blocks
            for (float i = 20; i < dist - 20; i += 10)
            {
                Vector2 pos = start + dir * i;
                if (terrain.IsPixelSolid((int)pos.X, (int)pos.Y))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

