namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement: the kind of room object it places, where, and its family's setup with the typed argument that
/// setup reads (#2383). Setup payloads are bounded domain records, not a store that can supply arbitrary values.
/// </summary>
public sealed record RoomObjectPlacement
{
    /// <summary>A placement of <paramref name="kind"/> at <paramref name="block"/> that its family sets up with <paramref name="setup"/>.</summary>
    /// <param name="kind">The kind of room object placed.</param>
    /// <param name="block">Placement in 16-pixel room blocks.</param>
    /// <param name="setup">The setup the placement's family runs, with what the placement gives it, decoded at import.</param>
    internal RoomObjectPlacement(RoomObjectKind kind, Position16<RoomBlocks> block, RoomObjectPlacementSetup setup)
    {
        Kind = kind;
        Block = block;
        Setup = Ensure.NotNull(setup);
    }

    /// <summary>The kind of room object placed.</summary>
    public RoomObjectKind Kind { get; init; }

    /// <summary>
    /// Placement in 16-pixel room blocks. A placement whose object only holds its place, or watches the whole room, still
    /// stands on the game's block: the room's resident-trigger lookup gives a projectile's hit to the first active place
    /// at the struck block whatever it holds, so the block is observed (#2558).
    /// </summary>
    public Position16<RoomBlocks> Block { get; init; }

    /// <summary>The setup the placement's family runs, with what the placement gives it, decoded at import.</summary>
    internal RoomObjectPlacementSetup Setup { get; }

    /// <summary>Whether <see cref="Block"/> is a block of the room's grid or of the terrain beyond the room, decoded at import.</summary>
    public RoomObjectSite Site { get; init; }
}
