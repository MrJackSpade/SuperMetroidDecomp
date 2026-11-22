namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The segments of Kraid's arm's normal swing (<see cref="KraidArmState"/>).</summary>
internal static partial class KraidArmSwingDefinitions
{
    /// <summary>The segments an animation of the normal swing starts at: the swing and its pause.</summary>
    internal static readonly KraidPartSegment[] NormalSwing = [KraidPartSegment.ArmNormal, KraidPartSegment.ArmNormalPause];

    /// <summary>The segments an animation starts at that has yet to reach the pause before its first frame shows: the swing.</summary>
    internal static readonly KraidPartSegment[] BeforePause = [KraidPartSegment.ArmNormal];
}
