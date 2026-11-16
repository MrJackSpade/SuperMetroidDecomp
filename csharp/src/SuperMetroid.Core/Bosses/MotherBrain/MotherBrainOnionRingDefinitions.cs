namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's onion rings (#2171): how long one waits at her mouth, how fast it flies and the arc it is aimed within.</summary>
internal static class MotherBrainOnionRingDefinitions
{
    /// <summary>The updates an onion ring stays at Mother Brain's mouth before it flies.</summary>
    internal const int MouthDelay = 8;

    /// <summary>The speed an onion ring flies at along its aim: four pixels and 80 256ths an update.</summary>
    internal static readonly SignedEightEight Speed = SignedEightEight.FromParts(4, 80);

    /// <summary>The lowest aim an onion ring launches at: a sixteenth of a turn.</summary>
    internal static readonly TrigonometryAngle LowestAim = new(16);

    /// <summary>The highest aim an onion ring launches at: nine thirty-seconds of a turn.</summary>
    internal static readonly TrigonometryAngle HighestAim = new(72);

    /// <summary>Where the onion-ring aim clamp stops raising aims to the highest and starts lowering them to the lowest: three quarters of a turn.</summary>
    internal static readonly TrigonometryAngle ClampSplit = new(192);
}
