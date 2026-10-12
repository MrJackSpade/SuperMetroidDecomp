using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>F455–F4E0 cloud pre-instructions, including the one-call activation handoff.</summary>
internal static class EndingCloudMotion
{
    /// <summary>Steps.</summary>
    public static void Step(EndingSprite wrapper, ushort scale)
    {
        bool side = wrapper.Role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB
            or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudLeftB;
        bool top = wrapper.Role is EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB;
        bool bottom = wrapper.Role is EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB;
        if (!side && !top && !bottom)
            throw new ArgumentException("Actor is not an escape cloud.", nameof(wrapper));
        if (!wrapper.CloudMoving)
        {
            // Changing a pre-instruction does not execute the new callback immediately.
            // Once activated, movement no longer tests the zoom threshold.
            wrapper.CloudMoving = side ? scale >= EndingCreditsRomData.Motion.CloudMotionStartScale
                : scale < EndingCreditsRomData.Motion.CloudSceneBScaleLimit;
            return;
        }
        var sprite = wrapper.Sprite;
        if (side)
        {
            int direction = wrapper.Role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB ? -1 : 1;
            sprite.WholePosition += new Offset16<ScreenPixels>((short)direction, (short)(direction * 2));
        }
        else
            sprite.WholePosition += new Offset16<ScreenPixels>(0, (short)(top ? 1 : -1));
    }
}
