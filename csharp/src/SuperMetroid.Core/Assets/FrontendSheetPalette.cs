namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colours of a frontend sheet frame drawn in colours chosen per draw (#1392, #1464): the sheet colours the frame's
/// palette-inheriting parts in colour group <see cref="FrameColourGroup"/>, and a draw in a screen colouring held in
/// another set shows that set's colours for those parts.
/// </summary>
internal static class FrontendSheetPalette
{
    /// <summary>The image colour group a frontend sheet colours each frame's palette-inheriting parts in: its first.</summary>
    internal static ImageColourGroup FrameColourGroup { get; } = ImageColourGroup.Every[0];
}
