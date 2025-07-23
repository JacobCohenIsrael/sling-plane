using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum.Simulation.Systems
{
    [Preserve]
    public unsafe class PlayersSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlaneCompleteStop
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var runtimePlayer = f.GetPlayerData(player);
            var entity = f.Create(runtimePlayer.PlayerAvatar);
            f.Add(entity, new PlayerLink
            {
                Player = player
            });

            var rampEntityRef = f.Create(runtimePlayer.Ramp);
            f.Add(rampEntityRef, new PlayerLink
            {
                Player = player
            });
            
            f.Unsafe.GetPointer<Transform3D>(entity)->Teleport(f, new FPVector3(player * 2, 1, 0));
            f.Unsafe.GetPointer<Transform3D>(rampEntityRef)->Teleport(f, new FPVector3(player * 2, FP._0_20, FP._1_10));
        }

        public void OnPlaneCompleteStop(Frame f, EntityRef entity, PlayerRef player)
        {
            var transform3D = f.Unsafe.GetPointer<Transform3D>(entity);
            transform3D->Teleport(f, new FPVector3(player * 2, 1, 0));
            transform3D->Rotation = FPQuaternion.Identity;
        }
    }
}