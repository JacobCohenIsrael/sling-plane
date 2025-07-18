using Quantum;
using UnityEngine;

namespace Gamefather.Game.Plane
{
    public class PlaneView : QuantumEntityView
    {
        [SerializeField] private GameObject virtualCamera;
        
        public override void OnInitialize()
        {
            base.OnInitialize();
            var game = QuantumRunner.DefaultGame;
            var f = game.Frames.Predicted;
            var playerLink = f.Get<PlayerLink>(EntityRef);
            if (game.PlayerIsLocal(playerLink.Player))
            {
                virtualCamera.SetActive(true);
            }
        }
    }
}
