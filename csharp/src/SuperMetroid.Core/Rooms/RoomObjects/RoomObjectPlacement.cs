namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement: where it stands and its family's setup with the typed argument that setup reads (#2383), which
/// names the kind of room object it places (#2557). Setup payloads are bounded domain records, not a store that can supply
/// arbitrary values.
/// </summary>
public sealed record RoomObjectPlacement
{
    /// <summary>A placement at <paramref name="block"/> that its family sets up with <paramref name="setup"/>.</summary>
    /// <param name="block">Placement in 16-pixel room blocks.</param>
    /// <param name="setup">The setup the placement's family runs, with what the placement gives it, decoded at import.</param>
    internal RoomObjectPlacement(Position16<RoomBlocks> block, RoomObjectPlacementSetup setup)
    {
        Block = block;
        Setup = Ensure.NotNull(setup);
    }

    /// <summary>The kind of room object placed, which its setup names.</summary>
    public RoomObjectKind Kind => Setup.Kind;

    /// <summary>Placement in 16-pixel room blocks.</summary>
    public Position16<RoomBlocks> Block { get; private init; }

    /// <summary>The setup the placement's family runs, with what the placement gives it, decoded at import.</summary>
    internal RoomObjectPlacementSetup Setup { get; }

    /// <summary>Whether <see cref="Block"/> is a block of the room's grid or of the terrain beyond the room, decoded at import.</summary>
    public RoomObjectSite Site { get; init; }
}
