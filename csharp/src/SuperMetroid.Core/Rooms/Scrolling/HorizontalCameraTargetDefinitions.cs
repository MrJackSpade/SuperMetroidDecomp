namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Where on the screen, in pixels from its left edge, each camera distance mode aims to keep Samus.</summary>
internal static partial class HorizontalCameraTargetDefinitions
{
    /// <summary>
    /// How far behind the screen's centre normal tracking keeps Samus, toward the side she faces away from, so she sees
    /// further ahead of her.
    /// </summary>
    internal const int NormalTrackingLead = 32;

    /// <summary>Where boss tracking keeps Samus while she faces right.</summary>
    internal const int BossTrackingFacingRightX = 64;

    /// <summary>Where boss tracking keeps Samus while she faces left.</summary>
    internal const int BossTrackingFacingLeftX = 80;

    /// <summary>How far in from the screen's right edge the right-edge mode keeps Samus, whichever way she faces.</summary>
    internal const int RightEdgeInset = 32;
}
