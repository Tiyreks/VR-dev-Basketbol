using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

// Keeps WASD walking in the XR Device Simulator at one steady speed.
// The simulator adds the sideways and forward input without normalizing them,
// so holding two keys (diagonal) is about 1.41x faster. This runs right before the
// simulator each frame and scales its body speed down while moving diagonally.
[DefaultExecutionOrder(XRInteractionUpdateOrder.k_DeviceSimulator - 1)]
[RequireComponent(typeof(XRDeviceSimulator))]
public class SimulatorWalkSpeed : MonoBehaviour
{
    public float walkSpeed = 3.5f;

    private XRDeviceSimulator simulator;

    private void Awake()
    {
        simulator = GetComponent<XRDeviceSimulator>();
    }

    private void Update()
    {
        float x = ReadAxis(simulator.keyboardXTranslateAction);
        float z = ReadAxis(simulator.keyboardZTranslateAction);
        float length = Mathf.Max(1f, new Vector2(x, z).magnitude);

        // Walking speed = translate speed x body multiplier, so solve for the multiplier.
        simulator.keyboardBodyTranslateMultiplier = walkSpeed / simulator.keyboardZTranslateSpeed / length;
    }

    private static float ReadAxis(UnityEngine.InputSystem.InputActionReference reference)
    {
        return reference != null && reference.action != null ? reference.action.ReadValue<float>() : 0f;
    }
}
