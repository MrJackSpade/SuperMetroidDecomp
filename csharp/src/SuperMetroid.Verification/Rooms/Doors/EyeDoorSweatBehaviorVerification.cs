using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1296: the eye door's sweat drop runs typed falling and floor-splash programs.</summary>
internal static class EyeDoorSweatBehaviorVerification
{
    /// <summary>
    /// The sweat's spawn starts its typed fall, which loops as the native list does; a real floor collision starts the
    /// splash, which stops the drop, applies the native four-pixel correction and deletes it on the native update.
    /// </summary>
    internal static void VerifyEyeDoorSweatBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, EyeDoorSweatNativeListAddresses.Extent, "eye-door sweat");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        const int roomWidth = 16;
        var foreground = new ushort[roomWidth * 16];
        foreground[5 * roomWidth + 6] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
        RoomLevelData level = CreateRoom(new(roomWidth, 16), foreground, new byte[foreground.Length], blockDefinitions: new byte[1024 * 8]);
        var request = new EyeDoorProjectileRequest(EyeDoorProjectileDefinition.Sweat, Parameter: 4, PlmBlockIndex: 4 * roomWidth + 7, DoorBit: 0);
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, roomWidth, new Bank80SystemState());
        RoomEnemyProjectileSlot sweat = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { Cursor.Program: EyeDoorSweatProgram.Falling }, "the sweat starts its fall");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, sweat,
            EyeDoorSweatNativeListAddresses.Entry(EyeDoorSweatProgram.Falling), 20, "eye-door sweat fall");
        AssertTrue(!deleted, "the fall loops");

        EyeDoorBehavior.RunEyeDoorSweatPreInstruction(sweat, level);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { Cursor.Program: EyeDoorSweatProgram.HitFloor }, "the floor collision starts the splash");
        AssertEqual((ushort)EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, sweat.PreInstruction,
            "the splash stops the drop with the native inert pre-instruction");
        AssertEqual((ushort)76, sweat.Position.Y.Whole, "the splash applies the native four-pixel floor correction");
        (deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, sweat,
            EyeDoorSweatNativeListAddresses.Entry(EyeDoorSweatProgram.HitFloor), 30, "eye-door sweat splash");
        AssertTrue(deleted, "the splash deletes the drop");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the sweat's native list bytes");
        Console.WriteLine("  Eye-door sweat behavior: the falling loop and the floor splash match their native lists.");
    }

    /// <summary>Runs the eye-door sweat's program oracle and behavior suite.</summary>
    [VerificationCommand("--eye-door-sweat-programs")]
    internal static int EyeDoorSweatProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations), () => EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations(rom));
        Suite(nameof(VerifyEyeDoorSweatBehavior), () => VerifyEyeDoorSweatBehavior(rom));
        return 0;
    }
}
