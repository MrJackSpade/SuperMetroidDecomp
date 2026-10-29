using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The live enemy state the NMI display latch, actor drawing and window registers read at their own points. Reads are
/// live; the latch that captures them stays latched.
/// </summary>
internal interface IEnemyDisplaySources
{
    /// <summary>The loaded Mother Brain, if any.</summary>
    MotherBrainEnemyState? MotherBrain { get; }

    /// <summary>The loaded Draygon, if any.</summary>
    DraygonEnemyState? Draygon { get; }

    /// <summary>The loaded Phantoon, if any.</summary>
    PhantoonEnemyState? Phantoon { get; }

    /// <summary>Ceres Ridley, if loaded.</summary>
    RidleyEnemyState? CeresRidley { get; }

    /// <summary>
    /// The body layer a boss shows (#1352, #1353, #1466): Kraid's while he owns it, otherwise the one a boss attached;
    /// null without either.
    /// </summary>
    BossBodyLayer? BossBody { get; }

    /// <summary>Where Kraid's body layer is placed while it is shown (#1352); otherwise null.</summary>
    Position16<BackgroundPixels>? KraidBodyPlacement { get; }

    /// <summary>Crocomire's BG2 scroll while Crocomire is loaded; otherwise null.</summary>
    Position16<BackgroundPixels>? CrocomireBg2Scroll { get; }

    /// <summary>The morph-ball eye's beam.</summary>
    MorphBallEyeBeamState MorphBallEyeBeam { get; }
}
