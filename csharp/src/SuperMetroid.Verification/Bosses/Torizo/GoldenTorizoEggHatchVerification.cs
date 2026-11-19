using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #2021: a Golden Torizo egg hatches into the hatching animation of the side it was laid facing, by name, whichever way
/// it has rolled since, and charges the way it faces now.
/// </summary>
internal static class GoldenTorizoEggHatchVerification
{
    /// <summary>
    /// An egg laid facing one way that has turned about on a wall hatches showing its laid side and charges the other way;
    /// the cartridge's rolling list wakes into the same hatching animation (<see cref="TorizoProjectileAnimationsVerification"/>).
    /// </summary>
    internal static void VerifyGoldenTorizoEggHatchesOnItsLaidSide()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256],
            new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid(), []);
        foreach (bool laidFacingRight in new[] { true, false })
        {
            EnemyProjectile egg = enemies.Projectiles.InPlace(0);
            FamilyProjectileStarts.Initialize(enemies, egg, RoomEnemyProjectileKind.GoldenTorizoEgg, SpriteGraphics.SharedStandard);
            egg.StartAnimation(new TorizoProjectileAnimation(laidFacingRight
                ? TorizoProjectileSegment.EggBouncingRight
                : TorizoProjectileSegment.EggBouncingLeft));
            // The egg has rolled into a wall and turned about; its hatch timer runs out this update.
            var eggState = new GoldenTorizoEggState(laidFacingRight ? HorizontalDirection.Right : HorizontalDirection.Left)
            {
                Facing = laidFacingRight ? HorizontalDirection.Left : HorizontalDirection.Right,
            };
            eggState.HatchTimer.LastFor(1);
            egg.State = eggState;

            TorizoAttacks.BounceEgg(egg, level);

            TorizoProjectileSegment hatching = laidFacingRight ? TorizoProjectileSegment.EggHatchingRight : TorizoProjectileSegment.EggHatchingLeft;
            AssertTrue(egg.Animation is TorizoProjectileAnimation { PlayingSegment: var shown } && shown == hatching,
                $"an egg laid facing {(laidFacingRight ? "right" : "left")} hatches into {hatching}; it shows {egg.Animation}");
            AssertEqual(SignedEightEight.FromPixels(laidFacingRight ? -1 : 1), egg.Velocity.X,
                "the hatched egg charges the way it faces after turning about");
        }
        Console.WriteLine("Golden Torizo egg hatch: each egg hatches showing the side it was laid facing and charges the way it faces.");
    }
}
