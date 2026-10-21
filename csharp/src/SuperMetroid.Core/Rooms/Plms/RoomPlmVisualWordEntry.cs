namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable metatile/flip reference for one single-block PLM draw.</summary>
/// <param name="Id">Compiled identity of the draw.</param>
/// <param name="VisualWord">Twelve-bit metatile reference and parent flips, excluding the physical collision nibble.</param>
public sealed record RoomPlmVisualWordEntry(string Id, int VisualWord);
