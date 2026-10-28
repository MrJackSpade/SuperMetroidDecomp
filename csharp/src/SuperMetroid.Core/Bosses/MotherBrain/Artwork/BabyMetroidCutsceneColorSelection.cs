namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The Mother Brain cutscene Baby's selected colours: its initial image and its death fade.</summary>
/// <param name="Initial">Compiled fifteen-color image shown before the death fade.</param>
/// <param name="Fade">Six displayed fade rows, with native formula or supplied edited colors.</param>
internal sealed record BabyMetroidCutsceneColorSelection(BabyMetroidInitialPalette Initial, BabyMetroidFade Fade);
