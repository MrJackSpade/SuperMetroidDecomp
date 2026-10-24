namespace SuperMetroid.Core.Assets;

/// <summary>
/// A run of a layer's characters that an animation paints (#1406): tile words naming one of them show the picture of the
/// animation's selected frame. Asset extraction declares each run when it pictures the animation's frames.
/// </summary>
/// <param name="Animation">The animation.</param>
/// <param name="FirstCharacter">The first character the animation paints, as tile words number it.</param>
/// <param name="CharacterCount">The characters it paints.</param>
public readonly record struct AnimatedCharacterRun(ArtworkAnimation Animation, int FirstCharacter, int CharacterCount)
{
    /// <summary>Whether the run paints <paramref name="character"/>.</summary>
    public bool Paints(int character) => character >= FirstCharacter && character < FirstCharacter + CharacterCount;
}
