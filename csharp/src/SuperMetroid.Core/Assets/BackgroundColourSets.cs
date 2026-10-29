namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's background colour sets (#1405). A room load fills them with the tileset's colours, room FX writes its
/// blend into layer 3's, and room effects, set pieces and bosses change them. The four-colour rows layer 3 shows are
/// the first two sets read four colours at a time, so a writer names its colours as a <see cref="BackgroundColourRun"/>
/// or a set.
/// </summary>
public sealed class BackgroundColourSets : ColourSets;
