namespace SuperMetroid.DesktopVerification;

/// <summary>Result of changing status text above a fixed gameplay viewport.</summary>
public readonly record struct HostViewportLayoutSmokeTestResult(
    Rectangle BeforeCanvasBounds,
    Rectangle AfterCanvasBounds,
    Size BeforeToolbarPreferredSize,
    Size AfterToolbarPreferredSize);
