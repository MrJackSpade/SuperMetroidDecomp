using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEvirDeathFrame()
    {
        var samus = CreateDropTestSamus();
        samus.Position = samus.Position.WithWhole(new(128, 128));
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        var fixture = CreateEnemyDropFixture(samus, [1]);
        var enemies = fixture.System;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var states = (EvirEnemyState?[])typeof(RoomEnemySystem).GetField("_evirStates", flags)!.GetValue(enemies)!;
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeEvir", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState?>>(enemies);
        var initializeProjectile = typeof(RoomEnemySystem).GetMethod("InitializeEvirProjectile", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        for (int index = 6; index <= 8; index++)
        {
            var slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = index == 8 ? EnemyDefinitionId.EvirProjectile : EnemyDefinitionId.Evir;
            slot.Definition = RoomEnemyDefinitionCatalog.Get(slot.EnemyDefinitionPointer);
            slot.AiBank = slot.Definition.Bank;
            slot.Position = slot.Position.WithWhole(new(128, 128));
            slot.Radius = new(slot.Definition.Radius.X, slot.Definition.Radius.Y);
            slot.Health = slot.Definition.Health;
            slot.SpritemapPointer = 0x8b59;
            slot.Properties = index == 6 ? (ushort)0 : (ushort)EnemyProperties.IgnoreSamusCollision;
            slot.Parameter1 = index == 7 ? (ushort)1 : (ushort)0;
            if (index == 8) initializeProjectile(slot); else initialize(slot, samus);
        }
        var body = enemies.Slots[6];
        var arms = enemies.Slots[7];
        var projectile = enemies.Slots[8];
        AssertEqual((ushort)1, states[6]!.FacingDirection, "live Evir initially faces right");
        // Contact kills the body after the real scheduler has selected all three slots.
        // Arms and spit are marked deleted by the native contact tail but still receive
        // their already-scheduled calls before next frame's population pass removes them.
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level, ResolveSamusContactBeforeAi = true });
        AssertEqual(EnemyDefinitionId.None, body.EnemyDefinitionPointer, "Screw Attack clears Evir body during contact");
        AssertEqual((ushort)1, enemies.EnemiesKilled, "only body counts as a killed enemy");
        AssertTrue(arms.Properties.HasAny(EnemyProperties.Deleted) && projectile.Properties.HasAny(EnemyProperties.Deleted),
            "Evir contact tail marks both following physical slots deleted");
        AssertEqual((ushort)1, arms.FrameCounter, "already-selected arms still receive their native final AI call");
        AssertEqual((ushort)0xfffc, arms.Position.X.Whole, "arms read cleared body X and facing rather than stale pre-death values");
        AssertEqual((ushort)10, arms.Position.Y.Whole, "arms use cleared body Y plus native offset");
        AssertEqual((ushort)0xfffc, projectile.Position.X.Whole, "idle spit retains the physical cleared-body alias");
        AssertEqual((ushort)18, projectile.Position.Y.Whole, "idle spit uses cleared body Y plus native offset");
        AssertTrue(enemies.EnemyProjectiles.Any(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "normal death explosion remains alive");
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level });
        AssertEqual(EnemyDefinitionId.None, arms.EnemyDefinitionPointer, "next frame removes deleted arms");
        AssertEqual(EnemyDefinitionId.None, projectile.EnemyDefinitionPointer, "next frame removes deleted spit");
        AssertEqual(0, enemies.ActiveEnemyIndexes.Count, "no orphan Evir remains active");
        AssertThrows<InvalidDataException>(() => initialize(arms, samus),
            "initialization still rejects a missing body even when a prior typed state survives");
        Console.WriteLine("Evir death frame: real contact kill, cleared-slot arm/spit reads, single death and next-frame removal confirmed.");
    }
}
