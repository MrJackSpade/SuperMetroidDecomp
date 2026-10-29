namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A run of characters a block's picture reads, all shown by one artwork (#1431).</summary>
/// <param name="FirstCharacter">The first character, as tile words number it.</param>
/// <param name="CharacterCount">The characters from it the run spans.</param>
public readonly record struct RoomBlockCharacterRun(int FirstCharacter, int CharacterCount);
