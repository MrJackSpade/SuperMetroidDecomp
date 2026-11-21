namespace SuperMetroid.Core.Samus;

/// <summary>
/// A hold on Samus's movement that keeps its own state after another owner takes over, as an owner's hand-over ends it
/// (<see cref="SamusMovementOwnerDefinitions"/>).
/// </summary>
internal enum SamusMovementHoldEnd
{
    /// <summary>The shinespark's hold, ended whatever it is doing; its boost and colouring survive.</summary>
    Shinespark,

    /// <summary>The shinespark's hold, ended only while it moves her, so that a stored shine stays stored.</summary>
    ShinesparkMovingHer,

    /// <summary>Crystal Flash's hold; its suit colouring survives.</summary>
    CrystalFlash,

    /// <summary>Draygon's hold, whose escape check would otherwise go on counting presses.</summary>
    DraygonHold,

    /// <summary>The drain's hold, which would otherwise lock her again.</summary>
    Drain,
}
