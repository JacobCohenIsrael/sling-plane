using UnityEngine;

namespace Gamefather.Game.UI
{
    public class LookAtCamera : MonoBehaviour
    {
        private enum Mode
        {
            LookAt,
            LookAtInverted,
            CameraForward,
            CameraForwardInverted
        }

        [SerializeField] private Mode mode;
    
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                return;
            }
        
            switch (mode) 
            {
                case Mode.LookAt:
                    transform.LookAt(mainCamera.transform);
                    break;
                case Mode.LookAtInverted:
                    var dirFromCamera = transform.position - mainCamera.transform.position;
                    transform.LookAt(transform.position + dirFromCamera);
                    break;
                case Mode.CameraForward:
                    transform.forward = mainCamera.transform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    transform.forward = -mainCamera.transform.forward;
                    break;
            }
        }
    }
}