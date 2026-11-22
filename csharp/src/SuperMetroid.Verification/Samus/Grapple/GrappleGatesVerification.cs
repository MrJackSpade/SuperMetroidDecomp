using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Samus.Grapple.GrappleEnemyDeathVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.RoomObjects.DownwardGateObjectsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Grapple;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;

/// <summary>Checks that rejected Grapple shots leave both green gate sprites visible.</summary>
internal static class GrappleGatesVerification
{
    /// <summary>Compares gate OAM across twelve frames around each rejected Grapple impact and checks the dud sound.</summary>
    internal static void VerifyGrappleGreenGateVisibility()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (var trigger in new[] { new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Left), new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Right) })
        {
            var (bus, level, roomObjects, gateIndex) = CreateDownwardGateFixture(trigger);
            // Use the cartridge's actual gate sprite maps for the observable OAM check.
            SeedRetailGateSpriteMaps(bus, retail);
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 0) { ReadRandom = () => 0, SeedRandom = _ => { } }, installedBosses: InstalledFixtures.BossArtwork);
            // The gate draws from the sheets of the set its room loads; Green Hill Zone, the room this fixture models, loads
            // its set with its own population (an empty population would keep the session's standard sprites), whose enemies
            // take the game's compiled definitions: the gate fixture's sparse bus holds no enemy header.
            enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(RoomEnemyPopulationDefinitions.GreenHillZone, BgArtworkFixtures.For(new SnesVram()), level, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())));
            enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(roomObjects.Gates.TakeDownwardGateProjectileRequests().Single());
            enemies.StepEnemyProjectiles(level, RoomEnemyFrameFixtures.StandaloneSamus());
            enemies.StepEnemyProjectiles(level, RoomEnemyFrameFixtures.StandaloneSamus());
            var oam = new OamBuffer();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            AssertTrue(oam.NextByteOffset > 0, "closed gate has visible OAM before grapple impact");
            byte[] expected = oam.LowTable[..oam.NextByteOffset].ToArray();
            int hitIndex = gateIndex + (trigger == new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Left) ? -1 : 1);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
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
                    var result = SamusGrappleMovement.StepFiringAndCommit(new RoomTerrain(level, roomObjects), samus, GameplayAction.Shoot, enemyCollision: GrappleEnemyFixtures.NoEnemy);
                    AssertEqual(GrapplePhase.CancelPending, result.Phase, "green gate rejects grapple attachment");
                }
                StepDownwardGateObject(roomObjects, level);
                AssertEqual(0, roomObjects.Gates.TakeDownwardGateProjectileRequests().Count, "rejected grapple does not wake or replace gate actor");
                enemies.StepEnemyProjectiles(level, RoomEnemyFrameFixtures.StandaloneSamus());
                enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
                AssertTrue(oam.LowTable[..oam.NextByteOffset].SequenceEqual(expected),
                    $"green gate OAM remains visible and identical on {trigger} frame {frame}");
                if (frame == 2)
                    AssertTrue(roomObjects.Sounds.SoundRequests.Any(r => r.SoundEffect == DownwardGateDefinitions.RejectedShotSound), "rejected grapple emits gate dud sound");
            }
        }
        Console.WriteLine("  Grapple green gates: both sides retain exact gate OAM before, during and after impact.");
    }

    /// <summary>Reads a retail gate sprite-map byte for the fixture's exact OAM comparison.</summary>
    private static byte GrappleGreenGateVisibilityRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Copies the retail bank-$8D sprite maps into the gate fixture for the observable OAM check.</summary>
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
     VerifyGrappleGreenGateVisibility(); VerifyGrappleEnemyDeath(); return 0; 
    }

    /// <summary>Runs only the green-gate Grapple visibility verification suite.</summary>
    [VerificationCommand("--grapple-gates")]
    internal static int GrappleGatesCommand()
    {
        Suite(nameof(VerifyGrappleGreenGateVisibility), () => VerifyGrappleGreenGateVisibility());
        return 0;
    }
}
