using Photon.Deterministic;
using UnityEngine;
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
            HandleMovement(f, ref filter);
        }

        private void HandleInputs(Frame f, ref Filter filter)
        {
            var input = f.GetPlayerInput(filter.Link->Player);
            var rotation = input->Rotation.Normalized;

            if (input->Sling.IsDown && !filter.PlaneStats->WasLaunched)
            {
                var impulse = filter.Transform->Forward * filter.PlaneStats->SlingPower;
                filter.PhysicsBody->Velocity += impulse;
                filter.PlaneStats->LaunchTick = f.Number;
                filter.PlaneStats->WasLaunched = true;
            }

            // Apply torque based on input
            var forward = filter.Transform->Forward;
            var right = filter.Transform->Right;
            var roll = -rotation.X;
            var pitch = rotation.Y;
            var responseModifier = filter.PlaneStats->ResponsiveModifier;

            filter.PhysicsBody->AddTorque(right * pitch * responseModifier);
            filter.PhysicsBody->AddTorque(forward * roll * responseModifier);
        }
        
        private void HandleMovement(Frame f, ref Filter filter)
        {
            UpdateDistancePassed(ref filter);

            if (!filter.PlaneStats->WasLaunched)
                return;
            
            var up = filter.Transform->Up;
            var velocity = filter.PhysicsBody->Velocity;
            var forward = filter.Transform->Forward;

            var planeConfig = f.Get<PlaneConfig>(filter.Entity);
            var movementConfig = f.FindAsset<MovementConfig>(planeConfig.MovementConfig.Id);

            ApplyLift(filter, velocity, forward, up, movementConfig);
            ApplyGlide(filter, velocity, forward, movementConfig);
            ApplyDrag(filter, velocity, forward, movementConfig);
            ApplyThrust(f, ref filter, forward, movementConfig);
            CheckCompleteStop(f, ref filter, movementConfig);
        }
        
        private void ApplyLift(Filter filter, FPVector3 velocity, FPVector3 forward, FPVector3 up, MovementConfig config)
        {
            var forwardSpeed = FPVector3.Dot(velocity, forward);
            var angleFactor = FPMath.Abs(FP._1 - FPMath.Abs(FPVector3.Dot(forward.Normalized, FPVector3.Up)));
            var liftForce = up * forwardSpeed * config.LiftPower * angleFactor;
            filter.PhysicsBody->AddForce(liftForce);
        }
        
        private void ApplyThrust(Frame f, ref Filter filter, FPVector3 forward, MovementConfig config)
        {
            var launchDuration = filter.PlaneStats->ThrustDuration;
            var peakTime = launchDuration / FP._2;
            var timeSinceLaunch = (f.Number - filter.PlaneStats->LaunchTick) * f.DeltaTime;

            FP thrustFactor;
            if (timeSinceLaunch <= peakTime)
                thrustFactor = timeSinceLaunch / peakTime;
            else if (timeSinceLaunch <= launchDuration)
                thrustFactor = FP._1 - ((timeSinceLaunch - peakTime) / (launchDuration - peakTime));
            else
                thrustFactor = FP._0;

            filter.PlaneStats->Thrust = filter.PlaneStats->MaxThrust * thrustFactor;
            filter.PhysicsBody->AddForce(forward * filter.PlaneStats->Thrust * config.MaxThrottlePower);
        }
        
        private void ApplyDrag(Filter filter, FPVector3 velocity, FPVector3 forward, MovementConfig config)
        {
            var alignment = FPMath.Max(FPVector3.Dot(forward.Normalized, FPVector3.Up), FP._0);
            var dragMultiplier = FPMath.Log(FP._1 + alignment * FP._9, 10);
            var dragForce = -velocity.Normalized * velocity.SqrMagnitude * config.DragPower * dragMultiplier;
            Debug.Log($"dragMultiplier {dragMultiplier}");
            filter.PhysicsBody->AddForce(dragForce);
        }
        
        private void ApplyGlide(Filter filter, FPVector3 velocity, FPVector3 forward, MovementConfig config)
        {
            var verticalSpeed = -velocity.Y;
            var alignmentWithHorizontal = FPMath.Abs(FPVector3.Dot(forward.Normalized, FPVector3.Up));
            var glideFactor = FP._1 - alignmentWithHorizontal;
            if (verticalSpeed > FP._0)
            {
                var glideThrust = forward * verticalSpeed * config.GlidePower * glideFactor;
                filter.PhysicsBody->AddForce(glideThrust);
            }
        }
        
        private void UpdateDistancePassed(ref Filter filter)
        {
            filter.PlaneStats->DistancePassed =
                FPMath.Abs(filter.Transform->Position.Z - filter.PlaneStats->InitialPosition.Z);
        }
        
        private void CheckCompleteStop(Frame f, ref Filter filter, MovementConfig config)
        {
            if (!filter.PlaneStats->WasLaunched || filter.PhysicsBody->Velocity.Magnitude > FP._0_05)
                return;

            f.Signals.OnPlaneCompleteStop(filter.Entity, filter.Link->Player);
            filter.PlaneStats->WasLaunched = false;
            filter.PlaneStats->SlingPower += FP._0_05;
            filter.PlaneStats->MaxThrust = FPMath.Min(filter.PlaneStats->MaxThrust + config.ThrustPowerIncrease, FP._1);
            filter.PlaneStats->ThrustDuration =
                FPMath.Min(filter.PlaneStats->ThrustDuration + config.ThrustDurationIncrease, config.MaxThrustDuration);
            filter.PlaneStats->Thrust = FP._0;
            filter.PhysicsBody->Velocity = FPVector3.Zero;
            filter.PhysicsBody->AngularVelocity = FPVector3.Zero;
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
            component->MaxThrust = movementConfig.InitialThrustPower;
            component->Thrust = movementConfig.InitialThrustPower;
            component->ThrustDuration = movementConfig.InitialThrustDuration;
            component->LaunchTick = 0;
        }
    }
}
