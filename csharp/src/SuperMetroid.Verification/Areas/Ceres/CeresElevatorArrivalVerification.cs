using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
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
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyCeresElevatorArrivalGraphicsIndex()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledDefinitions), () => VerifyCompiledDefinitions(rom));
        var memory = new TestAddressSpace();
        EnemyProjectileSpritemapCatalog artwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidOperationException("Verification requires the installed projectile compositions.");
        var bus = NewCeresArrivalDefinitionReadGuard(memory);

        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0000)) };
        var arrival = new CeresElevatorArrivalState(samus, artwork);
        arrival.Step(samus);

        // With native graphics index zero, each actor shows its composition at tile base zero in the parts' own
        // palettes; a leaked enemy graphics word would add a base tile and OR another OBJ palette.
        VerificationAssert.AssertTrue(arrival.pad.GraphicsIndex == 0 && arrival.platform.GraphicsIndex == 0,
            "Ceres arrival actors draw with graphics index zero");
        var oam = new OamBuffer();
        oam.BeginFrame();
        arrival.DrawAsOam(oam, SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElev0),
            camera: Position16<RoomPixels>.Zero);
        var expected = new OamBuffer();
        expected.BeginFrame();
        foreach (CeresElevatorArrivalState.CeresElevatorProjectile actor in new[] { arrival.pad, arrival.platform })
            expected.AddEnemySpritemap(artwork.Get(CeresElevatorArrivalDefinitions.Frame(actor.Pose
                    ?? throw new InvalidOperationException("Verification requires a shown arrival pose."))).Span,
                actor.Position.ToScreen(Position16<RoomPixels>.Zero).ToOam(), 0, 0);
        VerificationAssert.AssertTrue(expected.NextByteOffset > 0 && expected.ImageEquals(oam),
            "Ceres pad and level-data concealer draw their compositions at tile base zero in their own palettes");

        int frames = 1;
        while (!arrival.IsComplete && frames++ < 200)
            arrival.Step(samus);
        VerificationAssert.AssertTrue(arrival.IsComplete,
            "Ceres arrival's compiled instruction programs delete both projectiles");
        VerificationAssert.AssertEqual(
            CeresElevatorArrivalDefinitions.LandingSamusY,
            samus.Position.Y.Whole,
            "Ceres arrival retains the native landing coordinate");

        Console.WriteLine(
            "  Ceres elevator: definitions/programs match cartridge data, run without bank-$86 reads, and retain native graphics/landing behavior.");
    }

    /// <summary>Matches the arrival projectiles' definitions and typed programs (#641) to the retail bank-$86 lists.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres elevator arrival definitions")]
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
            VerificationAssert.AssertEqual(cartridge.ReadCartridgeWord(address + 4),
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
                    VerificationAssert.AssertTrue(step is AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.StopStep, $"{at}: delete");
                    break;
                }
                if (word == CeresElevatorArrivalDefinitions.GotoOpcode)
                {
                    var goTo = VerificationAssert.AssertIs<AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.GoToStep>(step, $"{at}: goto");
                    VerificationAssert.AssertEqual(operand,
                        NativeEnemyProgramOracle.EntryOf(CeresElevatorArrivalNativeListAddresses.Entries, goTo.Target), $"{at}: goto target");
                    break;
                }
                var show = VerificationAssert.AssertIs<AnimationProgramStep<CeresElevatorArrivalProgram, CeresElevatorArrivalPose, NoAnimationCue>.ShowStep>(step, $"{at}: frame");
                VerificationAssert.AssertEqual(word, show.Duration, $"{at}: duration");
                VerificationAssert.AssertEqual(operand, EnemyProjectileCompositionSources.Spritemap(CeresElevatorArrivalDefinitions.Frame(show.Frame)), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
            }
            VerificationAssert.AssertEqual(steps.Count, index, $"Ceres elevator {program} ends where its native list does");
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
