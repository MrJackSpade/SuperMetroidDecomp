namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a breaking block shows as it breaks up (#1994): a plain rock, the rock broken into pieces, the pieces crumbled to
/// dust, then nothing. A block growing back shows them in reverse.
/// </summary>
internal enum BlockBreakupStage
{
    /// <summary>The block shown as a plain rock as it starts to break.</summary>
    Rock,
    /// <summary>The rock broken into pieces.</summary>
    Pieces,
    /// <summary>The pieces crumbled to dust.</summary>
    Dust,
    /// <summary>Nothing: the block is gone and stands open.</summary>
    Gone,
}
