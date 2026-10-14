namespace SuperMetroid.Desktop;

/// <summary>Measurements and rejection outcomes from the headless debugger-state compatibility and deterministic-continuation check.</summary>
/// <param name="SavedFrame">Sixteen-bit frontend frame number stored in the saved state.</param>
/// <param name="ContinuationFrames">Updates compared after restoration against the uninterrupted gameplay, pixel, PCM, and acknowledgement sequence.</param>
/// <param name="StateFileBytes">Saved state file size in bytes after its build identifiers are changed.</param>
/// <param name="WrongRomRejected">Whether loading with a modified cartridge digest produced the expected rejection.</param>
/// <param name="EmptySlotReported">Whether an unused slot was correctly reported as absent.</param>
public readonly record struct DebuggerSaveStateSmokeTestResult(
    ushort SavedFrame,
    int ContinuationFrames,
    long StateFileBytes,
    bool WrongRomRejected,
    bool EmptySlotReported);
