namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One editable Kraid ceiling or spike draw in native block order.</summary>
/// <param name="Id">Compiled identity of a crumble stage, ceiling background, spike column, or defeated-room clearing layout.</param>
/// <param name="Blocks">Metatile/flip words in the single horizontal run's native order; the catalog copies every frame array.</param>
public sealed record RoomPlmKraidVisualEntry(string Id, ushort[] Blocks);
