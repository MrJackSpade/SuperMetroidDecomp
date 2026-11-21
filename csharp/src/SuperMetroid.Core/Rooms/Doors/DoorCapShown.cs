namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What a coloured or grey cap's sequence shows (#1296, #1994): one of the caps' looks, drawn in the paint that draws it;
/// facing comes from the cap.
/// </summary>
/// <param name="Look">The look shown.</param>
/// <param name="Paint">Which paint draws it: the cap's own, the grey caps' or the blue's.</param>
internal readonly record struct DoorCapShown(DoorCapLook Look, DoorCapShownPaint Paint);
