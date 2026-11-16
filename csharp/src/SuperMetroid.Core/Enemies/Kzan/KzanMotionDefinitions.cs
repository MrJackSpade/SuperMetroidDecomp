namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How a Kzan's two parts hold together and how it feels for Samus beneath it (#2149).</summary>
internal static class KzanMotionDefinitions
{
    /// <summary>How far below the Kzan's top its lower part follows, in pixels.</summary>
    internal const int BottomBelowTop = 12;

    /// <summary>How far below Samus's middle the Kzan samples her when it tests whether it touches her from below, in pixels.</summary>
    internal const int TouchSampleDrop = 5;
}
