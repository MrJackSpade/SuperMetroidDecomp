using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Bombs set off over terrain as the game sets them off, for checks of what a bomb's or a Power Bomb's blast does to the
/// blocks it reaches.
/// </summary>
internal static class BombExplosions
{
    /// <summary>
    /// Lays a normal bomb at the centre of <paramref name="block"/>, as a Samus with Bombs standing there pressing Shoot lays
    /// it, and runs the bombs' updates until its fuse burns out and its explosion's cross reacts with the block and the four
    /// around it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The bomb was not laid.</exception>
    internal static void ExplodeBombAt(SamusTerrain terrain, Position16<RoomBlocks> block)
    {
        SamusState samus = InstalledFixtures.CreateSamus();
        samus.EquippedItems |= SamusEquipmentFlags.Bombs;
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new(block.X * 16 + 8, block.Y * 16 + 8));
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), InstalledFixtures.SamusArtwork());
        bombs.TryPlaceBomb(samus, GameplayAction.Shoot, GameplayAction.Shoot, out _);
        if (bombs.Bombs.All(bomb => !bomb.IsActive))
            throw new InvalidOperationException($"No bomb was laid at block {block}.");
        // The update the fuse burns out on reacts with the cross; the fuse lasts its whole timer from the bomb's laying.
        for (int update = 0; update <= MorphBallBombDefinitions.InitialBombTimer; update++)
            bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);
    }

    /// <summary>
    /// The first reaching update of a Power Bomb blast at the centre of <paramref name="block"/>, whose reach still lies inside
    /// that block: its boundary's four edges each react with the block.
    /// </summary>
    internal static void ReachBlockWithPowerBomb(SamusTerrain terrain, Position16<RoomBlocks> block) =>
        SamusBombProjectileSystem.ReactToPowerBombBoundary(terrain, new(block.X * 16 + 8, block.Y * 16 + 8), PowerBombRadius.FromWholePixels(0),
            SamusProjectileType.Of(SamusProjectileFamily.PowerBomb));
}
