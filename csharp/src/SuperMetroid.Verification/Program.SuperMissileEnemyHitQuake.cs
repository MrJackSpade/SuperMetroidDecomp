using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// A Super Missile touching an enemy starts quake type $12 for thirty calls
    /// ($A0:9CB7/$A0:A1E9); only its impact on a block ($93:8125) uses type $14, the type
    /// crawlers fall for. In the 13% movie a Super Missile kills a Sciser in Crab Maze and
    /// the other Scisers keep crawling.
    /// </summary>
    private static void VerifySuperMissileEnemyHitQuake()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CreateRoom(64, 16, new ushort[64 * 16], new byte[64 * 16]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose, XPosition = 64, YPosition = 96,
            SelectedHudItem = 2, SuperMissiles = 3,
        };
        SamusProjectileSystem projectiles = CreateProjectileFixture();
        SamusBombProjectileSystem bombs = CreateBombFixture();
        projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, bombs);
        AssertEqual(SamusProjectileFamily.SuperMissile, projectiles.Slots[0].PackedType.Family, "X fires a Super Missile");

        projectiles.ApplyEnemyCollisionPrelude(0, markCollisionState: true);
        AssertEqual(SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType, projectiles.EarthquakeType,
            "an enemy hit starts quake type $12");
        AssertEqual(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration, projectiles.EarthquakeTimer,
            "for thirty calls");
        Console.WriteLine("  Super Missile enemy-hit quake: type $12, which leaves crawlers crawling.");
    }
}
