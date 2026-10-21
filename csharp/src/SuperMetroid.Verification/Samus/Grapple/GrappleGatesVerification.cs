using static SuperMetroid.Verification.Bosses.Draygon.DraygonTilemapProductionVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleEnemyDeathVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGatePlmsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;

/// <summary>Checks that rejected Grapple shots leave both green gate sprites visible.</summary>
internal static class GrappleGatesVerification
{
    /// <summary>Compares gate OAM across twelve frames around each rejected Grapple impact and checks the dud sound.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyGrappleGreenGateVisibility()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (var trigger in new[] { DownwardGateTriggerBehavior.GreenLeft, DownwardGateTriggerBehavior.GreenRight })
        {
            var (bus, level, plms, gateIndex) = CreateDownwardGateFixture(trigger);
            // Use the cartridge's actual gate sprite maps for the observable OAM check.
            SeedRetailGateSpriteMaps(bus, retail);
            SeedEmptyRoomEnemyPopulation(bus, 0x9600);
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0)) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
            enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9600), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new SnesCgram()) { Level = level, Samus = new SamusState() });
            enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(plms.Gates.TakeDownwardGateProjectileRequests().Single(), level.Size.X);
            enemies.StepEnemyProjectiles(level, null);
            enemies.StepEnemyProjectiles(level, null);
            var oam = new OamBuffer();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            AssertTrue(oam.NextByteOffset > 0, "closed gate has visible OAM before grapple impact");
            byte[] expected = oam.LowTable[..oam.NextByteOffset].ToArray();
            int hitIndex = gateIndex + (trigger == DownwardGateTriggerBehavior.GreenLeft ? -1 : 1);
            var samus = new SamusState
            {
                Position = new(new((ushort)(hitIndex % level.Size.X * 16 + 8), 0), new((ushort)(hitIndex / level.Size.X * 16 + 8), 0)),
            };
            for (int frame = 0; frame < 12; frame++)
            {
                // Runtime clears OAM before alpha. An exception on impact interrupts the
                // draw later in the frame; the opt-in reporter then exposes partial OAM.
                oam.BeginFrame();
                if (frame == 2)
                {
                    SeedStationaryGrappleCollisionProbe(samus);
                    var result = SamusGrappleMovement.StepFiring(level, samus, SnesButton.X, plms);
                    AssertEqual(GrapplePhase.CancelPending, result.Phase, "green gate rejects grapple attachment");
                }
                StepDownwardGatePlm(plms, level);
                AssertEqual(0, plms.Gates.TakeDownwardGateProjectileRequests().Count, "rejected grapple does not wake or replace gate actor");
                enemies.StepEnemyProjectiles(level, null);
                enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
                AssertTrue(oam.LowTable[..oam.NextByteOffset].SequenceEqual(expected),
                    $"green gate OAM remains visible and identical on {trigger} frame {frame}");
                if (frame == 2)
                    AssertTrue(plms.Sounds.SoundRequests.Any(r => r.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2, DownwardGatePlmRomData.RejectedShotSound)), "rejected grapple emits gate dud sound");
            }
        }
        Console.WriteLine("  Grapple green gates: both sides retain exact gate OAM before, during and after impact.");
    }

    /// <summary>Reads a retail gate sprite-map byte for the fixture's exact OAM comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple green gate visibility")]
    private static byte GrappleGreenGateVisibilityRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Copies the retail bank-$8D sprite maps into the gate fixture for the observable OAM check.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture copying retail bank $8D sprite maps for green-gate visibility")]
    private static void SeedRetailGateSpriteMaps(TestAddressSpace bus, ISnesAddressSpace retail)
    {
        var maps = new byte[0x8000];
        for (int i = 0; i < maps.Length; i++) maps[i] = GrappleGreenGateVisibilityRomByte(retail, 0x8d8000 + i);
        bus.WriteBytes(0x8d8000, maps);
    }

    /// <summary>Runs the gate, enemy-death, and Draygon artwork fixture checks together.</summary>
    [VerificationCommand("--enemy-art-fixtures")]
    internal static int EnemyArtFixturesCommand()
    {
     VerifyGrappleGreenGateVisibility(); VerifyGrappleEnemyDeath(); VerifyDraygonTilemapProduction(); return 0; 
    }

    /// <summary>Runs only the green-gate Grapple visibility verification suite.</summary>
    [VerificationCommand("--grapple-gates")]
    internal static int GrappleGatesCommand()
    {
        Suite(nameof(VerifyGrappleGreenGateVisibility), () => VerifyGrappleGreenGateVisibility());
        return 0;
    }
}
