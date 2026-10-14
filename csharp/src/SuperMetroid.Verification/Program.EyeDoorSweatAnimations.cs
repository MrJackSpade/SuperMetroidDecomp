using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>
/// #1296: the eye-door sweat drop's typed programs against the retail cartridge. Both programs must match their
/// bank-$86 lists update by update, and the real producer and floor collision must start them.
/// </summary>
internal static class EyeDoorSweatAnimationsVerification
{
    /// <summary>
    /// The falling and splash programs match their native lists, the producer starts the falling drop, and a floor
    /// collision stops the drop and starts the splash, which deletes it after its three poses.
    /// </summary>
    internal static void VerifyEyeDoorSweatAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int compared = CompareProjectile(cartridge, EyeDoorSweatProgram.Falling, RoomEffectNativeListAddresses.EyeDoorSweatFalling, 64);
        compared += CompareProjectile(cartridge, EyeDoorSweatProgram.HitFloor, RoomEffectNativeListAddresses.EyeDoorSweatHitFloor, 64);
        VerifyProducerAndFloorImpact();
        Console.WriteLine($"  Eye-door sweat programs: {compared} updates of the falling drop and its splash match bank $86.");
    }

    /// <summary>Runs one typed program beside its native list; returns the updates compared.</summary>
    private static int CompareProjectile(IImportCartridgeSource cartridge, EyeDoorSweatProgram program, ushort entry, int updates)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(new TestAddressSpace());
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.EyeDoorSweat, graphicsIndex: 0);
        projectile.StartAnimation(new EyeDoorSweatAnimation(program));
        var native = new NativeEnemyProjectileListDefinitions.Reader(entry, projectile.InstructionTimer.RawValue);
        int update = 0;
        for (; update < updates && !native.Deleted; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"eye-door sweat {program} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.PreInstructionCleared)
                AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, projectile.PreInstruction, $"{at}: pre-instruction cleared");
        }
        return update;
    }

    /// <summary>The real producer and floor collision start the typed programs with the native timing.</summary>
    private static void VerifyProducerAndFloorImpact()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(new TestAddressSpace());
        const int roomWidth = 16;
        var foreground = new ushort[roomWidth * 16];
        foreground[5 * roomWidth + 6] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
        RoomLevelData level = CreateRoom(new(roomWidth, 16), foreground, new byte[foreground.Length],
            blockDefinitions: new byte[1024 * 8]);
        var request = new EyeDoorProjectileRequest(EyeDoorProjectileDefinition.Sweat, Parameter: 4,
            PlmBlockIndex: 4 * roomWidth + 7, DoorBit: 0);
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, roomWidth, new Bank80SystemState());
        RoomEnemyProjectileSlot sweat = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { Program: EyeDoorSweatProgram.Falling },
            "the producer starts the falling drop");

        EyeDoorBehavior.RunEyeDoorSweatPreInstruction(sweat, level);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { Program: EyeDoorSweatProgram.HitFloor },
            "a floor collision starts the splash");
        AssertEqual((ushort)EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, sweat.PreInstruction,
            "the splash stops the drop's movement");
        AssertEqual((ushort)1, sweat.InstructionTimer.RawValue, "the splash's first pose shows on this pass");
        AssertEqual((ushort)76, sweat.Position.Y.Whole, "the splash lifts the drop four pixels onto the floor");

        var shown = new List<ushort>();
        for (int update = 0; update < 32 && sweat.IsActive; update++)
        {
            ushort timer = sweat.InstructionTimer.RawValue;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(sweat, null, Position16<RoomPixels>.Zero);
            if (timer == 1 && sweat.IsActive)
                shown.Add(sweat.PresentationOperandAddress);
        }
        AssertTrue(!sweat.IsActive, "the splash deletes the drop");
        AssertTrue(shown.SequenceEqual(EyeDoorSweatVisualDefinitions.ProjectileFrames.Skip(1).Select(EyeDoorSweatVisualDefinitions.PresentationKey)),
            "the splash shows its three poses in order");
    }

    /// <summary>Runs the eye-door sweat program parity checks against the repository cartridge.</summary>
    [VerificationCommand("--eye-door-sweat-animations")]
    internal static int EyeDoorSweatAnimationsCommand()
    {
        Suite(nameof(VerifyEyeDoorSweatAnimations), () => VerifyEyeDoorSweatAnimations(LoadRepositoryRom()));
        return 0;
    }
}
