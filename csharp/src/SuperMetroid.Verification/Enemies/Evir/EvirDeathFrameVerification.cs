using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
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
        samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.ScrewAttack;
        var fixture = CreateEnemyDropFixture(samus, [1]);
        var enemies = fixture.System;
        for (int index = 6; index <= 8; index++)
        {
            var place = enemies.Places.InUpdateOrder[index];
            // The body and its arms are placed as their room places them, whose settings name the part each is (#2146).
            place.SetSpawnParameter1(EnemyDefinitionId.Evir, index == 7 ? (ushort)1 : (ushort)0);
            // The body bobs at half a pixel per update, one of the two speeds retail Evirs use.
            if (index == 6) place.SetSpawnParameter2(EnemyDefinitionId.Evir, 8);
            place.Bind(index == 8 ? EnemyDefinitionId.EvirProjectile : EnemyDefinitionId.Evir, place.Definition);
            place.Bind(place.DefinitionId, RoomEnemyDefinitionLookup.Get(place.DefinitionId));
            place.Position = place.Position.WithWhole(new(128, 128));
            place.Radius = new(place.Definition.Radius.X, place.Definition.Radius.Y);
            place.Health = place.Definition.Health;
            place.SpritemapPointer = 0x8b59;
            place.Contact = index == 6 ? EnemyContact.None : EnemyContact.IgnoresSamus;
            place.Processing = EnemyProcessing.None;
            place.Presence = EnemyPresence.None;
            if (index == 8)
                enemies.EvirBehavior.InitializeEvirProjectile(place);
            else if (index == 7)
                enemies.EvirBehavior.InitializeEvirArms(place);
            else
                enemies.EvirBehavior.InitializeEvirBody(place, samus);
        }
        var body = enemies.Places.InUpdateOrder[6];
        var arms = enemies.Places.InUpdateOrder[7];
        var projectile = enemies.Places.InUpdateOrder[8];
        AssertEqual(HorizontalDirection.Right, (enemies.Places.InUpdateOrder[6].SpeciesState<EvirBodyState>() ?? throw new InvalidOperationException("Verification requires place 6's Evir state.")).Facing, "live Evir initially faces right");
        // Contact kills the body after the real scheduler has selected all three places.
        // Arms and spit are marked deleted by the native contact tail but still receive
        // their already-scheduled calls before next frame's population pass removes them.
        enemies.StepStandaloneFrame(enemies.StandaloneFrame(fixture.Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        AssertEqual(EnemyDefinitionId.None, body.DefinitionId, "Screw Attack clears Evir body during contact");
        AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "only body counts as a killed enemy");
        AssertTrue(arms.Presence.HasAny(EnemyPresence.Deleted) && projectile.Presence.HasAny(EnemyPresence.Deleted),
            "Evir contact tail marks both following physical slots deleted");
        AssertEqual(1, arms.FrameCounter, "already-selected arms still receive their native final AI call");
        AssertEqual((ushort)0xfffc, arms.Position.X.Whole, "arms read cleared body X and facing rather than stale pre-death values");
        AssertEqual((ushort)10, arms.Position.Y.Whole, "arms use cleared body Y plus native offset");
        AssertEqual((ushort)0xfffc, projectile.Position.X.Whole, "idle spit retains the physical cleared-body alias");
        AssertEqual((ushort)18, projectile.Position.Y.Whole, "idle spit uses cleared body Y plus native offset");
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "normal death explosion remains alive");
        enemies.StepStandaloneFrame(enemies.StandaloneFrame(fixture.Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        AssertEqual(EnemyDefinitionId.None, arms.DefinitionId, "next frame removes deleted arms");
        AssertEqual(EnemyDefinitionId.None, projectile.DefinitionId, "next frame removes deleted spit");
        AssertEqual(0, enemies.ActiveEnemies.Count, "no orphan Evir remains active");
        AssertThrows<InvalidDataException>(() => enemies.EvirBehavior.InitializeEvirArms(arms),
            "initialization still rejects a missing body");
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
