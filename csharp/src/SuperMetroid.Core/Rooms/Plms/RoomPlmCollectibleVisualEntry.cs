namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable metatile/flip reference for one collectible draw frame.</summary>
/// <param name="Id">Compiled identity of an empty, orb, tank, dynamic-slot, or shot-reveal frame.</param>
/// <param name="VisualWord">Twelve-bit metatile reference and parent flips, excluding the physical collision nibble.</param>
public sealed record RoomPlmCollectibleVisualEntry(string Id, ushort VisualWord);
