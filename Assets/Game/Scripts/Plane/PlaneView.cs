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
        [SerializeField] private Transform propeller;
        [SerializeField] private AudioSource propSound;
        
        private PlayerRef playerRef = PlayerRef.None;
        
        public override void OnInitialize()
        {
            base.OnInitialize();
            var game = QuantumRunner.DefaultGame;
            var f = game.Frames.Predicted;
            var playerLink = f.Get<PlayerLink>(EntityRef);
            playerRef = playerLink.Player;
            if (game.PlayerIsLocal(playerLink.Player))
            {
                virtualCamera.SetActive(true);
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

            propeller.Rotate(Vector3.right * (planeStats.Thrust.AsFloat * Time.deltaTime * 20000f));
            propSound.volume = planeStats.WasLaunched ? planeStats.Thrust.AsFloat * 0.5f : 0f;
            var planeStatsDistancePassed = planeStats.DistancePassed;
            distanceText.text = $"{planeStatsDistancePassed.AsInt}m";
            distanceText.gameObject.SetActive(planeStatsDistancePassed > FP._0_05);
        }
    }
}
