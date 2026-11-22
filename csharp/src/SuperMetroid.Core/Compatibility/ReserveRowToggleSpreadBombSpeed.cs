namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the equipment toggle does to play when it lands on the first reserve row (#1505): moving into the reserve
/// column and toggling on the same update changes no equipment, but changes the vertical speed of a charged bomb
/// spread's bomb in the last bomb place. The speed's whole pixels keep their part below 1024 and their multiple of 8192,
/// and the part between becomes 3072; the speed's fraction is not touched. The bomb resumes with that speed after the
/// pause.
/// </summary>
/// <remarks>
/// Consulted by the pause screen's equipment toggle when the reserve column's first row is selected. Evidence: the
/// toggle at <c>$82:B568</c> reads the reserve category's equipment word and mask through null table entries, so both
/// are the scratch word holding the row's label offset; the word tests as set, is cleared, and the disabled-label
/// palette loop at <c>$82:A29D</c> then writes through that cleared word. Its first write points the loop at the
/// projectile area, so it repaints the last bomb place's vertical speed (read whole by the spread bomb's update at
/// <c>$90:D919</c>) and the low bytes' neighbours: projectile directions, which every reader masks to their low byte,
/// and bomb directions, which nothing reads. The other reserve rows take their tables from scratch other code left,
/// which only full memory emulation could reproduce, and stay refused under #1500.
/// </remarks>
[CompatibilityService("A same-frame equipment toggle into the first reserve row sets part of the last spread bomb's whole vertical speed")]
public static partial class ReserveRowToggleSpreadBombSpeed
{
    /// <summary>The whole pixels below which the speed keeps its own part.</summary>
    private const int KeptBelow = 1024;

    /// <summary>The whole pixels whose multiple the speed keeps; the part between this and <see cref="KeptBelow"/> is set.</summary>
    private const int SetWithin = 8192;

    /// <summary>What the part of the speed between <see cref="KeptBelow"/> and <see cref="SetWithin"/> becomes.</summary>
    private const int SetPart = 3072;

    /// <summary>
    /// The last bomb place's whole vertical speed after the toggle: its part below 1024 and its multiple of 8192 kept,
    /// and the part between set to 3072; the speed's fraction is not touched. The pause screen applies it to the bomb
    /// frozen in that place.
    /// </summary>
    /// <param name="wholeSpeed">The bomb's whole vertical speed before the toggle, -32768 through 32767.</param>
    public static int SpeedAfterToggle(int wholeSpeed) =>
        wholeSpeed - FloorRemainder(wholeSpeed, SetWithin) + SetPart + FloorRemainder(wholeSpeed, KeptBelow);

    /// <summary>The remainder of <paramref name="value"/> past its multiple of <paramref name="span"/>, never negative.</summary>
    private static int FloorRemainder(int value, int span) => ((value % span) + span) % span;
}
