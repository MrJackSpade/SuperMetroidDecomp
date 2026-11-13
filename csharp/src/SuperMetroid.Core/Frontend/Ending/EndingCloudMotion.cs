namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The clouds' frame behaviours, including the one-call activation handoff.</summary>
internal static class EndingCloudMotion
{
    /// <summary>
    /// Activates side clouds above the zoom threshold and top/bottom clouds below the scene-B
    /// limit. Movement begins on the next call; active side clouds move diagonally while
    /// top and bottom clouds move vertically without retesting zoom.
    /// </summary>
    public static void Step(EndingSprite wrapper, FloorZoom zoom)
    {
        bool side = wrapper.Role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB
            or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudLeftB;
        bool top = wrapper.Role is EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB;
        bool bottom = wrapper.Role is EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB;
        if (!side && !top && !bottom)
            throw new ArgumentException("Actor is not an escape cloud.", nameof(wrapper));
        if (!wrapper.CloudMoving)
        {
            // Changing a frame behaviour does not execute the new callback immediately.
            // Once activated, movement no longer tests the zoom threshold.
            wrapper.CloudMoving = side ? zoom >= EndingMotionDefinitions.CloudMotionStartZoom
                : zoom < EndingMotionDefinitions.CloudSceneBZoomLimit;
            return;
        }
        var sprite = wrapper.Sprite;
        if (side)
        {
            int direction = wrapper.Role is EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudRightB ? -1 : 1;
            sprite.WholePosition += new Offset16<ScreenPixels>(direction, direction * 2);
        }
        else
            sprite.WholePosition += new Offset16<ScreenPixels>(0, top ? 1 : -1);
    }
}
