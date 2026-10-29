using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access: gameplay's owner-keyed colours at the native boundary (#1405, #1464), for checks that compare
/// or set colours by their cartridge positions. A position is routed to the one owner colour it decodes to, so a read or
/// write of one colour touches only that colour; reading the whole image builds it from the owners' shown sets.
/// </summary>
internal static class GameplayColoursAccess
{
    /// <summary>Verification access to <see cref="GameplayColours"/> by native position.</summary>
    extension(GameplayColours self)
    {
        /// <summary>Every colour as a frame shows it now, in the cartridge's colour order, built from the owners per read.</summary>
        internal ReadOnlySpan<RgbColor> Colors => NativeColourImage.Encode(self.ShownBackgrounds(), self.ShownSprites());

        /// <summary>The colours as colour memory.</summary>
        public IPaletteColors AsPaletteColors() => new PositionalColours(self);

        /// <summary>The one owner colour native position <paramref name="position"/> decodes to, read and written in place.</summary>
        internal ColourRun AtPosition(int position)
        {
            Ensure.BetweenInclusive(0, NativeColourImage.ColorCount - 1, position);
            if (position < GameplayColourPositionDefinitions.SpriteSetsFirst)
                return NativeBackgroundSets.At(self.BackgroundSets, position - GameplayColourPositionDefinitions.BackgroundSetsFirst);
            int sprite = position - GameplayColourPositionDefinitions.SpriteSetsFirst;
            int set = sprite / ColourSets.SetColours;
            int colour = sprite % ColourSets.SetColours;
            foreach (IGameplayColourOwner owner in self.owners)
            {
                if (owner is SamusColours samus && set == SamusColourDefinitions.SuitColourGroup.Number)
                    return samus.Suit.Slice(colour, 1);
                if (owner is SamusColours beams && set == SamusColourDefinitions.BeamColourGroup.Number)
                    return beams.Beam.Slice(colour, 1);
            }
            return self.SpriteSets.All.Slice(sprite, 1);
        }

        /// <summary>
        /// Gives each owner the colours of <paramref name="image"/>, a native 256-colour image, at the positions it holds,
        /// as the cartridge's initial load does.
        /// </summary>
        [WholeImageFixture("every gameplay colour from a 256-colour native colour image")]
        internal void LoadStartingImage(ReadOnlySpan<RgbColor> image)
        {
            Ensure.LengthEqual(image, NativeColourImage.ColorCount);
            for (int position = 0; position < NativeColourImage.ColorCount; position++)
                self.AtPosition(position).Write(0, image[position]);
        }
    }

    /// <summary>Gameplay's colours read and written by native position.</summary>
    private sealed class PositionalColours(GameplayColours colours) : IPaletteColors
    {
        /// <inheritdoc />
        public ReadOnlySpan<RgbColor> Colors => colours.Colors;

        /// <inheritdoc />
        public void SetColor(int index, RgbColor color) => colours.AtPosition(index).Write(0, color);

        /// <inheritdoc />
        public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
        {
            if ((bytes.Length & 1) != 0)
                throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
            int count = bytes.Length / RgbColor.ByteCount;
            if (destinationIndex < 0 || destinationIndex + count > NativeColourImage.ColorCount)
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "A palette load must remain within 256 colours.");
            for (int color = 0; color < count; color++)
                SetColor(destinationIndex + color, RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1])));
        }

        /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
        public RgbaPixel GetRgba(int index) => colours.AtPosition(index)[0].ToRgbaPixel();
    }
}
