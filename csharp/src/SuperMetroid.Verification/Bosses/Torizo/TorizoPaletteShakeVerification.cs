using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoLandingLifetimeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Torizo palette instruction steps without extending the authored landing earthquake.</summary>
internal static class TorizoPaletteShakeVerification
{
    /// <summary>Checks shake duration, the fades' shades, save-state continuation, and the import's palette colours.</summary>
    internal static int VerifyTorizoPaletteShake()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var actor = enemies.SlotTable.Slots[0];
        actor.Bind(EnemyDefinitionId.BombTorizo, actor.Definition);
        actor.Bind(actor.DefinitionId, RoomEnemyHeaders.Get(actor.DefinitionId));
        // The initializer's function words; the build-pinned debugger state below rejects the undefined zero defaults.
        var state = new TorizoEnemyState(actor, false) { Function = TorizoFunction.Falling, FrameBehavior = TorizoFrameBehavior.Idle };
        enemies.BombTorizoBehavior.Access.TorizoState = state;
        void Execute(TorizoCueKind kind) =>
            AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(
                    actor, state, new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounter = 0 },
                    new TorizoCue.Command(kind), TorizoAnimations.Start(TorizoBodyProgram.BombTorizoInitial0Resumed)) is null,
                "Torizo palette cue runs on");
        enemies.Earthquake.Start(NativeEarthquakeTypes.Shake(4), 32);
        // The native awakening loop invokes B271 every four frames. It must not extend a pending landing quake.
        Execute(TorizoCueKind.FadeToNormalColors);
        for (int frame = 0; frame < 96; frame++)
        {
            if (frame < 64 && frame % 4 == 0)
                Execute(TorizoCueKind.AdvanceColourFade);
            int timerBeforeShake = enemies.Earthquake.RemainingFrames;
            var shake = enemies.HandleRoomShaking(false);
            // An accepted shake frame is exactly one that consumes an earthquake-timer tick.
            AssertEqual(frame < 32, enemies.Earthquake.RemainingFrames != timerBeforeShake, $"Palette instruction cannot extend landing shake at frame {frame}");
            if (frame < 32)
            {
                AssertEqual((short)0, shake.Foreground.X, "Landing BG1 X");
                AssertEqual((short)(((32 - frame) & 2) == 0 ? 2 : -2), shake.Foreground.Y, "Landing BG1 Y");
            }
        }
        // Sixteen steps: fourteen move the colours, the fifteenth settles them, the sixteenth takes the counter's zero
        // step, which moves nothing, so the statue shows its settled colours.
        AssertEqual(PictureShading.Shaded(TorizoShades.Shared).With(TorizoShades.Body(new TorizoLook(TorizoLookKind.Normal))),
            state.Colours.Shading, "A fade toward the settled colours that has moved nothing shows them");

        PictureShading beforeBlack = state.Colours.Shading;
        Execute(TorizoCueKind.FadeToBlack);
        AssertEqual(beforeBlack, state.Colours.Shading, "Black target setup does not black out the current colours");
        // The running fade's counter stands at one: thirteen more steps move the colours and the fourteenth settles them.
        for (int call = 0; call < 14; call++)
            Execute(TorizoCueKind.AdvanceColourFade);
        AssertEqual(PictureShading.Shaded(TorizoShades.Shared).With(TorizoShades.Body(new TorizoLook(TorizoLookKind.Black))),
            state.Colours.Shading, "The completed fade settles the body and belly in black");
        AssertTrue(!enemies.Earthquake.IsShaking, "Completed fade does not create an earthquake");

        Execute(TorizoCueKind.FadeToNormalColors);
        Execute(TorizoCueKind.AdvanceColourFade);
        Execute(TorizoCueKind.AdvanceColourFade);
        // Save mid-fade: the fade's start, target and step must survive, not restart. They are the statue's colours, which
        // are saved on their own: the whole statue state reaches the enemy system's graph through its body slot.
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, state.Colours);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<TorizoColours>(saved);
        AssertEqual(state.Colours.Shading, restored.Shading, "Mid-fade save preserves the fade's step");

        // A hurt colouring written mid-fade restarts the fade from it at the counter value it stands at (#1512).
        state.Colours.Show(new TorizoLook(TorizoLookKind.White));
        Execute(TorizoCueKind.AdvanceColourFade);
        PictureShade fromWhite = TorizoShades.Fading(new TorizoLook(TorizoLookKind.White), new TorizoLook(TorizoLookKind.Normal), 2, 2);
        AssertEqual(PictureShading.Shaded(TorizoShades.Shared).With(fromWhite), state.Colours.Shading,
            "The fade restarts from the hurt flash's white at the counter value it stands at");

        // The import pre-colours each step: the black fade's last moving step and the normal fade's are their targets'.
        IReadOnlyList<SheetShade> shades = TorizoSheetShades.Of(InstalledFixtures.EnemyTiles.AuxiliaryColors
            ?? throw new InvalidOperationException("The installed enemy artwork holds the Torizos' colours."),
            InstalledFixtures.ImportMaps.RoomPaletteFx, PaletteFxObject.BombTorizoBelly);
        SheetShade toBlack = shades.Single(shade => shade.Shade == TorizoShades.Fading(new TorizoLook(TorizoLookKind.Normal), new TorizoLook(TorizoLookKind.Black), 1, 13));
        AssertTrue(toBlack.Inks.SequenceEqual(Enumerable.Range(16, 32)), "The fades recolour only sprite rows one and two");
        AssertTrue(toBlack.Colours.All(colour => colour == RgbColor.Black), "The black fade's last step is black");
        SheetShade toNormal = shades.Single(shade => shade.Shade == TorizoShades.Fading(new TorizoLook(TorizoLookKind.Black), new TorizoLook(TorizoLookKind.Normal), 1, 13));
        VerifyTorizoPaletteCatalogsAgainstRetailRom(toNormal, shades);
        VerifyRestartedFadeAgainstRetailRom(shades.Single(shade => shade.Shade == fromWhite));
        VerifyTorizoSheetRows();
        VerifyBellyGlow(shades);
        Suite(nameof(VerifyTorizoLandingLifetime), () => VerifyTorizoLandingLifetime());
        Console.WriteLine("Torizo palette opcode: repeated calls preserve 32-frame landing shake, name their fade's shades, fade only sprite rows 1/2, save mid-fade and create no quake; the import's Torizo colours match the cartridge.");
        return 0;
    }

    /// <summary>Compares the normal fade's last step and every imported Torizo colouring with bank $AA.</summary>
    private static void VerifyTorizoPaletteCatalogsAgainstRetailRom(SheetShade toNormal, IReadOnlyList<SheetShade> shades)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SheetShade golden = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.Golden)));
        SheetShade shared = shades.Single(shade => shade.Shade == TorizoShades.Shared);
        SheetShade bombInitial = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.BombStatue)));
        SheetShade goldenInitial = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.GoldenInitial)));
        for (int i = 0; i < 32; i++)
        {
            AssertEqual(rom.ReadCartridgeWord(0xaa8707 + i * 2), toNormal.Colours[i].ToWord(), "Normal fade reaches pinned cartridge target");
            AssertEqual(rom.ReadCartridgeWord(0xaa8787 + i * 2), golden.Colours[i].ToWord(), "Shared Golden fade target matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa8687 + i * 2), shared.Colours[i].ToWord(), $"Torizo shared orb/flame and statue-shading colour {i} matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa86c7 + i * 2), bombInitial.Colours[i].ToWord(), $"Bomb Torizo initial body colour {i} matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa8747 + i * 2), goldenInitial.Colours[i].ToWord(), $"Golden Torizo initial body colour {i} matches cartridge");
        }
    }

    /// <summary>
    /// Compares the import's colours of a fade restarted from white at counter value two with one step of the cartridge's
    /// gradual colour change from white toward the normal colours bank $AA holds.
    /// </summary>
    private static void VerifyRestartedFadeAgainstRetailRom(SheetShade fromWhite)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int i = 0; i < 32; i++)
        {
            ushort goal = rom.ReadCartridgeWord(0xaa8707 + i * 2);
            int expected = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int to = goal >> shift & 31;
                int moved = ((31 << 8) + Math.Sign(to - 31) * (Math.Abs(to - 31) * 256 / (TorizoPaletteDefinitions.FadeDenominator - 2 + 1))) >> 8;
                expected |= moved << shift;
            }
            AssertEqual((ushort)expected, fromWhite.Colours[i].ToWord(), $"Torizo fade restarted from white, colour {i}, takes one cartridge step");
        }
    }

    /// <summary>
    /// Checks that Bomb Torizo's sheet is coloured in the rows his parts name, as his initialization clears his draw's
    /// palette bits (bank $AA's spritemaps name rows one, two and five), so the hurt flash, which clears the same bits,
    /// recolours none of his colours.
    /// </summary>
    private static void VerifyTorizoSheetRows()
    {
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
            RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.BombTorizo));
        // His sheet composed in-process, which knows the palette index each pixel was composed as (#1633).
        SpriteImage image = EnemySheetFixtures.ComposedSheet(set, EnemyDefinitionId.BombTorizo);
        int[] rows = [.. image.ColourKeys.SelectMany(key => InstalledPictureColoursConstants.ComposedIndicesOf(image, key)).Select(index => index / 16).Distinct().Order()];
        AssertTrue(rows.Length > 0 && rows.All(row => row is 1 or 2 or 5), "Bomb Torizo's sheet is coloured in the rows his parts name");
        AssertEqual(0, image.Shade(SuperMetroid.Core.Enemies.Drawing.EnemyShades.HurtFlash).Substitutions.Length, "The hurt flash recolours none of Bomb Torizo's colours");
    }

    /// <summary>
    /// Checks the Bomb Torizo's belly glow (#1512, #1597): the statue's colours show the shade of the glow's current record
    /// over his own colouring, whatever colouring is written but his white flash, and a fade steps the glowing colours
    /// from the record shown;
    /// the import's record shades recolour the three colours the program's colour index names with the colours of bank
    /// $8D's program, and its fade from a record takes the cartridge's step from those colours.
    /// </summary>
    private static void VerifyBellyGlow(IReadOnlyList<SheetShade> shades)
    {
        SheetShade[] records = [.. shades.Where(shade =>
            shade.Shade == PaletteFxShades.Of(PaletteFxColours.TorizoBelly, new PaletteFxFrame(PaletteFxProgram.BombTorizoBelly1, shade.Shade.Step)))];
        AssertTrue(records.Length > 0, "The import pre-colours the belly glow's records");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // The program's six records are ten bytes apart, each a duration and three colours.
        var cartridge = new HashSet<(ushort, ushort, ushort)>(Enumerable.Range(0, 6).Select(record =>
            (rom.ReadCartridgeWord(0x8de2f3 + record * 10), rom.ReadCartridgeWord(0x8de2f5 + record * 10), rom.ReadCartridgeWord(0x8de2f7 + record * 10))));
        var imported = new HashSet<(ushort, ushort, ushort)>(records.Select(record =>
            (record.Colours[0].ToWord(), record.Colours[1].ToWord(), record.Colours[2].ToWord())));
        foreach (SheetShade record in records)
            AssertTrue(record.Inks.SequenceEqual([25, 26, 27]), "The glow recolours the tenth through twelfth colours of the first body row");
        AssertTrue(cartridge.SetEquals(imported), "The glow's records show bank $8D's colours");

        PaletteFxFrame shown = new(PaletteFxProgram.BombTorizoBelly1, records[0].Shade.Step);
        var colours = new TorizoColours();
        colours.Show(new TorizoLook(TorizoLookKind.Normal));
        colours.ShowBellyGlow(shown);
        AssertEqual(PaletteFxShades.Of(PaletteFxColours.TorizoBelly, shown), colours.Shading.Shades[^1], "The glow's record lies over the statue's colouring");
        colours.FadeToward(new TorizoLook(TorizoLookKind.Black));
        var counter = new SharedFadeStepCount();
        counter.Advance();
        colours.Step(counter);
        PictureShade fading = TorizoShades.BellyFading(shown, new TorizoLook(TorizoLookKind.Black), 1, 1);
        AssertEqual(fading, colours.Shading.Shades[^1], "The death fade steps the glowing colours from the record shown");
        SheetShade faded = shades.Single(shade => shade.Shade == fading);
        for (int colour = 0; colour < faded.Colours.Length; colour++)
        {
            ushort start = records[0].Colours[colour].ToWord();
            int expected = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int from = start >> shift & 31;
                expected |= ((from << 8) - from * 256 / TorizoPaletteDefinitions.FadeDenominator) >> 8 << shift;
            }
            AssertEqual((ushort)expected, faded.Colours[colour].ToWord(), $"The glow's colour {colour} takes the cartridge's first step toward black");
        }
        colours.Show(new TorizoLook(TorizoLookKind.White));
        AssertEqual(PictureShading.Shaded(TorizoShades.Shared).With(TorizoShades.Body(new TorizoLook(TorizoLookKind.White))), colours.Shading,
            "The white hurt flash covers the glow");
        colours.Show(new TorizoLook(TorizoLookKind.Normal));
        AssertEqual(PictureShading.Shaded(TorizoShades.Shared).With(TorizoShades.Body(new TorizoLook(TorizoLookKind.Normal)))
            .With(PaletteFxShades.Of(PaletteFxColours.TorizoBelly, shown)), colours.Shading,
            "Once the flash ends, the glow shows its current record at once over the colouring written");
    }

    /// <summary>Runs the focused Torizo palette and landing-shake checks.</summary>
    [VerificationCommand("--torizo-palette-shake")]
    internal static int TorizoPaletteShakeCommand()
    {
        return VerifyTorizoPaletteShake();
    }
}
