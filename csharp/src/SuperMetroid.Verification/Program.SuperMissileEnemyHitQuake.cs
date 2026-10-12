using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Represents super missile enemy hit quake verification.</summary>
internal static class SuperMissileEnemyHitQuakeVerification
{
    /// <summary>
    /// A Super Missile touching an enemy starts quake type $12 for thirty calls
    /// ($A0:9CB7/$A0:A1E9); only its impact on a block ($93:8125) uses type $14, the type
    /// crawlers fall for. In the 13% movie a Super Missile kills a Sciser in Crab Maze and
    /// the other Scisers keep crawling.
    /// </summary>
    internal static void VerifySuperMissileEnemyHitQuake()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CreateRoom(new(64, 16), new ushort[64 * 16], new byte[64 * 16]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 96)),
            SelectedHudItem = 2, SuperMissiles = 3,
        };
        SamusProjectileSystem projectiles = CreateProjectileFixture();
        SamusBombProjectileSystem bombs = CreateBombFixture();
        projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), bombs);
        AssertEqual(SamusProjectileFamily.SuperMissile, projectiles.Slots[0].PackedType.Family, "X fires a Super Missile");

        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(0, markCollisionState: true);
        AssertEqual(SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType, projectiles.Earthquakes.EarthquakeType,
            "an enemy hit starts quake type $12");
        AssertEqual(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration, projectiles.Earthquakes.EarthquakeTimer,
            "for thirty calls");
        Console.WriteLine("  Super Missile enemy-hit quake: type $12, which leaves crawlers crawling.");
    }

    /// <summary>Super missile enemy hit quake command.</summary>
    [VerificationCommand("--super-missile-enemy-hit-quake")]
    internal static int SuperMissileEnemyHitQuakeCommand()
    {
        Suite(nameof(VerifySuperMissileEnemyHitQuake), () => VerifySuperMissileEnemyHitQuake());
        return 0;
    }
}
