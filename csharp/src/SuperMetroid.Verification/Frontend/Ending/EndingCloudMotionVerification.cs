using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks each ending cloud's activation threshold and subsequent native motion.</summary>
internal static class EndingCloudMotionVerification
{
    /// <summary>The scene a cloud's drift reads: only the turning floor's zoom.</summary>
    private sealed class CloudScene : IEndingActorScene
    {
        /// <inheritdoc/>
        public EndingCreditsPhase Phase => EndingCreditsPhase.SideCloudEscape;

        /// <inheritdoc/>
        public FloorZoom FloorZoom { get; set; }

        /// <inheritdoc/>
        public bool ShowsExplodingPlanet => false;
    }

    /// <summary>Checks eight directions, threshold crossing without early movement, and twelve movement steps.</summary>
    internal static void VerifyEndingCloudMotion()
    {
        foreach (EndingSpriteRole role in new[] { EndingSpriteRole.SideCloudRightLeading, EndingSpriteRole.SideCloudRightTrailing,
            EndingSpriteRole.SideCloudLeftLeading, EndingSpriteRole.SideCloudLeftTrailing, EndingSpriteRole.TopCloudPattern,
            EndingSpriteRole.TopCloudEdge, EndingSpriteRole.BottomCloudEdge, EndingSpriteRole.BottomCloudPattern })
        {
            bool side = role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing or EndingSpriteRole.SideCloudLeftLeading or EndingSpriteRole.SideCloudLeftTrailing;
            bool negative = role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing or EndingSpriteRole.BottomCloudEdge or EndingSpriteRole.BottomCloudPattern;
            var sprite = new CinematicActor<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue>(new(200, 200), EndingShades.Drawn(EndingSpriteColouring.Clouds), EndingSceneAnimations.Pictures,
                EndingSceneAnimation.TopCloudPattern);
            var wrapper = new EndingPictureSprite(sprite, role);
            // The cloud moves as the ending steps it: it waits for the zoom's threshold, the update that crosses it starts
            // the drift without moving, and from the next it drifts even once the zoom falls back past the threshold, which
            // would not start it.
            var scene = new CloudScene { FloorZoom = FloorZoomConstants.FromTwoHundredFiftySixths(side ? 95 : 176) };
            wrapper.StepMotion(scene);
            AssertTrue((int)sprite.ScreenPlace.X == 200 && (int)sprite.ScreenPlace.Y == 200, $"{role} waits for native zoom threshold");
            scene.FloorZoom = FloorZoomConstants.FromTwoHundredFiftySixths(side ? 96 : 175);
            wrapper.StepMotion(scene);
            AssertTrue((int)sprite.ScreenPlace.X == 200 && (int)sprite.ScreenPlace.Y == 200, "activation does not run the movement callback early");
            scene.FloorZoom = FloorZoomConstants.FromTwoHundredFiftySixths(side ? 0 : 200);
            for (int frame = 1; frame <= 12; frame++)
            {
                wrapper.StepMotion(scene);
                int direction = negative ? -1 : 1;
                AssertEqual(200 + (side ? direction * frame : 0), (int)sprite.ScreenPlace.X, "native cloud horizontal trajectory");
                AssertEqual(200 + direction * frame * (side ? 2 : 1), (int)sprite.ScreenPlace.Y, "native cloud vertical trajectory persists after threshold reversal");
            }
        }
    }
}
