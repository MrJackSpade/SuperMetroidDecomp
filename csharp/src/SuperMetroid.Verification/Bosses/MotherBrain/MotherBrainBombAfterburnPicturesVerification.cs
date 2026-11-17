using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks #1537: the afterburn an expiring Mother Brain bomb leaves, which Ridley's projectile code spawns with the room's
/// shared enemy-projectile graphics, draws every frame of its chain in Mother Brain's room: the horizontal centre, the
/// directional afterburns it sends out and their final puff on hitting a wall, each from the room's installed sheets.
/// </summary>
internal static class MotherBrainBombAfterburnPicturesVerification
{
    /// <summary>The afterburn chain's animations: the centre a bomb leaves, the afterburns it sends out, their final puff.</summary>
    private static readonly RidleyFireballSegment[] ChainSegments =
        [RidleyFireballSegment.HorizontalCenter, RidleyFireballSegment.DirectionalAfterburn, RidleyFireballSegment.AfterburnFinal];

    /// <summary>Draws each frame of the chain in Mother Brain's room through the production spawned-object draw.</summary>
    internal static void VerifyMotherBrainBombAfterburnPictures()
    {
        EnemyPresentation presentation = SpawnedObjectFixtures.Presentation(TourianRoomStateDefinitions.MotherBrainDefault);
        // The graphics Ridley's projectile code spawns every fireball and afterburn stage with.
        SpriteGraphics graphics = SpriteGraphics.SharedIn(EnemyShades.InProjectileLook);
        EnemyProjectileFrameId[] frames = ChainSegments
            .SelectMany(segment => RidleyFireballAnimations.Segments.Beats(segment))
            .OfType<AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Frame>()
            .Select(beat => beat.Shown)
            .Distinct()
            .ToArray();
        // The three stages show one set of five pictures, each stage's copies one frame since #2234.
        VerificationAssertions.AssertEqual(5, frames.Length, "the afterburn chain's three stages show the same five frames");
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        foreach (var frame in frames)
        {
            // Fails loudly, naming the sheets, when the room's sheets hold no picture of the frame.
            presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile), EnemySheetDefinitions.ProjectileFrameName(frame),
                new Offset16<ScreenPixels>(128, 128), graphics);
        }
        sprites.FinalizeFrame();
        VerificationAssertions.AssertTrue(sprites.Draws.Length > 0, "the afterburn chain draws pictures in Mother Brain's room");
        Console.WriteLine($"  Mother Brain bomb afterburn: {frames.Length} chain frames resolve on Mother Brain's room's sheets.");
    }
}
