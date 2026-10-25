using SuperMetroid.Core.Input;

namespace SuperMetroid.DesktopVerification.Input;

/// <summary>Result of the headless host-key routing regression.</summary>
public readonly record struct HostKeyboardInputSmokeTestResult(
    ControllerButton EnterControllerWord,
    ControllerButton ReleasedControllerWord);
