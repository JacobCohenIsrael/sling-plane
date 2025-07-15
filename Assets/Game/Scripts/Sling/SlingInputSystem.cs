using UnityEngine;
using UnityEngine.InputSystem;

namespace Gamefather.Game.Sling
{
    public class SlingInputSystem : MonoBehaviour
{
    public Transform plane;
    public float launchForceMultiplier = 10f;
    public LineRenderer aimLine;

    private Camera cam;
    private Vector3 dragStartWorld;
    private bool isDragging = false;

    [Header("Input Actions")]
    public InputActionReference pointerPositionAction;
    public InputActionReference pointerClickAction;

    void OnEnable()
    {
        pointerPositionAction.action.Enable();
        pointerClickAction.action.Enable();
    }

    void OnDisable()
    {
        pointerPositionAction.action.Disable();
        pointerClickAction.action.Disable();
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (pointerClickAction.action.WasPressedThisFrame())
        {
            Vector2 pointerScreen = pointerPositionAction.action.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(pointerScreen);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == plane)
            {
                isDragging = true;
                dragStartWorld = hit.point;
                aimLine.positionCount = 2;
            }
        }

        if (isDragging)
        {
            Vector2 pointerScreen = pointerPositionAction.action.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(pointerScreen);

            if (pointerClickAction.action.IsPressed() && Physics.Raycast(ray, out RaycastHit hit))
            {
                aimLine.SetPosition(0, plane.position);
                aimLine.SetPosition(1, hit.point);
            }

            if (pointerClickAction.action.WasReleasedThisFrame())
            {
                isDragging = false;
                aimLine.positionCount = 0;

                if (Physics.Raycast(ray, out RaycastHit releaseHit))
                {
                    Vector3 launchDir = dragStartWorld - releaseHit.point;
                    var rb = plane.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.AddForce(launchDir * launchForceMultiplier, ForceMode.Impulse);
                }
            }
        }
    }
}
}

