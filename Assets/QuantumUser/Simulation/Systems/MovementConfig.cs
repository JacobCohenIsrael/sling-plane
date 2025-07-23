using Photon.Deterministic;

namespace Quantum
{
    public class MovementConfig : AssetObject
    {
        public FP ImpulsePower;
        public FP Responsivness;
        public FP LiftPower;
        public FP MaxThrottlePower;
        public FP ThrustPowerIncrease;
        public FP InitialThrustPower;
        public FP MaxThrustDuration;
        public FP InitialThrustDuration;
        public FP ThrustDurationIncrease;
        public FP GlidePower;
        public FP DragPower;
    }
}