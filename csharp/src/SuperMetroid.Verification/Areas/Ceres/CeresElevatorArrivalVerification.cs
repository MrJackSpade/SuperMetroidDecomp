using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Programs;
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

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0000)) };
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
    /// every arrival draw's picture carries the concealer's shade, which shows exactly that sprite colour black.
    /// </summary>
    private static void VerifyConcealerShade(ISnesAddressSpace rom, CeresElevatorArrivalState arrival)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        VerificationAssertions.AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x82812d), "the new-game clear loads an immediate colour");
        VerificationAssertions.AssertEqual((ushort)0, cartridge.ReadCartridgeWord(0x82812e), "the new-game clear's colour is black");
        VerificationAssertions.AssertEqual((byte)0x8f, rom.ReadCartridgeByte(0x828130), "the new-game clear stores a long address");
        VerificationAssertions.AssertEqual((byte)0x7e, rom.ReadCartridgeByte(0x828133), "the new-game clear stores into work RAM");
        int spriteColour = (cartridge.ReadCartridgeWord(0x828131) - NativeSnapshotMemory.PaletteBuffer) / sizeof(ushort) - SpriteColoursFirst;
        var frame = new SpriteFrameBuffer();
        frame.BeginFrame();
        arrival.Draw(frame, SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevDefault),
            Position16<RoomPixels>.Zero);
        frame.FinalizeFrame();
        VerificationAssertions.AssertTrue(frame.Draws.Length > 0, "the arrival draws while it runs");
        foreach (SpriteDraw draw in frame.Draws)
        {
            SpriteImage sheet = ((ImageSpriteDraw)draw).Image.Sheet;
            ShadeInks inks = sheet.Shade(CeresElevatorShades.ConcealerCleared);
            // The shade names the sheet's own colours; the import composed each as a palette index (#1544).
            VerificationAssertions.AssertTrue(inks.Inks.Select(ink => SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ComposedIndexOf(sheet, ink))
                    .SequenceEqual([spriteColour]) && inks.Colours.SequenceEqual([RgbColor.Black]),
                "the concealer's shade shows the cartridge's cleared sprite colour black");
        }
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
                NativeEnemyProgramOracle.EntryOf(CeresElevatorArrivalNativeListAddresses.Entries, definition.Program),
                "Ceres elevator definition names its program's native list");
        }

        foreach ((CeresElevatorArrivalProgram program, ushort entry) in CeresElevatorArrivalNativeListAddresses.Entries)
        {
            var steps = CeresElevatorArrivalAnimations.Programs.Steps(program);
            ushort cursor = entry;
            int index = 0;
            while (true)
            {
                int address = CeresElevatorArrivalDefinitions.BankBase | cursor;
                ushort word = cartridge.ReadCartridgeWord(address);
                ushort operand = cartridge.ReadCartridgeWord(address + 2);
                var step = steps[index++];
                string at = $"Ceres elevator {program} at $86:{cursor:X4}";
                if (word == CeresElevatorArrivalDefinitions.DeleteOpcode)
                {
                    VerificationAssertions.AssertTrue(step is AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.StopStep, $"{at}: delete");
                    break;
                }
                if (word == CeresElevatorArrivalDefinitions.GotoOpcode)
                {
                    var goTo = VerificationAssertions.AssertIs<AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.GoToStep>(step, $"{at}: goto");
                    VerificationAssertions.AssertEqual(operand,
                        NativeEnemyProgramOracle.EntryOf(CeresElevatorArrivalNativeListAddresses.Entries, goTo.Target), $"{at}: goto target");
                    break;
                }
                var show = VerificationAssertions.AssertIs<AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.ShowStep>(step, $"{at}: frame");
                VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(operand, EnemyProjectileCompositionSources.Spritemap(CeresElevatorArrivalAnimations.Frame(show.Frame)), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
            }
            VerificationAssertions.AssertEqual(steps.Count, index, $"Ceres elevator {program} ends where its native list does");
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
