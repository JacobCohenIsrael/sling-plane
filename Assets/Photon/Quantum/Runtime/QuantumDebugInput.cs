namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.InputSystem;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDebugInput : MonoBehaviour {

    [SerializeField]
    private InputActionReference rotateAction;

    [SerializeField]
    private InputActionReference slingAction;
    
    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
      rotateAction.action.Enable();
      slingAction.action.Enable();
    }

    private void OnDisable() {
      rotateAction.action.Disable();
      slingAction.action.Disable();
    }

    /// <summary>
    /// Set an empty input when polled by the simulation.
    /// </summary>
    /// <param name="callback"></param>
    public void PollInput(CallbackPollInput callback) {
#if DEBUG
      if (callback.IsInputSet) {
        Debug.LogWarning($"{nameof(QuantumDebugInput)}.{nameof(PollInput)}: Input was already set by another user script, unsubscribing from the poll input callback. Please delete this component.", this);
        QuantumCallback.UnsubscribeListener(this);
        return;
      }
#endif

      var rotate = rotateAction.action.ReadValue<UnityEngine.Vector2>();
      var sling = slingAction.action.IsPressed();
      Quantum.Input i = new Quantum.Input();
      i.Rotation = new FPVector3(rotate.x.ToFP(), rotate.y.ToFP(), FP._0);
      i.Sling = sling;
      callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }
  }
}
