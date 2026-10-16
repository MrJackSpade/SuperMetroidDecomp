using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileProgramOracleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#1292: the thirty misc-dust animations and the eye door's smoke run typed programs through the room interpreter.</summary>
internal static class MiscDustProjectileBehaviorVerification
{
    /// <summary>
    /// Every misc-dust selector spawned by the room-graphics dust producer starts its typed program and plays to deletion
    /// (the looping elevator pad loops), and the eye door's smoke reaches deletion, each shown frame keeping its native
    /// duration, with both the misc-dust and Mother Brain list bytes forbidden.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for misc-dust frame durations")]
    internal static void VerifyMiscDustProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        CartridgeReadGuard guard = ForbiddingSharedLists(rom);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(16384, 16384)) };
        foreach (MiscDustAnimation animation in Enum.GetValues<MiscDustAnimation>())
        {
            RoomEnemySystem enemies = CreateRoomEnemySystem();
            enemies.Projectiles.SpawnRoomGraphicsDustExplosion(new Position16<RoomPixels>(128, 96), animation);
            RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);
            MiscDustProjectileProgram program = MiscDustProjectileDefinitions.Program(animation);
            AssertTrue(dust.Animation is MiscDustProjectileAnimation started && started.Cursor.Program == program,
                $"misc-dust animation {animation} starts {program}");

            if (animation == MiscDustAnimation.UnusedLoopingElevatorPad)
            {
                RunForcedTicks(rom, enemies, dust, samus, 4);
                AssertTrue(dust.IsActive && dust.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.LoopingElevatorPad },
                    "room-system looping misc-dust animation keeps looping its program");
                continue;
            }

            int calls = 0;
            while (dust.IsActive && calls < 40)
            {
                RunForcedTicks(rom, enemies, dust, samus, 1);
                calls++;
            }
            AssertTrue(!dust.IsActive, $"room-system finite misc-dust animation {animation} reaches delete");
        }

        RoomEnemySystem smokeEnemies = CreateRoomEnemySystem();
        smokeEnemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(EyeDoorProjectileDefinition.Smoke, Parameter: 3, PlmBlockIndex: 4 * 16 + 7, DoorBit: 0),
            roomWidthInBlocks: 16,
            new Bank80SystemState());
        RoomEnemyProjectileSlot smoke = smokeEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        AssertTrue(smoke.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.SmallExplosion },
            "Eye Door smoke parameter three selects the shared small explosion");
        RunForcedTicks(rom, smokeEnemies, smoke, samus, 7);
        AssertTrue(!smoke.IsActive, "Eye Door smoke reaches delete through the shared small explosion");

        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the shared misc-dust or Mother Brain list bytes");
        Console.WriteLine("  Misc-dust behavior: all thirty selectors and the eye door's smoke run their typed programs to their native ends.");
    }

    /// <summary>A guard rejecting every byte of the misc-dust and Mother Brain effect lists, which share their explosions.</summary>
    internal static CartridgeReadGuard ForbiddingSharedLists(ISnesAddressSpace rom) =>
        NativeEnemyProjectileListGuard.Forbidding(rom, MiscDustProjectileNativeListAddresses.Extent, "misc dust")
            .AlsoForbidding(SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEffectNativeListAddresses.Extent, "Mother Brain effect");

    /// <summary>A room enemy system whose random reads and draws return zero.</summary>
    internal static RoomEnemySystem CreateRoomEnemySystem()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        return enemies;
    }

    /// <summary>Forces <paramref name="count"/> frame expiries, checking each shown frame.</summary>
    internal static void RunForcedTicks(ISnesAddressSpace rom, RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile, SamusState samus, int count)
    {
        for (int tick = 0; tick < count; tick++)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            Process(rom, enemies, projectile, samus);
        }
    }

    /// <summary>
    /// Runs one projectile pass; a frame shown on it must be an installed key, leave its spritemap to installed content,
    /// and last as long as the native frame whose operand that key is.
    /// </summary>
    internal static void Process(ISnesAddressSpace rom, RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile, SamusState samus)
    {
        bool selectsFrame = projectile.InstructionTimer.RawValue == 1;
        enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, samus, Position16<RoomPixels>.Zero);
        if (!projectile.IsActive || !selectsFrame || projectile.InstructionTimer.IsZero || projectile.PresentationOperandAddress == 0)
            return;
        ushort operand = projectile.PresentationOperandAddress;
        AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(operand), "the shown frame is an installed-art identity");
        AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, projectile.SpritemapPointer,
            "the shown frame leaves spritemap resolution to installed content");
        AssertEqual(ReadNativeEnemyProjectileProgramWord(rom, unchecked((ushort)(operand - 2))), projectile.InstructionTimer.RawValue,
            "the shown frame lasts its native duration");
    }

    /// <summary>Runs the misc dust's program oracle and behavior suite.</summary>
    [VerificationCommand("--misc-dust-programs")]
    internal static int MiscDustProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(MiscDustProjectileAnimationsVerification.VerifyMiscDustProjectileAnimations), () => MiscDustProjectileAnimationsVerification.VerifyMiscDustProjectileAnimations(rom));
        Suite(nameof(VerifyMiscDustProjectileBehavior), () => VerifyMiscDustProjectileBehavior(rom));
        return 0;
    }
}
