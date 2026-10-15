namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One editable visual reference for a compiled breakable-Grapple draw list.</summary>
/// <param name="DrawPointer">Bank-$84 identity of the initial, one of three breakup, or blank one-block draw lists.</param>
/// <param name="VisualWord">Twelve-bit metatile reference and parent flips, excluding Grapple/air collision bits.</param>
public sealed record RoomPlmGrappleBlockVisualEntry(ushort DrawPointer, ushort VisualWord);
