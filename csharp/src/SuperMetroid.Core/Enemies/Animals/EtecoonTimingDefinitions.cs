namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How long an Etecoon's waits last and how it falls (#2149).</summary>
internal static partial class EtecoonTimingDefinitions
{
    /// <summary>
    /// How long a woken Etecoon flexes before its first hop, in updates: the longest wait an Etecoon makes.
    /// </summary>
    internal const int FlexDuration = 256;

    /// <summary>
    /// How long an Etecoon crouches before a hop, in updates: before each first hop, after landing a wall-jump route, on
    /// reaching its teaching spot and between teaching hops.
    /// </summary>
    internal const int HopCrouch = 11;

    /// <summary>How long an Etecoon that has hopped up beside Samus looks at her before it runs, in updates.</summary>
    internal const int FaceSamusPause = 32;

    /// <summary>How long an Etecoon clings to a wall before it jumps off, in updates.</summary>
    internal const int WallPause = 8;

    /// <summary>How long an Etecoon's hop holds its pose at the crest, in updates.</summary>
    internal const int CrestHold = 3;

    /// <summary>The falling speed, five pixels per update, from which an Etecoon stops falling faster.</summary>
    internal static readonly SignedSixteenSixteen FallingSpeedCap = SignedSixteenSixteen.FromWholePixels(5);
}
