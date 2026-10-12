using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class ProjectileContactPhaseVerification
{
    internal static void VerifyProjectileRuntimePhase()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.Room.ActiveRoom!.Pointer, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LevelData!;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.Size.X; x++)
            level.SetForegroundEntry(y * level.Size.X + x, y == 16 ? (ushort)0x8000 : (ushort)0);
        foreach (var actor in runtime.Enemies.SlotTable.Slots)
            actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        foreach (var actor in runtime.Enemies.Projectiles.EnemyProjectiles) actor.Clear();
        runtime.InitializeDebugGroundedSamus(128, 235, 16);
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        var projectile = runtime.Enemies.Projectiles.EnemyProjectiles[^1];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
        projectile.InstructionTimer = 2;
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
                samus.KnockbackTimer, samus.KnockbackDirection);
            Console.WriteLine($"CONTACT_FRAME {frame} x={samus.Kinematics.Position.X.RawValue:X8} y={samus.Kinematics.Position.Y.RawValue:X8} pose={(int)samus.Pose:X2} health={samus.Health} timer={samus.KnockbackTimer} direction={samus.KnockbackDirection}");
            if (actual != expected[frame] || samus.Health != 79)
                failures.Add($"frame {frame}: {actual} != {expected[frame]}");
        }
        AssertEqual(0, failures.Count, "native projectile frame ordering: " + string.Join("; ", failures));
    }

    internal static void VerifyProjectileContactPhase(bool verifyPhase)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var failures = new List<string>();
        foreach (ushort suit in new ushort[] { 0, (ushort)SamusEquipmentFlags.VariaSuit, (ushort)SamusEquipmentFlags.GravitySuit })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            var samus = runtime.Player.Samus!;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new(120, 120));
            samus.Health = 99;
            samus.EquippedItems = suit;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            var projectile = runtime.Enemies.Projectiles.EnemyProjectiles[^1];
            projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
            projectile.InstructionTimer = 2;
            projectile.Position = projectile.Position.WithWhole(new(120, 120));
            projectile.Radius = new(8, 8);
            projectile.Damage = 20;
            projectile.InvincibilityFrames = 96;
            projectile.CanDamageSamus = true;
            runtime.Enemies.StepEnemyProjectiles(runtime.Room.LevelData!, samus);
            Console.WriteLine($"PROJECTILE suit={suit:X4} health={samus.Health} pose={(int)samus.Pose:X2} timer={samus.KnockbackTimer} direction={samus.KnockbackDirection} active={samus.KnockbackActive}");
            ushort expectedHealth = suit == 0 ? (ushort)79 : suit == (ushort)SamusEquipmentFlags.VariaSuit ? (ushort)89 : (ushort)94;
            if (samus.Health != expectedHealth) failures.Add($"suit {suit:X4}: expected health {expectedHealth}, got {samus.Health}");
            if (verifyPhase && (samus.Pose != SamusPoseId.FacingRightNormalPose || samus.KnockbackActive || samus.KnockbackDirection != 0))
                failures.Add($"suit {suit:X4}: projectile contact initialized hurt movement early");
            AssertEqual(5, samus.KnockbackTimer, "projectile publishes five-frame hurt request");
            AssertEqual(96, samus.InvincibilityTimer, "projectile publishes invincibility");
            AssertTrue(!projectile.IsActive, "nonpersistent projectile deletes on contact");
        }
        AssertEqual(0, failures.Count, string.Join("; ", failures));
    }

    [VerificationCommand("--projectile-runtime-phase")]
    internal static int ProjectileRuntimePhaseCommand()
    {
        Suite(nameof(VerifyProjectileRuntimePhase), () => VerifyProjectileRuntimePhase());
        return 0;
    }

    [VerificationCommand("--projectile-contact-phase")]
    [VerificationCommand("--projectile-contact-damage")]
    internal static int ProjectileContactPhaseCommand(string[] args)
    {
        Suite(nameof(VerifyProjectileContactPhase), () => VerifyProjectileContactPhase(args[0] == "--projectile-contact-phase"));
        return 0;
    }
}
