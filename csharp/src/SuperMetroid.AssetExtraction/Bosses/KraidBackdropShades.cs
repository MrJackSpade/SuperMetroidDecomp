using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Kraid's room backdrop (#1512), which the import pre-colours his room's pictures in: his backdrop
/// colours over the room colour group his growth recolours, and each step of its fades, as
/// <c>AdvanceKraidRoomBackgroundFade</c> moves each colour by its remaining distance over the steps left.
/// </summary>
internal static class KraidBackdropShades
{
    /// <summary>The first ink of a room picture the backdrop recolours: the fifth tile colour group's first.</summary>
    private const int FirstInk = 96;

    /// <summary>The fades' length: their last moving step.</summary>
    private const int Denominator = KraidBackgroundDefinitions.BackdropFadeLastStep - 1;

    /// <summary>Every backdrop shade of a room whose standard colours are <paramref name="standard"/>, to Kraid's <paramref name="kraid"/>.</summary>
    internal static IReadOnlyList<SheetShade> Of(ReadOnlySpan<RgbColor> standard, KraidColorCatalog kraid)
    {
        ArgumentNullException.ThrowIfNull(kraid);
        var room = new RgbColor[KraidPalettes.BandColors];
        var backdrop = new RgbColor[KraidPalettes.BandColors];
        for (int colour = 0; colour < backdrop.Length; colour++)
        {
            room[colour] = standard[FirstInk + colour];
            backdrop[colour] = kraid.Resolve(KraidPaletteSource.RoomBackdrop, colour);
        }
        var black = new RgbColor[backdrop.Length];
        var shades = new List<SheetShade> { SheetShade.Run(KraidShades.Backdrop, FirstInk, backdrop) };
        for (int steps = 0; steps < KraidShades.BackdropFadeShades; steps++)
        {
            shades.Add(SheetShade.Run(KraidShades.BackdropFadingInFromRoomColours(steps), FirstInk, Faded(room, backdrop, steps)));
            shades.Add(SheetShade.Run(KraidShades.BackdropFadingOut(steps), FirstInk, Faded(backdrop, black, steps)));
            shades.Add(SheetShade.Run(KraidShades.BackdropFadingInFromBlack(steps), FirstInk, Faded(black, backdrop, steps)));
        }
        return shades;
    }

    /// <summary>Each of <paramref name="from"/>'s colours after <paramref name="steps"/> steps of the fade toward <paramref name="to"/>'s.</summary>
    private static RgbColor[] Faded(RgbColor[] from, RgbColor[] to, int steps)
    {
        var faded = new RgbColor[from.Length];
        for (int colour = 0; colour < faded.Length; colour++)
        {
            RgbColor current = from[colour];
            for (int step = 0; step < steps; step++)
                current = GradualColourFade.Step(step, current, to[colour], Denominator);
            faded[colour] = current;
        }
        return faded;
    }
}
