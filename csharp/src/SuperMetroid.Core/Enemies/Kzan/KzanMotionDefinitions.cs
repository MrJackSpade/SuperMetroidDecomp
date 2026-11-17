namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How a Kzan's two parts hold together, how its top falls, rests and rises, and how it feels for Samus beneath it (#2149, #2339).</summary>
internal static class KzanMotionDefinitions
{
    /// <summary>How far below the Kzan's top its lower part follows, in pixels.</summary>
    internal const int BottomBelowTop = 12;

    /// <summary>How far below Samus's middle the Kzan samples her when it tests whether it touches her from below, in pixels.</summary>
    internal const int TouchSampleDrop = 5;

    /// <summary>How long a landed Kzan's top rests before it rises, in updates (#2339).</summary>
    internal const int RiseWait = 64;

    /// <summary>How far a rising Kzan's top moves each update: half a pixel upward (#2339).</summary>
    internal static SignedSixteenSixteen RisingStep { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);

    /// <summary>How much faster a falling Kzan's top falls each update: a sixteenth of a pixel (#2339).</summary>
    internal static SignedSixteenSixteen FallAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(1);

    /// <summary>The fastest a Kzan's top falls: four pixels an update (#2339).</summary>
    internal static SignedSixteenSixteen FastestFall { get; } = SignedSixteenSixteen.FromParts(4, 0);
}
