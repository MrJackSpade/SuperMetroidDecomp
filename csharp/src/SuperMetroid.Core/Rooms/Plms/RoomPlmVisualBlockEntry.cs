using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable appearance of one single-block PLM draw.</summary>
/// <param name="Id">Compiled identity of the draw.</param>
/// <param name="Appearance">The block the draw shows and its mirroring.</param>
public sealed record RoomPlmVisualBlockEntry(string Id, RoomBlockAppearance Appearance);
