namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The next of the eight setup updates of the X-ray cone effect to run, or
/// <see cref="Complete"/> once setup has handed the cone to its per-update beam step.
/// Each member names one setup update.
/// </summary>
public enum XraySetupStage
{
    /// <summary>All eight calls ran; the main frame behaviour's successor owns the beam.</summary>
    Complete = 0,
    /// <summary>
    /// Call one: time freezes and the background's view is kept to restore when the beam ends.
    /// </summary>
    FreezeTimeSaveBackground = 1,
    /// <summary>Call two: the game reads the foreground's right half; the call only spends its update.</summary>
    ReadForegroundSecondScreen = 2,
    /// <summary>Call three: the game reads the foreground's left half; the call only spends its update.</summary>
    ReadForegroundFirstScreen = 3,
    /// <summary>
    /// Call four, where the game builds the reveal's left half;
    /// the port shows the reveal from the room as each frame shows it, so the call only spends its update.
    /// </summary>
    BuildRevealFirstScreen = 4,
    /// <summary>
    /// Call five, where the game builds the reveal's right half; the call only spends its update.
    /// </summary>
    BuildRevealSecondScreen = 5,
    /// <summary>Call six: the game shows the reveal's left half; the call only spends its update.</summary>
    TransferRevealFirstScreen = 6,
    /// <summary>Call seven: the beam is initialized and the game shows the reveal's right half.</summary>
    InitializeTransferRevealSecondScreen = 7,
    /// <summary>Call eight: the backdrop takes the X-ray's colour.</summary>
    BackdropColor = 8,
}
