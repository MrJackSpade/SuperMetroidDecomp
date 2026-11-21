using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #2558: Mother Brain's mouth laser takes the damage of the enemy the room drew last before her brain fired it, which in
/// her phase-two fight is her body: it deals her body's definition's damage, not her brain's, whatever an installation
/// gives either.
/// </summary>
internal static class MotherBrainMouthLaserDamageVerification
{
    /// <summary>Her body's damage, unlike her brain's, so the laser shows whose it takes.</summary>
    private const int BodyDamage = 37;

    /// <summary>Her brain's damage, unlike her body's.</summary>
    private const int BrainDamage = 99;

    /// <summary>Plays her brain's laser attack until it fires and checks the laser carries her body's damage.</summary>
    internal static void VerifyMotherBrainMouthLaserDamage()
    {
        var enemies = EnemySystemFixtures.New(MotherBrainBrainDraws.Session(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace bodyPlace = enemies.Places.InUpdateOrder[0];
        bodyPlace.Bind(EnemyDefinitionId.MotherBrainBody, EnemyDefinitionFixtures.Blank with { Damage = BodyDamage });
        RoomEnemyPlace brainPlace = enemies.Places.InUpdateOrder[1];
        brainPlace.Bind(EnemyDefinitionId.MotherBrainHead, EnemyDefinitionFixtures.Blank with { Damage = BrainDamage });
        var state = new MotherBrainEnemyState(bodyPlace.HeldOccupant(), brainPlace, new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation());
        enemies.MotherBrainEncounter.BeginAsSetUp(state);
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.Laser);

        int draws = MotherBrainAnimations.HeadSegments.Beats(MotherBrainHeadSegment.Laser)
            .OfType<SuperMetroid.Core.Animation.AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame>().Sum(frame => frame.Duration);
        EnemyProjectile? laser = null;
        for (int draw = 0; draw < draws && laser is null; draw++)
        {
            enemies.DrawMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
            laser = enemies.Projectiles.EnemyProjectiles.SingleOrDefault(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser);
        }

        EnemyProjectile fired = laser ?? throw new InvalidOperationException("Verification failed: her brain's laser attack fired no laser.");
        AssertEqual(BodyDamage, fired.Damage, "the mouth laser deals her body's definition's damage, the enemy the room drew last");
        Console.WriteLine("Mother Brain mouth laser damage: the laser takes her body's definition's damage.");
    }
}
