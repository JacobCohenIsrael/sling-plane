using UnityEngine.Scripting;

namespace Quantum.Systems
{
    [Preserve]
    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public PlayerLink* Link;
            public PhysicsBody3D PhysicsBody;
        }
    }
}
