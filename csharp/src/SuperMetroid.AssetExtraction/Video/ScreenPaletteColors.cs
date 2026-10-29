using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// A frontend screen's own colours addressed by cartridge palette position (#1422, #1464), so the import pipeline's
/// palette views and its decoders of stored colour words can fill a screen's owners the way they fill an import scratch
/// CGRAM. A position is routed to the one owner colour it decodes to; reading every colour builds the image from the
/// owners per read.
/// </summary>
/// <param name="screen">The screen's colours.</param>
internal sealed class ScreenPaletteColors(ScreenPalette screen) : IPaletteColors
{
    /// <inheritdoc />
    public ReadOnlySpan<RgbColor> Colors => ScreenColourImage.Encode(screen);

    /// <inheritdoc />
    public void SetColor(int index, RgbColor color) => AtPosition(screen, index).Write(0, color);

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
        int count = bytes.Length / RgbColor.ByteCount;
        Ensure.BetweenInclusive(0, ScreenColourImage.ColorCount - count, destinationIndex);
        for (int color = 0; color < count; color++)
            SetColor(destinationIndex + color,
                RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1])));
    }

    /// <summary>The one owner colour of <paramref name="screen"/> that cartridge position <paramref name="position"/> decodes to.</summary>
    internal static ColourRun AtPosition(ScreenPalette screen, int position)
    {
        Ensure.NotNull(screen);
        Ensure.BetweenInclusive(0, ScreenColourImage.ColorCount - 1, position);
        return position < GameplayColourPositionDefinitions.SpriteSetsFirst
            ? screen.BackgroundSets.All.Slice(position - GameplayColourPositionDefinitions.BackgroundSetsFirst, 1)
            : screen.SpriteSets.All.Slice(position - GameplayColourPositionDefinitions.SpriteSetsFirst, 1);
    }
}
