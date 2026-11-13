using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's crossfade between the narration and a flashback scene (#1512, #1597): the scene it fades out, the scene
/// it fades in and the steps it has taken. The incoming scene shows at the level of the steps taken, the outgoing one at
/// the rest, and every other scene at full; before the first crossfade every scene shows at full. The pictures that show a
/// scene name the shade of its level, which the import pre-colours; the narration text shows its level as a colour effect.
/// </summary>
/// <remarks>
/// <see cref="Levels"/> steps separate black from a colour, and a colour at level <c>n</c> shows each channel scaled by
/// <c>n / Levels</c>, rounded down. That is the rounding the game's per-channel fixed-point sums produce, so
/// an early fade-in step can still show black.
/// </remarks>
public sealed class IntroCrossfade
{
    /// <summary>The steps between black and a colour, which a whole crossfade takes.</summary>
    internal const int Levels = 32;

    /// <summary>The scene the crossfade fades out, or none before the first crossfade.</summary>
    private readonly IntroCrossfadeScene? outgoing;
    /// <summary>The scene the crossfade fades in, or none before the first crossfade.</summary>
    private readonly IntroCrossfadeScene? incoming;
    /// <summary>The steps the crossfade has taken, from zero through <see cref="Levels"/>.</summary>
    private int steps;

    /// <summary>The opening before its first crossfade: every scene shows at full.</summary>
    internal IntroCrossfade()
    {
    }

    /// <summary>Starts a crossfade from <paramref name="from"/> to <paramref name="to"/>: the incoming scene starts black.</summary>
    /// <exception cref="ArgumentException">The two scenes are the same.</exception>
    internal IntroCrossfade(IntroCrossfadeScene from, IntroCrossfadeScene to)
    {
        if (from == to)
            throw new ArgumentException($"The opening cannot crossfade {from} into itself.", nameof(to));
        outgoing = from;
        incoming = to;
    }

    /// <summary>Takes one step: the outgoing scene moves one level toward black and the incoming one toward its colours.</summary>
    /// <exception cref="InvalidOperationException">No crossfade has started, or the crossfade has already taken every step.</exception>
    internal void Step()
    {
        if (incoming is null)
            throw new InvalidOperationException("The opening has not started a crossfade.");
        if (steps == Levels)
            throw new InvalidOperationException($"The opening's crossfade from {outgoing} to {incoming} has already taken its {Levels} steps.");
        steps++;
    }

    /// <summary>The level <paramref name="scene"/> shows, from zero (black) to <see cref="Levels"/> (its colours).</summary>
    internal int LevelOf(IntroCrossfadeScene scene) =>
        scene == incoming ? steps : scene == outgoing ? Levels - steps : Levels;

    /// <summary>
    /// The narration text's shading (#1512): the narration's level as a colour effect over its glow colours, or its glow
    /// colours alone at full.
    /// </summary>
    internal PictureShading NarrationShading(PictureShading glow)
    {
        int level = LevelOf(IntroCrossfadeScene.Narration);
        return level == Levels ? Ensure.NotNull(glow) : Ensure.NotNull(glow).With(new ColourLevel(level, Levels));
    }

    /// <summary>A scene picture's shading: the shade of each scene's level below full.</summary>
    internal PictureShading SceneShading
    {
        get
        {
            ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>();
            foreach (IntroCrossfadeScene scene in Enum.GetValues<IntroCrossfadeScene>())
            {
                int level = LevelOf(scene);
                if (level < Levels)
                    shades.Add(IntroShades.Faded(scene, level));
            }
            return new PictureShading(shades.ToImmutable(), []);
        }
    }

    /// <summary>
    /// An actor's shading: <paramref name="drawn"/>, then each scene's level below full over the actor's own-palette parts
    /// and over the parts its drawn shade colours.
    /// </summary>
    internal PictureShading ActorShading(PictureShade drawn)
    {
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>();
        shades.Add(drawn);
        foreach (IntroCrossfadeScene scene in Enum.GetValues<IntroCrossfadeScene>())
        {
            int level = LevelOf(scene);
            if (level >= Levels)
                continue;
            shades.Add(IntroShades.Faded(scene, level));
            shades.Add(IntroShades.Faded(scene, level, drawn));
        }
        return new PictureShading(shades.ToImmutable(), []);
    }
}
