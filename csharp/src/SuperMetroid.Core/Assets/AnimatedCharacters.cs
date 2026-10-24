using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The runs of one layer's characters that animations paint (#1406), as asset extraction declared them when it pictured
/// the animations' frames. A character an animation paints shows that animation's selected frame; any other character
/// shows what is bound over it.
/// </summary>
public sealed class AnimatedCharacters
{
    /// <summary>A layer no animation paints.</summary>
    public static AnimatedCharacters None { get; } = new([]);

    /// <summary>Keeps the runs, each a run of characters one animation paints.</summary>
    /// <exception cref="ArgumentException">A run is empty or begins before the first character.</exception>
    public AnimatedCharacters(IEnumerable<AnimatedCharacterRun> runs)
    {
        Runs = [.. Ensure.NotNull(runs)];
        foreach (AnimatedCharacterRun run in Runs)
        {
            if (run.FirstCharacter < 0 || run.CharacterCount <= 0)
                throw new ArgumentException($"{run.Animation} paints an empty run or one before the first character.", nameof(runs));
        }
    }

    /// <summary>The runs.</summary>
    public ImmutableArray<AnimatedCharacterRun> Runs { get; }

    /// <summary>
    /// What <paramref name="character"/> shows if an animation that paints it has selected a frame in
    /// <paramref name="frames"/>: that frame, and how far into its pictured characters the character lies, in units of
    /// <paramref name="characterWords"/> per character.
    /// </summary>
    public bool TryShowing(int character, int characterWords, ArtworkFrameSelections frames, out BgArtworkShown shown)
    {
        Ensure.NotNull(frames);
        foreach (AnimatedCharacterRun run in Runs)
        {
            if (run.Paints(character) && frames.TryShown(run.Animation, out ArtworkFrame frame))
            {
                shown = new BgArtworkShown(frame.Artwork, (character - run.FirstCharacter) * characterWords);
                return true;
            }
        }
        shown = default;
        return false;
    }
}
