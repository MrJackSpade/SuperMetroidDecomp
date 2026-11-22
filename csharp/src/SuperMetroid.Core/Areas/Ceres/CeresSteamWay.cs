using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// One way a Ceres steam vent blows (#2557): its puff's pictures in order. Every way plays the same vent animation
/// (<see cref="CeresSteamSegment"/>, <see cref="CeresSteamAnimations"/>), so only these differ, and the way's segments and
/// animations are that animation over them.
/// </summary>
internal sealed partial class CeresSteamWay
{
    /// <summary>A vent blowing up.</summary>
    internal static CeresSteamWay Up { get; } = new(
    [
        EnemyFrameId.CeresSteamUp, EnemyFrameId.CeresSteamUpJetBursting, EnemyFrameId.CeresSteamUpJetLengthening, EnemyFrameId.CeresSteamUpJetFull,
        EnemyFrameId.CeresSteamUpPlumeDrifting, EnemyFrameId.CeresSteamUpPlumeScattering, EnemyFrameId.CeresSteamUpPlumeFading,
    ]);

    /// <summary>A vent blowing left.</summary>
    internal static CeresSteamWay Left { get; } = new(
    [
        EnemyFrameId.CeresSteamLeft, EnemyFrameId.CeresSteamLeftJetBursting, EnemyFrameId.CeresSteamLeftJetLengthening, EnemyFrameId.CeresSteamLeftJetFull,
        EnemyFrameId.CeresSteamLeftPlumeDrifting, EnemyFrameId.CeresSteamLeftPlumeScattering, EnemyFrameId.CeresSteamLeftPlumeFading,
    ]);

    /// <summary>A vent blowing down.</summary>
    internal static CeresSteamWay Down { get; } = new(
    [
        EnemyFrameId.CeresSteamDown, EnemyFrameId.CeresSteamDownJetBursting, EnemyFrameId.CeresSteamDownJetLengthening, EnemyFrameId.CeresSteamDownJetFull,
        EnemyFrameId.CeresSteamDownPlumeDrifting, EnemyFrameId.CeresSteamDownPlumeScattering, EnemyFrameId.CeresSteamDownPlumeFading,
    ]);

    /// <summary>A vent blowing right.</summary>
    internal static CeresSteamWay Right { get; } = new(
    [
        EnemyFrameId.CeresSteamRight, EnemyFrameId.CeresSteamRightJetBursting, EnemyFrameId.CeresSteamRightJetLengthening, EnemyFrameId.CeresSteamRightJetFull,
        EnemyFrameId.CeresSteamRightPlumeDrifting, EnemyFrameId.CeresSteamRightPlumeScattering, EnemyFrameId.CeresSteamRightPlumeFading,
    ]);

    /// <summary>Builds the way whose puff shows <paramref name="puff"/>.</summary>
    private CeresSteamWay(IReadOnlyList<EnemyFrameId> puff)
    {
        Segments = CeresSteamAnimations.Blowing(puff);
        StartingAt = CeresSteamAnimations.StartingAt(Segments);
        Activation = CeresSteamAnimations.ActivationOf(StartingAt);
    }

    /// <summary>The vent animation's segments over this way's puff.</summary>
    internal AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind> Segments { get; }

    /// <summary>The animation each segment a vent of this way starts at starts (<see cref="CeresSteamAnimations.StartingAt"/>).</summary>
    internal IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; }

    /// <summary>Where a dormant vent of this way goes on from once its wait runs out (<see cref="CeresSteamAnimations.ActivationOf"/>).</summary>
    internal AnimationPlayback<CeresSteamSegment> Activation { get; }
}
