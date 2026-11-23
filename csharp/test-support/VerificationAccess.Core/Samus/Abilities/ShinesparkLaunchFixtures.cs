using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access: a Shinespark's directional launch by the pose it launches in, for checks that name launches by pose.</summary>
internal static class ShinesparkLaunchFixtures
{
    /// <summary>Every directional launch.</summary>
    private static readonly ShinesparkLaunch[] Launches =
    [
        ShinesparkLaunch.HorizontalRight, ShinesparkLaunch.HorizontalLeft, ShinesparkLaunch.VerticalRight,
        ShinesparkLaunch.VerticalLeft, ShinesparkLaunch.DiagonalRight, ShinesparkLaunch.DiagonalLeft,
    ];

    /// <summary>The launch whose pose is <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No launch is in that pose.</exception>
    internal static ShinesparkLaunch Of(SamusPoseId pose) => Array.Find(Launches, launch => launch.Pose == pose) ??
        throw new ArgumentOutOfRangeException(nameof(pose), pose, "No Shinespark launch is in that pose.");
}
