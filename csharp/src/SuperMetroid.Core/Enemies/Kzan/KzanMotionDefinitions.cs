namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How a Kzan's two parts hold together and how it feels for Samus beneath it (#2149).</summary>
internal static class KzanMotionDefinitions
{
    /// <summary>How far below the Kzan's top its lower part follows, in pixels.</summary>
    internal const int BottomBelowTop = 12;

    /// <summary>
    /// How far below Samus's centre the Kzan reckons her when it tests whether she stands on it, in pixels: five, where
    /// every other enemy reckons three (<see cref="EnemyCollisionDefinitions.SamusStandingBias"/>).
    /// </summary>
    internal const int SamusStandingBias = 5;
}
