using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>WriteBeamPalette_Y ($90:ACCD), including its long-indirect CPU read context.</summary>
internal static class SamusBeamPaletteLoader
{
    public static void Load(ISnesAddressSpace bus, SnesCgram cgram, ushort pointer)
    {
        for (int color = 0; color < SamusProjectileRomData.Palettes.ColorCount; color++)
        {
            // Each iteration executes a fresh LDA [$00],Y and therefore re-fetches the
            // pointer's bank before its two data reads. The Chainsaw table overrun starts
            // one byte below ROM; returning a global default byte would get color zero wrong.
            ushort word = SnesIndirectLongDataRead.ReadWord(bus,
                (byte)(SamusProjectileRomData.Banks.Movement >> 16), pointer, (ushort)(color * 2));
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + color, word);
        }
    }
}
