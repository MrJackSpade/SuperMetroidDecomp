using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Phantoon's body and of his room once the Wrecked Ship's power has come on (#1512, #2251), which the
/// import pre-colours his body sheet and his room's pictures in: each settled colouring of his body over the sixth tile
/// colour group, and the room's powered colours over every other colour group. His fades and the power coming on are
/// typed fades between them, published by the simulation.
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
            .. PhantoonHealthBandDefinitions.HighestFirst.Reverse().Select(PhantoonBodyLook.Health)];
        return [.. settled.Select(look => SheetShade.Run(PhantoonShades.Body(look), BodyFirstInk, Colours(phantoon, look)))];
    }

    /// <summary>The shades of his room's pictures: the room once the power has come on.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> PowerOn(BossSourceArtwork bosses)
    {
        PhantoonColorCatalog phantoon = bosses.PhantoonColors ?? throw new InvalidDataException("Phantoon's room is shaded from the installed Phantoon colours.");
        // Each part the power recolours is painted in its own colour group of the room.
        return [SheetShade.Combined(PhantoonShades.Powered, PhantoonSourceDefinitions.PowerOnGroups.Select(part =>
            SheetShade.Run(PhantoonShades.Powered, part.Value * GroupColours,
                [.. Enumerable.Range(0, GroupColours).Select(colour => phantoon.ResolvePowerOn(part.Key, colour))])))];
    }

    /// <summary>The sixteen colours of his body showing <paramref name="look"/>.</summary>
    private static RgbColor[] Colours(PhantoonColorCatalog phantoon, PhantoonBodyLook look) => look.Kind switch
    {
        PhantoonBodyLookKind.Black => new RgbColor[GroupColours],
        PhantoonBodyLookKind.White => [.. Enumerable.Repeat(RgbColor.White, GroupColours)],
        PhantoonBodyLookKind.FadedOut => [.. Enumerable.Range(0, PhantoonColorFormat.FadeOutCount).Select(phantoon.ResolveFadeOut)],
        PhantoonBodyLookKind.Health => [.. Enumerable.Range(0, PhantoonColorFormat.HealthBandColorCount).Select(colour => phantoon.ResolveHealth(
            look.Band ?? throw new InvalidDataException("A Phantoon health colouring names its health band."), colour))],
        _ => throw new InvalidDataException($"Undefined Phantoon body colouring {look.Kind}."),
    };


}
