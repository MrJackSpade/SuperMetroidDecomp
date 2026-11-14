using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// Checks the import's sprite-row palette-FX shades against the cartridge (#1512): the Ridley and Phantoon sprite statues'
/// grey-out and the escape's shutter flash, each record pre-coloured over the sprite row its group writes.
/// </summary>
internal static class PaletteFxSpriteShadesVerification
{
    /// <summary>Colours in one sprite row.</summary>
    private const int RowColours = 16;

    /// <summary>
    /// For each sprite-row group, the shades from the installed colours equal the shades coloured straight from the
    /// cartridge's records, record for record, ink for ink, and start at the sprite colour the group's records write.
    /// </summary>
    /// <param name="bus">The cartridge the records' colours are read from.</param>
    /// <param name="installed">The installed palette-FX colours.</param>
    internal static void VerifyPaletteFxSpriteShades(ISnesAddressSpace bus, IPaletteFxColorSource installed)
    {
        var native = new ReferencePaletteFxColorSource(bus);
        // The sprite row each group writes: the statues' rows of the statue scene and the first enemy row the shutters
        // and Mother Brain's brain draw in.
        (ColourEffectId Effect, ColourEffectColours Group, int Row)[] groups =
        [
            (ColourEffectId.GreyOutTourianStatueRidley, ColourEffectColours.RidleyStatue, 1),
            (ColourEffectId.GreyOutTourianStatuePhantoon, ColourEffectColours.PhantoonStatue, 2),
            (ColourEffectId.TourianShutterRedFlashing, ColourEffectColours.TourianShutters, 1),
        ];
        foreach ((ColourEffectId effect, ColourEffectColours group, int row) in groups)
        {
            SheetShade[] fromInstalled = [.. PaletteFxSpriteShades.Of(effect, group, row * RowColours, installed)];
            SheetShade[] fromCartridge = [.. PaletteFxSpriteShades.Of(effect, group, row * RowColours, native)];
            VerificationAssertions.AssertTrue(fromInstalled.Length > 0 && fromInstalled.Length == fromCartridge.Length,
                $"{group}: one shade per record its program shows");
            for (int index = 0; index < fromInstalled.Length; index++)
            {
                SheetShade shade = fromInstalled[index];
                SheetShade expected = fromCartridge[index];
                VerificationAssertions.AssertTrue(shade.Shade == expected.Shade && shade.Inks.SequenceEqual(expected.Inks)
                    && shade.Colours.SequenceEqual(expected.Colours), $"{group} shade {shade.Shade} equals the cartridge's record colours");
                int first = group.CartridgeColour - GameplayColourPositionDefinitions.SpriteSetsFirst;
                VerificationAssertions.AssertTrue(shade.Inks.All(ink => ink >= first && ink < (row + 1) * RowColours),
                    $"{group} shade {shade.Shade} lies from the sprite colour the cartridge's records start at to the row's end");
            }
        }
        Console.WriteLine("Palette-FX sprite shades: statue grey-outs and the shutter flash match the cartridge's records.");
    }
}
