using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test25.Entities
{
    public class DroneProjectile : Projectile
    {
        private enum DroneState
        {
            Launching,
            Searching,
            Striking
        }

        private DroneState _state;
        private float _stateTimer;
        private Tank _target;
        private List<Tank> _potentialTargets;

        // Tuning
        private const float LaunchDuration = 0.5f; // Time to fly up before searching
        private const float SearchDuration = 1.0f; // Time to hover/search
        private const float Speed = 300f;
        private const float StrikingSpeed = 500f;
        private const float TurnSpeed = 5f;

        public DroneProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
            _state = DroneState.Launching;
            _stateTimer = 0f;
            Damage = 40f;
            ExplosionRadius = 40f;
        }

        private Tank _owner;

        public void SetOwner(Tank owner)
        {
            _owner = owner;
        }

        public void SetTargets(List<Tank> targets)
        {
            _potentialTargets = targets;
        }

        public override void UpdatePhysics(GameTime gameTime, float wind, float gravity)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _stateTimer += dt;

            switch (_state)
            {
                case DroneState.Launching:
                    // Standard physics for launch
                    base.UpdatePhysics(gameTime, wind, gravity);

                    // Transition
                    if (_stateTimer >= LaunchDuration)
                    {
                        _state = DroneState.Searching;
                        _stateTimer = 0;
                        Velocity *= 0.1f; // Slow down to hover
                    }

                    break;

                case DroneState.Searching:
                    // Hover effect (bobbing)
                    Velocity = new Vector2(0, (float)Math.Sin(_stateTimer * 5) * 50f);
                    Position += Velocity * dt;

                    if (_stateTimer >= SearchDuration)
                    {
                        FindTarget();
                        _state = DroneState.Striking;
                    }

                    break;

                case DroneState.Striking:
                    if (_target != null && _target.IsActive)
                    {
                        // Homing Logic
                        Vector2 direction = _target.Position - Position;
                        if (direction != Vector2.Zero)
                        {
                            direction.Normalize();

                            // Smooth turn
                            // Current heading
                            float currentAngle = Rotation;
                            float targetAngle = (float)Math.Atan2(direction.Y, direction.X);

                            // Lerp angle? Or just steer velocity
                            // Simple steering:
                            Vector2 desiredVelocity = direction * StrikingSpeed;
                            Velocity = Vector2.Lerp(Velocity, desiredVelocity, dt * TurnSpeed);
                        }
                    }
                    else
                    {
                        // Target lost/dead, just keep going or find new?
                        // Just fall/gravity or keep straight
                        // Let's add gravity back if no target
                        Vector2 v = Velocity;
                        v.Y += gravity * dt;
                        Velocity = v;
                    }

                    Position += Velocity * dt;

                    // Update rotation to face movement
                    if (Velocity.LengthSquared() > 1f)
                    {
                        Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
                    }

                    break;
            }
        }

        private void FindTarget()
        {
            if (_potentialTargets == null) return;

            float closestDist = float.MaxValue;
            Tank bestTarget = null;

            foreach (var tank in _potentialTargets)
            {
                if (!tank.IsActive) continue;
                if (tank == _owner) continue; // Ignore self

                float dist = Vector2.Distance(Position, tank.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = tank;
                }
            }

            _target = bestTarget;
        }

        // Override CheckCollision to allow flying through terrain during launch/search if desired?
        // For now, let's keep standard collision so it doesn't clip through walls.
    }
}
