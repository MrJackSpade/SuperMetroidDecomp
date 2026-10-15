namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The five room-pixel joint positions written by <c>$A9:91B8-$92AA</c>. Segment four is the
/// brain's attachment point; segments one through three also own the encounter's neck hitboxes.
/// </summary>
internal readonly record struct MotherBrainNeckGeometry(
    Position16<RoomPixels> Segment0,
    Position16<RoomPixels> Segment1,
    Position16<RoomPixels> Segment2,
    Position16<RoomPixels> Segment3,
    Position16<RoomPixels> Segment4);
