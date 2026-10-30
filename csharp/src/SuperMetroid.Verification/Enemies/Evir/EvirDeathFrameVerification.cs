using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Enemies.Evir;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Evir body, arm, and spit slot behavior across a contact-kill frame boundary.</summary>
internal static class EvirDeathFrameVerification
{
    /// <summary>Confirms already-scheduled child AI observes the cleared body slot before deleted children are removed next frame.</summary>
    internal static void VerifyEvirDeathFrame()
    {
        var samus = CreateDropTestSamus();
        samus.Position = samus.Position.WithWhole(new(128, 128));
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        var fixture = CreateEnemyDropFixture(samus, [1]);
        var enemies = fixture.System;
        Action<RoomEnemySlot, SamusState?> initialize = enemies.EvirBehavior.InitializeEvir;
        Action<RoomEnemySlot> initializeProjectile = enemies.EvirBehavior.InitializeEvirProjectile;
        for (int index = 6; index <= 8; index++)
        {
            var slot = enemies.SlotTable.Slots[index];
            slot.Bind(index == 8 ? EnemyDefinitionId.EvirProjectile : EnemyDefinitionId.Evir, slot.Definition);
            slot.Bind(slot.DefinitionId, RoomEnemyHeaders.Get(slot.DefinitionId));
            slot.Position = slot.Position.WithWhole(new(128, 128));
            slot.Radius = new(slot.Definition.Radius.X, slot.Definition.Radius.Y);
            slot.Health = slot.Definition.Health;
            slot.SpritemapPointer = 0x8b59;
            slot.Properties = EnemyPropertyWord.FromWord(index == 6 ? (ushort)0 : (ushort)EnemyProperties.IgnoreSamusCollision);
            slot.SetSpawnParameter1(index == 7 ? (ushort)1 : (ushort)0);
            // The body bobs at speed record 8, one of the two records retail Evirs use.
            if (index == 6) slot.SetSpawnParameter2(8);
            if (index == 8) initializeProjectile(slot); else initialize(slot, samus);
        }
        var body = enemies.SlotTable.Slots[6];
        var arms = enemies.SlotTable.Slots[7];
        var projectile = enemies.SlotTable.Slots[8];
        AssertEqual(EvirFacing.Right, (enemies.SlotTable.Slots[6].SpeciesState<EvirEnemyState>() ?? throw new InvalidOperationException("Verification requires slot 6's Evir state.")).Facing, "live Evir initially faces right");
        // Contact kills the body after the real scheduler has selected all three slots.
        // Arms and spit are marked deleted by the native contact tail but still receive
        // their already-scheduled calls before next frame's population pass removes them.
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level, ResolveSamusContactBeforeAi = true });
        AssertEqual(EnemyDefinitionId.None, body.DefinitionId, "Screw Attack clears Evir body during contact");
        AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "only body counts as a killed enemy");
        AssertTrue(arms.Properties.HasAny(EnemyProperties.Deleted) && projectile.Properties.HasAny(EnemyProperties.Deleted),
            "Evir contact tail marks both following physical slots deleted");
        AssertEqual(1, arms.FrameCounter, "already-selected arms still receive their native final AI call");
        AssertEqual((ushort)0xfffc, arms.Position.X.Whole, "arms read cleared body X and facing rather than stale pre-death values");
        AssertEqual((ushort)10, arms.Position.Y.Whole, "arms use cleared body Y plus native offset");
        AssertEqual((ushort)0xfffc, projectile.Position.X.Whole, "idle spit retains the physical cleared-body alias");
        AssertEqual((ushort)18, projectile.Position.Y.Whole, "idle spit uses cleared body Y plus native offset");
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "normal death explosion remains alive");
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level });
        AssertEqual(EnemyDefinitionId.None, arms.DefinitionId, "next frame removes deleted arms");
        AssertEqual(EnemyDefinitionId.None, projectile.DefinitionId, "next frame removes deleted spit");
        AssertEqual(0, enemies.ActiveEnemies.Count, "no orphan Evir remains active");
        AssertThrows<InvalidDataException>(() => initialize(arms, samus),
            "initialization still rejects a missing body even when a prior typed state survives");
        Console.WriteLine("Evir death frame: real contact kill, cleared-slot arm/spit reads, single death and next-frame removal confirmed.");
    }

    /// <summary>Runs the focused Evir contact-death ordering check.</summary>
    [VerificationCommand("--evir-death-frame")]
    internal static int EvirDeathFrameCommand()
    {
        Suite(nameof(VerifyEvirDeathFrame), () => VerifyEvirDeathFrame());
        return 0;
    }
}
