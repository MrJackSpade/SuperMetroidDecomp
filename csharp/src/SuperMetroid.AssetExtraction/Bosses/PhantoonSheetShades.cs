using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Phantoon's body and of his room as the Wrecked Ship's power comes on (#1512), which the import pre-colours
/// his body sheet and his room's pictures in: each settled colouring of his body over the sixth tile colour group, each
/// run of steps of each fade from one toward another, as his fades move each colour shown by its remaining distance over
/// the steps left (a run starts past the zero step, which moves nothing, and at any value through its last, as a hurt
/// colouring written mid-fade restarts the fade from that colouring at the step value it stands at), and each
/// step of the power coming on over every other colour group, from the room's own colours.
/// </summary>
internal static class PhantoonSheetShades
{
    /// <summary>The first ink of his body's colours on a body picture.</summary>
    private const int BodyFirstInk = 112;

    /// <summary>The colours of one colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>The shades of his body sheet.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Body(BossSourceArtwork bosses)
    {
        PhantoonColorCatalog phantoon = bosses.PhantoonColors ?? throw new InvalidDataException("Phantoon is shaded from the installed Phantoon colours.");
        PhantoonBodyLook[] settled =
            [PhantoonBodyLook.Black, PhantoonBodyLook.White, PhantoonBodyLook.FadedOut,
            .. Enumerable.Range(0, PhantoonColorDefinitions.HealthBandCount).Select(PhantoonBodyLook.Health)];
        PhantoonBodyLook[] targets =
            [PhantoonBodyLook.FadedOut, .. Enumerable.Range(0, PhantoonColorDefinitions.HealthBandCount).Select(PhantoonBodyLook.Health)];
        var shades = new List<SheetShade>();
        foreach (PhantoonBodyLook look in settled)
            shades.Add(SheetShade.Run(PhantoonShades.Body(look), BodyFirstInk, Colours(phantoon, look)));
        foreach (PhantoonBodyLook from in settled)
        {
            RgbColor[] start = Colours(phantoon, from);
            foreach (PhantoonBodyLook to in targets)
            {
                RgbColor[] goal = Colours(phantoon, to);
                foreach (int denominator in PhantoonShades.FadeDenominators)
                {
                    for (int first = 1; first <= denominator + 1; first++)
                    {
                        for (int last = first; last <= denominator + 1; last++)
                            shades.Add(SheetShade.Run(PhantoonShades.Fading(from, to, denominator, first, last), BodyFirstInk,
                                Faded(start, goal, denominator, first, last)));
                    }
                }
            }
        }
        return shades;
    }

    /// <summary>The shades of his room's pictures, whose standard colours are <paramref name="standard"/>: the power coming on.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> PowerOn(BossSourceArtwork bosses, ReadOnlySpan<RgbColor> standard)
    {
        PhantoonColorCatalog phantoon = bosses.PhantoonColors ?? throw new InvalidDataException("Phantoon's room is shaded from the installed Phantoon colours.");
        // The parts the power fades are the room's first colour groups, in order.
        int inks = PhantoonColorFormat.PoweredParts.Length * GroupColours;
        RgbColor[] start = standard[..inks].ToArray();
        RgbColor[] goal = [.. Enumerable.Range(0, inks).Select(ink =>
            phantoon.ResolvePowerOn(PhantoonColorFormat.PoweredParts[ink / GroupColours], ink % GroupColours))];
        return [.. Enumerable.Range(0, PhantoonShades.PowerOnDenominator + 2).Select(numerator =>
            SheetShade.Run(PhantoonShades.PowerOn(numerator), 0, Faded(start, goal, PhantoonShades.PowerOnDenominator, 0, numerator)))];
    }

    /// <summary>The sixteen colours of his body showing <paramref name="look"/>.</summary>
    private static RgbColor[] Colours(PhantoonColorCatalog phantoon, PhantoonBodyLook look) => look.Kind switch
    {
        PhantoonBodyLookKind.Black => new RgbColor[GroupColours],
        PhantoonBodyLookKind.White => [.. Enumerable.Repeat(RgbColor.White, GroupColours)],
        PhantoonBodyLookKind.FadedOut => [.. Enumerable.Range(0, PhantoonColorFormat.FadeOutCount).Select(phantoon.ResolveFadeOut)],
        PhantoonBodyLookKind.Health => [.. Enumerable.Range(0, PhantoonColorFormat.HealthBandColorCount).Select(colour => phantoon.ResolveHealth(look.Band, colour))],
        _ => throw new InvalidDataException($"Undefined Phantoon body colouring {look.Kind}."),
    };

    /// <summary>Each of <paramref name="start"/>'s colours after the fade's steps toward <paramref name="goal"/>'s of values <paramref name="first"/> through <paramref name="last"/>.</summary>
    private static RgbColor[] Faded(RgbColor[] start, RgbColor[] goal, int denominator, int first, int last)
    {
        var faded = new RgbColor[start.Length];
        for (int ink = 0; ink < faded.Length; ink++)
        {
            RgbColor current = start[ink];
            for (int step = first; step <= last; step++)
                current = GradualColourFade.Step(step, current, goal[ink], denominator);
            faded[ink] = current;
        }
        return faded;
    }
}
