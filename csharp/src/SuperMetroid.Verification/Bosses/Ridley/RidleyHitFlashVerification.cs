using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Checks Ridley's hurt flash against the cartridge (#1512): his draw clears his palette bits on flashing frames, so every
/// pixel of his, drawn in row seven by his initialization's palette index with parts that name no row of their own, shows
/// the common row's colour, whatever his encounter loaded into his own row.
/// </summary>
internal static class RidleyHitFlashVerification
{
    /// <summary>The row Ridley's initialization's palette index selects for his draws.</summary>
    private const int RidleyRow = 7;

    /// <summary>The colours of one palette row.</summary>
    private const int RowColours = 16;

    /// <summary>
    /// Checks that Ridley's flashing draw lays the flash after his encounter's shades, and that the import's flash shade of
    /// his sheet lists every ink he paints, each in the common row's colour, so none of the encounter's colours shows through.
    /// </summary>
    internal static int VerifyRidleyHitFlash()
    {
        var colours = new RidleyColours();
        colours.Sprites.Lay(RidleyShades.NorfairStart);
        AssertTrue(colours.SpriteShading(true).Shades.SequenceEqual([RidleyShades.NorfairStart, EnemyShades.HitFlash]),
            "Ridley's flashing draw lays the hurt flash after the shades his encounter laid");
        AssertTrue(colours.SpriteShading(false).Shades.SequenceEqual([RidleyShades.NorfairStart]),
            "Ridley's ordinary draw shows the shades his encounter laid");

        EnemySheetInputs inputs = EnemySheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory);
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
            candidate.Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Ridley));
        EnemySheetSource source = EnemySheetComposer.Sources(inputs, set).Enemies.Single(sheet => sheet.Enemy == EnemyDefinitionId.Ridley);
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(source.Sources, 0, 0);
        int[] painted = [.. sheet.PaletteIndices.Where(ink => ink != 0).Select(ink => (int)ink).Distinct().Order()];
        // Each ink is his part's own row and colour, drawn from that row with his draws' palette bits ORed in (#1512).
        AssertTrue(painted.Length > 0 && painted.All(ink => (ink / RowColours | sheet.Manifest.PaletteBits) == RidleyRow),
            $"Ridley's sheet is coloured in row seven, as his draws select (palette bits {sheet.Manifest.PaletteBits}; own rows " +
            string.Join(", ", painted.Select(ink => ink / RowColours).Distinct().Order()) + ")");

        Dictionary<string, SpriteSheetShadeDocument> shades = sheet.Manifest.Shades
            ?? throw new InvalidOperationException("Ridley's sheet installs its shades.");
        SpriteSheetShadeDocument flash = shades[EnemyShades.HitFlash.Key];
        AssertTrue(flash.Inks.SequenceEqual(painted), "The hurt flash lists every ink Ridley paints, those whose flash colour equals their own among them");
        for (int index = 0; index < flash.Inks.Length; index++)
        {
            // The flash clears his draws' palette bits, so each ink shows its own row.
            RgbColor own = source.Sources.SpriteColours[flash.Inks[index]];
            AssertEqual(own.ToWord(), flash.Colours[index], $"Ridley's ink {flash.Inks[index]} flashes in its own row's colour");
        }

        // Laid in the draw's order, the encounter's colours never show on a flashing frame.
        SpriteSheetShadeDocument start = shades[RidleyShades.NorfairStart.Key];
        var shown = new Dictionary<int, ushort>();
        foreach (SpriteSheetShadeDocument laid in new[] { start, flash })
        {
            for (int index = 0; index < laid.Inks.Length; index++)
                shown[laid.Inks[index]] = laid.Colours[index];
        }
        foreach (int ink in painted)
            AssertEqual(source.Sources.SpriteColours[ink].ToWord(), shown[ink], $"Flashing Ridley's ink {ink} shows its own row over his encounter's load");
        Console.WriteLine("Ridley hurt flash: the draw lays the flash after the encounter's shades, and the import's flash shade lists every painted ink in its own row's colours.");
        return 0;
    }

    /// <summary>Runs the focused Ridley hurt-flash check.</summary>
    [VerificationCommand("--ridley-hit-flash")]
    internal static int RidleyHitFlashCommand() => VerifyRidleyHitFlash();
}
