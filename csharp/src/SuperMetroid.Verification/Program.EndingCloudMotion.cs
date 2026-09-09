using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyEndingCloudMotion()
    {
        foreach (EndingSpriteRole role in new[] { EndingSpriteRole.CloudRightA, EndingSpriteRole.CloudRightB,
            EndingSpriteRole.CloudLeftA, EndingSpriteRole.CloudLeftB, EndingSpriteRole.CloudTopA,
            EndingSpriteRole.CloudTopB, EndingSpriteRole.CloudBottomA, EndingSpriteRole.CloudBottomB })
        {
            bool side = role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudLeftB;
            bool negative = role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB or EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB;
            var sprite = new IntroDiscoverySprite(200, 200, 0, 0);
            var wrapper = new EndingSprite(sprite, role);
            EndingCloudMotion.Step(wrapper, (ushort)(side ? 95 : 176));
            AssertTrue(!wrapper.CloudMoving, "cloud waits for native zoom threshold");
            EndingCloudMotion.Step(wrapper, (ushort)(side ? 96 : 175));
            AssertTrue(wrapper.CloudMoving, "cloud switches pre-instruction at threshold");
            AssertEqual((ushort)200, sprite.YPosition, "activation does not run the movement callback early");
            for (int frame = 1; frame <= 12; frame++)
            {
                EndingCloudMotion.Step(wrapper, (ushort)(side ? 0 : 200));
                int direction = negative ? -1 : 1;
                AssertEqual((ushort)(200 + (side ? direction * frame : 0)), sprite.XPosition, "native cloud horizontal trajectory");
                AssertEqual((ushort)(200 + direction * frame * (side ? 2 : 1)), sprite.YPosition, "native cloud vertical trajectory persists after threshold reversal");
            }
        }
    }
}
