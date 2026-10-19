using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Enemies;

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

    /// <summary>The loaded Kraid, if any.</summary>
    KraidEnemyState? Kraid { get; }

    /// <summary>Crocomire's BG2 scroll while Crocomire is loaded; otherwise null.</summary>
    Position16<Bg2Pixels>? CrocomireBg2Scroll { get; }

    /// <summary>The morph-ball eye's beam.</summary>
    MorphBallEyeBeamState MorphBallEyeBeam { get; }

    /// <summary>The morph-ball eye states by slot.</summary>
    IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates { get; }
}
