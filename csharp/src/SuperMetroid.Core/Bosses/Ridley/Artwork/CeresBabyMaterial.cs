namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The glass, dome and outline inputs the Baby/container material composes its colours from.</summary>
/// <param name="GlassRed">The glass highlight's red.</param>
/// <param name="GlassBlue">The glass highlight's blue.</param>
/// <param name="GlassMiddleRed">The middle glass's red.</param>
/// <param name="GlassGreenStep">How far each glass shade's green and blue fall below the maximum.</param>
/// <param name="DomeTint">The dome's red and blue.</param>
/// <param name="DomeGreen">The brightest dome green.</param>
/// <param name="DomeShadeStep">The green step between dome shades.</param>
/// <param name="OutlineRed">The outline's red.</param>
internal readonly record struct CeresBabyMaterial(int GlassRed, int GlassBlue, int GlassMiddleRed, int GlassGreenStep,
    int DomeTint, int DomeGreen, int DomeShadeStep, int OutlineRed);
