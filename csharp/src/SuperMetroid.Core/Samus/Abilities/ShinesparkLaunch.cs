using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// A shinespark's launch (#2553): the directional shinespark pose it starts Samus in and the spark that pose launches. Only
/// the six launches the game has exist, so every launch has its spark.
/// </summary>
public sealed class ShinesparkLaunch
{
    /// <summary>Only the launches declared here exist.</summary>
    /// <param name="pose">The directional shinespark pose the launch starts Samus in.</param>
    /// <param name="phase">The spark the pose launches.</param>
    private ShinesparkLaunch(SamusPoseId pose, ShinesparkPhase phase)
    {
        Pose = pose;
        Phase = phase;
    }

    /// <summary>The directional shinespark pose the launch starts Samus in.</summary>
    public SamusPoseId Pose { get; }

    /// <summary>The spark the launch's pose launches: horizontal, vertical or diagonal.</summary>
    public ShinesparkPhase Phase { get; }

    /// <summary>The horizontal spark facing right.</summary>
    public static ShinesparkLaunch HorizontalRight { get; } = new(SamusPoseId.ShinesparkHorizontalRightPose, ShinesparkPhase.Horizontal);

    /// <summary>The horizontal spark facing left.</summary>
    public static ShinesparkLaunch HorizontalLeft { get; } = new(SamusPoseId.ShinesparkHorizontalLeftPose, ShinesparkPhase.Horizontal);

    /// <summary>The vertical spark facing right.</summary>
    public static ShinesparkLaunch VerticalRight { get; } = new(SamusPoseId.ShinesparkVerticalRightPose, ShinesparkPhase.Vertical);

    /// <summary>The vertical spark facing left.</summary>
    public static ShinesparkLaunch VerticalLeft { get; } = new(SamusPoseId.ShinesparkVerticalLeftPose, ShinesparkPhase.Vertical);

    /// <summary>The diagonal spark facing right.</summary>
    public static ShinesparkLaunch DiagonalRight { get; } = new(SamusPoseId.ShinesparkDiagonalRightPose, ShinesparkPhase.Diagonal);

    /// <summary>The diagonal spark facing left.</summary>
    public static ShinesparkLaunch DiagonalLeft { get; } = new(SamusPoseId.ShinesparkDiagonalLeftPose, ShinesparkPhase.Diagonal);
}
