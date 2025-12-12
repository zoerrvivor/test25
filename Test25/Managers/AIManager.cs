using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Test25.Entities;
using Test25.Utilities;

namespace Test25.Managers
{
    public class AIManager
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
            bool isGameOver, Action fireAction)
        {
            if (isProjectileInAir || isGameOver) return;

            var activeTank = players[currentPlayerIndex];
            if (!activeTank.IsActive) return;

            _aiTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_aiTimer < 1.0f) return; // Delay before acting

            if (!_aiAiming)
            {
                // 1. Select Weapon based on Personality
                SelectWeaponForAi(activeTank);

                // 2. Find target based on Personality
                Tank target = FindTarget(activeTank, players);

                // 3. Calculate firing solution
                if (target != null)
                {
                    Vector2 diff = target.Position - activeTank.Position;
                    bool targetIsRight = target.Position.X > activeTank.Position.X;
                    _aiTargetAngle = targetIsRight ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;

                    float g = Constants.Gravity;
                    float range = Math.Abs(diff.X);

                    // Simple physics approximation: R = v^2 / g => v = Sqrt(R*g)
                    // Note: This is accurate for 45 degrees (PiOver4).
                    // If we want more varied angles eventually, we'd need full ballistic calc.
                    float v = (float)Math.Sqrt(range * g);

                    _aiTargetPower = v / Constants.PowerMultiplier;

                    // Apply Personality Errors
                    float aimError = activeTank.Personality.AimError;
                    float powerError = activeTank.Personality.PowerError;
                    
                    _aiTargetAngle += (float)(Rng.Instance.NextDouble() * aimError * 2 - aimError);
                    _aiTargetPower += (float)(Rng.Instance.NextDouble() * powerError * 2 - powerError);

                    if (_aiTargetPower > activeTank.Health) _aiTargetPower = activeTank.Health;
                    if (_aiTargetPower < 0) _aiTargetPower = 0;

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

        private void SelectWeaponForAi(Tank aiTank)
        {
            var p = aiTank.Personality;
            // Filter available weapons (Count > 0 or Infinite)
            var available = new List<Weapon>();
            foreach(var w in aiTank.Inventory)
            {
                if (w is Weapon weapon) // it is
                {
                    if (weapon.Count > 0 || weapon.IsInfinite) available.Add(weapon);
                }
            }
            
            Weapon choice = null;

            if (p.WeaponPreference == WeaponPreference.Aggressive)
            {
                // Try to find Nuke or high damage
                choice = available.Find(w => w.Name == "Nuke");
                if (choice == null) choice = available.Find(w => w.Damage > 50);
            }
            else if (p.WeaponPreference == WeaponPreference.Chaos)
            {
                // Try MIRV, Roller, Dirt
                choice = available.Find(w => w.Type == ProjectileType.Mirv || w.Type == ProjectileType.Roller);
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
            float bestValue = pref == TargetPreference.Weakest ? float.MaxValue : float.MinValue; 
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
                float val = 0;
                switch (pref)
                {
                    case TargetPreference.Closest:
                        val = Vector2.Distance(activeTank.Position, target.Position);
                        if (val < bestValue) { bestValue = val; bestTarget = target; }
                        break;
                    case TargetPreference.Weakest:
                        val = target.Health;
                        if (val < bestValue) { bestValue = val; bestTarget = target; }
                        break;
                    case TargetPreference.Strongest:
                        val = target.Health;
                        if (val > bestValue) { bestValue = val; bestTarget = target; }
                        break;
                }
            }

            return bestTarget ?? others[0];
        }
    }
}
