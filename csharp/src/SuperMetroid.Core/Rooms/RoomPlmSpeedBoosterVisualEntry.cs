namespace SuperMetroid.Core.Rooms;

/// <summary>The visible block selected by one Speed Booster terrain frame.</summary>
/// <param name="Id">Case-sensitive visual identity <c>bomb-reveal</c> for native draw $84:A4F3.</param>
/// <param name="Blocks">Mutable input array containing exactly one twelve-bit visual word: a ten-bit 16-by-16 metatile index plus parent flip bits, without collision bits.</param>
public sealed record RoomPlmSpeedBoosterVisualEntry(string Id, ushort[] Blocks);
