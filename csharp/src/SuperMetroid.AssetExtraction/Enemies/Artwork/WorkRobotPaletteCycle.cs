using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed Work Robot glow colours, asked for by the place the glow shows (#2556). The installed document holds one
/// colour record per glow step in the order the glow shows them (<see cref="WorkRobotGlowAnimations.Cycle"/>); the
/// steps that show one place show it in one colouring (#2393). Cadence and ownership of the robot's colours stay the
/// glow's (<see cref="WorkRobotGlowDefinitions"/>).
/// </summary>
public sealed class WorkRobotPaletteCycle
{
    /// <summary>The first of the robot's sheet inks the glow recolours: the glow's colours replace that ink and the three after it.</summary>
    private const int FirstGlowInk = 9;

    /// <summary>The installed records, one per glow step in the glow's order.</summary>
    private readonly InstalledColourRows steps;

    /// <summary>Keeps the complete installed records.</summary>
    /// <param name="steps">Each glow step's four colours, in the glow's order.</param>
    internal WorkRobotPaletteCycle(InstalledColourRows steps) => this.steps = steps;

    /// <summary>The colours the glow shows <paramref name="place"/> in: the record of every step showing it, which must agree.</summary>
    /// <exception cref="InvalidDataException">Two steps showing the place have different colours, or no step shows it.</exception>
    internal RgbColor[] GlowColours(WorkRobotGlowPlace place)
    {
        RgbColor[][] showing = [.. Enumerable.Range(0, WorkRobotGlowAnimations.Cycle.Count)
            .Where(step => WorkRobotGlowAnimations.Cycle[step] == place)
            .Select(step => Enumerable.Range(0, WorkRobotPaletteDefinitions.ColorCount).Select(colour => steps.Colour(step, colour)).ToArray())];
        if (showing.Length == 0)
            throw new InvalidDataException($"No Work Robot glow step shows {place}.");
        if (showing.Any(colours => !colours.SequenceEqual(showing[0])))
            throw new InvalidDataException($"The Work Robot glow colour records for {place} differ; each place has one colouring.");
        return showing[0];
    }

    /// <summary>The shade that shows the robot's glow at <paramref name="place"/>, recolouring its glow inks in the colour group from <paramref name="firstInkOfGroup"/>.</summary>
    /// <exception cref="InvalidDataException">The place's colouring is not one colouring.</exception>
    internal SheetShade GlowShade(WorkRobotGlowPlace place, int firstInkOfGroup) =>
        SheetShade.Run(WorkRobotShades.Glow(place), firstInkOfGroup + FirstGlowInk, [.. GlowColours(place)]);
}
