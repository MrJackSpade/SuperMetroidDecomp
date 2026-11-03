using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Replacements for the reserve arrow's two starting colours.</summary>
/// <param name="Color6">The color6.</param>
/// <param name="Color11">The color11.</param>
public readonly record struct ReserveArrowColorEdits(RgbColor? Color6, RgbColor? Color11);
