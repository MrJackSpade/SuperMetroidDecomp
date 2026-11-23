namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phase Kraid's body runs once he has sunk, or in a room he was found defeated in (<see cref="KraidStageRun.Sunk"/>).
/// </summary>
public enum KraidSunkPhase
{
    /// <summary>Hides his body once he has sunk, before the room's backdrop is restored.</summary>
    HideBody,
    /// <summary>
    /// Waits the updates his death keeps the room dark before fading it back in (#1888), on the body's phase wait.
    /// </summary>
    WaitBeforeFadeIn,
    /// <summary>Fades the ordinary background in, requests room music, and records Kraid defeated.</summary>
    FadeInBackground,
    /// <summary>His death has played out: his body is given a dead enemy's properties once the camera leaves the room's left edge.</summary>
    Finished,
}
