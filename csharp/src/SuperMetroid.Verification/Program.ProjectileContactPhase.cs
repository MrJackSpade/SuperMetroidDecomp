using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyProjectileRuntimePhase()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
            level.SetForegroundEntry(y * level.WidthInBlocks + x, y == 16 ? (ushort)0x8000 : (ushort)0);
        foreach (var actor in runtime.Enemies.Slots)
            actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
        runtime.InitializeDebugGroundedSamus(128, 235, 16);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        var projectile = runtime.Enemies.EnemyProjectiles[^1];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;
        projectile.InstructionTimer = 2;
        projectile.XPosition = 128; projectile.YPosition = 235;
        projectile.XRadius = projectile.YRadius = 8;
        projectile.Damage = 20; projectile.InvincibilityFrames = 96;
        projectile.CanDamageSamus = true;

        // Original CPU trace: beta first, inert projectile contact afterward, timers last.
        // Include subpositions: a standing floor collision snaps the fraction to FFFF.
        (uint X, uint Y, byte Pose, ushort Timer, ushort Direction)[] expected =
        [
            (0x00800000, 0x00ebffff, SamusPoseIds.FacingRightNormalPose, 4, 0),
            (0x00800000, 0x00ebffff, SamusPoseIds.KnockbackRightPose, 3, 2),
            (0x00818000, 0x00e6ffff, SamusPoseIds.KnockbackRightPose, 2, 2),
        ];
        var failures = new List<string>();
        for (int frame = 0; frame < expected.Length; frame++)
        {
            runtime.StepFrame(0);
            var actual = (samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
                samus.KnockbackTimer, samus.KnockbackDirection);
            Console.WriteLine($"CONTACT_FRAME {frame} x={samus.Kinematics.XFixed:X8} y={samus.Kinematics.YFixed:X8} pose={samus.Pose:X2} health={samus.Health} timer={samus.KnockbackTimer} direction={samus.KnockbackDirection}");
            if (actual != expected[frame] || samus.Health != 79)
                failures.Add($"frame {frame}: {actual} != {expected[frame]}");
        }
        AssertEqual(0, failures.Count, "native projectile frame ordering: " + string.Join("; ", failures));
    }

    private static void VerifyProjectileContactPhase(bool verifyPhase)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var failures = new List<string>();
        foreach (ushort suit in new ushort[] { 0, (ushort)SamusEquipmentFlags.VariaSuit, (ushort)SamusEquipmentFlags.GravitySuit })
        {
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = samus.YPosition = 120;
            samus.Health = 99;
            samus.EquippedItems = suit;
            samus.RefreshCollisionRadii(bus);
            var projectile = runtime.Enemies.EnemyProjectiles[^1];
            projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            projectile.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;
            projectile.InstructionTimer = 2;
            projectile.XPosition = projectile.YPosition = 120;
            projectile.XRadius = projectile.YRadius = 8;
            projectile.Damage = 20;
            projectile.InvincibilityFrames = 96;
            projectile.CanDamageSamus = true;
            runtime.Enemies.StepEnemyProjectiles(runtime.LevelData!, samus);
            Console.WriteLine($"PROJECTILE suit={suit:X4} health={samus.Health} pose={samus.Pose:X2} timer={samus.KnockbackTimer} direction={samus.KnockbackDirection} active={samus.KnockbackActive}");
            ushort expectedHealth = suit == 0 ? (ushort)79 : suit == (ushort)SamusEquipmentFlags.VariaSuit ? (ushort)89 : (ushort)94;
            if (samus.Health != expectedHealth) failures.Add($"suit {suit:X4}: expected health {expectedHealth}, got {samus.Health}");
            if (verifyPhase && (samus.Pose != SamusPoseIds.FacingRightNormalPose || samus.KnockbackActive || samus.KnockbackDirection != 0))
                failures.Add($"suit {suit:X4}: projectile contact initialized hurt movement early");
            AssertEqual(5, samus.KnockbackTimer, "projectile publishes five-frame hurt request");
            AssertEqual(96, samus.InvincibilityTimer, "projectile publishes invincibility");
            AssertTrue(!projectile.IsActive, "nonpersistent projectile deletes on contact");
        }
        AssertEqual(0, failures.Count, string.Join("; ", failures));
    }
}
