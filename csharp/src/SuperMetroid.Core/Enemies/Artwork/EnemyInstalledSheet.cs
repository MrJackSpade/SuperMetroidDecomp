using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One enemy's installed tile sheet and the palette it is drawn with.</summary>
/// <param name="Definition">The enemy the sheet belongs to.</param>
/// <param name="Sheet">The enemy's installed characters.</param>
/// <param name="Palette">The palette the characters are drawn with.</param>
internal readonly record struct EnemyInstalledSheet(EnemyDefinitionId Definition, RoomCharacterAtlas Sheet, EnemyPaletteSheet Palette);
