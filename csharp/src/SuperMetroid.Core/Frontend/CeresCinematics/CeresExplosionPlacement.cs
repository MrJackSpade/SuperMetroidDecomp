namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres destruction blast: where it appears and when.</summary>
/// <param name="Offset">Screen offset of the blast from the scrolling turning-floor anchor.</param>
/// <param name="DelayFrames">Updates before the blast's first picture shows, one through sixty-four.</param>
internal readonly record struct CeresExplosionPlacement(Offset16<ScreenPixels> Offset, int DelayFrames);
