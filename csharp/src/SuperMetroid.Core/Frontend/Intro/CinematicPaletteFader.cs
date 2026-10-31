using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's cinematic palette cross-fader: each screen colour sits at a fade level between black and its target,
/// and every step moves a group's colours one level toward black or toward the target.
/// </summary>
/// <remarks>
/// <see cref="Levels"/> steps separate black from a target, and a colour at level <c>n</c> shows each channel of its
/// target scaled by <c>n / Levels</c>, rounded down. That is the rounding the original's per-channel fixed-point
/// accumulators produce, so an early fade-in step can still show black. The fader holds one level per screen colour,
/// by the owner that holds the colour, and addresses them by the screen's colour groups (#1464).
/// </remarks>
public sealed class CinematicPaletteFader
{
    /// <summary>The steps between black and a target colour.</summary>
    internal const int Levels = 32;

    /// <summary>The colour each screen colour fades toward.</summary>
    private readonly PerScreenColour<RgbColor> targets = new();
    /// <summary>Each screen colour's fade level, from zero (black) to <see cref="Levels"/> (its target).</summary>
    private readonly PerScreenColour<int> levels = new();

    /// <summary>Ports <c>DecomposePaletteDataForFading</c> at $8B:8C09: every colour starts at its target.</summary>
    /// <param name="target">The screen colours the fade reaches.</param>
    public CinematicPaletteFader(ScreenColours target)
    {
        Ensure.NotNull(target);
        target.Backgrounds.CopyTo(targets.Backgrounds);
        target.Sprites.CopyTo(targets.Sprites);
        levels.Backgrounds.Fill(Levels);
        levels.Sprites.Fill(Levels);
    }

    /// <summary>Ports <c>ClearYColorsFromIndexX</c> at $8B:8C5E: the group's colours become black.</summary>
    /// <param name="group">The colours to clear.</param>
    internal void Clear(ScreenColourGroup group) => ScreenColourGroups.In(levels, group).Clear();

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c> at $8B:8C83: the group's colours take one step toward black.</summary>
    /// <param name="group">The colours to fade.</param>
    internal void FadeOut(ScreenColourGroup group)
    {
        Span<int> groupLevels = ScreenColourGroups.In(levels, group);
        for (int color = 0; color < groupLevels.Length; color++)
            groupLevels[color]--;
    }

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c> at $8B:8CB2: the group's colours take one step toward their target.</summary>
    /// <param name="group">The colours to fade.</param>
    internal void FadeIn(ScreenColourGroup group)
    {
        Span<int> groupLevels = ScreenColourGroups.In(levels, group);
        for (int color = 0; color < groupLevels.Length; color++)
            groupLevels[color]++;
    }

    /// <summary>Ports <c>ComposeFadingPalettes</c> at $8B:8CEA into the screen's colours.</summary>
    /// <exception cref="InvalidOperationException">A colour was faded past black or past its target.</exception>
    public void ComposeInto(ScreenPalette colours)
    {
        Ensure.NotNull(colours);
        Compose(targets.Backgrounds, levels.Backgrounds, colours.BackgroundSets.All);
        Compose(targets.Sprites, levels.Sprites, colours.SpriteSets.All);
    }

    /// <summary>Writes the visible colour of each of one owner's colours to that owner's colours.</summary>
    private static void Compose(ReadOnlySpan<RgbColor> targetColours, ReadOnlySpan<int> fadeLevels, ColourRun owner)
    {
        for (int color = 0; color < targetColours.Length; color++)
            owner.Write(color, Faded(targetColours[color], fadeLevels[color]));
    }

    /// <summary><paramref name="target"/> at fade <paramref name="level"/>: each channel scaled by the level, rounded down.</summary>
    /// <exception cref="InvalidOperationException">The level lies outside black through the target.</exception>
    private static RgbColor Faded(RgbColor target, int level)
    {
        if ((uint)level > Levels)
            throw new InvalidOperationException($"A cinematic colour was faded to level {level}, outside 0 through {Levels}.");
        return new RgbColor(target.Red * level / Levels, target.Green * level / Levels, target.Blue * level / Levels);
    }
}
