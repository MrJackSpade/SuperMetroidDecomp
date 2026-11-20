namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room effect's live liquid surface while the effect has one (#1546, #2554): the room line it lies on after rising and
/// tide processing, which the effect moves. A tide can carry it above the room's top, to a negative line.
/// </summary>
public sealed class RoomLiquidSurface
{
    /// <summary>Creates the surface on <paramref name="line"/>.</summary>
    /// <param name="line">The room line the surface lies on.</param>
    internal RoomLiquidSurface(int line) => Line = line;

    /// <summary>The room line the surface lies on, negative above the room's top.</summary>
    [IntegerQuantity("room line of the liquid surface")]
    public int Line { get; internal set; }
}
