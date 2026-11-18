namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The setup a placement's family runs as the room's population places it (#2288, #2383), with the typed argument the
/// placement gives it, decoded at import. Each family's setup is its own type, so a placement cannot carry an argument its
/// family does not read, nor lack one its family requires; the population loader hands it the room's family setups.
/// </summary>
internal abstract record RoomObjectPlacementSetup
{
    /// <summary>Only the family setups below derive from this.</summary>
    private protected RoomObjectPlacementSetup()
    {
    }

    /// <summary>Runs this setup of a placement of <paramref name="kind"/> through <paramref name="setups"/>.</summary>
    internal abstract void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind);
}
