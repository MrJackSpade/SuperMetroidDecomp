using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
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
