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
            // The right-side clouds drift left and up, the left-side clouds right and down, two pixels down for one across.
            HorizontalDirection drift = wrapper.Role is EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudRightTrailing
                ? HorizontalDirection.Left : HorizontalDirection.Right;
            sprite.ScreenPlace += drift == HorizontalDirection.Left ? new Offset16<ScreenPixels>(-1, -2) : new Offset16<ScreenPixels>(1, 2);
        }
        else
        {
            // The top clouds drift down, the bottom clouds up, a pixel an update.
            VerticalDirection drift = top ? VerticalDirection.Down : VerticalDirection.Up;
            sprite.ScreenPlace += new Offset16<ScreenPixels>(0, drift == VerticalDirection.Down ? 1 : -1);
        }
    }
}
