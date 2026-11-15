namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five draws of a breakable Grapple block: the intact block, the rock it breaks into, the rock in pieces and in dust,
/// and the cleared air (#1994).
/// </summary>
internal enum GrappleBlockDraw
{
    /// <summary>The intact block, with Grapple collision.</summary>
    Intact,
    /// <summary>The block shown as a plain rock as it starts to break, already air.</summary>
    Rock,
    /// <summary>The rock broken into pieces.</summary>
    Pieces,
    /// <summary>The pieces crumbled to dust.</summary>
    Dust,
    /// <summary>The cleared block: blank air.</summary>
    Blank,
}
