using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access: gameplay's colours presented as colour memory, for checks that compare or set colours by their
/// cartridge positions (#1405). Reads show the composition; a write changes the colour at its position by reloading the
/// whole image through the owners, as the cartridge's initial load does.
/// </summary>
internal static class GameplayColoursAccess
{
    /// <summary>Verification access to <see cref="GameplayColours"/> by position.</summary>
    extension(GameplayColours self)
    {
        /// <summary>The colours as colour memory.</summary>
        public IPaletteColors AsPaletteColors() => new PositionalColours(self);
    }

    /// <summary>Gameplay's colours read and written by position.</summary>
    private sealed class PositionalColours(GameplayColours colours) : IPaletteColors
    {
        /// <inheritdoc />
        public ReadOnlySpan<RgbColor> Colors => colours.Colors;

        /// <inheritdoc />
        public void SetColor(int index, RgbColor color)
        {
            RgbColor[] image = colours.Colors.ToArray();
            image[Ensure.BetweenInclusive(0, GameplayColours.ColorCount - 1, index)] = color;
            colours.LoadStartingImage(image);
        }

        /// <inheritdoc />
        public RgbaPixel GetRgba(int index) => colours.Colors[Ensure.BetweenInclusive(0, GameplayColours.ColorCount - 1, index)].ToRgbaPixel();

        /// <inheritdoc />
        public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
        {
            if ((bytes.Length & 1) != 0)
                throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
            int count = bytes.Length / 2;
            if (destinationIndex < 0 || destinationIndex + count > GameplayColours.ColorCount)
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "A palette load must remain within 256 colours.");
            RgbColor[] image = colours.Colors.ToArray();
            for (int color = 0; color < count; color++)
                image[destinationIndex + color] = RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1]));
            colours.LoadStartingImage(image);
        }
    }
}
