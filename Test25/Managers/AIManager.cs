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
                // 1. Find target
                Tank target = null;
                float minDist = float.MaxValue;
                foreach (var p in players)
                {
                    if (p != activeTank && p.IsActive)
                    {
                        float dist = Vector2.Distance(activeTank.Position, p.Position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            target = p;
                        }
                    }
                }

                // 2. Calculate firing solution
                if (target != null)
                {
                    Vector2 diff = target.Position - activeTank.Position;
                    bool targetIsRight = target.Position.X > activeTank.Position.X;
                    _aiTargetAngle = targetIsRight ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;

                    float g = Constants.Gravity;
                    float range = Math.Abs(diff.X);

                    // Simple physics approximation: R = v^2 / g => v = Sqrt(R*g)
                    float v = (float)Math.Sqrt(range * g);

                    _aiTargetPower = v / Constants.PowerMultiplier;

                    // Add randomness (error)
                    _aiTargetAngle += (float)(Rng.Instance.NextDouble() * 0.2 - 0.1);
                    _aiTargetPower += (float)(Rng.Instance.NextDouble() * 10 - 5);

                    if (_aiTargetPower > activeTank.Health) _aiTargetPower = activeTank.Health;
                    if (_aiTargetPower < 0) _aiTargetPower = 0;

                    _aiAiming = true;
                }
            }
            else
            {
                // 3. Move turret and power towards target
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

                // 4. Fire when ready
                if (aimed && powered && !_aiHasFired)
                {
                    fireAction?.Invoke();
                    _aiHasFired = true;
                }
            }
        }
    }
}
