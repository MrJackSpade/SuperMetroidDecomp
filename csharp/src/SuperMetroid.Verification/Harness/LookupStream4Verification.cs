using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Areas.Escape.ZebesEscapeExplosionDefinitionsVerification;
using System.Runtime.CompilerServices;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonAnimationsVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingTextPresentationVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkBotwoonColorsVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.PaletteFxInstructionCodesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
namespace SuperMetroid.Verification.Harness;

/// <summary>Compares stream-four imported definitions and runtime consumers with pinned cartridge data.</summary>
internal static class LookupStream4Verification
{
    /// <summary>Checks every foreground explosion palette hold, CGRAM exposure, deletion, and star handoff tick.</summary>
    internal static void VerifyLookupStream4ForegroundCadence(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyZebesExplosionForegroundPaletteFxColorDefinitions), () => VerifyZebesExplosionForegroundPaletteFxColorDefinitions(rom));
        ushort nativeProgram = rom.ReadCartridgeWord(0x8de1ca);
        AssertEqual(nativeProgram, ZebesExplosionForegroundPaletteFxColorDefinitions.ProgramStart, "Native foreground entry");
        int destination = rom.ReadCartridgeWord((0x8d0000 | nativeProgram + 2)) / 2;
        var rows = new List<LookupStream4Row>();
        int cursor = nativeProgram + 4;
        while (rom.ReadCartridgeWord((0x8d0000 | cursor)) < 0x8000)
        {
            ushort duration = rom.ReadCartridgeWord((0x8d0000 | cursor));
            AssertEqual(duration, ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.Duration(rows.Count), "Every native foreground duration");
            cursor += 2;
            var colors = new List<ushort>();
            while (rom.ReadCartridgeWord((0x8d0000 | cursor)) < 0x8000)
            {
                colors.Add(rom.ReadCartridgeWord((0x8d0000 | cursor)));
                cursor += 2;
            }
            AssertEqual((ushort)PaletteFxInstruction.Wait, rom.ReadCartridgeWord((0x8d0000 | cursor)), "Native foreground wait");
            cursor += 2;
            rows.Add(new LookupStream4Row(duration, colors.ToArray()));
        }
        AssertEqual((ushort)PaletteFxInstruction.Delete, rom.ReadCartridgeWord((0x8d0000 | cursor)), "Native foreground delete");
        AssertEqual(cursor, (int)ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.DeleteInstructionPointer, "Derived foreground terminal address");
        var guarded = NewPaletteFxMechanicsForbiddenBus(rom, out StrongBox<int> guardedPresentationReads);
        var fxSession = new ColourEffectSessionState();
        var fx = new ColourEffectSystem(fxSession);
        var cgram = new SnesCgram();
        fx.Start(ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer, 0);
        int ticks = 0;
        foreach (var row in rows)
        for (int hold = 0; hold < row.Duration; hold++)
        {
            fx.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(fx, cgram, new ReferencePaletteFxColorSource(guarded));
            for (int color = 0; color < SnesCgram.ColorCount; color++)
            {
                ushort expected = color >= destination && color < destination + row.Colors.Length
                    ? row.Colors[color - destination] : (ushort)0;
                AssertEqual(expected, cgram.Colors[color], "Actual foreground color exposure and untouched CGRAM");
            }
            AssertTrue(fx.IsRunning(ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer), "Foreground remains active during native exposure");
            ticks++;
        }
        AssertEqual(rom.ReadCartridgeWord(0x8beb71), ticks, "Native stars handoff matches complete foreground exposure");
        AssertEqual(rom.ReadCartridgeWord(0x8beb71), CinematicSpriteAnimations.Segments.Beats(CinematicSpriteSegment.ZebesExplosionRightStars)[0] switch
        {
            AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame frame => frame.Duration,
            AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Pause pause => pause.Duration,
            var other => throw new InvalidOperationException($"The stars' first beat {other} lasts no updates."),
        }, "Actual actor program retains the shared native handoff");
        AssertEqual(ticks, ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.CycleFrames, "Derived complete exposure");
        fx.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
        PaletteFxRoomLook.LayShownShades(fx, cgram, new ReferencePaletteFxColorSource(guarded));
        AssertTrue(!fx.IsRunning(ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer), "Foreground deletes immediately after final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "Foreground mechanics avoid live ROM reads");
        foreach (int invalid in new[] { -1, rows.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.Duration(invalid), "Foreground duration domain");
        Console.WriteLine("Foreground cadence:35 native controls,16 independently decoded durations,144 actual CGRAM exposures plus exact deletion/native star handoff/read guard/bounds pass; color artwork remains required.");
    }

    /// <summary>Checks title tube and display shades against native words.</summary>
    private static void VerifyLookupStream4TitleAmbientColors(ISnesAddressSpace rom)
    {
        byte[] json = TitlePaletteExtractor.Extract(rom);
        TitlePalettePresentation stock = TitlePaletteFile.Read(new MemoryStream(json));
        var native = new Dictionary<ushort, ushort>();
        foreach (var program in TitleScreenAmbientPaletteOwners.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            ushort pointer = (ushort)(program.FramePointer(frame) + 2 + index * 2);
            native.Add(pointer, CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8d0000 | pointer)));
            AssertTrue(stock.TryReadColor(pointer, out RgbColor value), "installed title ambient color resolves");
            AssertEqual(native[pointer], value, "native title ambient color");
        }
        var stored = stock.animatedColors;
        AssertEqual(native.Count, stored.Count, "every ambient colour is held as installed");
        AssertEqual(36, TitlePalettePresentationTooling.ColorPointers.Count, "title color identity count preserved");
        AssertEqual(36, TitlePalettePresentationTooling.ColorPointers.Distinct().Count(), "title color identities enumerate once");
        for (int pointer = 0xc7fa; pointer <= 0xc87a; pointer++)
            AssertEqual(native.ContainsKey((ushort)pointer), stock.TryReadColor((ushort)pointer, out _),
                "title ambient color domain excludes controls and unaligned bytes");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = TitleScreenAmbientPaletteOwners.All[invalid],
                "title ambient owner list domain");
    }

    /// <summary>Runs both title ambient loops for two cycles and checks per-tick CGRAM colors without live cartridge reads.</summary>
    internal static void VerifyLookupStream4TitleAmbientMechanics(CartridgeImportAddressSpace rom)
    {
        int ticks = 0;
        foreach (var definition in TitleScreenAmbientPaletteOwners.All)
        {
            ushort nativeProgram = rom.ReadCartridgeWord((0x8d0000 | definition.DefinitionPointer + 2));
            AssertEqual(nativeProgram, definition.ProgramStart, "Native ambient definition selects calculated program");
            int destination = rom.ReadCartridgeWord((0x8d0000 | nativeProgram + 2)) / 2;
            var nativeRows = new List<LookupStream4Row>();
            int cursor = nativeProgram + 4;
            while (rom.ReadCartridgeWord((0x8d0000 | cursor)) < 0x8000)
            {
                ushort duration = rom.ReadCartridgeWord((0x8d0000 | cursor));
                cursor += 2;
                var colors = new List<ushort>();
                while (rom.ReadCartridgeWord((0x8d0000 | cursor)) < 0x8000)
                {
                    colors.Add(rom.ReadCartridgeWord((0x8d0000 | cursor)));
                    cursor += 2;
                }
                AssertEqual((ushort)PaletteFxInstruction.Wait, rom.ReadCartridgeWord((0x8d0000 | cursor)), "Native ambient record wait");
                cursor += 2;
                nativeRows.Add(new LookupStream4Row(duration, colors.ToArray()));
            }
            AssertEqual((ushort)PaletteFxInstruction.Goto, rom.ReadCartridgeWord((0x8d0000 | cursor)), "Native ambient terminal loop");
            AssertEqual((ushort)(nativeProgram + 4), rom.ReadCartridgeWord((0x8d0000 | cursor + 2)), "Native ambient loop target");
            AssertEqual(nativeRows.Count, definition.FrameCount, "Native ambient selected phase count");
            AssertEqual(cursor, (int)definition.LoopInstructionPointer, "Calculated terminal layout");
            var guarded = NewPaletteFxMechanicsForbiddenBus(rom, out StrongBox<int> guardedPresentationReads);
            var fxSession = new ColourEffectSessionState();
            var fx = new ColourEffectSystem(fxSession);
            var cgram = new SnesCgram();
            fx.Start(definition.DefinitionPointer, 0);
            for (int cycle = 0; cycle < 2; cycle++)
            foreach (var row in nativeRows)
            for (int hold = 0; hold < row.Duration; hold++)
            {
                fx.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(fx, cgram, new ReferencePaletteFxColorSource(guarded));
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                {
                    ushort expected = color >= destination && color < destination + row.Colors.Length
                        ? row.Colors[color - destination] : (ushort)0;
                    AssertEqual(expected, cgram.Colors[color], "Actual ambient shade/dwell/destination and untouched CGRAM");
                }
                ticks++;
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts, "Ambient mechanics have no live cartridge dependency");
            AssertTrue(fx.IsRunning(definition.DefinitionPointer), "Ambient loop stays active");
        }
        AssertEqual(164, ticks, "Both exact ambient loops confirmed twice");
        Console.WriteLine("Title ambient mechanics:28 native controls, independent record decoding,164 actual per-tick CGRAM states, untouched colors and mechanics read guard pass; independent color artwork remains pending.");
    }
    /// <summary>Combines title ambient palette artwork and mechanics checks through the installed color source.</summary>
    internal static void VerifyLookupStream4TitleAmbientComplete(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyLookupStream4TitleAmbientColors), () => VerifyLookupStream4TitleAmbientColors(rom));
        byte[] json = TitlePaletteExtractor.Extract(rom);
        TitlePaletteDocument Document() => RequiredJson.Deserialize<TitlePaletteDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        static ushort Pack(PaletteRgb5 value) => (ushort)(value.Red | value.Green << 5 | value.Blue << 10);
        var document = Document();
        var selected = TitlePaletteFile.Read(new MemoryStream(json));
        var guarded = NewPaletteFxMechanicsForbiddenBus(rom, out StrongBox<int> guardedPresentationReads);
        var fxSession = new ColourEffectSessionState();
        var fx = new ColourEffectSystem(fxSession);
        foreach (var program in TitleScreenAmbientPaletteOwners.All)
            fx.Start(program.DefinitionPointer, 0);
        for (int tick = 0; tick <= 80; tick++)
        {
            // A frontend screen's step shows each loop's record without writing colours (#1512); the import pre-colours
            // the record's shade from the selected colours, which must hold the frame's authored colours.
            fx.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            foreach (var program in TitleScreenAmbientPaletteOwners.All)
            {
                var rows = program.Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight
                    ? document.BabyMetroidTubeLight : document.FlickeringDisplays;
                int frame = tick / program.FrameDuration % program.FrameCount;
                ColourEffectFrame record = fx.FrameOf(program.Colours)
                    ?? throw new InvalidOperationException($"{program.Colours} shows no record on tick {tick}.");
                RgbColor[] shown = PaletteFxRecordShades.Shade(program.Colours, record, new InkRange(0, int.MaxValue), selected).Colours;
                for (int index = 0; index < program.ColorsPerFrame; index++)
                    AssertEqual(Pack(rows[frame][index]), shown[index].ToWord(),
                        "Actual simultaneous title loops preserve stock output and wrap");
            }
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "Installed title mechanics avoid live reads");
        AssertEqual(0, guardedPresentationReads.Value, "Installed title colors avoid live reads");
        Console.WriteLine("Title ambient complete:36 native words/zero stock overrides,81 actual simultaneous-loop CGRAM states and no live reads pass.");
    }

    /// <summary>Confirms the ending font preserves cartridge space glyph data and layout boundaries.</summary>
    internal static void VerifyLookupStream4EndingFontSpaces(ISnesAddressSpace rom)
    {
        byte[] decoded = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(rom),
            0x97E7DE, EndingArtworkExtentDefinitions.FloorBytes);
        byte[] native = decoded.AsSpan(0, 160 * 32).ToArray();
        byte[] png = EndingFontAtlasExtractor.Extract(rom);
        var atlas = EndingFontCharacters.Read(new MemoryStream(png));
        AssertTrue(native.SequenceEqual(atlas.Characters.Span), "all5120 original ending font bytes match");
        AssertTrue(native.SequenceEqual(EndingFontCharacters.FromPlanarBytes(native).Characters.Span), "legacy native import keeps the same characters");
        AssertEqual((ushort)0x4F, EndingTextGlyphs.CompileGlyph(' ', EndingTextStyle.ResultSmall).PackedWord, "native small text space selects blank tile4F");
        AssertEqual((ushort)0x7F, EndingTextGlyphs.CompileGlyph(' ', EndingTextStyle.CopyrightLarge).PackedWord, "native large text space selects blank tile7F");
        AssertThrows<InvalidDataException>(() => EndingFontCharacters.FromPlanarBytes(new byte[5119]), "native font byte domain preserved");
        Console.WriteLine("Ending font:5120 native bytes, the space glyphs and the byte domain pass.");
    }

    /// <summary>Checks Maridia palette colors and source ownership against the native palette rows.</summary>
    internal static void VerifyLookupStream4MaridiaColors(CartridgeImportAddressSpace rom)
    {
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        var stock = RoomPaletteFxPresentationFile.Read(new MemoryStream(json));
        var sites = new List<LookupStream4Site>();
        for (int owner = 0; owner < 3; owner++)
        {
            int frames = owner == 2 ? 8 : 4, colors = owner == 1 ? 4 : 8;
            int first = owner == 0 ? 0xF4EF : owner == 1 ? 0xF547 : 0xF57F;
            for (int frame = 0; frame < frames; frame++)
            for (int color = 0; color < colors; color++)
            {
                ushort pointer = (ushort)(first + frame * (colors + 2) * 2 + color * 2);
                sites.Add(new LookupStream4Site(owner, frame, color, pointer));
                AssertTrue(stock.TryReadColor(pointer, out RgbColor installed), "installed environmental color resolves");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer), installed, "native installed environmental color");
            }
        }
        AssertEqual(112, sites.Count, "native environmental color domain");
        foreach (var definition in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            var nativeBus = NewPaletteFxMechanicsForbiddenBus(rom, out _);
            var nativeSession = new ColourEffectSessionState();
            var native = new ColourEffectSystem(nativeSession); var installedSession = new ColourEffectSessionState();
                                                                 var installed = new ColourEffectSystem(installedSession);
            var nativeCgram = new SnesCgram(); var installedCgram = new SnesCgram();
            native.Start(definition.DefinitionPointer, 0);
            installed.Start(definition.DefinitionPointer, 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
            {
                native.Step(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(nativeBus));
                installed.Step(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(installed, installedCgram, stock);
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)), "actual installed environmental cycle matches original colors each step");
            }
            AssertTrue(installed.IsRunning(definition.DefinitionPointer), "environmental palette retains native loop behavior");
        }
        Console.WriteLine("Maridia colors:112 installed native words and three actual installed cycles pass.");
    }

    /// <summary>Checks the planet text glyphs and placement data against cartridge definitions.</summary>
    internal static void VerifyLookupStream4PlanetText(CartridgeImportAddressSpace rom)
    {
        var all = PlanetZebesTextPaletteFxColorDefinitions.All;
        AssertEqual(2, all.Count, "two text fade owners");
        for (int owner = 0; owner < 2; owner++)
        {
            AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | (0xE1B2 + owner * 4)), all[owner].ProgramStart, "native text definition target");
            AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | (all[owner].ProgramStart + 2)), all[owner].AnimatedColours.CartridgeByteIndex, "native text colours start where the text colour group is placed");
        }
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        var stock = RoomPaletteFxPresentationFile.Read(new MemoryStream(json));
        for (int owner = 0; owner < 2; owner++)
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 3; color++)
        {
            ushort pointer = (ushort)((owner == 0 ? 0xC914 : 0xC96A) + frame * 10 + color * 2);
            AssertEqual(pointer, all[owner].ColorPointer(frame, color), "independent native timed-color layout");
            AssertTrue(stock.TryReadColor(pointer, out RgbColor installed), "installed text color available");
            AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer), installed, "installed native text color");
        }
        foreach (var definition in all)
        {
            var nativeBus = NewPaletteFxMechanicsForbiddenBus(rom, out StrongBox<int> nativeBusPresentationReads);
            var installedBus = NewPaletteFxMechanicsForbiddenBus(rom, out StrongBox<int> installedBusPresentationReads);
            var nativeSession = new ColourEffectSessionState();
            var native = new ColourEffectSystem(nativeSession); var installedSession = new ColourEffectSessionState();
                                                                 var installed = new ColourEffectSystem(installedSession);
            var nativeCgram = new SnesCgram(); var installedCgram = new SnesCgram();
            native.Start(definition.DefinitionPointer, 0);
            installed.Start(definition.DefinitionPointer, 0);
            for (int step = 0; step <= 24; step++)
            {
                native.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(nativeBus));
                installed.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(installed, installedCgram, stock);
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)), "actual installed text fade matches native colors each step");
            }
            AssertEqual(0, installedBusPresentationReads.Value, "installed text fade makes zero live color reads");
            AssertTrue(!installed.IsRunning(definition.DefinitionPointer), "installed text fade deletes at original time");
        }
        foreach (int invalid in new[] { -1, 2, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = all[invalid], "text owner bounds");
        Console.WriteLine("PLANET ZEBES:48 installed native colors and both actual installed one-shot fades pass.");
    }

    /// <summary>Checks Ceres fade phases, native color rows, and installed CGRAM exposure.</summary>
    internal static void VerifyLookupStream4CeresFades(ISnesAddressSpace rom)
    {
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        foreach (bool eyes in new[] { true, false })
        {
            int count = eyes ? 3 : 11, start = eyes ? 0xA6E2AA : 0xA6E30A;
            ushort Native(int row, int color)
            {
                int address = start + (row * count + color) * 2;
                return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            }
            IReadOnlyList<SheetShade> shades = RidleySheetShades.CeresSprites(stock);
            for (int row = 0; row < 16; row++)
            {
                ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0), shades,
                    eyes ? RidleyShades.EyeFade(row) : RidleyShades.BodyFade(row), BossShadeColours.Sprites);
                for (int color = 0; color < count; color++)
                {
                    ushort expected = Native(row, color);
                    AssertEqual(expected, eyes ? stock.ResolveEyeFade(row, color) : stock.ResolveBodyFade(row, color), "stock fade sample");
                    AssertEqual(expected, cgram[(eyes ? 252 : 145) + color], "actual fade CGRAM destination");
                    if (!eyes) AssertEqual(expected, cgram[241 + color], "body fade duplicates its BG colors into OBJ palette");
                }
                AssertEqual(0, cgram[eyes ? 251 : 144], "preceding fade CGRAM unchanged");
                AssertEqual(0, cgram[eyes ? 255 : 156], "following fade CGRAM unchanged");
            }
            foreach (int row in new[] { -1, 16, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => eyes ? stock.ResolveEyeFade(row, 0) : stock.ResolveBodyFade(row, 0), "fade row bounds");
            foreach (int color in new[] { -1, count, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => eyes ? stock.ResolveEyeFade(0, color) : stock.ResolveBodyFade(0, color), "fade color bounds");
        }
        Console.WriteLine("Ceres fades:224 installed native words and32 actual row applications pass.");
    }

    /// <summary>Checks Ceres alarm palette timing and color output at its native CGRAM destination.</summary>
    internal static void VerifyLookupStream4CeresAlarm(ISnesAddressSpace rom)
    {
        ushort Native(int row, int color)
        {
            int address = 0xA6C1DF + (row * 3 + color) * 2;
            return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
        }
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        var floor = CeresRidleyMode7ColorFile.Read(new MemoryStream(CeresRidleyMode7ColorExtractor.Extract(rom)));
        for (int row = 0; row < 16; row++)
        for (int color = 0; color < 3; color++)
            AssertEqual(Native(row, color), stock.ResolveAlarm(row, color), "installed native alarm color");
        IReadOnlyList<SheetShade> room = [.. RidleySheetShades.CeresRoom(stock, floor), .. RidleySheetShades.CeresWarning(stock)];
        Action<RidleyEnemyState, int> update = CeresRidleyGetaway.UpdateCeresSelfDestructAlarm;
        var state = new RidleyEnemyState { SelfDestructAlarmStep = 15 };
        for (ushort nmi = 0; nmi < 64; nmi++)
        {
            update(state, nmi);
            ushort[] laid = RidleyLaidColours.Of(state, [], room, 0x1234);
            int row = nmi / 4;
            AssertEqual((ushort)row, state.SelfDestructAlarmStep, "actual alarm advances only every fourth NMI and wraps");
            for (int color = 0; color < 256; color++)
                AssertEqual(color is >= 97 and <= 99 ? Native(row, color - 97) : (ushort)0x1234, laid[color], "actual gated producer preserves exact row and other CGRAM colors");
        }
        for (int row = 0; row < 16; row++)
        {
            ushort[] cgram = BossShadeColours.Filled(0);
            cgram[96] = 0x1234; cgram[100] = 0x2345;
            BossShadeColours.Lay(cgram, room, RidleyShades.Alarm(row), BossShadeColours.Room);
            for (int color = 0; color < 3; color++)
                AssertEqual(Native(row, color), cgram[97 + color], "actual alarm CGRAM write");
            AssertEqual(0x1234, cgram[96], "alarm preceding CGRAM unchanged");
            AssertEqual(0x2345, cgram[100], "alarm following CGRAM unchanged");
        }
        foreach (int row in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveAlarm(row, 0), "alarm row bounds");
        foreach (int color in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveAlarm(0, color), "alarm color bounds");
        Console.WriteLine("Ceres alarm:48 native colors,16 actual row writes,64 gated producer calls and bounds pass.");
    }

    /// <summary>Checks Maridia palette definition pointers and their imported color rows.</summary>
    internal static void VerifyLookupStream4MaridiaPaletteDefinitions(CartridgeImportAddressSpace rom)
    {
        var all = MaridiaEnvironmentalPaletteFxColorDefinitions.All;
        AssertEqual(3, all.Count, "three native environmental owners");
        int colors = 0;
        for (int index = 0; index < 3; index++)
        {
            var definition = all[index];
            int pointer = rom.ReadCartridgeWordFixedBank(0x8D0000 | (0xF795 + index * 4 + 2));
            AssertEqual((ushort)pointer, definition.ProgramStart, "native definition selects calculated setup");
            AssertEqual((MaridiaEnvironmentalPaletteOwner)index, definition.Owner, "owner order");
            AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | (pointer + 2)), definition.AnimatedColours.CartridgeByteIndex, "native colours start where the definition's colour group is placed");
            pointer += 4;
            int frame = 0;
            while (rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer) < 0x8000)
            {
                AssertEqual((ushort)pointer, definition.FramePointer(frame), "native timed record stride");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer), definition.Duration, "required native cadence");
                pointer += 2;
                int color = 0;
                while (rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer) != (ushort)PaletteFxInstruction.Wait)
                {
                    AssertEqual((ushort)pointer, definition.ColorPointer(frame, color), "native color identity");
                    color++; colors++; pointer += 2;
                }
                AssertEqual(definition.ColorsPerFrame, color, "native color group extent");
                pointer += 2; frame++;
            }
            AssertEqual(definition.FrameCount, frame, "native rotation cycle extent");
            AssertEqual((ushort)pointer, definition.LoopInstructionPointer, "native loop opcode location");
            AssertEqual((ushort)PaletteFxInstruction.Goto, rom.ReadCartridgeWordFixedBank(0x8D0000 | pointer), "native goto identity");
            AssertEqual(definition.FirstFramePointer, rom.ReadCartridgeWordFixedBank(0x8D0000 | (pointer + 2)), "native loop target");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(-1), "negative frame rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(frame), "exhausted frame rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.ColorPointer(0, color: -1), "negative color rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.ColorPointer(0, definition.ColorsPerFrame), "exhausted color rejection");
        }
        AssertEqual(112, colors, "native presentation words remain independently owned");
        foreach (int index in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = all[index], "owner selector bounds");
        Console.WriteLine("Maridia palette definitions: three native programs,44 mechanics words,112 color identities and actual complete loops pass; timing/destination/group inputs remain required.");
    }

    /// <summary>Checks ending shake displacement and timing data against its native program.</summary>
    internal static void VerifyLookupStream4EndingShake(ISnesAddressSpace rom)
    {
        int Native(int address)
        {
            ushort whole = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            ushort fraction = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 2);
            return unchecked((int)(SixteenSixteenWords.FromWhole(whole) | fraction));
        }
        for (int index = 0; index < 16; index++)
            AssertEqual(Native(0x8BDD02 + index * 4), EndingShipShake.PlanetFastDelta(index).RawValue, "native fast shake signed fixed-point value");
        for (int index = 0; index < 8; index++)
            AssertEqual(Native(0x8BDDAD + index * 4), EndingShipShake.PlanetSlowDelta(index).RawValue, "native slow shake signed fixed-point value");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 32768), EndingShipShake.PlanetFastDelta(9), "native DD26 fast sample9 is positive half-pixel");
        AssertEqual(-SignedSixteenSixteen.FromParts(0, 32768), EndingShipShake.PlanetFastDelta(11), "native DD2E fast sample11 is negative half-pixel");
        AssertEqual(SignedSixteenSixteen.FromParts(2, 0), Enumerable.Range(0, 16).Select(EndingShipShake.PlanetFastDelta).Aggregate(SignedSixteenSixteen.Zero, (sum, delta) => sum + delta), "fast cycle shares exact two-pixel translation");
        AssertEqual(SignedSixteenSixteen.FromParts(2, 0), Enumerable.Range(0, 8).Select(EndingShipShake.PlanetSlowDelta).Aggregate(SignedSixteenSixteen.Zero, (sum, delta) => sum + delta), "slow cycle shares exact two-pixel translation");
        foreach (bool fast in new[] { true, false })
        {
            int count = fast ? 16 : 8, address = fast ? 0x8BDD02 : 0x8BDDAD;
            var state = new EndingCreditsState(SuperMetroid.Core.Audio.AudioQueues.Standalone(), new EndingCreditsSetup { GameTimeHours = 0, GameTimeMinutes = 0 });
            state.Stage.FloorPosition = state.Stage.FloorPosition.WithWholeX(100).WithFractionX(0xC000);
            state.Stage.FloorZoom = FloorZoom.FromFloorPixelsPerScreenPixel(12); state.Stage.PhaseTimer = 100;
            state.Stage.Phase = fast ? EndingCreditsPhase.PlanetEscapeFast : EndingCreditsPhase.PlanetEscapeSlow;
            Action step = fast ? state.Flyaway.StepPlanetEscapeFast : state.Flyaway.StepPlanetEscapeSlow;
            uint expected = 100u << 16 | 0xC000;
            int expectedAngle = 0, expectedZoom = 0xC00;
            for (int frame = 0; frame <= count; frame++)
            {
                expected = unchecked(expected + (uint)Native(address + (frame % count) * 4));
                if (fast || expectedAngle != 0xe0) expectedAngle = unchecked((byte)(expectedAngle - (fast ? 4 : 1)));
                expectedZoom -= fast ? 8 : 2;
                step();
                AssertEqual(RotationAngle.FromSteps((byte)expectedAngle), state.Stage.FloorAngle, "rotation context advances independently of selected signs");
                AssertEqual(FloorZoom.FromTwoHundredFiftySixths(expectedZoom), state.Stage.FloorZoom, "zoom context advances independently of selected signs");
                AssertEqual(FixedCoordinate.WholeOf(unchecked((int)expected)), state.Stage.FloorPosition.X.Whole, "production shake whole position follows native indexed values");
                AssertEqual((ushort)expected, state.Stage.FloorPosition.X.Fraction, "production shake fraction preserves carry/borrow");
                AssertEqual((ushort)((frame + 1) % count), state.Flyaway.PlanetMotionIndex, "production shake advances after selection and wraps native mask");
            }
            state.Stage.FloorZoom = fast ? EndingMotionDefinitions.PlanetFastEndZoom : EndingMotionDefinitions.PlanetSlowEndZoom;
            state.Flyaway.PlanetMotionIndex = (ushort)(count - 1);
            state.Stage.FloorPosition = state.Stage.FloorPosition.WithWholeX(0).WithFractionX(0x4000);
            step();
            uint finalPosition = unchecked(0x4000u + (uint)Native(address + (count - 1) * 4));
            AssertEqual(FixedCoordinate.WholeOf(unchecked((int)finalPosition)), state.Stage.FloorPosition.X.Whole, "terminal negative sample wraps whole position before handoff");
            AssertEqual((ushort)finalPosition, state.Stage.FloorPosition.X.Fraction, "terminal fraction is preserved before handoff");
            AssertEqual(fast ? EndingCreditsPhase.PlanetEscapeSlow : EndingCreditsPhase.PlanetEscapeAccelerating, state.Stage.Phase, "native zoom boundary chooses next phase after selected motion");
            AssertEqual(0, state.Flyaway.PlanetMotionIndex, "handoff preserves wrapped/reset index");
            if (!fast)
            {
                AssertEqual(0, state.Flyaway.PlanetVelocity.WholePixels, "slow handoff initializes independent acceleration whole");
                AssertEqual((ushort)0x8000, state.Flyaway.PlanetVelocity.Fraction, "slow handoff initializes independent acceleration fraction");
            }
        }
        foreach (int index in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingShipShake.PlanetFastDelta(index), "fast shake bounds");
        foreach (int index in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingShipShake.PlanetSlowDelta(index), "slow shake bounds");
        Console.WriteLine("Ending shake:24 native signed values, common two-pixel cycle displacement, both production cycles/index wraps, independent rotation/zoom, both phase handoffs and fixed-point carries pass.");
    }

    /// <summary>Checks Spore Spawn health-dependent palette stages against native colors.</summary>
    internal static void VerifyLookupStream4SporeHealthRamp(ISnesAddressSpace rom)
    {
        ushort[][] native = Enumerable.Range(0, 4)
            .Select(frame => Enumerable.Range(0, 16).Select(color => CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e379 + frame * 32 + color * 2))).ToArray())
            .ToArray();
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorFile.Read(new MemoryStream(source));
        Check(stock, native);
        Console.WriteLine("Spore health: all64 installed native colors pass.");

        static void Check(SporeSpawnColorCatalog catalog, ushort[][] expected)
        {
            for (int frame = 0; frame < 4; frame++)
            for (int color = 0; color < 16; color++)
                AssertEqual(expected[frame][color], catalog.ResolveHealth(frame, color), $"Health row {frame} color {color}");
        }
    }

    /// <summary>Checks the healthy Spore Spawn palette aliases the intended native color range.</summary>
    internal static void VerifyLookupStream4SporeHealthyAlias(ISnesAddressSpace rom)
    {
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorFile.Read(new MemoryStream(source));
        Verify(stock);
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveSpore(invalid), "Spore alias color bounds remain exact");
        Console.WriteLine("Spore colours: all16 installed native words and the canonical identity pass.");

        void Verify(SporeSpawnColorCatalog catalog)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + color * 2))).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[] spores = Read(0xa5e359);
            ushort[][] health = Rows(0xa5e379, 4);
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(spores[color], catalog.ResolveSpore(color), "Spore colours");
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(health[frame][color], catalog.ResolveHealth(frame, color), "Body health rows");
            }
            string identity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", spores);
                content.AppendWordFrames("health", health);
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(identity, catalog.ContentIdentity, "Healthy-palette alias preserves canonical content hash");
        }
    }

    /// <summary>Checks Spore Spawn level-fade phases and their source palette rows.</summary>
    internal static void VerifyLookupStream4SporeLevelFade(ISnesAddressSpace rom)
    {
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorFile.Read(new MemoryStream(source));
        Verify(stock);
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathLevel(invalid, 0), "Level frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathLevel(0, invalid), "Level color bounds remain exact");
        Console.WriteLine("Spore level fade: all112 installed native words and the identity pass.");

        void Verify(SporeSpawnColorCatalog catalog)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + color * 2))).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] level = Rows(0xa5e4f9, 7);
            for (int frame = 0; frame < 7; frame++)
                for (int color = 0; color < 16; color++)
                {
                    AssertEqual(level[frame][color], catalog.ResolveDeathLevel(frame, color), "Calculated and unresolved level words");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e5d9 + frame * 32 + color * 2)), catalog.ResolveDeathBackground(frame, color), "Background layer words");
                }
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", Rows(0xa5e379, 4));
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", level);
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Level calculation preserves canonical selected content identity");
        }
    }

    /// <summary>Checks Spore Spawn background-fade stages against native palette words.</summary>
    internal static void VerifyLookupStream4SporeBackgroundFade(ISnesAddressSpace rom)
    {
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorFile.Read(new MemoryStream(source));
        Verify(stock);
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathBackground(invalid, 0), "Background frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathBackground(0, invalid), "Background color bounds remain exact");
        Console.WriteLine("Spore background fade: all112 installed native words and the identity pass.");

        void Verify(SporeSpawnColorCatalog catalog)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + color * 2))).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] background = Rows(0xa5e5d9, 7);
            for (int frame = 0; frame < 7; frame++)
                for (int color = 0; color < 16; color++)
                {
                    AssertEqual(background[frame][color], catalog.ResolveDeathBackground(frame, color), "Final and interior background colors");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e4f9 + frame * 32 + color * 2)), catalog.ResolveDeathLevel(frame, color), "Level palette words");
                }
            for (int frame = 0; frame < 8; frame++)
                for (int color = 0; color < 16; color++)
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e3f9 + frame * 32 + color * 2)), catalog.ResolveDeathSprite(frame, color), "Sprite fade words");
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", Rows(0xa5e379, 4));
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", background);
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Background fade preserves exact selected canonical identity");
        }
    }

    /// <summary>Checks Spore Spawn sprite-fade stages against native palette words.</summary>
    internal static void VerifyLookupStream4SporeSpriteFade(ISnesAddressSpace rom)
    {
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorFile.Read(new MemoryStream(source));
        Verify(stock);
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathSprite(invalid, 0), "Sprite fade frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathSprite(0, invalid), "Sprite fade color bounds remain exact");
        Console.WriteLine("Spore sprite fade: all128 installed native words and canonical identity pass.");

        void Verify(SporeSpawnColorCatalog catalog)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + color * 2))).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] sprite = Rows(0xa5e3f9, 8);
            ushort[][] health = Rows(0xa5e379, 4);
            for (int frame = 0; frame < 8; frame++)
                for (int color = 0; color < 16; color++)
                    AssertEqual(sprite[frame][color], catalog.ResolveDeathSprite(frame, color), "Endpoint and intermediate sprite fade words");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e359 + color * 2)), catalog.ResolveSpore(color), "Spore palette words");
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(health[frame][color], catalog.ResolveHealth(frame, color), "Health rows");
                for (int frame = 0; frame < 7; frame++)
                {
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e4f9 + frame * 32 + color * 2)), catalog.ResolveDeathLevel(frame, color), "Level colors");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5e5d9 + frame * 32 + color * 2)), catalog.ResolveDeathBackground(frame, color), "Background colors");
                }
            }
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", health);
                content.AppendWordFrames("deathSprite", sprite);
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Calculated fade retains exact selected canonical identity framing");
        }
    }
    /// <summary>Checks the ending result panel artwork and its installed presentation mapping.</summary>
    internal static void VerifyLookupStream4EndingResultPanel(ISnesAddressSpace rom)
    {
        byte[] source = EndingTextExtractor.Extract(rom);
        ushort[] native = ReadEndingWords(rom, EndingSourceDefinitions.EndingResultPanel,
            EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth);
        var stock = EndingTextPresentation.Load(new MemoryStream(source));
        AssertTrue(native.SequenceEqual(BackgroundTileWordBytes.Words(stock.BuildResultPanel())), "Installed credit lines preserve all288 original producer-panel cells, encoded through the codec");
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source)), stock.ContentIdentity, "Result layout preserves raw document identity");
        BackgroundTile[] mutableOutput = stock.BuildResultPanel();
        mutableOutput[0] = mutableOutput[0] with { Tile = mutableOutput[0].Tile ^ 1 };
        AssertTrue(native.SequenceEqual(BackgroundTileWordBytes.Words(stock.BuildResultPanel())), "Result output snapshots remain independent of the catalog");
        Console.WriteLine("Ending result panel:288 native cells, the document identity and immutable output snapshots pass.");
    }

    /// <summary>Checks ending subtitle glyph selection and layout against imported presentation data.</summary>
    internal static void VerifyLookupStream4EndingSubtitle(ISnesAddressSpace rom)
    {
        byte[] source = EndingTextExtractor.Extract(rom);
        ushort[] native = ReadEndingWords(rom, EndingSourceDefinitions.EndingJapaneseSubtitle,
            EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth);
        var stock = EndingTextPresentation.Load(new MemoryStream(source));
        for (int cell = 0; cell < native.Length; cell++)
            AssertEqual(native[cell], stock.JapaneseSubtitle[cell].PackedWord, "Installed subtitle cell agrees with native");
        Verify(stock);
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source)), stock.ContentIdentity, "Subtitle conversion preserves complete document identity");
        foreach (int invalid in new[] { int.MinValue, -1, native.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.JapaneseSubtitle[invalid], "Installed subtitle preserves indexed bounds");
        BackgroundTile[] shortBuffer = new BackgroundTile[native.Length - 1];
        Array.Fill(shortBuffer, BackgroundTileWordBytes.Tile(0x1234));
        AssertThrows<ArgumentException>(() => stock.JapaneseSubtitle.CopyTo(shortBuffer), "Subtitle rejects short output before writing");
        AssertTrue(shortBuffer.All(value => value == BackgroundTileWordBytes.Tile(0x1234)), "Failed subtitle copy is atomic");
        Console.WriteLine("Ending subtitle:64 native cells and actual draw/clear transfer pass.");

        void Verify(EndingTextPresentation presentation)
        {
            ushort[] expected = native.ToArray();
            AssertEqual(expected.Length, presentation.JapaneseSubtitle.Count, "Subtitle view keeps native dimensions");
            AssertTrue(expected.SequenceEqual(presentation.JapaneseSubtitle.Select(tile => tile.PackedWord)), "Calculated subtitle enumerates exact native cells");
            BackgroundTile[] copied = Enumerable.Repeat(BackgroundTileWordBytes.Tile(0x5678), expected.Length + 1).ToArray();
            presentation.JapaneseSubtitle.CopyTo(copied);
            AssertTrue(expected.SequenceEqual(BackgroundTileWordBytes.Words(copied.AsSpan(0, expected.Length))), "Subtitle direct copy preserves every cell");
            AssertEqual((ushort)0x5678, copied[^1].PackedWord, "Subtitle copy leaves the destination suffix untouched");
            // The game shows the subtitle as its installed picture at its corner (#1467, #1555); the import pictures these words.
            var state = new EndingTypewriter(new EndingTypewriterSetup { Inventory = default, JapaneseText = true, Text = presentation.Typewriter() }, EndingTextSequence.ItemPercentage);
            state.InitialPausePending = false;
            state.TypedCount = presentation.Compile(EndingTextSequence.ItemPercentage).Length;
            state.Step();
            var subtitle = new EndingShownText(EndingPictureNames.JapaneseSubtitle, EndingResultTextLayout.JapaneseSubtitleCorner);
            AssertTrue(state.Shown().Contains(subtitle), "Actual ending state shows the subtitle picture at its corner");
            state.FrameHold = 1;
            state.Step();
            AssertTrue(!state.Shown().Contains(subtitle), "Actual ending state clears the subtitle at its hold boundary");
            AssertTrue(state.Completed && state.RequestedItemPercentageScroll, "Subtitle clear preserves completion and scroll handoff");
        }
    }

    /// <summary>Checks Ridley tail rest offsets and geometry against native coordinate data.</summary>
    internal static void VerifyLookupStream4TailRestGeometry(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyLookupStream4TailAngles), () => VerifyLookupStream4TailAngles(rom));
        for (int link = 0; link < 7; link++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d37c + link * 2)), RidleyTailReach.RestDistance(link).RawValue, "Shared tail rest geometry preserves the native initial separation");
        for (int link = 1; link < 7; link++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6cf7f + (link - 1) * 0x36)), RidleyTailReach.RestDistance(link).RawValue, "Native shrink threshold equals initialized rest geometry");
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyTailReach.RestDistance(invalid), "Shared rest geometry preserves bounded array selection");
        Action<RidleyEnemyState> update = RidleyTail.UpdateRidleyTailDistances;
        foreach (int delta in new[] { -1, 0, 1 })
        {
            var state = new RidleyEnemyState
            {
                TailSegments = Enumerable.Range(0, 7).Select(link => new RidleyTailSegment
                {
                    Distance = new SignedEightEight(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d37c + link * 2)) + delta),
                }).ToArray(),
            };
            update(state);
            for (int link = 0; link < 7; link++)
            {
                int before = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d37c + link * 2)) + delta;
                ushort expected = (ushort)(before - (link != 0 && delta > 0 ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6cf8b + (link - 1) * 0x36)) : 0));
                AssertEqual(expected, state.TailSegments[link].Distance.RawValue, "Actual tail shrinking preserves strict threshold, native decrement and excluded base");
            }
        }
        var extending = new RidleyEnemyState
        {
            TailExtensionSpeed = SignedEightEight.FromParts(0, 240),
            TailSegments = Enumerable.Range(0, 7).Select(link => new RidleyTailSegment
            {
                Distance = new SignedEightEight(link == 0 ? 123 : CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6cf72 + (link - 1) * 0x36)) - 1),
                TargetDistance = new SignedEightEight(1),
            }).ToArray(),
        };
        update(extending);
        for (int link = 1; link < 7; link++)
        {
            AssertEqual<SignedEightEight?>(null, extending.TailSegments[link].TargetDistance, "Actual tail extension clears the passed target first");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6cf72 + (link - 1) * 0x36)), extending.TailSegments[link].Distance.RawValue, "Actual tail still extends and clamps on the target-clearing frame");
        }
        AssertEqual(123, extending.TailSegments[0].Distance.RawValue, "Tail base stays outside extension updates");
        Console.WriteLine("Tail rest geometry: seven initial distances, six original shrink thresholds and actual boundary/extension ordering pass; chosen lengths remain required.");
    }

    /// <summary>Checks the Baby Metroid transfer graphics phase at the native VRAM locations.</summary>
    internal static void VerifyLookupStream4BabyTransferPhase(ISnesAddressSpace rom)
    {
        for (int phase = 0; phase < 4; phase++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6acda + phase * 2)), CeresNativeWords.FloorAnimationFrames.Word(CeresFloorAnimation.BabyFrameForPhase(phase)), "Baby reflected phase preserves native transfer identity");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresFloorAnimation.BabyFrameForPhase(invalid), "Baby transfer phase preserves bounded selection");
        // The getaway draws the capsule's frames over the room's turning floor, which each NMI publishes; it writes no VRAM (#1349, #1467).
        RoomFloor map = RoomBackgroundsFixtures.CeresFloor();
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new StandaloneRoomEnemyServices(() => 0) { Floor = map })
            { BossArtworkBinding = { CeresRidleyGetawayBoostDelays = CeresRidleyGetawayBoostDelayFiles.Read(new MemoryStream(CeresRidleyGetawayBoostDelayFiles.Extract(rom))) } };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        Action<RidleyEnemyState, SamusState?, int> tick = enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor;
        var state = new RidleyEnemyState { GetawayFrameIndex = 1 };
        byte[] untouched = new byte[SnesVram.ByteCount];
        Array.Fill(untouched, (byte)0x5a);
        CeresFloorAnimationFrame? Capsule() => map.Shown.TryGetValue(CeresFloorPart.BabyCapsule, out CeresFloorAnimationFrame capsule) ? capsule : null;
        int expectedPhase = 0;
        for (ushort nmi = 0; nmi < 16; nmi++)
        {
            vram.LoadBytes(0, untouched);
            CeresFloorAnimationFrame? before = Capsule();
            bool advances = (nmi & 3) == 0;
            if (advances) expectedPhase = (expectedPhase + 1) & 3;
            tick(state, null, nmi);
            AssertTrue(vram.Bytes.SequenceEqual(untouched), "The getaway leaves VRAM to the published floor");
            AssertEqual((ushort)expectedPhase, state.GetawayBabyFrame, "Actual getaway advances Baby before selection only on every fourth NMI");
            AssertEqual(advances ? CeresFloorAnimation.BabyFrameForPhase(expectedPhase) : before, Capsule(),
                "Actual getaway draws the native capsule frame and keeps it on non-update NMIs");
        }
        Console.WriteLine("Baby reflected phase: four original pointers and one actual 16-NMI cycle preserve advance-before-select and the drawn capsule frames.");
    }

    /// <summary>Checks Fly animation frame identities against the cartridge instruction data.</summary>
    internal static void VerifyFlyFrameIdentities(CartridgeImportAddressSpace rom)
    {
        NativeEnemySpritemapDefinition[] flyFrames = FlyVisualDefinitions.Frames();
        AssertEqual(4, flyFrames.Length, "fly calculated frame catalog count");
        for (int frame = 0; frame < flyFrames.Length; frame++)
        {
            AssertEqual((byte)0xa2, flyFrames[frame].Bank, "fly native frame bank");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b015 + frame * 4)), flyFrames[frame].Pointer, "fly native instruction frame identity");
            AssertEqual($"fly_shared_{frame}", flyFrames[frame].Name, "fly editable frame name");
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | flyFrames[frame].Pointer)), "fly native single OAM object");
        }
    }

    /// <summary>Checks HUD icon tiles and their source identities against native artwork.</summary>
    internal static void VerifyLookupStream4HudIcons(ISnesAddressSpace rom)
    {
        byte[] json=GameplayHudPresentationExtractor.Extract(rom);
        static ushort Word(GameplayHudCell cell)=>SnesBgTilemapWord.Create(cell.TileRow*32+cell.TileColumn,cell.Palette,cell.Priority,
            (cell.FlipX?PictureFlip.Horizontal:0)|(cell.FlipY?PictureFlip.Vertical:0)).Raw;
        var stock=GameplayHudPresentationFile.Read(new MemoryStream(json));
        {
            // The stock icons draw the cartridge's icon words at its icon offsets.
            ushort[] native=Enumerable.Repeat(stock.Blank.PackedWord,96).ToArray();int sourceWord=0;
            for(int item=0;item<5;item++)
            {
                stock.TryApplyIcon(native,item);int width=item==0?3:2;int anchor=CartridgeImportSource.Require(rom).ReadCartridgeWord(0x809d6e+item*2)/2;
                for(int cell=0;cell<width*2;cell++,sourceWord++)
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8099a3+sourceWord*2),native[anchor+cell/width*32+cell%width],"Stock icon draws the native word at the native offset");
            }
        }
        {
            var document=RequiredJson.Deserialize<GameplayHudPresentationDocument>(json,ArtworkJson.Options);
            using var output=new MemoryStream();GameplayHudPresentationFile.Write(output,document);byte[] bytes=output.ToArray();var selected=GameplayHudPresentationFile.Read(new MemoryStream(bytes));
            ushort[] tiles=Enumerable.Repeat(selected.Blank.PackedWord,96).ToArray();
            for(int item=0;item<5;item++)
            {
                var icon=document.Icons[GameplayHudDefinitions.IconNames[item]];int width=item==0?3:2;
                selected.TryApplyIcon(tiles,item);
                for(int cell=0;cell<width*2;cell++)
                    AssertEqual(Word(icon.Cells[cell]),tiles[(icon.Anchor.Y+cell/width)*32+icon.Anchor.X+cell%width],"Every stock icon cell");
            }
            {
                var hud=new HudState();hud.BindPresentation(selected.Presentation);hud.Initialize(HudSnapshot.CeresDebug with{EquippedItems=(SamusEquipmentFlags.XrayScope|SamusEquipmentFlags.GrappleBeam),MaxMissiles=1,MaxSuperMissiles=1,MaxPowerBombs=1});
                for(int item=0;item<5;item++){var icon=document.Icons[GameplayHudDefinitions.IconNames[item]];int width=item==0?3:2;for(int cell=0;cell<width*2;cell++)AssertEqual(Word(icon.Cells[cell]),hud.TileWords(selected)[(icon.Anchor.Y+cell/width)*32+icon.Anchor.X+cell%width],"Actual HUD inventory draws installed icon");}
            }
            for(int item=0;item<5;item++)selected.TryApplyIcon(tiles,item);
            for(int item=0;item<5;item++){var icon=document.Icons[GameplayHudDefinitions.IconNames[item]];int width=item==0?3:2;for(int cell=0;cell<width*2;cell++)AssertEqual(Word(icon.Cells[cell]),tiles[(icon.Anchor.Y+cell/width)*32+icon.Anchor.X+cell%width],"Occupied icon guard preserves current cells");}
        }
        foreach(int invalid in new[]{-1,5,int.MinValue,int.MaxValue})AssertThrows<ArgumentOutOfRangeException>(()=>stock.TryApplyIcon(new ushort[96],invalid),"Icon item domain");
        const int scale=6,imageWidth=128*scale,imageHeight=16*scale;byte[] pixels=new byte[imageWidth*imageHeight];int sourceIndex=0,startX=0;
        for(int item=0;item<5;item++)
        {
            int width=item==0?3:2;
            for(int cell=0;cell<width*2;cell++,sourceIndex++)
            {
                int word=CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8099a3+sourceIndex*2);
                for(int y=0;y<8;y++)for(int x=0;x<8;x++)
                {
                    int sx=(word&0x4000)!=0?7-x:x,sy=(word&0x8000)!=0?7-y:y,address=MapSourceDefinitions.Artwork.HudCharacters+(word&0x3ff)*16+sy*2;
                    int ink=(rom.ReadCartridgeByte(address)>>(7-sx)&1)|(rom.ReadCartridgeByte(address+1)>>(7-sx)&1)<<1;
                    for(int dy=0;dy<scale;dy++)for(int dx=0;dx<scale;dx++)pixels[((cell/width*8+y)*scale+dy)*imageWidth+(startX+cell%width*8+x)*scale+dx]=(byte)ink;
                }
            }
            startX+=(width+1)*8;
        }

        Console.WriteLine("HUD icons:22native words drawn at native offsets,the document hash/guards,the actual inventory initializer/readguard/domains pass;source pictograms exported.");
    }

    /// <summary>Checks the HUD template cells against their native layout words.</summary>
    internal static void VerifyLookupStream4HudTemplate(ISnesAddressSpace rom)
    {
        byte[] json=GameplayHudPresentationExtractor.Extract(rom);
        var stock=GameplayHudPresentationFile.Read(new MemoryStream(json));
        {
            ushort[] native=new ushort[96];stock.ApplyTemplate(native);
            for(int index=0;index<96;index++)AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8098cb+index*2),native[index],"Exact native template word");
        }
        {
            var document=RequiredJson.Deserialize<GameplayHudPresentationDocument>(json,ArtworkJson.Options);
            using var output=new MemoryStream();GameplayHudPresentationFile.Write(output,document);byte[] bytes=output.ToArray();
            var selected=GameplayHudPresentationFile.Read(new MemoryStream(bytes));
            ushort[] tiles=new ushort[96];selected.ApplyTemplate(tiles);
            for(int index=0;index<96;index++)
            {
                var cell=document.Template[index];ushort expected=SnesBgTilemapWord.Create(cell.TileRow*32+cell.TileColumn,cell.Palette,cell.Priority,(cell.FlipX?PictureFlip.Horizontal:0)|(cell.FlipY?PictureFlip.Vertical:0)).Raw;
                AssertEqual(expected,tiles[index],"Stock template cell");
            }
            {
                var hud=new HudState();hud.BindPresentation(selected.Presentation);hud.Initialize(HudSnapshot.CeresDebug);
                for(int row=0;row<3;row++)for(int x=26;x<32;x++)AssertEqual(tiles[row*32+x],hud.TileWords(selected)[row*32+x],"Actual initializer preserves selected initial minimap and border");
                for(int x=1;x<=4;x++)AssertEqual(tiles[64+x],hud.TileWords(selected)[64+x],"Actual initializer preserves selected ENERGY typography");
            }
        }
        Console.WriteLine("HUD template:96native words,the document hash,the full application and the actual initializer pass.");
    }

    /// <summary>Checks the installed HUD top row at the native VRAM tile positions.</summary>
    internal static void VerifyLookupStream4HudTopRow(ISnesAddressSpace rom)
    {
        byte[] json=GameplayHudPresentationExtractor.Extract(rom);
        var stock=GameplayHudPresentationFile.Read(new MemoryStream(json));
        {
            ushort[] native=BackgroundTileWordBytes.Words(stock.TopRowTiles());
            for(int column=0;column<32;column++)AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x80988b+column*2),native[column],"Native top-row word");
        }
        {
            var document=RequiredJson.Deserialize<GameplayHudPresentationDocument>(json,ArtworkJson.Options);
            using var output=new MemoryStream();GameplayHudPresentationFile.Write(output,document);byte[] bytes=output.ToArray();
            var selected=GameplayHudPresentationFile.Read(new MemoryStream(bytes));
            ushort[] row=BackgroundTileWordBytes.Words(selected.TopRowTiles());
            AssertEqual(32,row.Length,"Top row covers every HUD column");
            for(int column=0;column<32;column++)
            {
                var cell=document.TopRow[column];ushort expected=SnesBgTilemapWord.Create(cell.TileRow*32+cell.TileColumn,cell.Palette,cell.Priority,(cell.FlipX?PictureFlip.Horizontal:0)|(cell.FlipY?PictureFlip.Vertical:0)).Raw;
                AssertEqual(expected, row[column], "Every supplied top-row cell survives into the shown row");
            }
        }
        const int scale=8,width=256,height=64;byte[] pixels=new byte[width*height];
        for(int cell=0;cell<4;cell++)for(int y=0;y<8;y++)for(int x=0;x<8;x++)
        {
            int tile=cell==0?0x0f:cell==3?0x1c:0x1d,address=MapSourceDefinitions.Artwork.HudCharacters+tile*16+y*2;
            int ink=(rom.ReadCartridgeByte(address)>>(7-x)&1)|(rom.ReadCartridgeByte(address+1)>>(7-x)&1)<<1;
            for(int dy=0;dy<scale;dy++)for(int dx=0;dx<scale;dx++)pixels[(y*scale+dy)*width+(cell*8+x)*scale+dx]=(byte)ink;
        }

        Console.WriteLine("HUD top row:32native words,the document hash and the shown row pass; native border source exported.");
    }
    /// <summary>Checks the complete auto-reserve HUD display assembled from its component cells.</summary>
    internal static void VerifyLookupStream4HudAutoComplete(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyLookupStream4HudAutoCells), () => VerifyLookupStream4HudAutoCells(rom));
        byte[] json=GameplayHudPresentationExtractor.Extract(rom);
        static ushort Word(GameplayHudCell cell)=>SnesBgTilemapWord.Create(cell.TileRow*32+cell.TileColumn,cell.Palette,cell.Priority,
            (cell.FlipX?PictureFlip.Horizontal:0)|(cell.FlipY?PictureFlip.Vertical:0)).Raw;
        {
            var document=RequiredJson.Deserialize<GameplayHudPresentationDocument>(json,ArtworkJson.Options);
            using var output=new MemoryStream();GameplayHudPresentationFile.Write(output,document);byte[] bytes=output.ToArray();
            var selected=GameplayHudPresentationFile.Read(new MemoryStream(bytes));
            var hud=new HudState();hud.BindPresentation(selected.Presentation);
            hud.Initialize(HudSnapshot.CeresDebug with{ReserveMode=ReserveTankMode.Auto,ReserveHealth=1});
            foreach(bool full in new[]{true,false})
            {
                ushort[] tiles=Enumerable.Repeat((ushort)0x1234,96).ToArray();selected.ApplyAutoReserve(tiles,full);
                var cells=full?document.AutoReserve.ContainsEnergy:document.AutoReserve.Empty;
                for(int cell=0;cell<6;cell++)AssertEqual(Word(cells[cell]),tiles[GameplayHudStockCells.AutoReserveCellIndex(cell)],"All twelve stock AUTO cells");
                for(int tile=0;tile<96;tile++)if(tile%32 is not(8 or 9))AssertEqual((ushort)0x1234,tiles[tile],"AUTO leaves other HUD cells unchanged");
                hud.UpdateGameplayCounters(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()){Health=99,MaxHealth=99,ReserveTankMode=ReserveTankMode.Auto,ReserveEnergy=(ushort)(full?1:0)}, new GameplayAudioRequests());
                for(int cell=0;cell<6;cell++)AssertEqual(Word(cells[cell]),hud.TileWords(selected)[GameplayHudStockCells.AutoReserveCellIndex(cell)],"Actual HUD reserve state chooses exact supplied AUTO cells");
            }
            // The HUD is drawn from its current values (#1555): manual reserves show the AUTO cells cleared.
            hud.UpdateGameplayCounters(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()){Health=99,MaxHealth=99,ReserveTankMode=ReserveTankMode.Manual,ReserveEnergy=1}, new GameplayAudioRequests());
            for(int cell=0;cell<6;cell++)AssertEqual(selected.Blank.PackedWord,hud.TileWords(selected)[GameplayHudStockCells.AutoReserveCellIndex(cell)],"Manual reserves clear AUTO");
        }
        Console.WriteLine("HUD AUTO:12native words,the document hash,actual state updates,manual clear/readguard/neighbors pass.");
    }

    /// <summary>Checks auto-reserve HUD cell identities against the cartridge tile words.</summary>
    internal static void VerifyLookupStream4HudAutoCells(ISnesAddressSpace rom)
    {
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        GameplayHudArtwork stock = GameplayHudPresentationFile.Read(new MemoryStream(json));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertTrue(!MetadataShape.DeclaresField(typeof(GameplayHudPresentation), "autoReserveBasis", flags), "AUTO no longer stores whole-word basis cells");
        ushort[] tiles = new ushort[96];
        for (int state = 0; state < 2; state++)
        {
            stock.ApplyAutoReserve(tiles, state == 0);
            for (int cell = 0; cell < 6; cell++)
            {
                int address = 0x80998b + state * 12 + cell * 2;
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                AssertEqual(native, tiles[GameplayHudStockCells.AutoReserveCellIndex(cell)], "AUTO actual native cell draw");
            }
        }
    }

    /// <summary>Checks HUD digit tile identities and their native order.</summary>
    internal static void VerifyLookupStream4HudDigits(ISnesAddressSpace rom)
    {
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        GameplayHudArtwork stock = GameplayHudPresentationFile.Read(new MemoryStream(json));
        ushort[] tiles = new ushort[96];
        for (int digit = 0; digit < 10; digit++)
        {
            stock.ApplyEnergy(tiles, (ushort)(digit * 11), 99);
            for (int place = 0; place < 2; place++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x809dbf + digit * 2)), tiles[70 + place], "actual stock health digit draw");
            stock.ApplyAmmo(tiles, HudAmmoCounter.Missiles, (ushort)(digit * 111));
            for (int place = 0; place < 3; place++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x809dd3 + digit * 2)), tiles[74 + place], "actual stock ammo digit draw");
        }
        // A value naming no ammunition counter is rejected rather than drawn anywhere.
        foreach (HudAmmoCounter invalid in new[] { (HudAmmoCounter)int.MinValue, (HudAmmoCounter)(-1), (HudAmmoCounter)int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyAmmo(tiles, invalid, 0), "HUD ammunition counter exact domain");
    }

    /// <summary>Checks title screen sprite identities against the native artwork pointers.</summary>
    internal static void VerifyTitleSpriteIdentities(CartridgeImportAddressSpace rom)
    {
        var nativeTitlePointers = new HashSet<int> { 0x8103, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8ba0c7) };
        foreach (int start in new[] { 0x8ba03d, 0x8ba055, 0x8ba079, 0x8ba09d })
            for (int entry = start; CartridgeImportSource.Require(rom).ReadCartridgeWord(entry) < 0x8000; entry += 4)
                if (CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 2)) != 0) nativeTitlePointers.Add(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 2)));
        AssertEqual(31, nativeTitlePointers.Count, "native title sprite catalog size");
        AssertEqual(TitleGraphicsFormat.Sprites.Count, Enum.GetValues<TitleSprite>().Length, "title composition domain is complete");
        AssertTrue(nativeTitlePointers.SetEquals(SuperMetroid.AssetExtraction.Frontend.Title.TitleSpriteExtractor.MapSprites(rom).Values),
            "extraction maps every native title selector to exactly one named composition");
        AssertEqual(4, TitleSequenceDefinitions.Graphics.BabyAnimation.Count, "title baby animation frame count");
        for (int frame = 0; frame < 4; frame++)
        {
            ushort transfer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8ba133 + frame * 4));
            int nativeSource = CartridgeImportSource.Require(rom).ReadCartridgeLong((0x8b0001 + transfer));
            int firstSource = CartridgeImportSource.Require(rom).ReadCartridgeLong(0x8ba338);
            AssertEqual((nativeSource - firstSource) / 256,
                TitleFloorTileDefinitions.BabyPages[TitleSequenceDefinitions.Graphics.BabyAnimation[frame]], "native title baby source page of the frame's pose");
            AssertEqual((ushort)256, CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8b0004 + transfer)), "native baby transfer size");
            AssertEqual((ushort)0x3800, CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8b0006 + transfer)), "native baby transfer destination");
        }
        Console.WriteLine("Title identities: 31 sorted native selectors, the four Baby animation frames' poses and DMA sizes/destinations pass.");
    }

    /// <summary>Checks title card tile placement and dimensions against cartridge layout data.</summary>
    internal static void VerifyTitleCardLayout(CartridgeImportAddressSpace rom)
    {
        int steps = SuperMetroid.Verification.Frontend.Title.TitleCardOracle.CompareAll(rom);
        Console.WriteLine($"Title card layout: {steps} typed title-card steps match their native lists.");
    }

    /// <summary>Checks room sprite dispatch identities and their native table ordering.</summary>
    internal static void VerifyRoomSpriteDispatch(CartridgeImportAddressSpace rom)
    {
        Func<RoomEnemySystem, Position16<RoomPixels>, RoomSpriteObjectKind, SpriteGraphics, RoomSpriteObject> spriteSpawn =
            (owner, position, kind, graphics) => owner.SpriteObjects.SpawnRoomSpriteObject(position, kind, graphics);
        int kinds = 0;
        foreach (RoomSpriteObjectKind kind in Enum.GetValues<RoomSpriteObjectKind>().Distinct())
        {
            ushort entry = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb4bda8 + 2 * RoomSpriteObjectNativeWords.Kinds.Word(kind)));
            AssertEqual(entry, RoomSpriteObjectDefinitions.InstructionPointer(kind), "room sprite native program case");
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomSpriteObject spawned = spriteSpawn(enemies, new(0x1234, 0x5678), kind, SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook));
            AssertEqual(entry, spawned.InstructionPointer, "room sprite actual initial program");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb40000 | entry)), spawned.FrameHold, "room sprite actual initial native duration");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb40000 | (entry + 2))), spawned.SpritemapPointer, "room sprite actual initial native visual");
            AssertEqual(kind, spawned.Kind, "room sprite selected kind preserved");
            AssertEqual((ushort)0x1234, spawned.Position.X.Whole, "room sprite spawn X preserved");
            AssertEqual((ushort)0x5678, spawned.Position.Y.Whole, "room sprite spawn Y preserved");
            AssertEqual(SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook), spawned.Graphics, "room sprite tile base and colouring argument preserved");
            kinds++;
        }
        foreach (ushort invalid in new ushort[] { 62, 0x8000, 0xffff })
            AssertEqual(false, RoomSpriteObjectNativeWords.Kinds.TryDecode(invalid, out _),
                "room sprite native kind domain rejects invalid selector");
        Console.WriteLine($"Room sprite dispatch: {kinds} spawned kinds' native entries and actual spawns, initial timing/visuals and invalid selectors pass.");
    }

    /// <summary>Checks which Ridley frame pointers are presentation data and which are control words.</summary>
    private static void VerifyLookupStream4RidleyFrameDomain(ISnesAddressSpace rom)
    {
        var nativeFrames = new List<ushort>();
        var nativeLists = new SortedSet<ushort>();
        for (int frame = 0xe983; frame <= 0xead7;)
        {
            nativeFrames.Add((ushort)frame);
            int components = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | frame));
            for (int component = 0; component < components; component++)
                nativeLists.Add(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | (frame + 8 + component * 8))));
            frame += 2 + components * 8;
        }
        AssertEqual(nativeFrames.Count, RidleyCollisionDefinitions.FramePointers.Length, "Frame identity count follows native record extents");
        int ordinal = 0;
        foreach (ushort frame in RidleyCollisionDefinitions.FramePointers)
        {
            AssertEqual(nativeFrames[ordinal], frame, "Frame identity enumeration preserves native record order");
            AssertEqual(frame, RidleyCollisionDefinitions.FramePointers[ordinal++], "Frame identity indexing preserves order");
            var components = RidleyCollision.ComponentsAt(frame);
            foreach (int invalid in new[] { -1, components.Length })
                AssertThrows<IndexOutOfRangeException>(() => _ = components[invalid], "Calculated component view rejects out-of-range indices");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(nativeFrames.Contains((ushort)pointer), RidleyCollision.HasFrame((ushort)pointer), "Calculated frame membership preserves complete native identity domain");
        int compiledLists = RidleyCollisionDefinitions.FramePointers
            .SelectMany(frame =>
            {
                var frameComponents = RidleyCollision.ComponentsAt(frame);
                return Enumerable.Range(0, frameComponents.Length).Select(index => frameComponents[index].Hitboxes);
            })
            .Distinct(ReferenceEqualityComparer.Instance)
            .Count();
        AssertEqual(nativeLists.Count, compiledLists, "Components select one compiled list per distinct native hitbox list");
        foreach (int invalid in new[] { -1, nativeFrames.Count })
            AssertThrows<IndexOutOfRangeException>(() => _ = RidleyCollisionDefinitions.FramePointers[invalid], "Calculated frame view rejects out-of-range indices");
    }

    /// <summary>Checks beam palette relations and installed CGRAM and VRAM output against native data.</summary>
    internal static void VerifyLookupStream4BeamColorRelations(ISnesAddressSpace rom)
    {
        byte[] source = BeamPaletteExtractor.Extract(rom);
        var stock = BeamPaletteFile.Read(new MemoryStream(source));
        var artwork = BeamTileCatalog.Load(BeamTileExtractor.Extract(rom), stock);
        for (int selection = 0; selection < 12; selection++)
        {
            var actual = new SamusColours();
            // The beams name the selection's shade (#1512); the import resolves it from the selected palettes.
            SamusProjectileSystem.LoadBeamPalette(actual, SamusEquipmentWordCodec.FromBeamWord(checked((ushort)selection)) | SamusBeamFlags.Charge);
            int pointer = 0x900000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90c3c9 + selection * 2));
            RgbColor[] shown = actual.ShownBeams(InstalledFixtures.SamusShadeSources with { BeamPalettes = stock });
            for (int color = 0; color < 16; color++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + color * 2)), shown[color], "Beam paint preserves native selected rows");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Colours(SamusBeamCombinationTableOrder.FromTableIndex(invalid)), "Installed beam colors preserve selection bounds");
        Console.WriteLine("Beam paint:192 native selected words and12 actual masked-selection producers pass.");
    }

    /// <summary>Checks Ridley movement policy constants and branch data against the cartridge.</summary>
    private static void VerifyLookupStream4RidleyMovementPolicy(ISnesAddressSpace rom)
    {
        ushort morphedFrames = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6bc99), ordinaryFrames = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6bc9e);
        // No pose selects a movement type past the 28-entry table; one is a defect, not a policy.
        AssertThrows<InvalidOperationException>(() => RidleyGrabs.CanGrab((SamusMovementType)(-1)), "Undefined movement types fail loudly");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            if (!SamusMovementTypeNativeWords.Types.TryDecode(raw, out SamusMovementType movement))
            {
                AssertTrue(raw >= SamusMovementTypeNumbers.Count || SamusMovementTypeNumbers.Unselected.Contains(raw),
                    "Movement bytes name every movement type a pose selects in the 28-entry table");
                continue;
            }
            byte flags = rom.ReadCartridgeByte(0xa6bd04 + raw);
            AssertEqual((flags & 0x80) != 0, RidleyGrabs.CanGrab(movement), "Named movement policy preserves each movement type's grabbability");
            AssertEqual((flags & 0x40) != 0 ? morphedFrames : ordinaryFrames,
                RidleyGrabs.ReleaseIntangibilityFrames(movement), "Named movement policy preserves each movement type's release duration");
        }
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Func<SamusState?, bool> canGrab = NorfairRidleyCombat.SamusMovementUsesRidleyGrab;
        Action<RidleyEnemyState, SamusState?> release = NorfairRidleyDeath.ReleaseNorfairRidleyGrab;
        for (int pose = 0; pose <= 0xfc; pose++)
        {
            // A record no gameplay path gives Samus names no pose (#1517), so it has no grab policy to check.
            if (!SamusPoseNumbering.NamesPose(pose))
                continue;
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseNumbering.FromNumber(pose) };
            byte flags = rom.ReadCartridgeByte(0xa6bd04 + rom.ReadCartridgeByte(0x91b62a + pose * 8));
            AssertEqual((flags & 0x80) != 0, canGrab(samus), "Actual grab policy preserves each native real-pose classification");
            var state = new RidleyEnemyState { GrabState = RidleyGrabState.CarryingSamus, TailWhipRequest = 0, TailAction = RidleyTailAction.Pogo, IntangibilityTimer = 99 };
            release(state, samus);
            AssertEqual((flags & 0x40) != 0 ? morphedFrames : ordinaryFrames, state.IntangibilityTimer, "Actual release preserves native movement-dependent intangibility");
            AssertEqual(RidleyGrabState.Released, state.GrabState, "Actual release clears grab");
            AssertEqual(1, state.TailWhipRequest, "Actual release requests tail whip");
            AssertEqual(RidleyTailAction.Neutral, state.TailAction, "Actual release resets tail controller");
        }
        AssertTrue(!canGrab(null), "Missing Samus remains ungrabbable");
        var absentState = new RidleyEnemyState();
        release(absentState, null);
        AssertEqual(ordinaryFrames, absentState.IntangibilityTimer, "Missing Samus retains standing release fallback");
    }

    /// <summary>Checks Ridley tail angle selection data against native table values.</summary>
    private static void VerifyLookupStream4TailAngles(ISnesAddressSpace rom)
    {
        Func<RidleyTailSegment[]> create = RidleyTail.CreateInitialRidleyTailSegments;
        var segments = create();
        AssertEqual(7, segments.Length, "Tail initialization retains seven links");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6d2fe), RidleyTailDefinitions.IdealInterSegmentSpacing, "Tail separation matches the native initializer immediate");
        for (int index = 0; index < segments.Length; index++)
        {
            AssertEqual(RidleyTailNativeWords.AngleOf(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d38a + 2 * index))), segments[index].Angle, "Actual initialized link angle matches native progression");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d37c + 2 * index)), segments[index].Distance.RawValue, "Unresolved independent tail distance remains unchanged");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6d36e + 2 * index)), segments[index].Clockwise ? (ushort)0x8000 : (ushort)0, "Initial rotation direction remains unchanged");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6d2fe) + 1, segments[index].StaggerAngle, "Initial stagger remains ideal separation plus one");
        }
        var expectedFrames = new SortedSet<ushort> { 0xdc90, 0xdc97, 0xdc9e };
        for (int direction = 0; direction < 16; direction++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6dcba + 2 * direction));
            AssertEqual(expected, RidleySupplementalVisualDefinitionsConstants.TailTipFrameAt(direction), "Calculated reversed tail-tip sector preserves native identity");
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | expected)), "Each native tail-tip record contains exactly one OAM object");
            expectedFrames.Add(expected);
        }
        for (int frame = 0; frame < 20; frame++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6db02 + frame * 2));
            AssertEqual(expected, RidleySupplementalVisualDefinitionsConstants.WingFrameAt(frame), "Calculated downstroke/upstroke and facing preserve each native wing identity");
            expectedFrames.Add(expected);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 20, int.MaxValue })
            AssertThrows<InvalidDataException>(() => RidleySupplementalVisualDefinitionsConstants.WingFrameAt(invalid), "Wing phase/facing domain remains exact");
        AssertTrue(expectedFrames.SequenceEqual(RidleySupplementalVisualDefinitionsConstants.Frames().Select(frame => frame.Pointer)),
            "Calculated tail-tip source enumeration retains the complete ordered supplemental frame domain");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<InvalidDataException>(() => RidleySupplementalVisualDefinitionsConstants.TailTipFrameAt(invalid), "Tail-tip selector domain remains exact");
        foreach (int invalid in new[] { -1, 7 })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyTailReach.InitialAngle(invalid), "Initial angular domain contains exactly seven links");
    }

    /// <summary>Checks Ridley tail terrain response data against native table values.</summary>
    private static void VerifyLookupStream4TailTerrain(ISnesAddressSpace rom)
    {
        Func<RidleyEnemyState, RoomLevelData?, bool> touches = RidleyTail.RidleyTailTouchesTerrain;
        for (int solidRow = 0; solidRow < 2; solidRow++)
        {
            RoomTerrainBlock[] blocks = new RoomTerrainBlock[2];
            blocks[solidRow] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 2), new RoomLevelPlanes(blocks, new RoomBlockBehavior[2], new RoomBlockAppearance[2]), RoomLevelData.NothingBeyondGrid());
            for (int segmentIndex = 0; segmentIndex < 7; segmentIndex++)
            foreach (ushort y in new ushort[] { 13, 14, 15, 0xfff0, 0xfff1 })
            {
                var state = new RidleyEnemyState
                {
                    TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment { Position = new Position16<RoomPixels>(256, 0) }).ToArray(),
                };
                state.TailSegments[segmentIndex].Position = new Position16<RoomPixels>(0, y);
                bool expected = false;
                if (segmentIndex >= 2)
                {
                    int operand = 0xa6b7f2 + (6 - segmentIndex) * 0x14;
                    int nativeOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord(operand);
                    expected = (unchecked((ushort)(y + nativeOffset)) >> 4) == solidRow;
                }
                AssertEqual(expected, touches(state, level), "Actual tail terrain probes retain each native joint, ADC offset and word wrap");
            }
            AssertTrue(!touches(new RidleyEnemyState(), level), "Incomplete tail keeps terrain gate");
        }
        AssertTrue(!touches(new RidleyEnemyState(), null), "Missing terrain keeps tail gate");
    }

    /// <summary>Checks enemy notice-region bounds and their native coordinate ordering.</summary>
    private static void VerifyLookupStream4NoticeRegions(ISnesAddressSpace rom)
    {
        byte[] bytes = GameplayMessageNoticeExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<GameplayMessageNoticeDocument>(bytes, ArtworkJson.Options);
        var stock = GameplayMessageNoticePresentation.Load(new MemoryStream(bytes));
        foreach (var id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            var notice = document.Notices[id.ToString()];
            ExpectedRegion[] expectedRegions = GameplayMessageNotices.IsSaveConfirmation(id)
                ? [new ExpectedRegion(0, 8, 14), new ExpectedRegion(1, 8, 8), new ExpectedRegion(3, 10, 3), new ExpectedRegion(3, 19, 2)]
                : [new ExpectedRegion(0, 8, id == GameplayMessageId.MissileRechargeCompleted ? 14 : 15), new ExpectedRegion(2, 10, 10)];
            AssertTrue(expectedRegions.SequenceEqual(notice.Text.Select(region => new ExpectedRegion(region.Row, region.Column, region.Width))),
                "Calculated notice rectangles preserve every original row/column/width");
            AssertTrue(notice.Text.All(region => region.Alignment == GameplayMessageTextAlignment.Left),
                "Calculated notice regions retain left alignment");
            int source = 0x850000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x85869f + GameplayMessageNumbers.DefinitionEntry(id) * 6));
            var expected = new ushort[(notice.RowCount + 2) * 32];
            for (int column = 0; column < 32; column++)
                expected[column] = expected[expected.Length - 32 + column] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x858040 + column * 2));
            for (int cell = 0; cell < notice.RowCount * 32; cell++) expected[32 + cell] = CartridgeImportSource.Require(rom).ReadCartridgeWord((source + cell * 2));
            AssertTrue(expected.SequenceEqual(BackgroundTileWordBytes.Words(stock.Build(id))), "Derived notice export reproduces every native border/content word encoded through the codec");
            var stockMessages = new GameplayMessageArtwork(null, null, stock);
            var state = new GameplayMessageBoxState(new SaveChoiceInputCadence());
            state.Begin(id, 0);
            if (GameplayMessageNotices.IsSaveConfirmation(id))
                for (int column = 0; column < 32; column++) expected[128 + column] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8595c1 + column * 2));
            AssertTrue(expected.SequenceEqual(BackgroundTileWordBytes.Words(state.Tilemap(stockMessages))), "Actual notice initial tilemap preserves native YES row and all content");
            if (!GameplayMessageNotices.IsSaveConfirmation(id)) continue;
            for (int frame = 0; frame < 100 && state.Phase != GameplayMessageBoxPhase.AwaitingInput; frame++) state.Step(0);
            AssertEqual(GameplayMessageBoxPhase.AwaitingInput, state.Phase, "Save notice reaches existing input phase");
            // The selector reads the controller only after its two native lag frames ($85:84BA): hold Right until it does.
            for (int frame = 0; frame < 3 && state.ConfirmationSelectionYes; frame++)
                state.Step(ControllerButton.Right);
            AssertTrue(!state.ConfirmationSelectionYes, "Right moves the save selection to NO");
            for (int column = 0; column < 32; column++) expected[128 + column] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x859601 + column * 2));
            ushort[] shown = BackgroundTileWordBytes.Words(state.Tilemap(stockMessages));
            AssertTrue(expected.SequenceEqual(shown), "Actual notice NO selection retains native row and neighboring content; differing cells " +
                string.Join(", ", Enumerable.Range(0, expected.Length).Where(cell => expected[cell] != shown[cell])
                    .Select(cell => $"{cell}: expected {expected[cell]:X4}, got {shown[cell]:X4}")));
        }
    }

    /// <summary>Checks message dispatch IDs and their cartridge table positions.</summary>
    private static void VerifyLookupStream4MessageDispatch(ISnesAddressSpace rom)
    {
        int buttonBits = ControllerWordCodec.ToJoypadWord(ControllerButtons.All);
        for (int bits = 0; bits <= ushort.MaxValue; bits++)
        {
            ControllerButton held = ControllerWordCodec.FromJoypadWord((ushort)(bits & buttonBits), "message dispatch word");
            int winner = 0;
            while (winner < 7 && (bits & CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8583d5 + winner * 8))) == 0) winner++;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x858426 + 2 * winner));
            AssertEqual(expected, GameplayMessageButtonGlyphs.ResolveGlyph(held).PackedWord, "Native BIT order and all button glyph attributes, encoded through the codec");
            AssertEqual(expected, GameplayMessageTilemapDefinitions.Glyphs[GameplayMessageButtons.Shown(held)].PackedWord,
                "The button the game names for a binding retains first-match precedence");
        }
        GameplayMessagePanelPresentation panels = GameplayMessagePanelPresentation.Load(new MemoryStream(GameplayMessagePanelExtractor.Extract(rom)));
        var panelMessages = new GameplayMessageArtwork(null, panels);
        AssertThrows<ArgumentOutOfRangeException>(() => GameplayMessageButtonGlyphs.SpecialGlyphByteOffset(GameplayMessageId.None),
            "Message button patch rejects no message");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            // The native table lists messages one through twenty-seven; its last entry is a dummy no message selects.
            // The gunship's save prompt beyond the table is rejected, and other numbers name no message.
            if (!GameplayMessageNumbers.Numbers.TryDecode(raw, out GameplayMessageId id))
            {
                AssertTrue(raw is 0 or 27 or > 28, "Message numbers name every message one through twenty-six and the gunship's");
                continue;
            }
            if (raw > 27)
            {
                AssertThrows<ArgumentOutOfRangeException>(() => GameplayMessageButtonGlyphs.SpecialGlyphByteOffset(id), "Message button patch exact native domain");
                continue;
            }
            int expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x858749 + (raw - 1) * 2));
            AssertEqual(expected, GameplayMessageButtonGlyphs.SpecialGlyphByteOffset(id), "Native message button placement case");
            if (!panels.Contains(id))
                continue;
            // The panel names the button its binding's glyph shows; every other cell is the panel's own.
            BackgroundTile[] template = panels.Build(id);
            BackgroundTile[] cells = panelMessages.Tiles(new ShownMessage(id, GameplayMessageButtons.Shown(ControllerButtons.All), null));
            for (int index = 0; index < cells.Length; index++)
                AssertEqual(index == expected / 2 ? CartridgeImportSource.Require(rom).ReadCartridgeWord(0x858426) : template[index].PackedWord, cells[index].PackedWord,
                    "Actual button patch preserves every neighboring cell of the panel");
        }
    }
    /// <summary>Runs the stream-four table checks over installed presentation and behavior definitions.</summary>
    internal static void VerifyLookupStream4(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyLookupStream4RidleyFrameDomain), () => VerifyLookupStream4RidleyFrameDomain(rom));
        Suite(nameof(VerifyLookupStream4BeamColorRelations), () => VerifyLookupStream4BeamColorRelations(rom));
        Suite(nameof(VerifyLookupStream4RidleyMovementPolicy), () => VerifyLookupStream4RidleyMovementPolicy(rom));
        Suite(nameof(VerifyLookupStream4TailAngles), () => VerifyLookupStream4TailAngles(rom));
        Suite(nameof(VerifyLookupStream4TailTerrain), () => VerifyLookupStream4TailTerrain(rom));
        Suite(nameof(VerifyLookupStream4MessageDispatch), () => VerifyLookupStream4MessageDispatch(rom));
        Suite(nameof(VerifyLookupStream4NoticeRegions), () => VerifyLookupStream4NoticeRegions(rom));
        // The cartridge's hole table holds one eight-byte record per hole, in the compiled holes' order.
        for (int holeIndex = 0; holeIndex < BotwoonNavigationDefinitions.HoleCount; holeIndex++)
        {
            int offset = 8 * holeIndex;
            var hole = BotwoonNavigation.HoleAt(holeIndex);
            AssertEqual(new Position16<RoomPixels>(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb3949b + offset + 0), CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb3949b + offset + 4)), hole.TopLeft, "Botwoon named hole native top-left boundary");
            AssertEqual(new Position16<RoomPixels>((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb3949b + offset + 0) + 4), (ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb3949b + offset + 4) + 4)), hole.Target, "Botwoon named hole center");
        }
        Suite(nameof(VerifyLookupStream4LightningColors), () => VerifyLookupStream4LightningColors(rom));
        Suite(nameof(VerifyLookupStream4BotwoonColors), () => VerifyLookupStream4BotwoonColors(rom));
        Suite(nameof(VerifyLookupStream4DraygonColors), () => VerifyLookupStream4DraygonColors(rom));
        Suite(nameof(VerifyLookupStream4RidleyFireballOperands), () => VerifyLookupStream4RidleyFireballOperands());
        Suite(nameof(VerifyLookupStream4GeometryLayout), () => VerifyLookupStream4GeometryLayout(rom));
        Suite(nameof(VerifyLookupStream4PowerBombColors), () => VerifyLookupStream4PowerBombColors(rom));
        Suite(nameof(VerifyLookupStream4SporeAndFly), () => VerifyLookupStream4SporeAndFly(rom));
        Suite(nameof(VerifyLookupStream4Burial), () => VerifyLookupStream4Burial(rom));
        Suite(nameof(VerifyLookupStream4StatueColors), () => VerifyLookupStream4StatueColors(rom));
        Suite(nameof(VerifyLookupStream4Programs), () => VerifyLookupStream4Programs(rom));
        Suite(nameof(VerifyLookupStream4ProgramConsumers), () => VerifyLookupStream4ProgramConsumers(rom));
        for (byte theme = 0; theme < RoomGraphicsSetCodec.Count; theme++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8fe7a7 + 2 * theme));
            int source = 0x8f0000 | pointer;
            TilesetDefinition actual = RoomTilesets.Get(RoomGraphicsSetCodec.Decode(theme));
            AssertEqual(Long(source), actual.BlockDefinitionsAddress, "tileset native theme block resource");
            AssertEqual(Long(source + 3), actual.CharacterAddress, "tileset native theme character resource");
            AssertEqual(Long(source + 6), actual.PaletteAddress, "tileset native theme palette resource");
        }
        foreach (byte invalid in new byte[] { 0x1d, 0x7f, 0xff })
            AssertThrows<InvalidDataException>(() => RoomGraphicsSetCodec.Decode(invalid), "tileset exact graphics-theme domain");
        int Long(int address) => CartridgeImportSource.Require(rom).ReadCartridgeLong(address);
        int[] nativeBackgroundSources = LibraryBackgroundSourceInventory.Scan(rom)
            .Where(instruction => instruction.Command == LibraryBackgroundCommand.Decompress)
            .Select(instruction => instruction.SourceAddress).Distinct().Order().ToArray();
        AssertEqual(57, nativeBackgroundSources.Length, "native distinct compressed background source count");
        AssertEqual(nativeBackgroundSources.Length, Enum.GetValues<RoomBackgroundTilemap>().Length, "one named background tilemap per native source");
        // Each native decompression source decodes to the named tilemap the extraction table stores there (#1414).
        foreach (int source in nativeBackgroundSources)
        {
            RoomBackgroundTilemap tilemap = LibraryBackgroundOperandDecoding.DecompressedSource(source).Tilemap;
            AssertEqual(source, RoomBackgroundSourceDefinitions.Tilemaps[tilemap], "background tilemap native source");
            AssertThrows<InvalidDataException>(() => LibraryBackgroundOperandDecoding.DecompressedSource(source + 1),
                "background source interior byte names no tilemap");
        }
        foreach (int invalid in new[] { -1, 0, 0xffffff, int.MaxValue })
            AssertThrows<InvalidDataException>(() => LibraryBackgroundOperandDecoding.DecompressedSource(invalid), "background source absent identity rejected");

        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            var actual = RidleyPogo.Read(pattern, stage);
            int x = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b965 + 2 * pattern));
            int y = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b96d + 2 * pattern));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((x + 2 * stage)), actual.Velocity.X.RawValue, "stream4 native pogo horizontal magnitude");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((y + 2 * stage)), actual.Velocity.Y.RawValue, "stream4 native pogo signed vertical speed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b94d + 2 * stage)), actual.UpwardAcceleration.RawValue, "stream4 native pogo upward acceleration");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b959 + 2 * stage)), actual.DownwardAcceleration.RawValue, "stream4 native pogo downward acceleration");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogo.Read(invalid, 0), "stream4 invalid pogo pattern");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogo.Read(0, invalid), "stream4 invalid pogo stage");
        for (ushort parameter = 0; parameter <= 22; parameter += 2)
        {
            var actual = RidleyExplosionDefinitions.GetPart(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.RidleyExplosionPart(parameter));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6c6ce + parameter)), actual.Lifetime, "stream4 native breakup lifetime");
        }
        // The cartridge's tip list is the one its orientation, the direction half a pose on, numbers in sixteenths of a turn.
        for (int direction = 0; direction < 256; direction++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6c7ba + 2 * (((direction + 8) & 255) >> 4))), RidleyNativeProgramOracle.BreakupList(RidleyBreakup.SelectTailFrame(RidleyExplosionPart.TailTip, new TrigonometryAngle(direction))), "stream4 native tail-tip orientation program");
        foreach (ushort invalid in new ushort[] { 1, 23, 24, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.RidleyExplosionPart(invalid), "stream4 invalid breakup parameter");
    }

    /// <summary>Checks normal lightning palette rows and their source pointers against native values.</summary>
    private static void VerifyLookupStream4LightningColors(ISnesAddressSpace rom)
    {
        var program = CrateriaLightningPaletteFxColorDefinitions.Surface;
        byte[] originalJson = RoomPaletteFxPresentationExtractor.Extract(rom);
        using var originalStream = new MemoryStream(originalJson);
        var original = RoomPaletteFxPresentationFile.Read(originalStream);
        for (int frame = 0; frame < 13; frame++)
        for (int index = 0; index < 8; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x8d0000 | pointer);
            AssertTrue(original.TryReadColor(pointer, out RgbColor actual), "surface lightning installed color exists");
            AssertEqual(native, actual, "surface lightning installed native RGB5");
            AssertEqual(1, original.ColorPointers.Count(p => p == pointer), "surface lightning audit identity enumerates once");
        }
    }

    /// <summary>Checks stream-four enemy and projectile instruction words and terminal controls against cartridge programs.</summary>
    private static void VerifyLookupStream4Programs(ISnesAddressSpace rom)
    {

        for (ushort phase = 0; phase < 4; phase++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3894e + 2 * phase)), NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationChoices.MetareeAnimation((SkreeMetareeAnimationPhase)phase)), "stream4 native Metaree phase program");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3c69c + 2 * phase)), NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationChoices.SkreeAnimation((SkreeMetareeAnimationPhase)phase)), "stream4 native Skree phase program");
        }
        foreach (ushort invalid in new ushort[] { 4, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationChoices.MetareeAnimation((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Metaree phase");
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationChoices.SkreeAnimation((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Skree phase");
        }
    }

    /// <summary>Checks projectile instruction consumers select the compiled program words they expect.</summary>
    private static void VerifyLookupStream4ProgramConsumers(ISnesAddressSpace rom)
    {

        var electricityGuard = NativeEnemyProjectileListGuard.Forbidding(rom, SaveStationElectricityNativeListAddresses.Extent, "save-station electricity");
        var electricitySystem = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        electricitySystem.Projectiles.SpawnSaveStationElectricity(new(10, 2));
        var electricity = electricitySystem.Projectiles.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        for (int frame = 0; frame < 160; frame++)
        {
            electricity.FrameHold = WrappingCountdown.Start(1);
            electricitySystem.Projectiles.AdvanceEnemyProjectileAnimations(electricity, null, Position16<RoomPixels>.Zero);
            AssertTrue(electricity.IsActive, "stream4 electricity active throughout twenty cycles");
            AssertEqual((ushort)1, electricity.FrameHold.RawValue, "stream4 electricity one-frame timing");
            AssertEqual((ushort)(0xe689 + 4 * (frame % 8)), electricity.PresentationOperandAddress, "stream4 electricity installed presentation operand identity");
        }
        electricity.FrameHold = WrappingCountdown.Start(1);
        electricitySystem.Projectiles.AdvanceEnemyProjectileAnimations(electricity, null, Position16<RoomPixels>.Zero);
        AssertTrue(!electricity.IsActive, "stream4 electricity deletes after twenty complete cycles");
        AssertEqual(0, electricityGuard.ForbiddenReadAttempts, "stream4 electricity no mechanics ROM reads");
    }

    /// <summary>Checks statue palette rows and selection data against native colors.</summary>
    private static void VerifyLookupStream4StatueColors(ISnesAddressSpace rom)
    {

        var native = new Dictionary<ushort, ushort>();
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 8; color++)
            native.Add((ushort)(0xe240 + 20 * frame + 2 * color), CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8de240 + 20 * frame + 2 * color)));
        byte[] originalJson = RoomPaletteFxPresentationExtractor.Extract(rom);
        using var originalStream = new MemoryStream(originalJson);
        var original = RoomPaletteFxPresentationFile.Read(originalStream);
        foreach (var pair in native)
        {
            AssertTrue(original.TryReadColor(pair.Key, out RgbColor value), "stream4 original statue color remains readable");
            AssertEqual(pair.Value, value, "stream4 loaded original statue color");
        }
        ushort[] keys = original.ColorPointers.Where(native.ContainsKey).ToArray();
        AssertEqual(64, keys.Length, "stream4 all statue pointers enumerable");
        AssertEqual(64, keys.Distinct().Count(), "stream4 statue pointer enumeration has no duplicates");
    }

    /// <summary>Checks the burial presentation constants against the cartridge layout.</summary>
    private static void VerifyLookupStream4Burial(ISnesAddressSpace rom)
    {

        for (int entry = 0; entry < 6; entry++)
        {
            var actual = DraygonBurialEvirs.ForEntry(entry);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1af + 4 * entry)), actual.SubspeedMagnitude.X.Fraction, "stream4 burial radial X subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1b1 + 4 * entry)), actual.SubspeedMagnitude.Y.Fraction, "stream4 burial radial Y subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1c7 + 4 * entry)), actual.Initial.X, "stream4 burial radial spawn X including negative wrap");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1c9 + 4 * entry)), actual.Initial.Y, "stream4 burial radial spawn Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1df + 4 * entry)), (ushort)actual.Angle.RawValue, "stream4 burial radial angle");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<InvalidDataException>(() => DraygonBurialEvirs.ForEntry(invalid), "stream4 burial exact six-record domain");
    }

    /// <summary>Checks Spore Spawn and Fly visual identity data against native tables.</summary>
    private static void VerifyLookupStream4SporeAndFly(ISnesAddressSpace rom)
    {

        for (ushort index = 0; index < 4; index++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + 2 * index)), SporeSpawnProjectiles.StalkYOffset(index), "stream4 original stalk segment spacing");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + 2 * index)), SporeSpawnProjectiles.SpawnerX(index), "stream4 original ceiling emitter spacing");
        }
        foreach (ushort invalid in new ushort[] { 4, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnProjectiles.StalkYOffset(invalid), "stream4 stalk spawn domain");
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnProjectiles.SpawnerX(invalid), "stream4 emitter spawn domain");
        }
    }

    /// <summary>Checks power-bomb palette color stages against cartridge words.</summary>
    private static void VerifyLookupStream4PowerBombColors(ISnesAddressSpace rom)
    {
        byte[] original = PowerBombGlowColorExtractor.Extract(rom);
        var catalog = PowerBombGlowColorFile.Read(new MemoryStream(original));
        foreach (var sequence in Enum.GetValues<PowerBombGlowSequence>())
        {
            int count = PowerBombGlowColorFormat.Count(sequence);
            for (int index = 0; index < count; index++)
            {
                int address = PowerBombColorSources.Start(sequence) + index * 3;
                var native = new FadeLevelColour(rom.ReadCartridgeByte(address), rom.ReadCartridgeByte(address + 1), rom.ReadCartridgeByte(address + 2)).ToColour();
                AssertEqual(native, catalog.Resolve(sequence, index), "installed Power Bomb native RGB channels");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(sequence, invalid), "Power Bomb color index domain");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve((PowerBombGlowSequence)2, 0), "Power Bomb sequence domain");
    }

    /// <summary>
    /// Checks eye-door projectile geometry against the cartridge. Since #1414 the eye door's shot origins and sweat
    /// launches are typed per facing: this asserts that the origin and velocity pairs the retail eye-door operands
    /// select ($86:B65B shot origins at operands 0 and 20, $86:B6B1 sweat velocities at operands 0 and 4) equal them,
    /// rather than comparing every word of the tables, whose other entries no retail eye door selects.
    /// </summary>
    internal static void VerifyLookupStream4GeometryLayout(ISnesAddressSpace rom)
    {
        short NativeWord(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        // The attack initializer adds eight pixels to the table's X before placing the shot.
        AssertEqual(new Offset16<RoomPixels>(checked((short)(8 + NativeWord(0x86b65b))), NativeWord(0x86b65d)), EyeDoorEffectDefinitions.LeftShotOrigin, "left eye-door shot origin");
        AssertEqual(new Offset16<RoomPixels>(checked((short)(8 + NativeWord(0x86b65b + 20))), NativeWord(0x86b65b + 22)), EyeDoorEffectDefinitions.RightShotOrigin, "right eye-door shot origin");
        AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(NativeWord(0x86b6b1), NativeWord(0x86b6b3)), EyeDoorEffectDefinitions.LeftSweatLaunch, "left eye-door sweat launch");
        AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(NativeWord(0x86b6b1 + 4), NativeWord(0x86b6b1 + 6)), EyeDoorEffectDefinitions.RightSweatLaunch, "right eye-door sweat launch");
        // The fourth choice names the bottom cannon's word, $880A, which no room places and Draygon's setup disables: it fires
        // from none (#1686).
        AssertEqual(4, DraygonCannonData.FiringOrder.Length, "Draygon firing choices");
        AssertTrue(DraygonCannonData.FiringOrder[3] is null, "Draygon's fourth firing choice fires from no cannon");
        for (int index = 0; index < 3; index++)
        {
            var target = DraygonCannonData.FiringTarget(DraygonCannonData.FiringOrder[index] ?? throw new InvalidOperationException("Verification requires a firing cannon."));
            AssertEqual(DraygonWallCannonAddresses.Decode((ushort)(0x8804 + 2 * index)), target.Cannon, "Draygon original control-word selection");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa587e4 + 4 * index)), target.Origin.X, "Draygon original cannon X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa587e6 + 4 * index)), target.Origin.Y, "Draygon original cannon Y");
        }
        for (ushort index = 0; index < 8; index++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2cb77 + index * 2)), NativeEnemyAnimationOracle.EntryOf(MaridiaLargeSnailNativeListAddresses.Entries, MaridiaLargeSnailAnimationChoices.Animation(MaridiaSnailsNativeWords.MaridiaLargeSnailAnimations.Decode(index, "Oum animation index"))), "Oum native action/facing dispatch");
        for (int slot = 0; slot < 3; slot++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x81812b + slot * 2)), SaveRamLayout.SlotOffset(slot), "native SRAM slot origins");
        foreach (DraygonFiringCannon invalid in new[] { (DraygonFiringCannon)3, (DraygonFiringCannon)byte.MaxValue })
            AssertThrows<InvalidOperationException>(() => DraygonCannonData.FiringTarget(invalid), "Draygon cannon role domain");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SaveRamLayout.SlotOffset(invalid), "save slot exact domain");
        foreach (ushort invalid in new ushort[] { 8, 255, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => MaridiaSnailsNativeWords.MaridiaLargeSnailAnimations.Decode(invalid, "Oum animation index"), "Oum animation exact domain");
    }

    /// <summary>Checks Ridley's fireball frames keep their native operand order.</summary>
    private static void VerifyLookupStream4RidleyFireballOperands()
    {
        // Ridley's fireball programs are typed (#1287); VerifyRidleyAnimations matches them to the native lists, and
        // their installed frames keep the native operand addresses in list order.
        ushort[] operands =
        [
            0x9556, 0x955e, 0x9562, 0x9566, 0x956a, 0x956e,
            0x9578, 0x957c, 0x9580, 0x9584, 0x9588,
            0x95a4, 0x95aa, 0x95ae, 0x95b2, 0x95b6,
            0x95d7, 0x95dd, 0x95e1, 0x95e5, 0x95e9,
            0x960a, 0x9610, 0x9614, 0x9618, 0x961c,
        ];
        AssertTrue(operands.SequenceEqual(RidleyFireballVisualDefinitions.ProjectileFrames.Select((frame => EnemyProjectileProgramFrameSources.Operand(RidleyFireballVisualDefinitions.ShownProjectileFrames[frame])))),
            "Ridley fireball frames keep the native operand order");
    }

    /// <summary>Checks Draygon palette and presentation table boundaries against cartridge layout.</summary>
    internal static void VerifyLookupStream4DraygonPresentationLayout(ISnesAddressSpace rom)
    {
        // #1283: the typed programs own the frame layout; the oracle matches every frame to its native operand.
        Suite(nameof(VerifyDraygonAnimations), () => VerifyDraygonAnimations(rom));
        Console.WriteLine("Draygon presentation layout: every typed frame matches its native operand and duration.");
    }

    /// <summary>Checks Draygon intro palette phases and installed CGRAM colors against native stages.</summary>
    internal static void VerifyLookupStream4DraygonIntro(ISnesAddressSpace rom)
    {
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = DraygonColorExtractor.Extract(rom);
        var stock = DraygonColorFile.Read(new MemoryStream(json));
        {
            var document=RequiredJson.Deserialize<DraygonColorDocument>(json,ArtworkJson.Options);
            var selected=DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(document)));
            string hash=SelectedPresentationHash.Create("DraygonColorCatalog-v1",content=>
            {
                content.AppendWords("intro",document.Intro.Select(Pack).ToArray()); content.AppendWords("background",document.Background.Select(Pack).ToArray());
                content.AppendWords("sprite",document.Sprite.Select(Pack).ToArray()); content.AppendWords("whiteFlash",document.WhiteFlash.Select(Pack).ToArray());
                content.AppendWordFrames("healthBands",document.HealthBands.Select(row=>row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash,selected.ContentIdentity,"Intro content hash");
            for(int color=0;color<25;color++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a217+color*2)),stock.ResolveIntro(color),"Native intro word");
                AssertEqual(Pack(document.Intro[color]),selected.ResolveIntro(color),"Stock intro word");
                if(color>=17)AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6f50e+(color-17)*2)),stock.ResolveIntro(color),"Canonical escape surface identity");
            }
            for(int color=0;color<16;color++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a277+color*2)),selected.ResolveBackground(color),"Stock background word");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1f7+color*2)),selected.ResolveSprite(color),"Stock sprite word");
            }
            var cgram=new SnesCgram(); for(int color=0;color<256;color++)cgram.SetColor(color,RgbColor.FromWord(0x1234));
            selected.ApplyIntro(cgram);
            for(int color=0;color<256;color++)AssertEqual(color is >=144 and <169?Pack(document.Intro[color-144]):(ushort)0x1234,cgram.Colors[color],"Full intro target copy");
            {
                var artwork=EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { DraygonColors = selected };
                var enemies=new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance){BossArtwork = artworkBosses}; var actual=new SnesCgram(); var vram=new SnesVram();
                enemies.BossArtworkBinding.BodySheets = BossBodySheetFixtures.Stock.Sheets;
                new Action<RoomEnemySlot>(enemies.DraygonBehavior.Parts.InitializeDraygonBody)(enemies.SlotTable.Slots[0]);
                // The initializer shows his opening's colours: the shading he publishes, laid in the import's shades (#1512).
                LayDraygonShading(actual, selected, (enemies.DraygonBehavior.Parts.Draygon ?? throw new InvalidOperationException("Verification requires enemies.DraygonBehavior.Draygon.")).Colours);
                for(int color=0;color<256;color++)AssertEqual(color is >=144 and <169?Pack(document.Intro[color-144]):(ushort)0,actual.Colors[color],"Actual initializer shows only his opening's colours");
                AssertEqual(DraygonPhase.IntroInitialDelay,(enemies.DraygonBehavior.Parts.Draygon ?? throw new InvalidOperationException("Verification requires enemies.DraygonBehavior.Draygon.")).Phase,"Actual initializer phase");
            }
        }
        foreach(int invalid in new[]{-1,25,int.MinValue,int.MaxValue})AssertThrows<ArgumentOutOfRangeException>(()=>stock.ResolveIntro(invalid),"Original intro domain");
        Console.WriteLine("Draygon intro:25 native colours,the hash and full copy,the actual initializer and bounds pass.");
    }

    /// <summary>Checks Draygon sprite palette stages and their installed CGRAM output.</summary>
    internal static void VerifyLookupStream4DraygonSprite(ISnesAddressSpace rom)
    {
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = DraygonColorExtractor.Extract(rom);
        var stock = DraygonColorFile.Read(new MemoryStream(json));
        {
            var document=RequiredJson.Deserialize<DraygonColorDocument>(json,ArtworkJson.Options);
            var selected=DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(document)));
            string hash=SelectedPresentationHash.Create("DraygonColorCatalog-v1",content=>
            {
                content.AppendWords("intro",document.Intro.Select(Pack).ToArray()); content.AppendWords("background",document.Background.Select(Pack).ToArray());
                content.AppendWords("sprite",document.Sprite.Select(Pack).ToArray()); content.AppendWords("whiteFlash",document.WhiteFlash.Select(Pack).ToArray());
                content.AppendWordFrames("healthBands",document.HealthBands.Select(row=>row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash,selected.ContentIdentity,"Sprite content hash");
            for(int color=0;color<16;color++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1f7+color*2)),stock.ResolveSprite(color),"Native sprite source");
                AssertEqual(Pack(document.Sprite[color]),selected.ResolveSprite(color),"Stock sprite word");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a277+color*2)),selected.ResolveBackground(color),"Shared owner does not couple background instance");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a217+color*2)),selected.ResolveIntro(color),"Shared owner does not couple intro instance");
            }
            foreach(bool white in new[]{false,true})
            {
                var cgram=new SnesCgram(); for(int color=0;color<256;color++)cgram.SetColor(color,RgbColor.FromWord(0x1234));
                selected.ApplyHurt(cgram,white,5);
                for(int color=0;color<256;color++)
                {
                    ushort expected=0x1234;
                    if(color is >=80 and <96) expected=white?Pack(document.WhiteFlash[color-80]):color is >=89 and <93?Pack(document.HealthBands[5][color-89]):Pack(document.Background[color-80]);
                    if(color>=240)expected=Pack((white?document.WhiteFlash:document.Sprite)[color-240]);
                    AssertEqual(expected,cgram.Colors[color],"Full sprite/white and BG health restore");
                }
            }
            {
                var artwork=EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { DraygonColors = selected };
                var enemies=new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance){BossArtwork = artworkBosses}; var cgram=new SnesCgram();
                                var body=enemies.SlotTable.Slots[0]; var state=new DraygonEnemyState(body){Eye=enemies.SlotTable.Slots[1],Tail=enemies.SlotTable.Slots[2],Arms=enemies.SlotTable.Slots[3],HealthBand=5};
                Action<RoomEnemySlot,DraygonEnemyState,SamusState?> hurt = enemies.DraygonBehavior.Combat.ApplyDraygonHurt;
                foreach(ushort timer in new ushort[]{2,0})
                {
                    body.FlashTimer=timer; hurt(body,state,null); LayDraygonShading(cgram, selected, state.Colours);
                    for(int color=0;color<16;color++)AssertEqual(Pack((timer==2?document.WhiteFlash:document.Sprite)[color]),cgram.Colors[240+color],"Actual sprite hurt phase and clear/color targets");
                }
            }
        }
        foreach(int invalid in new[]{-1,16,int.MinValue,int.MaxValue})AssertThrows<ArgumentOutOfRangeException>(()=>stock.ResolveSprite(invalid),"Original sprite domain");
        Console.WriteLine("Draygon sprite:16 native colours,the hash,normal-white copies,actual hurt producer calls,background+intro words and bounds pass.");
    }

    /// <summary>Checks Draygon background palette stages and their installed CGRAM output.</summary>
    internal static void VerifyLookupStream4DraygonBackground(ISnesAddressSpace rom)
    {
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = DraygonColorExtractor.Extract(rom);
        var stock = DraygonColorFile.Read(new MemoryStream(json));
        {
            var document = RequiredJson.Deserialize<DraygonColorDocument>(json,ArtworkJson.Options);
            var selected = DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(document)));
            string hash = SelectedPresentationHash.Create("DraygonColorCatalog-v1",content =>
            {
                content.AppendWords("intro",document.Intro.Select(Pack).ToArray()); content.AppendWords("background",document.Background.Select(Pack).ToArray());
                content.AppendWords("sprite",document.Sprite.Select(Pack).ToArray()); content.AppendWords("whiteFlash",document.WhiteFlash.Select(Pack).ToArray());
                content.AppendWordFrames("healthBands",document.HealthBands.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash,selected.ContentIdentity,"Background canonical identity");
            for(int color=0;color<16;color++)
            {
                AssertEqual(Pack(document.Background[color]),selected.ResolveBackground(color),"Stock background channels installed");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a277+color*2)),stock.ResolveBackground(color),"All16 native material words");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a1f7+color*2)),selected.ResolveSprite(color),"Stock sprite occurrence");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa5a217+color*2)),selected.ResolveIntro(color),"Stock intro occurrence");
            }
            foreach(bool white in new[]{false,true})
            {
                var cgram=new SnesCgram(); for(int color=0;color<256;color++) cgram.SetColor(color,RgbColor.FromWord(0x1234));
                selected.ApplyHurt(cgram,white,3);
                for(int color=0;color<256;color++)
                {
                    ushort expected= 0x1234;
                    if(color is >=80 and <96) expected=white?Pack(document.WhiteFlash[color-80]):color is >=89 and <93?Pack(document.HealthBands[3][color-89]):Pack(document.Background[color-80]);
                    if(color>=240) expected=Pack((white?document.WhiteFlash:document.Sprite)[color-240]);
                    AssertEqual(expected,cgram.Colors[color],"Full normal/white copy with native belly overwrite order");
                }
            }
            {
                var artwork=EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { DraygonColors = selected };
                var enemies=new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance){BossArtwork = artworkBosses}; var cgram=new SnesCgram();
                var body=enemies.SlotTable.Slots[0]; var state=new DraygonEnemyState(body){Eye=enemies.SlotTable.Slots[1],Tail=enemies.SlotTable.Slots[2],Arms=enemies.SlotTable.Slots[3],HealthBand=3};
                Action<RoomEnemySlot,DraygonEnemyState,SamusState?> hurt = enemies.DraygonBehavior.Combat.ApplyDraygonHurt;
                foreach(ushort timer in new ushort[]{2,0})
                {
                    body.FlashTimer=timer; hurt(body,state,null); LayDraygonShading(cgram, selected, state.Colours);
                    for(int color=0;color<16;color++) AssertEqual(timer==2?Pack(document.WhiteFlash[color]):color is >=9 and <=12?Pack(document.HealthBands[3][color-9]):Pack(document.Background[color]),cgram.Colors[80+color],"Actual hurt producer preserves targets and health overlay");
                }
            }
        }
        foreach(int invalid in new[]{-1,16,int.MinValue,int.MaxValue}) AssertThrows<ArgumentOutOfRangeException>(()=>stock.ResolveBackground(invalid),"Original background domain");
        Console.WriteLine("Draygon background:16 native colours,the hash,normal-white copies,actual hurt producer calls/belly overwrite/bounds pass.");
    }

    /// <summary>Checks Draygon health-driven palette selection and installed CGRAM output.</summary>
    internal static void VerifyLookupStream4DraygonHealth(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyLookupStream4DraygonColors), () => VerifyLookupStream4DraygonColors(rom));
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = DraygonColorExtractor.Extract(rom);
        {
            var document = RequiredJson.Deserialize<DraygonColorDocument>(json, ArtworkJson.Options);
            var selected = DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(document)));
            string hash = SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
            {
                content.AppendWords("intro", document.Intro.Select(Pack).ToArray()); content.AppendWords("background", document.Background.Select(Pack).ToArray());
                content.AppendWords("sprite", document.Sprite.Select(Pack).ToArray()); content.AppendWords("whiteFlash", document.WhiteFlash.Select(Pack).ToArray());
                content.AppendWordFrames("healthBands", document.HealthBands.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash, selected.ContentIdentity, "Draygon health resource identity");
            for (int band = 0; band < 8; band++)
            {
                var cgram = new SnesCgram(); for (int color = 0; color < 256; color++) cgram.SetColor(color, RgbColor.FromWord(0x1234));
                selected.ApplyHealthBand(cgram, band);
                for (int color = 0; color < 256; color++)
                    AssertEqual(color is >= 89 and < 93 ? Pack(document.HealthBands[band][color - 89]) : (ushort)0x1234, cgram.Colors[color], "Exact changed health transfer and neighbors");
                for (int shade = 0; shade < 4; shade++)
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa596af + (band * 4 + shade) * 2)),
                        selected.ResolveHealthBand(band, shade), "All native shades");
            }
            {
                var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { DraygonColors = selected };
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artworkBosses };
                var cgram = new SnesCgram();
                RoomEnemySlot body = enemies.SlotTable.Slots[0];
                var state = new DraygonEnemyState(body) { Eye = enemies.SlotTable.Slots[1], Tail = enemies.SlotTable.Slots[2], Arms = enemies.SlotTable.Slots[3] };
                Action<DraygonEnemyState> update = DraygonCombat.UpdateDraygonHealthBand;
                Action<RoomEnemySlot,DraygonEnemyState,SamusState?> hurt = enemies.DraygonBehavior.Combat.ApplyDraygonHurt;
                for (int band = 0; band < 8; band++)
                {
                    body.Health = (ushort)((7 - band) * 750); state.HealthBand = null;
                    update(state); LayDraygonShading(cgram, selected, state.Colours);
                    AssertEqual(band, state.HealthBand, "Actual health threshold selects native color band");
                    for (int shade = 0; shade < 4; shade++) AssertEqual(Pack(document.HealthBands[band][shade]), cgram.Colors[89 + shade], "Actual health producer shows the band's colours");
                    PictureShading heldBand = DraygonRoomShading(state.Colours); update(state);
                    AssertEqual(heldBand, DraygonRoomShading(state.Colours), "Same health band keeps the shading it shows");
                    body.FlashTimer = 2; hurt(body,state,null); LayDraygonShading(cgram, selected, state.Colours);
                    for (int color = 0; color < 16; color++) AssertEqual(Pack(document.WhiteFlash[color]),cgram.Colors[80 + color],"Actual white hurt phase");
                    body.FlashTimer = 0; hurt(body,state,null); LayDraygonShading(cgram, selected, state.Colours);
                    for (int color = 0; color < 16; color++)
                    {
                        AssertEqual(color is >= 9 and <= 12 ? Pack(document.HealthBands[band][color - 9]) : Pack(document.Background[color]), cgram.Colors[80 + color], "Actual normal hurt restores selected belly band");
                        AssertEqual(Pack(document.Sprite[color]),cgram.Colors[240 + color],"Separate sprite paint");
                    }
                }
            }
        }
        Console.WriteLine("Draygon health:32 native colors,the hash,full copies,actual health and hurt calls/held band/restore pass.");
    }
    /// <summary>Checks imported Draygon color rows against the native palette source.</summary>
    private static void VerifyLookupStream4DraygonColors(ISnesAddressSpace rom)
    {
        byte[] original = DraygonColorExtractor.Extract(rom);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stock = DraygonColorFile.Read(new MemoryStream(original));
        {
            var document = RequiredJson.Deserialize<DraygonColorDocument>(original, options);
            var catalog = DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(document)));
            ushort Native(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            string identity = SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
            {
                content.AppendWords("intro", document.Intro.Select(Native).ToArray());
                content.AppendWords("background", document.Background.Select(Native).ToArray());
                content.AppendWords("sprite", document.Sprite.Select(Native).ToArray());
                content.AppendWords("whiteFlash", document.WhiteFlash.Select(Native).ToArray());
                content.AppendWordFrames("healthBands", document.HealthBands.Select(row => row.Select(Native).ToArray()).ToArray());
            });
            AssertEqual(identity, catalog.ContentIdentity, "Draygon calculated palette preserves selected content identity");
            for (int color = 0; color < 16; color++)
                AssertEqual(Native(document.WhiteFlash[color]), catalog.ResolveWhiteFlash(color), "Draygon native flash");
            for (int band = 0; band < 8; band++)
            {
                var cgram = new SnesCgram();
                catalog.ApplyHealthBand(cgram, band);
                for (int color = 0; color < 4; color++)
                {
                    ushort expected = Native(document.HealthBands[band][color]);
                    AssertEqual(expected, catalog.ResolveHealthBand(band, color), "Draygon native health color");
                    AssertEqual(expected, cgram.Colors[89 + color], "Draygon health interpolation actual CGRAM transfer");
                }
            }
            var flashCgram = new SnesCgram();
            catalog.ApplyHurt(flashCgram, true, -1);
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(Native(document.WhiteFlash[color]), flashCgram.Colors[80 + color], "Draygon flash background transfer");
                AssertEqual(Native(document.WhiteFlash[color]), flashCgram.Colors[240 + color], "Draygon flash sprite transfer");
            }
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveWhiteFlash(invalid), "Draygon flash bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealthBand(8, 0), "Draygon health band bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealthBand(0, 4), "Draygon health color bounds");
        AssertThrows<InvalidDataException>(() => stock.ApplyHealthBand(new SnesCgram(), 8), "Draygon out-of-range health band rejected");
    }

    /// <summary>Checks Norfair reveal room palette frames and destination colors against cartridge data.</summary>
    internal static void VerifyLookupStream4NorfairReveal(ISnesAddressSpace rom)
    {
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = NorfairRidleyColorExtractor.Extract(rom);
        var stock = NorfairRidleyColorFile.Read(new MemoryStream(json));
        {
            var document = RequiredJson.Deserialize<NorfairRidleyColorDocument>(json, ArtworkJson.Options);
            var selected = NorfairRidleyColorFile.Read(new MemoryStream(NorfairRidleyColorFile.Write(document)));
            string hash = SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
            {
                content.AppendWords("initial", document.Initial.Select(Pack).ToArray());
                content.AppendWordFrames("reveal", document.Reveal.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash, selected.ContentIdentity, "Reveal canonical content hash");
            for (int row = 0; row < 15; row++)
            {
                ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0x1234), RidleySheetShades.NorfairRoom(selected), RidleyShades.NorfairReveal(row), BossShadeColours.Room);
                for (int color = 0; color < 256; color++)
                    AssertEqual(color is >= 113 and < 127 ? Pack(document.Reveal[row][color - 113]) : (ushort)0x1234,
                        cgram[color], "Every reveal row's full CGRAM copy");
                for (int color = 0; color < 14; color++)
                {
                    AssertEqual(Pack(document.Reveal[row][color]), selected.ResolveReveal(row, color), "Direct revealed color equals supplied sample");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6a50b + (row * 14 + color) * 2)), stock.ResolveReveal(row, color), "All native stock rows remain immutable");
                }
            }
            {
                var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { NorfairRidleyColors = selected };
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = artworkBosses };
                enemies.VerificationServices().IsAreaBossDefeated = () => false;
                RoomEnemySlot slot = enemies.SlotTable.Slots[0]; slot.Bind(EnemyDefinitionId.Ridley, slot.Definition);
                new Action<RoomEnemySlot>(enemies.RidleyBehavior.Combat.InitializeNorfairRidley)(slot);
                RidleyEnemyState state = (enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires enemies.RidleyEncounter.Ridley."));
                state.Phase = RidleyPhase.WaitBeforeLiftoff; state.PhaseTimer = 0;
                var liquid = new RoomFxOverlayState(new RoomFxSession());
                enemies.RoomFx.BindRoomFx(liquid);
                Action<RoomEnemySlot, RidleyEnemyState> tick = enemies.RidleyBehavior.Combat.TickNorfairRidleyArenaReveal;
                for (int call = 0; call < 46; call++)
                {
                    tick(slot, state);
                    ushort[] cgram = RidleyLaidColours.Of(state, RidleySheetShades.NorfairSprites(selected), RidleySheetShades.NorfairRoom(selected), 0x1234);
                    int row = Math.Min(call / 3, 14);
                    AssertEqual(2 - call % 3, state.PhaseTimer, "Actual three-call producer cadence preserved");
                    AssertEqual(call < 45 ? (ushort)(row + 1) : (ushort)0, state.FadeStep, "Actual row and zero terminator progression");
                    for (int color = 0; color < 256; color++)
                        AssertEqual(color is >= 113 and < 127 ? Pack(document.Reveal[row][color - 113])
                            : color is >= 160 and < 192 ? Pack(document.Initial[color - 160])
                            : color is 127 or >= 241 ? (ushort)0 : (ushort)0x1234,
                            cgram[color], "Actual producer preserves full CGRAM and held rows");
                }
                AssertEqual(RidleyPhase.ClearVelocity, state.Phase, "Native terminator handoff");
                AssertEqual<int?>(RidleyLiquidDefinitions.BattleHeight, liquid.TargetSurface, "Native liquid target handoff");
                AssertEqual(8, state.WingAnimationTimer, "Native wing timer handoff");
                AssertTrue(state.TailSegments.All(segment => segment.Active), "Native tail activation handoff");
            }
        }
        foreach (int row in new[] { -1, 15, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveReveal(row, 0), "Original reveal row domain");
        }
        foreach (int color in new[] { -1, 14, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveReveal(0, color), "Original reveal color domain");
        Console.WriteLine("Norfair reveal:210 installed native words,the hash,full row copies,real producer calls/cadence/terminator and domain checks pass.");
    }

    /// <summary>Checks Norfair initial palette state and its installed CGRAM colors.</summary>
    internal static void VerifyLookupStream4NorfairInitial(ISnesAddressSpace rom)
    {
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        byte[] json = NorfairRidleyColorExtractor.Extract(rom);
        var original = RequiredJson.Deserialize<NorfairRidleyColorDocument>(json, ArtworkJson.Options);
        var stock = NorfairRidleyColorFile.Read(new MemoryStream(json));
        for (int color = 0; color < 15; color++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6f50e + color * 2)), stock.ResolveInitial(color + 1), "Exact escape material occurrence");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6e1f1 + color * 2)), stock.ResolveInitial(color + 17), "Exact Baby material occurrence");
        }
        {
            var document = RequiredJson.Deserialize<NorfairRidleyColorDocument>(json, ArtworkJson.Options);
            var selected = NorfairRidleyColorFile.Read(new MemoryStream(NorfairRidleyColorFile.Write(document)));
            string hash = SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
            {
                content.AppendWords("initial", document.Initial.Select(Pack).ToArray());
                content.AppendWordFrames("reveal", document.Reveal.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(hash, selected.ContentIdentity, "Initial hash preserves exact supplied words and separate reveal");
            ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0x1234), RidleySheetShades.NorfairSprites(selected), RidleyShades.NorfairStart, BossShadeColours.Sprites);
            for (int color = 0; color < 256; color++)
                AssertEqual(color is >= 160 and < 192 ? Pack(document.Initial[color - 160]) : (ushort)0x1234,
                    cgram[color], "Initial full CGRAM copy");
            for (int color = 0; color < 32; color++)
            {
                AssertEqual(Pack(document.Initial[color]), selected.ResolveInitial(color), "Initial supplied value preserved");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6e1cf + color * 2)), stock.ResolveInitial(color), "Stock instance immutable");
            }
            for (int row = 0; row < 15; row++) for (int color = 0; color < 14; color++)
                AssertEqual(Pack(original.Reveal[row][color]), selected.ResolveReveal(row, color), "Reveal words");
            {
                var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { NorfairRidleyColors = selected };
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = artworkBosses };
                var guard = NewProjectileCompositionForbiddenBus();
                enemies.VerificationServices().IsAreaBossDefeated = () => false;
                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                slot.Bind(EnemyDefinitionId.Ridley, slot.Definition);
                new Action<RoomEnemySlot>(enemies.RidleyBehavior.Combat.InitializeNorfairRidley)(slot);
                ushort[] shown = RidleyLaidColours.Of(enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the initialized Ridley."),
                    RidleySheetShades.NorfairSprites(selected), RidleySheetShades.NorfairRoom(selected), 0x1234);
                for (int color = 0; color < 256; color++)
                    AssertEqual(color is >= 160 and < 192 ? Pack(document.Initial[color - 160])
                        : color is >= 113 and <= 127 or >= 241 and <= 255 ? (ushort)0 : (ushort)0x1234,
                        shown[color], "Actual initializer copies both targets then clears separate reveal ranges");
            }
        }
        foreach (int color in new[] { -1, 32, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveInitial(color), "Initial existing array-domain contract");
        Console.WriteLine("Norfair initial:32 installed native words,the hash,full copy,the real initializer and210 reveal words pass.");
    }

    /// <summary>Checks Botwoon color progression and installed CGRAM slots against native rows.</summary>
    internal static void VerifyLookupStream4BotwoonColors(ISnesAddressSpace rom)
    {
        byte[] original = BotwoonColorExtractor.Extract(rom);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stock = BotwoonColorFile.Read(new MemoryStream(original));
        for (int color = 0; color < 16; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb39319 + color * 2)), stock.HealthColor(0, color), "native initial/header alias remains exact");
        {
            var document = RequiredJson.Deserialize<BotwoonColorDocument>(original, options);
            var catalog = BotwoonColorFile.Read(new MemoryStream(BotwoonColorFile.Write(document)));
            ushort Expected(int band, int color) => CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb3971b + (band * 16 + color) * 2));
            ushort Native(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            string identity = SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
                content.AppendWordFrames("health", document.Health.Select(row => row.Select(Native).ToArray()).ToArray()));
            AssertEqual(identity, catalog.ContentIdentity, "Botwoon selected identity");
            for (int band = 0; band < 8; band++) for (int color = 0; color < 16; color++)
            {
                AssertEqual(Expected(band, color), catalog.HealthColor(band, color), "native endpoint, intermediate and copied metadata channels");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb3971b + (band * 16 + color) * 2)), stock.HealthColor(band, color), "stock owner remains immutable");
            }
            {
                var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                var artworkBosses = new BossSourceArtwork { BotwoonColors = catalog };
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artworkBosses };
                var guard = NewBotwoonColorReadGuard(rom);
                Action<RoomEnemySlot, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonHealthBand;
                RoomEnemySlot head = enemies.SlotTable.Slots[0];
                var state = new BotwoonEnemyState();
                // Botwoon's sprites are pre-coloured in each band's colours over his row, the seventh (#1512).
                IReadOnlyList<SheetShade> botwoonShades = BossSheetShades.OwnRow(artworkBosses, EnemyDefinitionId.Botwoon, 7 * 16);
                ushort[] Shown() => BossShadeColours.Laid(state.BodyShading.Shades, botwoonShades, BossShadeColours.Sprites, 0x1234);
                for (int band = 0; band < 8; band++)
                {
                    state.HealthBandsPassed = band;
                    head.Health = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb3981b + band * 2));
                    ushort[] before = Shown();
                    update(head, state);
                    AssertEqual(band, state.HealthBandsPassed, "exact threshold does not advance");
                    ushort[] held = Shown();
                    for (int color = 0; color < 256; color++) AssertEqual(before[color], held[color], "exact threshold preserves CGRAM");
                    head.Health--;
                    update(head, state);
                    AssertEqual(band + 1, state.HealthBandsPassed, "below threshold advances exactly one band");
                    ushort[] cgram = Shown();
                    for (int color = 0; color < 256; color++)
                        AssertEqual(color < 240 ? (ushort)0x1234 : Expected(band, color - 240), cgram[color], "actual producer copies all16 including transparent metadata and preserves neighbors");
                }
                ushort[] completed = Shown(); update(head, state);
                ushort[] after = Shown();
                for (int color = 0; color < 256; color++) AssertEqual(completed[color], after[color], "completed palette remains unchanged");
                AssertEqual(0, guard.ForbiddenReadAttempts, "actual palette producer performs no native palette reads");
            }
        }
        foreach (int invalid in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.HealthColor(invalid, 0), "Botwoon health band domain");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.HealthColor(0, invalid), "Botwoon health color domain");
        Console.WriteLine("Botwoon installed colours:128 native outputs,16 initial aliases,the hash identity,actual threshold/copy/completed calls and native-read guard pass.");
    }

    /// <summary>Checks Oum instruction-list boundaries and pointer order against the cartridge.</summary>
    internal static void VerifyLookupStream4OumListLayout(ISnesAddressSpace rom)
    {
        var nativeFrames = new Dictionary<ushort, ushort>();
        for (int frame = 0xcb87; frame <= 0xcca9; frame += 10)
            nativeFrames.Add((ushort)frame, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | (frame + 8))));
        AssertEqual(nativeFrames.Count, MaridiaLargeSnailCollisionDefinitionsConstants.FramePointers.Length, "Oum calculated frame count follows original extent");
        int ordinal = 0;
        foreach (ushort expectedFrame in nativeFrames.Keys)
            AssertEqual(expectedFrame, MaridiaLargeSnailCollisionDefinitionsConstants.FramePointers[ordinal++], "Oum calculated frame indexing preserves native order");
        foreach (int invalid in new[] { int.MinValue, -1, nativeFrames.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = MaridiaLargeSnailCollisionDefinitionsConstants.FramePointers[invalid], "Oum calculated frame view preserves index bounds");
        var selectedLists = new HashSet<ushort>();
        int rectangles = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort frame = (ushort)raw;
            bool valid = nativeFrames.TryGetValue(frame, out ushort expected);
            AssertEqual(valid, MaridiaLargeSnailCollisionDefinitions.HasFrame(frame), "Oum frame domain remains exact");
            if (!valid)
            {
                AssertThrows<InvalidDataException>(() => MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame), "Oum rejects every non-frame pointer");
                continue;
            }
            ushort actual = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(expected, actual, "Calculated Oum record identity matches original frame operand");
            selectedLists.Add(actual);
            var boxes = MaridiaLargeSnailCollisionDefinitions.HitboxesAt(actual);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | expected)), boxes.Length, "Calculated Oum pointer reaches the native rectangle count");
            for (int index = 0; index < boxes.Length; index++)
            {
                int pointer = 0xa20000 | (expected + 2 + index * 12);
                var box = boxes[index];
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer)), box.Left, "Oum selected left bound");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 2))), box.Top, "Oum selected top bound");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 4))), box.Right, "Oum selected right bound");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 6))), box.Bottom, "Oum selected bottom bound");
                AssertEqual(EnemyReactionAccess.Of(0xa2, CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 8)), shot: false), (EnemyReaction?)box.Touch, "Oum selected touch callback");
                AssertEqual(EnemyReactionAccess.Of(0xa2, CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 10)), shot: true), (EnemyReaction?)box.Shot, "Oum selected shot callback");
                rectangles++;
            }
        }
        AssertTrue(nativeFrames.Keys.SequenceEqual(MaridiaLargeSnailCollisionDefinitionsConstants.FramePointers.ToArray()), "Oum frame enumeration preserves native order");
        AssertTrue(selectedLists.SetEquals(MaridiaLargeSnailCollisionDefinitions.HitboxPointers), "Oum list calculation covers the original independent payload exactly");
        Console.WriteLine($"Oum calculated list identities: {nativeFrames.Count} frames, {rectangles} native rectangles and complete ushort rejection domain pass.");
    }

    /// <summary>Checks the authored breakup phase sequence against native instruction order.</summary>
    internal static void VerifyLookupStream4BreakupOrder(ISnesAddressSpace rom)
    {
        ushort[] expected = Enumerable.Range(0, 12).Select(index =>
            CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6c933 + index * 7) + 12)))).ToArray();
        foreach (int freeSlots in new[] { 12, 2, 0 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            foreach (RoomEnemySlot slot in enemies.SlotTable.Slots) slot.Bind((EnemyDefinitionId)0xffff, slot.Definition);
            for (int index = 1; index <= freeSlots; index++) enemies.SlotTable.Slots[index].Clear();
            int randomCalls = 0;
            enemies.VerificationServices().NextRandom = () => (ushort)(0x100 + 0x10 * randomCalls++);
            Action<RidleyEnemyState> spawn = enemies.RidleyBehavior.Death.SpawnNorfairRidleyBreakupActors;
            var state = new RidleyEnemyState
            {
                TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                {
                    Position = new Position16<RoomPixels>((ushort)(100 + index), (ushort)(200 + index)),
                }).ToArray(),
            };
            // Each part's initialization AI starts it from the encounter's shared Ridley state.
            enemies.RidleyEncounter.Ridley = state;
            if (freeSlots < expected.Length)
                AssertThrows<InvalidOperationException>(() => spawn(state), "Breakup preserves partial allocation before pool exhaustion");
            else spawn(state);
            AssertTrue(state.DeathBreakupSpawned, "Breakup commits its one-shot guard before allocation");
            AssertEqual(freeSlots, randomCalls, "Breakup consumes RNG once per successfully allocated fragment only");
            for (int index = 0; index < freeSlots; index++)
            {
                RoomEnemySlot fragment = enemies.SlotTable.Slots[index + 1];
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.RidleyExplosion, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(expected[index], 0)), fragment.Spawn.Population.SpawnSettings, "Actual breakup slot order follows original native spawn calls");
                RidleyExplosionFragmentState fragmentState = fragment.SpeciesState<RidleyExplosionFragmentState>()
                    ?? throw new InvalidOperationException("A spawned fragment has no state.");
                AssertEqual((ushort)((0x100 + 0x10 * index) & 0x130), fragmentState.Velocity.X.RawValue, "Actual breakup maps each random draw to its original fragment");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6c6ce + expected[index])), fragmentState.Lifetime, "Actual breakup preserves each unresolved native lifetime");
            }
            spawn(state);
            AssertEqual(freeSlots, randomCalls, "Breakup retry after success or failure does not repeat allocation or RNG");
        }
        Console.WriteLine("Ridley ordered spawning: native twelve-call order, exact RNG assignment, partial/full pool exhaustion and one-shot retry behavior pass.");
    }

    /// <summary>Checks the installed beam tiles and their VRAM words against the native uploads.</summary>
    internal static void VerifyLookupStream4BeamTileGeometry(ISnesAddressSpace rom)
    {
        var files = BeamTileExtractor.Extract(rom);
        var palettes = BeamPaletteFile.Read(new MemoryStream(BeamPaletteExtractor.Extract(rom)));
        var catalog = BeamTileCatalog.Load(files, palettes);
        for (int selection = 0; selection < 12; selection++)
        {
            int sourceAddress = 0x9a0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90c3b1 + selection * 2));
            byte[] native = Enumerable.Range(0, 256).Select(offset => rom.ReadCartridgeByte(sourceAddress + offset)).ToArray();
            byte[] png = files[BeamTileAtlasDefinitions.FileName(SamusBeamCombinationTableOrder.FromTableIndex(selection))];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), SamusBeamCombinationTableOrder.FromTableIndex(selection));
            AssertTrue(native.SequenceEqual(atlas.Characters.Span), "Installed DMA view matches all256 original bytes per beam selection");
            var directVram = new SnesVram();
            atlas.LoadTo(directVram);
            AssertTrue(native.SequenceEqual(directVram.Bytes.Slice(0x6300 * 2, 256)), "Actual direct beam upload uses the installed native bytes");
            VerifyQueued(catalog, selection, native);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => BeamTileAtlas.Load(new MemoryStream(), SamusBeamCombinationTableOrder.FromTableIndex(invalid)), "Beam atlas rejects unsupported selection before decoding PNG");
        Console.WriteLine("Beam tiles: twelve native256-byte uploads and the bound projectile page pass.");

        // The projectile page the selection binds shows the calculated characters where `$90:AC8D`'s DMA put them
        // (native immediates: $0100 bytes at VMADD $6300), and leaves the characters around them alone.
        void VerifyQueued(BeamTileCatalog selected, int selection, byte[] expected)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x90ac9f), expected.Length, "Bound beam characters span native immediate0100");
            int first = (CartridgeImportSource.Require(rom).ReadCartridgeWord(0x90acb6) - 0x6000) / 16;
            var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, selected, InstalledFixtures.ImportProjectiles.TrailTiles);
            ReadOnlySpan<byte> page = pages.For(SamusEquipmentWordCodec.FromBeamWord(checked((ushort)selection))).Characters;
            AssertTrue(expected.SequenceEqual(page.Slice(first * SpriteArtwork.CharacterByteCount, expected.Length).ToArray()),
                "The bound page shows the current calculated pixels at native destination6300 without cartridge reads");
            AssertEqual((byte)0, page[first * SpriteArtwork.CharacterByteCount - 1], "Beam characters leave the preceding character alone");
        }
    }

    /// <summary>Checks Ceres starting palette state and installed CGRAM colors.</summary>
    internal static void VerifyLookupStream4CeresStart(ISnesAddressSpace rom)
    {
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        for (int color = 0; color < 32; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xA6E16F + color * 2)), stock.ResolveStart(color), "Complete native additional palette");
        for (int color = 0; color < 16; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xA6F4EC + color * 2)), stock.ResolveStart(color), "Native normal door material occurrence");
        for (int color = 0; color < 15; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xA6E1F1 + color * 2)), stock.ResolveStart(17 + color), "Native Baby initial identity");
        {
            IReadOnlyList<SheetShade> shades = RidleySheetShades.CeresSprites(stock);
            ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0x1234), shades, RidleyShades.CeresStart, BossShadeColours.Sprites);
            for (int color = 0; color < 256; color++)
            {
                ushort expected = color is >= 160 and < 192
                    ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xA6E16F + (color - 160) * 2))
                    : (ushort)0x1234;
                AssertEqual(expected, cgram[color], "Exact full start copy and untouched neighbors");
            }
            BossShadeColours.Lay(cgram, shades, RidleyShades.Baby(CeresBabyShade.Resting), BossShadeColours.Sprites);
            for (int color = 0; color < 15; color++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xA6E1F1 + color * 2)),
                    cgram[CeresRidleyShadeDefinitionsConstants.BabyCgramIndex + color], "Baby resting colours over the start");
        }
        foreach (int color in new[] { -1, 32, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveStart(color), "Start domain");
        Console.WriteLine("Ceres start:32 native words,16 door/15 Baby occurrences,full start and Baby copies and bounds pass.");
    }

    /// <summary>Checks Ceres Baby palette phases and installed CGRAM colors.</summary>
    internal static void VerifyLookupStream4CeresBaby(ISnesAddressSpace rom)
    {
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        ushort Native(int row, int color)
        {
            int a = 0xa6e1f1 + (row * 15 + color) * 2;
            return CartridgeImportSource.Require(rom).ReadCartridgeWord(a);
        }
        // Each shade shows the native Baby row of the same rank: resting, then the horizontal, round and vertical poses.
        CeresBabyShade[] shadeOfRow = [CeresBabyShade.Resting, CeresBabyShade.HorizontalSquish, CeresBabyShade.Round, CeresBabyShade.VerticalSquish];
        for (int row = 0; row < 4; row++) for (int color = 0; color < 15; color++)
            AssertEqual(Native(row, color), stock.ResolveBaby(shadeOfRow[row], color), "All60 installed native Baby colors");
        for (int row = 0; row < 4; row++)
        {
            ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0x1234), RidleySheetShades.CeresSprites(stock), RidleyShades.Baby(shadeOfRow[row]), BossShadeColours.Sprites);
            for (int color = 0; color < 256; color++)
                AssertEqual(color is >= 177 and < 192 ? Native(row, color - 177) : (ushort)0x1234, cgram[color], "Baby actual copy and neighbors");
        }
        {
            IReadOnlyList<SheetShade> babySprites = RidleySheetShades.CeresSprites(stock);
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            Func<RidleyEnemyState, ushort> advance = baby => EnemyFrameCartridgeLookupTooling.Word(enemies.CeresRidleyBehavior.BabyScene.AdvanceCeresBabyDrawAnimation(baby));
            for (int phase = 0; phase < 12; phase++)
            {
                int cursor = 0xbf5d + phase * 8;
                int row = (CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | (cursor + 2)) - 0xe1f1) / 30;
                // The expressive loop's palette command for this phase is beat 1 + 2 * phase, its pose the next beat.
                var state = new RidleyEnemyState { BabyPlayback = new AnimationPlayback<CeresBabySegment>(CeresBabyAnimations.StartingWith(CeresBabySegment.ExpressiveLoop), new AnimationPlace(0, 1 + 2 * phase)), BabyFrameHold = 1, BabyVerticalVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1) };
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | (cursor + 6)), advance(state), "Actual palette instruction selects matching native pose");
                AssertEqual(new AnimationPlayback<CeresBabySegment>(CeresBabyAnimations.StartingWith(CeresBabySegment.ExpressiveLoop), new AnimationPlace(0, 3 + 2 * phase)), state.BabyPlayback, "Palette instruction runs on to its pose");
                AssertEqual(1, state.BabyFrameHold, "The new pose starts its elapsed count at one");
                ushort[] shown = RidleyLaidColours.Of(state, babySprites, [], 0);
                for (int color = 0; color < 15; color++)
                    AssertEqual(Native(row, color), shown[177 + color], "Actual pose-selected palette");
            }
        }
        foreach (int color in new[] { -1, 15, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBaby(CeresBabyShade.Resting, color), "Baby color domain");
        Console.WriteLine("Ceres Baby:60 native colors,actual15-color copies,12 native interpreter shape selections pass.");
    }

    /// <summary>Checks shared Ceres retreat colors and phase ownership against cartridge rows.</summary>
    internal static void VerifyLookupStream4CeresRetreatShared(ISnesAddressSpace rom)
    {
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        var floor = CeresRidleyMode7ColorFile.Read(new MemoryStream(CeresRidleyMode7ColorExtractor.Extract(rom)));
        for (int color = 0; color < 8; color++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6aa01 + color * 2)), stock.ResolveRetreatShared(color), "Native shared retreat colors");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6e171 + color * 2)), stock.ResolveRetreatShared(color), "Native startup material identity");
        }
        {
            var selected = stock;
            ushort[] cgram = BossShadeColours.Filled(0x1234);
            IReadOnlyList<SheetShade> retreatRoom = RidleySheetShades.CeresRoom(selected, floor);
            BossShadeColours.Lay(cgram, retreatRoom, RidleyShades.RetreatRoom, BossShadeColours.Room);
            BossShadeColours.Lay(cgram, retreatRoom, RidleyShades.RetreatShared, BossShadeColours.Room);
            // Ridley's sprites keep their own colours through the retreat (#1544): only the room's colours change.
            for (int color = 0; color < 256; color++)
            {
                int sharedColor = color is >= 33 and < 41 ? color - 33 : -1;
                ushort expected = sharedColor >= 0 ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6aa01 + sharedColor * 2))
                    : color is >= 81 and < 96 ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6a9e3 + (color - 81) * 2)) : (ushort)0x1234;
                AssertEqual(expected, cgram[color], "Exact retreat room colours");
            }
        }
        foreach (int color in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveRetreatShared(color), "Retreat shared domain");
        Console.WriteLine("Ceres shared retreat:8 native identities,exact dual-domain copies and bounds pass.");
    }

    /// <summary>Checks Ceres zoom and retreat palette phases in their native order.</summary>
    internal static void VerifyLookupStream4CeresZoomAndRetreat(ISnesAddressSpace rom)
    {
        byte[] zoomJson = CeresRidleyMode7ColorExtractor.Extract(rom);
        var zoomStock = CeresRidleyMode7ColorFile.Read(new MemoryStream(zoomJson));
        var ceresStock = CeresRidleyColorFile.Read(new MemoryStream(CeresRidleyColorExtractor.Extract(rom)));
        {
            var selected = zoomStock;
            var ridley = new RidleyEnemyState();
            IReadOnlyList<SheetShade> zoomRoom = RidleySheetShades.CeresRoom(ceresStock, selected);
            Action<ushort> apply = appliedZoom => CeresRidleyGetaway.UpdateCeresRidleyFloorZoomColours(ridley, FloorZoom.FromTwoHundredFiftySixths(appliedZoom));
            for (int row = 0; row < 9; row++)
            {
                apply((ushort)(row * 256 + 255));
                ushort[] cgram = RidleyLaidColours.Of(ridley, [], zoomRoom, 0x1234);
                for (int color = 0; color < 256; color++)
                {
                    ushort expected = color is >= 81 and < 96
                        ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b107 + row * 32 + (color - 81) * 2))
                        : (ushort)0x1234;
                    AssertEqual(expected, cgram[color], "Actual native high-byte zoom selection");
                    if (color is >= 81 and < 96) AssertEqual(expected, selected.Resolve(row, color - 81), "Installed zoom colour matches actual copy");
                }
            }
        }
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        {
            var selected = stock;
            ushort[] cgram = BossShadeColours.Filled(0x1234);
            IReadOnlyList<SheetShade> retreatRoom = RidleySheetShades.CeresRoom(selected, zoomStock);
            BossShadeColours.Lay(cgram, retreatRoom, RidleyShades.RetreatRoom, BossShadeColours.Room);
            BossShadeColours.Lay(cgram, retreatRoom, RidleyShades.RetreatShared, BossShadeColours.Room);
            for (int color = 0; color < 256; color++)
            {
                int shared = color is >= 33 and < 41 ? color - 33 : -1;
                ushort expected = color is >= 81 and < 96
                    ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6a9e3 + (color - 81) * 2))
                    : shared >= 0 ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6aa01 + shared * 2)) : (ushort)0x1234;
                AssertEqual(expected, cgram[color], "Retreat near-black source and exact room destinations");
            }
        }
        foreach (int invalid in new[] { -1, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => zoomStock.Resolve(invalid, 0), "Zoom row domain");
            AssertThrows<ArgumentOutOfRangeException>(() => zoomStock.Resolve(0, invalid), "Zoom color domain");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveRetreatBg(invalid), "Retreat color domain");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => zoomStock.Resolve(9, 0), "Zoom upper row bound");
        AssertThrows<ArgumentOutOfRangeException>(() => zoomStock.Resolve(0, 15), "Zoom upper color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveRetreatBg(15), "Retreat upper color bound");
        Console.WriteLine("Ceres zoom/retreat:150 native words,actual zoom selections/retreat copies and domains pass.");
    }

    /// <summary>Checks Ceres health-dependent palette phases and installed CGRAM output.</summary>
    internal static void VerifyLookupStream4CeresHealth(ISnesAddressSpace rom)
    {
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var stock = CeresRidleyColorFile.Read(new MemoryStream(json));
        ushort Native(int row, int color)
        {
            int a = 0xa6e46a + (row * 14 + color) * 2;
            return CartridgeImportSource.Require(rom).ReadCartridgeWord(a);
        }
        {
            IReadOnlyList<SheetShade> shades = RidleySheetShades.CeresSprites(stock);
            for (int row = 0; row < 3; row++)
            {
                ushort[] cgram = BossShadeColours.Lay(BossShadeColours.Filled(0x1234), shades, RidleyShades.Health(row), BossShadeColours.Sprites);
                for (int color = 0; color < 256; color++)
                {
                    ushort expected = color is >= 241 and < 255 ? Native(row, color - 241) : (ushort)0x1234;
                    AssertEqual(expected, cgram[color], "Health copy and neighbors");
                }
            }
        }
        {
            var selected = stock;
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            IReadOnlyList<SheetShade> healthShades = RidleySheetShades.CeresSprites(selected);
            Action<RoomEnemySlot, RidleyEnemyState> norfair = RidleyInteractions.UpdateNorfairRidleyHealthBand;
            Action<RidleyEnemyState> ceres = CeresRidleyHits.UpdateCeresRidleyHealthBand;
            void Check(RidleyEnemyState shown, int row)
            {
                ushort[] cgram = RidleyLaidColours.Of(shown, healthShades, [], 0x1234);
                for (int color = 0; color < 256; color++)
                {
                    ushort expected = row >= 0 && color is >= 241 and < 255 ? Native(row, color - 241) : (ushort)0x1234;
                    AssertEqual(expected, cgram[color], "Actual encounter health/shot selection preserves installed paint and exact copy");
                }
            }
            foreach (ushort health in new ushort[] { 9000, 8999, 5400, 5399, 1800, 1799 })
            {
                enemies.SlotTable.Slots[0].Health = health;
                var state = new RidleyEnemyState(); norfair(enemies.SlotTable.Slots[0], state);
                int row = health >= 9000 ? -1 : health >= 5400 ? 0 : health >= 1800 ? 1 : 2;
                AssertEqual((ushort)(row + 1), state.HealthStage, "Norfair health stage remains exact"); Check(state, row);
            }
            foreach (RidleyFightMode fight in new[] { RidleyFightMode.NotFighting, RidleyFightMode.Fighting }) foreach (ushort hits in new ushort[] { 49, 50, 69, 70, 89, 90 })
            {
                var hit = new RidleyEnemyState { FightMode = fight, HitCounter = hits };
                ceres(hit);
                Check(hit, fight == RidleyFightMode.NotFighting || hits < 50 ? -1 : hits < 70 ? 0 : 2);
            }
        }
        foreach (int row in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(row, 0), "Health row bounds");
        foreach (int color in new[] { -1, 14, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(0, color), "Health color bounds");
        Console.WriteLine("Ridley health:42 native words,actual copies,18 actual encounter selections and bounds/null pass.");
    }

    /// <summary>Checks HUD anchor coordinates and cell placement against native layout data.</summary>
    internal static void VerifyLookupStream4HudAnchors(ISnesAddressSpace rom)
    {
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<GameplayHudPresentationDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        GameplayHudArtwork stock = GameplayHudPresentationFile.Read(new MemoryStream(json));
        for (int tank = 0; tank < 14; tank++)
        {
            int native = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x809cce + tank * 2));
            AssertEqual(native / 2, GameplayHudStockCells.EnergyTankCell(tank), "native HUD tank offset");
            AssertEqual(native / 2, document.EnergyTanks.Anchors[tank].Y * 32 + document.EnergyTanks.Anchors[tank].X,
                "extracted native HUD tank anchor");
        }
        for (int item = 0; item < 5; item++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x809d6e + item * 2) / 2, GameplayHudStockCells.ItemCell(item), "native HUD item offset");
        for (int cell = 0; cell < 6; cell++)
        {
            int native = (CartridgeImportSource.Require(rom).ReadCartridgeWord((0x809b65 + cell * 7)) - 0xc608) / 2;
            AssertEqual(native, GameplayHudStockCells.AutoReserveCellIndex(cell), "native AUTO store destination");
        }
        ushort[] tiles = new ushort[96];
        stock.ApplyEnergy(tiles, 100, 1400);
        for (int tank = 0; tank < 14; tank++)
            AssertEqual((tank == 0 ? stock.FilledEnergyTank : stock.EmptyEnergyTank).PackedWord,
                tiles[CartridgeImportSource.Require(rom).ReadCartridgeWord((0x809cce + tank * 2)) / 2], "native tank draw destination");
        stock.ApplyAutoReserve(tiles, containsEnergy: true);
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x80998b + cell * 2)), tiles[GameplayHudStockCells.AutoReserveCellIndex(cell)], "native AUTO draw destination");
        stock.ClearAutoReserve(tiles);
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(stock.Blank.PackedWord, tiles[GameplayHudStockCells.AutoReserveCellIndex(cell)], "native AUTO clear destination");
        document.AutoReserve.Anchors[0] = document.AutoReserve.Anchors[1];
        AssertThrows<InvalidDataException>(() =>
        {
            using var written = new MemoryStream();
            GameplayHudPresentationFile.Write(written, document);
            _ = GameplayHudPresentationFile.Read(new MemoryStream(written.ToArray(), writable: false));
        }, "HUD anchor compilation rejects overlap on read");
        foreach (int invalid in new[] { -1, 14, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameplayHudStockCells.EnergyTankCell(invalid), "tank exact domain");
        foreach (int invalid in new[] { -1, 5, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameplayHudStockCells.ItemCell(invalid), "item exact domain");
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameplayHudStockCells.AutoReserveCellIndex(invalid), "AUTO exact domain");
    }

    /// <summary>Runs the beam geometry verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream-4-beam-geometry")]
    internal static int LookupStream4BeamGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream4BeamTileGeometry), () => VerifyLookupStream4BeamTileGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the auto-reserve cell verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-hud-auto-cells")]
    internal static int LookupStream4HudAutoCellsCommand()
    {
        Suite(nameof(VerifyLookupStream4HudAutoCells), () => VerifyLookupStream4HudAutoCells(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the HUD anchor verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-hud-anchors")]
    internal static int LookupStream4HudAnchorsCommand()
    {
        Suite(nameof(VerifyLookupStream4HudAnchors), () => VerifyLookupStream4HudAnchors(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the HUD digit verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-hud-digits")]
    internal static int LookupStream4HudDigitsCommand()
    {
        Suite(nameof(VerifyLookupStream4HudDigits), () => VerifyLookupStream4HudDigits(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the title sprite identity verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-title-identities")]
    internal static int LookupStream4TitleIdentitiesCommand()
    {
        Suite(nameof(VerifyTitleSpriteIdentities), () => VerifyTitleSpriteIdentities(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the title card layout verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-title-card")]
    internal static int LookupStream4TitleCardCommand()
    {
        Suite(nameof(VerifyTitleCardLayout), () => VerifyTitleCardLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the room sprite dispatch verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-sprite-dispatch")]
    internal static int LookupStream4SpriteDispatchCommand()
    {
        Suite(nameof(VerifyRoomSpriteDispatch), () => VerifyRoomSpriteDispatch(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Fly escape frame verification against a pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream4-fly-escape")]
    internal static int LookupStream4FlyEscapeCommand()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFlyFrameIdentities), () => VerifyFlyFrameIdentities(source));
        Suite(nameof(VerifyZebesEscapeExplosionDefinitions), () => VerifyZebesEscapeExplosionDefinitions(source));
        return 0;
    }

    /// <summary>Every shade Draygon's colours can lay over his body layer: his material colours, their white flash and each health band.</summary>
    private static readonly SuperMetroid.Core.Assets.PictureShade[] DraygonRoomShades =
    [
        SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonShades.Background,
        SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonShades.BackgroundWhite,
        .. Enumerable.Range(0, 8).Select(SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonShades.Health),
    ];

    /// <summary>The shading Draygon's colours give his body layer (#1512).</summary>
    private static PictureShading DraygonRoomShading(DraygonColours colours) =>
        GameplayShadingProbe.ShadingOf(colours, GameplayPictures.BossBody, DraygonRoomShades);

    /// <summary>
    /// Lays the shading Draygon's colours publish over <paramref name="cgram"/> as <paramref name="catalog"/>'s imported
    /// shades colour it (#1512): his room's shades over the background colours, then his sprites' over the sprite colours,
    /// each in the order he shows them.
    /// </summary>
    private static void LayDraygonShading(SnesCgram cgram, SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork.DraygonColorCatalog catalog, DraygonColours colours)
    {
        foreach (SuperMetroid.Core.Assets.PictureShade shade in DraygonRoomShading(colours).Shades)
            catalog.ShowImported(cgram, shade);
        foreach (SuperMetroid.Core.Assets.PictureShade shade in colours.Sprites.Shades)
            catalog.ShowImported(cgram, shade);
    }
}
