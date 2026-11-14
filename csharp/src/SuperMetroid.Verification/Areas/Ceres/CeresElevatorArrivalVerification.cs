using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Ceres elevator arrival graphics indexing and projectile OAM against native definitions.</summary>
internal static class CeresElevatorArrivalVerification
{
    /// <summary>
    /// Verifies the easy-to-miss graphics-index write in the shared Ceres elevator
    /// initializer at <c>$86:A301</c>.
    /// </summary>
    internal static void VerifyCeresElevatorArrivalGraphicsIndex()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledDefinitions), () => VerifyCompiledDefinitions(rom));
        var memory = new TestAddressSpace();
        EnemyProjectileSpritemapCatalog artwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidOperationException("Verification requires the installed projectile compositions.");
        var bus = NewCeresArrivalDefinitionReadGuard(memory);

        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0000)) };
        var arrival = new CeresElevatorArrivalState(samus);
        arrival.Step(samus);

        // With tile base zero, each actor shows its composition at tile base zero in the parts' own palettes; leaked
        // enemy graphics would add a base tile and select other colours. A new game clears the concealer's colour before
        // the arrival spawns (#1512), so both actors publish the shade that shows it black until they delete themselves.
        var noGraphics = SpriteGraphics.SharedIn(PictureShading.Shaded(CeresElevatorShades.ConcealerCleared));
        VerificationAssertions.AssertTrue(arrival.pad.Graphics == noGraphics && arrival.platform.Graphics == noGraphics,
            "Ceres arrival actors draw with tile base zero, the concealer's colour cleared");
        var oam = new OamBuffer();
        oam.BeginFrame();
        arrival.DrawAsOam(oam, SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevDefault),
            camera: Position16<RoomPixels>.Zero);
        var expected = new OamBuffer();
        expected.BeginFrame();
        foreach (CeresElevatorArrivalState.CeresElevatorProjectile actor in new[] { arrival.pad, arrival.platform })
            expected.AddEnemySpritemap(artwork.Get(CeresElevatorArrivalAnimations.Frame(actor.Pose
                    ?? throw new InvalidOperationException("Verification requires a shown arrival pose."))).Span,
                actor.Position.ToScreen(Position16<RoomPixels>.Zero).ToSpritePosition(), 0, 0);
        VerificationAssertions.AssertTrue(expected.NextByteOffset > 0 && expected.ImageEquals(oam),
            "Ceres pad and level-data concealer draw their compositions at tile base zero in their own palettes");

        Suite(nameof(VerifyConcealerShade), () => VerifyConcealerShade(rom, arrival));

        int frames = 1;
        while (!arrival.IsComplete && frames++ < 200)
            arrival.Step(samus);
        VerificationAssertions.AssertTrue(arrival.IsComplete,
            "Ceres arrival's compiled instruction programs delete both projectiles");
        VerificationAssertions.AssertEqual(
            CeresElevatorArrivalDefinitions.LandingSamusY,
            samus.Position.Y.Whole,
            "Ceres arrival retains the native landing coordinate");

        Console.WriteLine(
            "  Ceres elevator: definitions/programs match cartridge data, run without bank-$86 reads, and retain native graphics/landing behavior.");
    }

    /// <summary>
    /// Checks the concealer's cleared colour against the cartridge (#1512): a new game's set-up stores zero into one sprite
    /// colour before the arrival spawns ($82:812D <c>LDA #$0000</c>, $82:8130 <c>STA.L</c> into the colour buffer), and
    /// every arrival draw's picture carries the concealer's shade, which shows exactly that sprite colour black. Each draw
    /// is held to what its own picture shows: the shade recolours, of the picture's colours, exactly those composed as that
    /// sprite colour, and the concealer's draw shows it. A sheet holding the pad apart from the concealer, as the common art
    /// held once holds it (#1544), has no colour of the concealer's for the shade to recolour.
    /// </summary>
    private static void VerifyConcealerShade(ISnesAddressSpace rom, CeresElevatorArrivalState arrival)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        VerificationAssertions.AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x82812d), "the new-game clear loads an immediate colour");
        VerificationAssertions.AssertEqual((ushort)0, cartridge.ReadCartridgeWord(0x82812e), "the new-game clear's colour is black");
        VerificationAssertions.AssertEqual((byte)0x8f, rom.ReadCartridgeByte(0x828130), "the new-game clear stores a long address");
        VerificationAssertions.AssertEqual((byte)0x7e, rom.ReadCartridgeByte(0x828133), "the new-game clear stores into work RAM");
        int spriteColour = (cartridge.ReadCartridgeWord(0x828131) - NativeSnapshotMemory.PaletteBuffer) / sizeof(ushort) - SpriteColoursFirst;
        // On the cartridge the concealer's shade clears one of the sprite colours, so in the sheet its set composes it shows black
        // exactly the colour keys whose pixels were composed as that colour. The sheet composed in-process knows the index each
        // pixel was composed as (#1544, #1633).
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.Of(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevDefault);
        SpriteImage[] composed = [.. EnemySheetFixtures.ComposedSpawnedSheets(set).Select(sheet => sheet.Image)
            .Where(image => image.Shades.ContainsKey(CeresElevatorShades.ConcealerCleared))];
        VerificationAssertions.AssertTrue(composed.Length > 0, "the Ceres elevator's graphics set composes a spawned sheet pre-coloured in the concealer's shade");
        foreach (SpriteImage sheet in composed)
        {
            ColourKey[] cleared = [.. sheet.ColourKeys
                .Where(key => SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ComposedIndicesOf(sheet, key).Contains(spriteColour))
                .OrderBy(Spelled, StringComparer.Ordinal)];
            KeyedSubstitution[] entries = [.. sheet.Shade(CeresElevatorShades.ConcealerCleared).Substitutions];
            ColourKey[] recoloured = [.. entries.Select(entry => entry.Replaced).OrderBy(Spelled, StringComparer.Ordinal)];
            VerificationAssertions.AssertTrue(cleared.Length > 0 && recoloured.SequenceEqual(cleared) && entries.All(entry => entry.Shown == RgbColor.Black),
                $"the concealer's shade shows exactly the cartridge's cleared sprite colour {spriteColour} of {sheet.Name} black: the keys composed "
                + $"as it are [{string.Join(", ", cleared)}], and it recolours [{string.Join(", ", entries.Select(entry => entry.Replaced))}] to "
                + $"[{string.Join(", ", entries.Select(entry => entry.Shown))}]");
        }
        // The arrival draws the installed pictures, some of whose pixels the installed concealer's shade shows black.
        var frame = new SpriteFrameBuffer();
        frame.BeginFrame();
        arrival.Draw(frame, SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevDefault),
            Position16<RoomPixels>.Zero);
        frame.FinalizeFrame();
        VerificationAssertions.AssertTrue(frame.Draws.Length > 0, "the arrival draws while it runs");
        bool showsCleared = false;
        foreach (SpriteDraw draw in frame.Draws)
        {
            SpriteSheetFrame picture = ((ImageSpriteDraw)draw).Image;
            SpriteImage sheet = picture.Sheet;
            HashSet<ColourKey> blackened = [.. sheet.Shade(CeresElevatorShades.ConcealerCleared).Substitutions
                .Where(entry => entry.Shown == RgbColor.Black).Select(entry => entry.Replaced)];
            showsCleared |= ShownKeys(picture).Any(blackened.Contains);
        }
        VerificationAssertions.AssertTrue(showsCleared,
            $"an arrival draw shows the cartridge's cleared sprite colour {spriteColour}, the concealer's solid pixels");
    }

    /// <summary>A colour key spelled for ordering keys in a failure.</summary>
    private static string Spelled(ColourKey key) => key.ToString();

    /// <summary>The colour keys <paramref name="picture"/>'s opaque pixels show.</summary>
    private static HashSet<ColourKey> ShownKeys(SpriteSheetFrame picture)
    {
        SpriteImage sheet = picture.Sheet;
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        int width = sheet.Size.X;
        var shown = new HashSet<ColourKey>();
        for (int row = 0; row < picture.Size.Y; row++)
        {
            int line = (picture.Corner.Y + row) * width + picture.Corner.X;
            for (int column = 0; column < picture.Size.X; column++)
            {
                if (pixels[line + column].A != 0)
                    shown.Add(sheet.KeyAt(line + column));
            }
        }
        return shown;
    }

    /// <summary>The colour buffer's first sprite colour.</summary>
    private const int SpriteColoursFirst = 128;

    /// <summary>Matches the arrival projectiles' definitions and typed programs (#641) to the retail bank-$86 lists.</summary>
    private static void VerifyCompiledDefinitions(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        foreach ((CeresElevatorProjectileDefinition definition, ushort definitionPointer) in new[]
        {
            new VerifyCompiledDefinitionsCase(CeresElevatorArrivalDefinitions.MovingPad, 0xa387),
            new VerifyCompiledDefinitionsCase(CeresElevatorArrivalDefinitions.StationaryPlatform, 0xa395),
        })
        {
            int address = CeresElevatorArrivalDefinitions.BankBase | definitionPointer;
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address + 4),
                NativeEnemyAnimationOracle.EntryOf(CeresElevatorArrivalNativeListAddresses.Entries, definition.Animation),
                "Ceres elevator definition names its program's native list");
        }

        foreach ((CeresElevatorArrivalSegment program, ushort entry) in CeresElevatorArrivalNativeListAddresses.Entries)
        {
            IReadOnlyList<AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>> beats = CeresElevatorArrivalAnimations.Segments.Beats(program);
            Animation<CeresElevatorArrivalSegment> animation = CeresElevatorArrivalAnimations.StartingWith(program);
            ushort cursor = entry;
            int index = 0;
            while (true)
            {
                int address = CeresElevatorArrivalDefinitions.BankBase | cursor;
                ushort word = cartridge.ReadCartridgeWord(address);
                ushort operand = cartridge.ReadCartridgeWord(address + 2);
                string at = $"Ceres elevator {program} at $86:{cursor:X4}";
                if (word == CeresElevatorArrivalDefinitions.DeleteOpcode)
                {
                    VerificationAssertions.AssertEqual(AnimationEnd.Stops, animation.End, $"{at}: delete");
                    break;
                }
                if (word == CeresElevatorArrivalDefinitions.GotoOpcode)
                {
                    // Each arrival list jumps back to its own start: the animation loops its one phase.
                    VerificationAssertions.AssertEqual(operand,
                        NativeEnemyAnimationOracle.EntryOf(CeresElevatorArrivalNativeListAddresses.Entries, program), $"{at}: goto target");
                    VerificationAssertions.AssertEqual(AnimationEnd.LoopsFrom(0), animation.End, $"{at}: goto");
                    break;
                }
                VerificationAssertions.AssertTrue(index < beats.Count, $"{at}: {program} has a beat for the native frame");
                var frame = VerificationAssertions.AssertIs<AnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>.Frame>(beats[index++], $"{at}: frame");
                VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(operand, EnemyProjectileCompositionSources.Spritemap(CeresElevatorArrivalAnimations.Frame(frame.Shown)), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
            }
            VerificationAssertions.AssertEqual(beats.Count, index, $"Ceres elevator {program} ends where its native list does");
            VerificationAssertions.AssertTrue(animation.Phases.SequenceEqual(index == 0
                    ? Array.Empty<AnimationPhase<CeresElevatorArrivalSegment>>()
                    : new[] { new AnimationPhase<CeresElevatorArrivalSegment>(program, 0, index, 1) }),
                $"Ceres elevator {program} plays its list once per pass");
        }
    }

    /// <summary>Forbids compiled Ceres arrival definition and instruction bytes during playback.</summary>
    private static CartridgeReadGuard NewCeresArrivalDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86a28b and < 0x86a2a1 or >= 0x86a387 and < 0x86a3a3,
                address => $"Ceres arrival read compiled bank-$86 byte ${address:X6}.");

    /// <summary>Runs the Ceres elevator arrival graphics-index and landing checks.</summary>
    [VerificationCommand("--ceres-elevator-arrival-definitions")]
    internal static int CeresElevatorArrivalDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresElevatorArrivalGraphicsIndex), () => VerifyCeresElevatorArrivalGraphicsIndex());
        return 0;
    }
}
