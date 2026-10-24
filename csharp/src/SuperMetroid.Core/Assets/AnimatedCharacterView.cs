namespace SuperMetroid.Core.Assets;

/// <summary>
/// What a layer's animated characters show now (#1406): the runs animations paint there, with the frames the room's
/// animations have selected. The display resolves each character's picture through it before what is bound there.
/// </summary>
/// <param name="Characters">The runs of the layer's characters animations paint.</param>
/// <param name="Frames">The frames the room's animations show.</param>
public readonly record struct AnimatedCharacterView(AnimatedCharacters Characters, ArtworkFrameSelections Frames)
{
    /// <summary>A layer no animation paints.</summary>
    public static AnimatedCharacterView None => new(AnimatedCharacters.None, new ArtworkFrameSelections());

    /// <summary>
    /// What <paramref name="character"/> shows if an animation that paints it shows a frame: the frame, and how far into
    /// its pictured characters the character lies, in units of <paramref name="characterWords"/> per character.
    /// </summary>
    public bool TryShowing(int character, int characterWords, out BgArtworkShown shown)
    {
        if (Characters is null || Frames is null)
        {
            shown = default;
            return false;
        }
        return Characters.TryShowing(character, characterWords, Frames, out shown);
    }
}
