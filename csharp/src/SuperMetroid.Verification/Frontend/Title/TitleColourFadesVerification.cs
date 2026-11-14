using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>
/// Checks the title's logo and copyright fades (#1714) against the cartridge: which colour effect each composition's
/// first showing starts, the title update it starts on, the record the composition shows on every update from then on,
/// and the colours each installed record shade shows on the composition's pictures.
/// </summary>
internal static class TitleColourFadesVerification
{
    /// <summary>The operand of the final zoom's spawn of the fading logo's sprite object, at <c>$8B:9EAC</c>.</summary>
    private const ushort ZoomEndSpawnOperand = 0x9ead;
    /// <summary>The fading logo's sprite-object definition.</summary>
    private const ushort LogoDefinition = 0xa107;
    /// <summary>The fading logo's initialisation function, which spawns the logo's colour effect.</summary>
    private const ushort LogoInitialisation = 0x9eb3;
    /// <summary>The operand of the logo initialisation's colour-effect spawn, at <c>$8B:9EC5</c>.</summary>
    private const ushort LogoEffectOperand = 0x9ec6;
    /// <summary>The fading logo's instruction list: its hold, then the copyright's spawn.</summary>
    private const ushort LogoInstructions = 0xa0c5;
    /// <summary>The instruction that spawns the fading copyright's sprite object.</summary>
    private const ushort SpawnCopyrightInstruction = 0x9ecd;
    /// <summary>The operand of that instruction's spawn, after its one-byte register push.</summary>
    private const ushort SpawnCopyrightOperand = 0x9ecf;
    /// <summary>The fading copyright's sprite-object definition.</summary>
    private const ushort CopyrightDefinition = 0xa113;
    /// <summary>The fading copyright's initialisation function, which spawns the copyright's colour effect.</summary>
    private const ushort CopyrightInitialisation = 0x9eff;
    /// <summary>The operand of the copyright initialisation's colour-effect spawn, at <c>$8B:9F11</c>.</summary>
    private const ushort CopyrightEffectOperand = 0x9f12;
    /// <summary>The fading copyright's instruction list: its hold, then the demo countdown's start.</summary>
    private const ushort CopyrightInstructions = 0xa0e1;
    /// <summary>The instruction that starts the demo countdown and the idle title screen.</summary>
    private const ushort StartDemoCountdownInstruction = 0x9f19;
    /// <summary>The logo fade's colour-effect definition in bank $8D.</summary>
    private const ushort LogoEffectDefinition = 0xe194;
    /// <summary>The copyright fade's colour-effect definition in bank $8D.</summary>
    private const ushort CopyrightEffectDefinition = 0xe19c;

    /// <summary>
    /// The title's logo and copyright fades follow the cartridge: each composition's first showing starts the fade its
    /// sprite object's initialisation spawns, on the update the title shows it; every update from then on draws it with the
    /// record the fade's list shows by then, holding the last; and each record's shade shows the record's colours on the
    /// composition's pictures, the last being the logo's own colours and the copyright's colours as the skip restores them.
    /// </summary>
    internal static void VerifyTitleColourFades(CartridgeImportAddressSpace bus)
    {
        var cartridge = CartridgeImportSource.Require(bus);
        ushort Title(int pointer) => RomDataReader.ReadWordFixedBank(cartridge, 0x8b0000 | pointer);
        ushort Effects(int pointer) => RomDataReader.ReadWordFixedBank(cartridge, 0x8d0000 | pointer);

        // The final zoom spawns the fading logo, whose initialisation spawns the logo's fade; the logo's list holds, then
        // spawns the fading copyright, whose initialisation spawns the copyright's fade and whose list holds before the
        // demo countdown starts.
        AssertEqual(LogoDefinition, Title(ZoomEndSpawnOperand), "the final zoom's end spawns the fading logo");
        AssertEqual(LogoInitialisation, Title(LogoDefinition), "the fading logo initialises with its fade's spawn");
        AssertEqual(LogoInstructions, Title(LogoDefinition + 2 * sizeof(ushort)), "the fading logo runs its own list");
        AssertEqual(LogoEffectDefinition, Title(LogoEffectOperand), "the logo's initialisation spawns the logo fade");
        AssertEqual(TitleSequenceDefinitions.Timing.LogoHoldFrames, Title(LogoInstructions), "the logo holds before the copyright");
        AssertEqual(SpawnCopyrightInstruction, Title(LogoInstructions + 2 * sizeof(ushort)), "the logo's hold ends with the copyright's spawn");
        AssertEqual(CopyrightDefinition, Title(SpawnCopyrightOperand), "the logo spawns the fading copyright");
        AssertEqual(CopyrightInitialisation, Title(CopyrightDefinition), "the fading copyright initialises with its fade's spawn");
        AssertEqual(CopyrightInstructions, Title(CopyrightDefinition + 2 * sizeof(ushort)), "the fading copyright runs its own list");
        AssertEqual(CopyrightEffectDefinition, Title(CopyrightEffectOperand), "the copyright's initialisation spawns the copyright fade");
        AssertEqual(TitleSequenceDefinitions.Timing.CopyrightHoldFrames, Title(CopyrightInstructions), "the copyright holds before the demo countdown");
        AssertEqual(StartDemoCountdownInstruction, Title(CopyrightInstructions + 2 * sizeof(ushort)), "the copyright's hold ends with the demo countdown");
        // The definitions the sprite objects spawn run the lists the title's fades start at.
        AssertEqual(PaletteFxNativeListAddresses.Entry(ColourEffectSelection.Get(TitleSequenceDefinitions.Fades.Logo).InitialAnimation),
            Effects(LogoEffectDefinition + sizeof(ushort)), "the title's logo fade runs the logo fade definition's list");
        AssertEqual(PaletteFxNativeListAddresses.Entry(ColourEffectSelection.Get(TitleSequenceDefinitions.Fades.Copyright).InitialAnimation),
            Effects(CopyrightEffectDefinition + sizeof(ushort)), "the title's copyright fade runs the copyright fade definition's list");

        // Each record's duration and colours, read from the cartridge's lists.
        FadeRecords logo = new(ColourEffectColours.TitleLogo, ColourEffectAnimation.FadeInSuperMetroidTitleLogo,
            [.. Enumerable.Range(0, TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount)
                .Select(frame => (int)Effects(TitleLogoFadePaletteFxColorDefinitions.FramePointer(frame)))],
            [.. Enumerable.Range(0, TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount).Select(frame =>
                Enumerable.Range(0, TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame)
                    .Select(colour => RgbColor.FromWord(Effects(TitleLogoFadePaletteFxColorDefinitions.ColorPointer(frame, colour)))).ToArray())]);
        FadeRecords copyright = new(ColourEffectColours.NintendoCopyright, ColourEffectAnimation.FadeInNintendoCopyright,
            [.. Enumerable.Range(0, NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount)
                .Select(frame => (int)Effects(NintendoLogoFadePaletteFxColorDefinitions.FramePointer(frame)))],
            [.. Enumerable.Range(0, NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount).Select(frame =>
                Enumerable.Range(0, NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame)
                    .Select(colour => RgbColor.FromWord(Effects(NintendoLogoFadePaletteFxColorDefinitions.ColorPointer(frame, colour)))).ToArray())]);

        VerifyFadeSchedule(logo, copyright);
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.Title);
        PictureShading drawnLogo = PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Logo));
        PictureShading drawnCopyright = PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Copyright));
        VerifyRecordColours(sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo)), logo, drawnLogo, drawnLogo);
        VerifyRecordColours(sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright)), copyright, drawnCopyright,
            drawnCopyright.With(TitleShades.RestoredCopyright));
        Console.WriteLine($"  Title colour fades: the logo's and copyright's fades start as each first shows, show their " +
            $"{logo.Durations.Length} and {copyright.Durations.Length} cartridge records on the updates the lists show them, hold the last, " +
            "and each record's shade shows the cartridge record's colours.");
    }

    /// <summary>Runs the title colour fade check against the retail cartridge.</summary>
    [VerificationCommand("--title-colour-fades")]
    internal static int TitleColourFadesCommand()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyTitleColourFades), () => VerifyTitleColourFades(bus));
        return 0;
    }

    /// <summary>
    /// Runs the natural title from its first update until well into the idle title screen, checking on every update that
    /// the logo and the copyright draw with the record their fades show by then: the logo's fade from the update the logo
    /// first shows, the copyright's from the update the logo's hold ends and the copyright first shows, each record for its
    /// cartridge duration and the last held; no draw shows either fade before it starts.
    /// </summary>
    private static void VerifyFadeSchedule(FadeRecords logo, FadeRecords copyright)
    {
        TitleSequenceState title = InstalledFixtures.CreateTitle();
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.Title);
        SpriteSheetFrame logoFrame = sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo));
        SpriteSheetFrame copyrightFrame = sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright));
        // Every record shade either fade can show, which no other composition is drawn with.
        HashSet<PictureShade> fadeShades = [.. logo.Shades, .. copyright.Shades];
        int? logoStart = null, copyrightStart = null, titleScreenStart = null;
        int heldUpdates = logo.TotalDuration + copyright.TotalDuration;
        for (int update = 0; titleScreenStart is null || update < titleScreenStart + heldUpdates; update++)
        {
            AssertTrue(update < 3_000, "the natural title reaches its idle title screen");
            TitleSequencePhase before = title.Phase;
            title.Step(0);
            // The updates the logo, the copyright and the idle title screen first show.
            if (before != title.Phase && title.Phase == TitleSequencePhase.TitleLogoFade)
                logoStart = update;
            if (before != title.Phase && title.Phase == TitleSequencePhase.CopyrightFade)
            {
                copyrightStart = update;
                AssertEqual(TitleSequenceDefinitions.Timing.LogoHoldFrames, update - logoStart!.Value,
                    "the copyright first shows as the logo's cartridge hold ends");
            }
            if (before != title.Phase && title.Phase == TitleSequencePhase.TitleScreen)
            {
                titleScreenStart = update;
                AssertEqual(TitleSequenceDefinitions.Timing.CopyrightHoldFrames, update - copyrightStart!.Value,
                    "the idle title screen starts as the copyright's cartridge hold ends");
            }
            int logoDraws = 0, copyrightDraws = 0;
            foreach (SpriteDraw draw in title.CaptureRenderSnapshot().Paint.SpriteDraws)
            {
                var image = (ImageSpriteDraw)draw;
                PictureShading shading = image.Shading ?? throw new InvalidOperationException($"Title update {update} drew {image} in no shading.");
                if (image.Image == logoFrame)
                {
                    logoDraws++;
                    AssertTrue(logoStart is not null, $"the logo shows only from the final zoom's end (update {update})");
                    AssertEqual(PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Logo))
                            .With(ColourEffectShades.Of(logo.Colours, logo.FrameAfter(update - logoStart!.Value))), shading,
                        $"title update {update} draws the logo with its fade's record {update - logoStart.Value} updates into the fade");
                }
                else if (image.Image == copyrightFrame)
                {
                    copyrightDraws++;
                    AssertTrue(copyrightStart is not null, $"the copyright shows only from the logo's hold's end (update {update})");
                    PictureShading drawn = PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Copyright));
                    // The idle title screen's objects carry the restored copyright colours, which the settled fade's last
                    // record lies over.
                    if (titleScreenStart is not null)
                        drawn = drawn.With(TitleShades.RestoredCopyright);
                    AssertEqual(drawn.With(ColourEffectShades.Of(copyright.Colours, copyright.FrameAfter(update - copyrightStart!.Value))), shading,
                        $"title update {update} draws the copyright with its fade's record {update - copyrightStart.Value} updates into the fade");
                }
                else
                {
                    AssertTrue(!shading.Shades.Any(fadeShades.Contains) && !shading.OverEffects.Any(fadeShades.Contains),
                        $"title update {update} draws no other composition with a fade's record");
                }
            }
            AssertEqual(logoStart is null ? 0 : 1, logoDraws, $"title update {update} draws the logo once from the final zoom's end");
            AssertEqual(copyrightStart is null ? 0 : 1, copyrightDraws, $"title update {update} draws the copyright once from the logo's hold's end");
        }
        AssertEqual(TitleSequencePhase.TitleScreen, title.Phase, "the check ends on the idle title screen");
    }

    /// <summary>
    /// Checks every record's shade on <paramref name="frame"/>'s picture: each colour the composition shows in
    /// <paramref name="final"/>, which is <paramref name="records"/>' last record's, is a record colour, and under each
    /// record's shade laid over <paramref name="drawn"/> it shows that record's colour in the same place; a colour the fade
    /// does not write keeps its drawn colour.
    /// </summary>
    private static void VerifyRecordColours(SpriteSheetFrame frame, FadeRecords records, PictureShading drawn, PictureShading final)
    {
        SpriteImage picture = frame.Sheet;
        int width = picture.Size.X;
        var keys = new HashSet<ColourKey>();
        for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
        {
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
            {
                if (picture.Pixels[y * width + x].A != 0)
                    keys.Add(picture.KeyAt(y * width + x));
            }
        }
        RgbColor[] lastRecord = records.ColourSets[^1];
        RgbColor[] drawnColours = drawn.KeyColours(picture);
        RgbColor[] finalColours = final.KeyColours(picture);
        var written = new HashSet<int>();
        for (int record = 0; record < records.Durations.Length; record++)
        {
            RgbColor[] shown = drawn.With(ColourEffectShades.Of(records.Colours, new ColourEffectFrame(records.Animation, record))).KeyColours(picture);
            foreach (ColourKey key in keys)
            {
                int place = picture.PlaceOf(key) ?? throw new InvalidOperationException($"{picture} holds no place for its own key {key}.");
                int column = Array.IndexOf(lastRecord, finalColours[place]);
                if (column < 0)
                {
                    AssertEqual(drawnColours[place], shown[place], $"{records.Colours} record {record} keeps the drawn colour of {key}, which it does not write");
                    continue;
                }
                written.Add(column);
                AssertEqual(records.ColoursOf(record)[column], shown[place],
                    $"{records.Colours} record {record} shows its cartridge colour {column} where the composition shows that colour");
            }
        }
        AssertTrue(written.Count > 0, $"{records.Colours}'s fade recolours the composition");
    }

    /// <summary>One fade's cartridge records.</summary>
    /// <param name="Colours">The colour group the fade animates.</param>
    /// <param name="Animation">The segment whose frames are the fade's records.</param>
    /// <param name="Durations">Each record's duration in title updates.</param>
    /// <param name="ColourSets">Each record's colours, in the order it writes them.</param>
    private sealed record FadeRecords(ColourEffectColours Colours, ColourEffectAnimation Animation, int[] Durations, RgbColor[][] ColourSets)
    {
        /// <summary>The updates the fade takes to show every record.</summary>
        internal int TotalDuration => Durations.Sum();

        /// <summary>The shade of each record, in order.</summary>
        internal IEnumerable<PictureShade> Shades =>
            Enumerable.Range(0, Durations.Length).Select(record => ColourEffectShades.Of(Colours, new ColourEffectFrame(Animation, record)));

        /// <summary>The colours of record <paramref name="record"/>.</summary>
        internal RgbColor[] ColoursOf(int record) => ColourSets[record];

        /// <summary>The record the fade shows <paramref name="updates"/> updates after the update it starts on, its last held.</summary>
        internal ColourEffectFrame FrameAfter(int updates)
        {
            int elapsed = 0;
            for (int record = 0; record < Durations.Length; record++)
            {
                elapsed += Durations[record];
                if (updates < elapsed)
                    return new ColourEffectFrame(Animation, record);
            }
            return new ColourEffectFrame(Animation, Durations.Length - 1);
        }
    }
}
