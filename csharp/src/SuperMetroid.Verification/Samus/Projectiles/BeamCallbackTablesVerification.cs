using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Projectiles.BeamSpeedRowsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks beam pre-instruction pointers and their installation by production firing paths.</summary>
internal static class BeamCallbackTablesVerification
{
    /// <summary>Compares charged and uncharged callback tables with the cartridge, then checks beam initialization or firing.</summary>
    internal static void VerifyBeamCallbackTables(bool initializeOnly = false)
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusBeamPreInstructionCodes.ChargedTable
                : SamusBeamPreInstructionCodes.UnchargedTable;
            for (int combination = 0; combination < Enum.GetValues<SamusBeamCombination>().Length; combination++)
            {
                AssertEqual(
                    CartridgeImportSource.Require(retail).ReadCartridgeWord((table + combination * sizeof(ushort))),
                    SamusBeamCallbackDefinitions.Resolve(charged, SamusBeamCombinations.FromTableIndex(combination)).NativePointer,
                    $"charged={charged} callback {combination:X1} matches cartridge");

            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusBeamCallbackDefinitions.Resolve(false, SamusBeamCombinations.FromTableIndex(16)),
            "beam combination rejects index beyond low nibble");

        var room = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new BackgroundTile[4]);
        foreach (bool charged in new[] { false, true })
        foreach (int beamType in Enumerable.Range(0, 12))
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = unchecked((SamusBeamFlags)((ushort)(beamType | (charged ? 0x1000 : 0)))),
            };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            if (initializeOnly)
            {
                projectiles.Firing.TryFireBeam(room, samus, ControllerButton.X, shared, null, charged);
                AssertTrue(projectiles.Slots[0].Type != 0 || projectiles.Slots[0].InstructionPointer != 0,
                    $"charged={charged} beam {beamType:X1} initializes");
                AssertEqual((SamusBeamCallbackDefinitions.Resolve(charged, SamusBeamCombinations.FromTableIndex(beamType)).Translated ?? throw new InvalidOperationException("Verification requires SamusBeamCallbackDefinitions.Resolve(charged, SamusBeamCombinations.FromTableIndex(beam....")),
                    projectiles.Slots[0].PreInstruction,
                    $"charged={charged} beam {beamType:X1} initializer installs compiled callback");
                continue;
            }
            if (charged)
            {
                for (int frame = 0; frame < 60; frame++)
                {
                    shared.StepFrame(new SamusTerrain { Level = room }, samus, 0, 0);
                    projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = frame == 0 ? ControllerButton.X : 0 }, new(0, 0), shared);
                }
            }
            projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = charged ? 0 : ControllerButton.X, ControllerNewInput = charged ? 0 : ControllerButton.X }, new(0, 0), shared);
            AssertTrue(projectiles.LastFrameResult.QueuedSoundEffect is not null,
                $"charged={charged} beam {beamType:X1} fires");
            // The fixture starts with every ordinary slot empty, so the fired shot is the
            // lowest live slot after the release frame.
            SamusProjectileSlot fired = projectiles.Slots.First(slot => slot.IsActive);
            SamusBeamCallbackDefinition expected =
                SamusBeamCallbackDefinitions.Resolve(charged, SamusBeamCombinations.FromTableIndex(beamType));
            AssertEqual(
                (expected.Translated ?? throw new InvalidOperationException("Verification requires expected.Translated.")),
                fired.PreInstruction,
                $"charged={charged} beam {beamType:X1} installs compiled callback");
        }
        Console.WriteLine(
            "Beam callback definitions: all 32 low-nibble words and 24 production firing paths pass with both source ranges forbidden.");
    }

    /// <summary>Runs the beam callback table and firing-path verification.</summary>
    [VerificationCommand("--beam-callback-tables")]
    internal static int BeamCallbackTablesCommand()
    {
        Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
        return 0;
    }
}
