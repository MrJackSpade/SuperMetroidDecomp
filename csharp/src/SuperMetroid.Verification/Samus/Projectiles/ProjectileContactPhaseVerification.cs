using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks enemy-projectile contact ordering, damage, and delayed hurt movement.</summary>
internal static class ProjectileContactPhaseVerification
{
    /// <summary>Compares three runtime frames with a native trace of floor snapping and knockback activation.</summary>
    internal static void VerifyProjectileRuntimePhase()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.Room.LoadedRoom.Pointer, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.Size.X; x++)
            level.SetForegroundEntry(y * level.Size.X + x, y == 16 ? (ushort)0x8000 : (ushort)0);
        foreach (var actor in runtime.Enemies.SlotTable.Slots)
            actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        foreach (var actor in runtime.Enemies.Projectiles.EnemyProjectiles) actor.Clear();
        runtime.InitializeDebugGroundedSamus(128, 235, 16);
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        var projectile = runtime.Enemies.Projectiles.EnemyProjectiles[^1];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
        projectile.InstructionTimer = NativeCountdown.Start(2);
        projectile.Position = projectile.Position.WithWhole(new(128, 235));
        projectile.Radius = new(8, 8);
        projectile.Damage = 20; projectile.InvincibilityFrames = 96;
        projectile.CanDamageSamus = true;

        // Original CPU trace: beta first, inert projectile contact afterward, timers last.
        // Include subpositions: a standing floor collision snaps the fraction to FFFF.
        (uint X, uint Y, SamusPoseId Pose, ushort Timer, ushort Direction)[] expected =
        [
            (0x00800000, 0x00ebffff, SamusPoseId.FacingRightNormalPose, 4, 0),
            (0x00800000, 0x00ebffff, SamusPoseId.KnockbackRightPose, 3, 2),
            (0x00818000, 0x00e6ffff, SamusPoseId.KnockbackRightPose, 2, 2),
        ];
        var failures = new List<string>();
        for (int frame = 0; frame < expected.Length; frame++)
        {
            runtime.StepFrame(0);
            var actual = (samus.Kinematics.Position.X.RawValue, samus.Kinematics.Position.Y.RawValue, samus.Pose,
                samus.KnockbackTimer.RawValue, (ushort)samus.KnockbackDirection);
            Console.WriteLine($"CONTACT_FRAME {frame} x={samus.Kinematics.Position.X.RawValue:X8} y={samus.Kinematics.Position.Y.RawValue:X8} pose={(int)samus.Pose:X2} health={samus.Health} timer={samus.KnockbackTimer.RawValue} direction={samus.KnockbackDirection}");
            if (actual != expected[frame] || samus.Health != 79)
                failures.Add($"frame {frame}: {actual} != {expected[frame]}");
        }
        AssertEqual(0, failures.Count, "native projectile frame ordering: " + string.Join("; ", failures));
    }

    /// <summary>Checks suit-scaled damage, five-frame hurt request, invincibility, and projectile deletion on contact.</summary>
    internal static void VerifyProjectileContactPhase(bool verifyPhase)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var failures = new List<string>();
        foreach (SamusEquipmentFlags suit in new[] { SamusEquipmentFlags.None, SamusEquipmentFlags.VariaSuit, SamusEquipmentFlags.GravitySuit })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            var samus = runtime.Player.IntroducedSamus;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new(120, 120));
            samus.Health = 99;
            samus.EquippedItems = (SamusEquipmentFlags)suit;
            samus.PoseMetadata.RefreshCollisionRadii();
            var projectile = runtime.Enemies.Projectiles.EnemyProjectiles[^1];
            projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
            projectile.InstructionTimer = NativeCountdown.Start(2);
            projectile.Position = projectile.Position.WithWhole(new(120, 120));
            projectile.Radius = new(8, 8);
            projectile.Damage = 20;
            projectile.InvincibilityFrames = 96;
            projectile.CanDamageSamus = true;
            runtime.Enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, samus);
            Console.WriteLine($"PROJECTILE suit={suit.ToNativeWord():X4} health={samus.Health} pose={(int)samus.Pose:X2} timer={samus.KnockbackTimer.RawValue} direction={samus.KnockbackDirection} active={samus.KnockbackActive}");
            ushort expectedHealth = suit == 0 ? (ushort)79 : suit == SamusEquipmentFlags.VariaSuit ? (ushort)89 : (ushort)94;
            if (samus.Health != expectedHealth) failures.Add($"suit {suit.ToNativeWord():X4}: expected health {expectedHealth}, got {samus.Health}");
            if (verifyPhase && (samus.Pose != SamusPoseId.FacingRightNormalPose || samus.KnockbackActive || samus.KnockbackDirection != 0))
                failures.Add($"suit {suit.ToNativeWord():X4}: projectile contact initialized hurt movement early");
            AssertEqual(5, samus.KnockbackTimer.RawValue, "projectile publishes five-frame hurt request");
            AssertEqual(96, samus.InvincibilityTimer.RawValue, "projectile publishes invincibility");
            AssertTrue(!projectile.IsActive, "nonpersistent projectile deletes on contact");
        }
        AssertEqual(0, failures.Count, string.Join("; ", failures));
    }

    /// <summary>Runs the full runtime contact-order trace check.</summary>
    [VerificationCommand("--projectile-runtime-phase")]
    internal static int ProjectileRuntimePhaseCommand()
    {
        Suite(nameof(VerifyProjectileRuntimePhase), () => VerifyProjectileRuntimePhase());
        return 0;
    }

    /// <summary>Runs contact damage checks, optionally asserting that hurt movement begins later.</summary>
    [VerificationCommand("--projectile-contact-phase")]
    [VerificationCommand("--projectile-contact-damage")]
    internal static int ProjectileContactPhaseCommand(string[] args)
    {
        Suite(nameof(VerifyProjectileContactPhase), () => VerifyProjectileContactPhase(args[0] == "--projectile-contact-phase"));
        return 0;
    }
}
