namespace SuperMetroid.Core.Rooms;

/// <summary>The elevatube PLM's single editable visible block.</summary>
/// <param name="Id">The compiled visual identity <c>elevatube-block</c>.</param>
/// <param name="Blocks">Exactly one metatile/flip word; the catalog retains its scalar value, not the supplied array.</param>
public sealed record RoomPlmMaridiaElevatubeVisualEntry(string Id, ushort[] Blocks);
