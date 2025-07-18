using System;
using Photon.Deterministic;
using Quantum;
using TMPro;
using UnityEngine;

namespace Gamefather.Game.Plane
{
    public class PlaneView : QuantumEntityView
    {
        [SerializeField] private GameObject virtualCamera;
        [SerializeField] private TMP_Text distanceText;
        
        private PlayerRef playerRef = PlayerRef.None;
        
        public override void OnInitialize()
        {
            base.OnInitialize();
            var game = QuantumRunner.DefaultGame;
            var f = game.Frames.Predicted;
            var playerLink = f.Get<PlayerLink>(EntityRef);
            if (game.PlayerIsLocal(playerLink.Player))
            {
                virtualCamera.SetActive(true);
                playerRef = playerLink.Player;
            }
        }

        private void Update()
        {
            if (playerRef == PlayerRef.None)
            {
                return;
            }
            
            var game = QuantumRunner.DefaultGame;
            var f = game.Frames.Predicted;
            var planeStats = f.Get<PlaneStats>(EntityRef);
            var playerLink = f.Get<PlayerLink>(EntityRef);
            if (playerRef != playerLink.Player)
            {
                return;
            }

            var planeStatsDistancePassed = planeStats.DistancePassed;
            distanceText.text = planeStatsDistancePassed.ToString("N0");
            distanceText.gameObject.SetActive(planeStatsDistancePassed > FP._0_05);
        }
    }
}
