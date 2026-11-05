namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>What drawing one Crystal Flash frame's scenes found, before its checks are made in frame order.</summary>
/// <param name="Visibility">Whether the frame shows any pixel of the body and of the color window.</param>
/// <param name="HudPreserved">Whether the frame's HUD pixels are the same with and without its color window; true when no window was drawn.</param>
internal readonly record struct CrystalFlashDrawnFrame(VerifyCrystalFlashVisualFrameResult Visibility, bool HudPreserved);
