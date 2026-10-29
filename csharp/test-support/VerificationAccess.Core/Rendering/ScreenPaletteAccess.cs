using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Verification access: a frontend screen's owner-keyed colours at the native boundary (#1422, #1464), for checks that
/// compare or set colours by their cartridge positions against native snapshots and recorded colour memory. A position
/// is routed to the one owner colour it decodes to; reading the whole image builds it from the owners per read.
/// </summary>
internal static class ScreenPaletteAccess
{
    /// <summary>Sets, loads and reads a screen's colours by native position.</summary>
    extension(ScreenPalette self)
    {
        /// <summary>Every colour in the cartridge's colour order, built from the owners per read.</summary>
        internal ReadOnlySpan<RgbColor> Colors => ScreenColourImage.Encode(self);

        /// <summary>The one owner colour native position <paramref name="position"/> decodes to, read and written in place.</summary>
        internal ColourRun AtPosition(int position) => ScreenPaletteColors.AtPosition(self, position);

        /// <summary>Writes the colour at native position <paramref name="index"/>.</summary>
        public void SetColor(int index, RgbColor color) => self.AtPosition(index).Write(0, color);

        /// <summary>Loads consecutive stored colour words from native position <paramref name="destinationIndex"/> onward.</summary>
        public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0) => self.AsPaletteColors().LoadBytes(bytes, destinationIndex);

        /// <summary>The colour at native position <paramref name="index"/> expanded to 8-bit RGBA.</summary>
        public RgbaPixel GetRgba(int index) => self.AtPosition(index)[0].ToRgbaPixel();

        /// <summary>The screen's colours viewed by native position, as the import pipeline's palette views take them.</summary>
        public IPaletteColors AsPaletteColors() => new ScreenPaletteColors(self);

        /// <summary>
        /// Gives each owner the colours of <paramref name="image"/>, a native 256-colour image, at the positions it holds.
        /// </summary>
        [WholeImageFixture("every screen colour from a 256-colour native colour image")]
        internal void LoadImage(ReadOnlySpan<RgbColor> image) => ScreenColourImage.Decode(image).LoadTo(self);
    }
}
