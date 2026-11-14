using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks each ending cloud's activation threshold and subsequent native motion.</summary>
internal static class EndingCloudMotionVerification
{
    /// <summary>Checks eight directions, threshold crossing without early movement, and twelve movement steps.</summary>
    internal static void VerifyEndingCloudMotion()
    {
        foreach (EndingSpriteRole role in new[] { EndingSpriteRole.SideCloudRightLeading, EndingSpriteRole.SideCloudRightTrailing,
            EndingSpriteRole.SideCloudLeftLeading, EndingSpriteRole.SideCloudLeftTrailing, EndingSpriteRole.TopCloudPattern,
            EndingSpriteRole.TopCloudEdge, EndingSpriteRole.BottomCloudEdge, EndingSpriteRole.BottomCloudPattern })
        {
            bool side = role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing or EndingSpriteRole.SideCloudLeftLeading or EndingSpriteRole.SideCloudLeftTrailing;
            bool negative = role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing or EndingSpriteRole.BottomCloudEdge or EndingSpriteRole.BottomCloudPattern;
            var sprite = new IntroDiscoverySprite(new(200, 200), EndingShades.Drawn(EndingSpriteColouring.Clouds), 0);
            var wrapper = new EndingSprite(sprite, role);
            EndingCloudMotion.Step(wrapper, FloorZoom.FromTwoHundredFiftySixths(side ? 95 : 176));
            AssertTrue(!wrapper.CloudMoving, "cloud waits for native zoom threshold");
            EndingCloudMotion.Step(wrapper, FloorZoom.FromTwoHundredFiftySixths(side ? 96 : 175));
            AssertTrue(wrapper.CloudMoving, "cloud switches pre-instruction at threshold");
            AssertEqual((ushort)200, sprite.WholePosition.Y, "activation does not run the movement callback early");
            for (int frame = 1; frame <= 12; frame++)
            {
                EndingCloudMotion.Step(wrapper, FloorZoom.FromTwoHundredFiftySixths(side ? 0 : 200));
                int direction = negative ? -1 : 1;
                AssertEqual((ushort)(200 + (side ? direction * frame : 0)), sprite.WholePosition.X, "native cloud horizontal trajectory");
                AssertEqual((ushort)(200 + direction * frame * (side ? 2 : 1)), sprite.WholePosition.Y, "native cloud vertical trajectory persists after threshold reversal");
            }
        }
    }
}
