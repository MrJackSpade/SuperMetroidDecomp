namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One fixed Ceres explosion offset and initial instruction delay.</summary>
/// <param name="Offset">Screen offset of the blast from the scrolling Mode-7 anchor.</param>
/// <param name="DelayFrames">Updates before the blast's first instruction runs, one through sixteen.</param>
internal readonly record struct CeresExplosionPlacement(Offset16<ScreenPixels> Offset, int DelayFrames);
