using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The scenes one Crystal Flash frame published, which decide whether its body and color window show.</summary>
/// <param name="Elapsed">Frames since the Crystal Flash started.</param>
/// <param name="Shown">The scene the frame shows.</param>
/// <param name="BeamsDark">The same scene with Samus's beam inks, which her Crystal Flash body is drawn in, in one colour.</param>
/// <param name="BeamsLight">The same scene with her beam inks in another colour, so a beam-ink pixel that shows differs.</param>
/// <param name="WithoutWindow">The same scene without its color window, or null while no window shows.</param>
/// <param name="CenterX">Samus's horizontal screen position, the middle of the region her body is looked for in.</param>
/// <param name="CenterY">Samus's vertical screen position, the middle of the region her body is looked for in.</param>
internal sealed record CrystalFlashFrameScenes(int Elapsed, LayeredScene Shown, LayeredScene BeamsDark, LayeredScene BeamsLight,
    LayeredScene? WithoutWindow, int CenterX, int CenterY);
