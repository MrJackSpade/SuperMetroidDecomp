namespace SuperMetroid.Core.Assets;

/// <summary>
/// One background artwork binding (#1422): artwork shown over a run of a set's characters from one onward. The artwork is
/// named, never held: the import pipeline resolves a name to its characters.
/// </summary>
/// <param name="Identity">The artwork shown.</param>
/// <param name="Set">The characters it shows over.</param>
/// <param name="FirstCharacter">The first character the artwork shows at.</param>
/// <param name="CharacterCount">The characters of the artwork shown, from its first.</param>
public sealed record BgArtworkBinding(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter, int CharacterCount);
