namespace SuperMetroid.Core.Frontend;

/// <summary>One fixed Ceres explosion offset and initial instruction delay.</summary>
internal readonly record struct CeresExplosionPlacement(Offset16<ScreenPixels> Offset, ushort DelayFrames);
