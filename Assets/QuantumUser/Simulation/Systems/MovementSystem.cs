using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum.Simulation.Systems
{
    [Preserve]
    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnComponentAdded<PlaneStats>
    {
        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.Link->Player == PlayerRef.None)
            {
                return;
            }
            HandleInputs(f, ref filter);
        }

        private void HandleInputs(Frame f, ref Filter filter)
        {
            var input = f.GetPlayerInput(filter.Link->Player);
            var rotation = input->Rotation.Normalized;
            var planeConfig = f.Get<PlaneConfig>(filter.Entity);
            var movementConfig = f.FindAsset<MovementConfig>(planeConfig.MovementConfig.Id);

            if (input->Sling.IsDown && !filter.PlaneStats->WasLaunched)
            {
                var impulse = filter.Transform->Forward * filter.PlaneStats->SlingPower;
                filter.PhysicsBody->Velocity += impulse;
                filter.PlaneStats->WasLaunched = true;
            }

            var velocity = filter.PhysicsBody->Velocity;
            var forward = filter.Transform->Forward;
            var right = filter.Transform->Right;
            var up = filter.Transform->Up;

            var forwardSpeed = FPVector3.Dot(velocity, forward);

            // Compute angle between forward and world up (for angle of attack effect)
            var angleFactor = FPVector3.Dot(forward.Normalized, FPVector3.Up);
            angleFactor = FPMath.Abs(FP._1 - FPMath.Abs(angleFactor)); // Max lift when level

            // Apply lift in the plane's up direction (affected by roll)
            var liftForce = up * forwardSpeed * movementConfig.LiftPower * angleFactor;
            filter.PhysicsBody->AddForce(liftForce);

            // Apply torque based on input
            var roll = -rotation.X;
            var pitch = rotation.Y;
            var responseModifier = filter.PlaneStats->ResponsiveModifier;

            filter.PhysicsBody->AddTorque(right * pitch * responseModifier);
            filter.PhysicsBody->AddTorque(forward * roll * responseModifier);

            filter.PlaneStats->DistancePassed =
                FPMath.Abs(filter.Transform->Position.Z - filter.PlaneStats->InitialPosition.Z);
            
            if (filter.PlaneStats->WasLaunched)
            {
                filter.PhysicsBody->AddForce(filter.Transform->Forward * movementConfig.ThrustPower);
            }
            
            if (filter.PlaneStats->WasLaunched && filter.PhysicsBody->Velocity.Z <= FP._0_05)
            {
                f.Signals.OnPlaneCompleteStop(filter.Entity, filter.Link->Player);
                filter.PlaneStats->WasLaunched = false;
                filter.PlaneStats->SlingPower += FP._0_05;
                filter.PhysicsBody->Velocity = FPVector3.Zero;
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* Link;
            public Transform3D* Transform;
            public PhysicsBody3D* PhysicsBody;
            public PlaneStats* PlaneStats;
        }

        public void OnAdded(Frame f, EntityRef entity, PlaneStats* component)
        {
            var rigidbody = f.Unsafe.GetPointer<PhysicsBody3D>(entity);
            var transform = f.Unsafe.GetPointer<Transform3D>(entity);
            var planeConfig = f.Get<PlaneConfig>(entity);
            var movementConfig = f.FindAsset<MovementConfig>(planeConfig.MovementConfig.Id);
            component->ResponsiveModifier = rigidbody->Mass / FP._10 * movementConfig.Responsivness;
            component->SlingPower = movementConfig.ImpulsePower;
            component->InitialPosition = transform->Position;
            component->DistancePassed = FP._0;
        }
    }
}
