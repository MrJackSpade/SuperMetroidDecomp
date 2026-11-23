namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The access Samus uses at a map, energy or missile station (#2545): the access block where its arm is drawn, and the
/// access, the station's and its side's, that selects the arm's draws.
/// </summary>
/// <param name="Block">The access block Samus ran into, where the arm is drawn.</param>
/// <param name="Access">The station and the side Samus used it from.</param>
internal readonly partial record struct StationAccessUse(Position16<RoomBlocks> Block, StationAccess Access);
