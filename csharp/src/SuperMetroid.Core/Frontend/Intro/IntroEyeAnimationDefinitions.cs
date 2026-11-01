namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Where the portrait eye is drawn, and where the original eye's background-object lists and draw records lie.
/// The simulation runs <see cref="IntroEyeAnimations"/> and draws at <see cref="PortraitCell"/>; the list and record
/// locations are read only by the eye artwork extraction and by the verification that checks the typed programs.
/// </summary>
internal static class IntroEyeAnimationDefinitions
{
    /// <summary>The portrait tilemap cell at which every eye frame's top-left tile is drawn: column 17, row 13.</summary>
    internal static Position16<TilemapCells> PortraitCell => new(17, 13);
}
