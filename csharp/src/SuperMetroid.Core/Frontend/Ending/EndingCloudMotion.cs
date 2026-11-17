namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The clouds' motion, including the one-update activation handoff.</summary>
internal static class EndingCloudMotion
{
    /// <summary>
    /// Activates side clouds above the zoom threshold and top/bottom clouds below the top-and-bottom-cloud escape's
    /// limit. Movement begins on the next update; active side clouds move diagonally while
    /// top and bottom clouds move vertically without retesting zoom.
    /// </summary>
    public static void Step(EndingSprite wrapper, FloorZoom zoom)
    {
        bool side = wrapper.Role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing
            or EndingSpriteRole.SideCloudLeftLeading or EndingSpriteRole.SideCloudLeftTrailing;
        bool top = wrapper.Role is EndingSpriteRole.TopCloudPattern or EndingSpriteRole.TopCloudEdge;
        bool bottom = wrapper.Role is EndingSpriteRole.BottomCloudEdge or EndingSpriteRole.BottomCloudPattern;
        if (!side && !top && !bottom)
            throw new ArgumentException("Actor is not an escape cloud.", nameof(wrapper));
        if (!wrapper.CloudMoving)
        {
            // Changing a cloud's motion does not run the new motion immediately.
            // Once activated, movement no longer tests the zoom threshold.
            wrapper.CloudMoving = side ? zoom >= EndingMotionDefinitions.CloudMotionStartZoom
                : zoom < EndingMotionDefinitions.TopAndBottomCloudZoomLimit;
            return;
        }
        var sprite = wrapper.Sprite;
        if (side)
        {
            int direction = wrapper.Role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing ? -1 : 1;
            sprite.ScreenPlace += new Offset16<ScreenPixels>(direction, direction * 2);
        }
        else
            sprite.ScreenPlace += new Offset16<ScreenPixels>(0, top ? 1 : -1);
    }
}
