using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Verification's bank-$8D view of palette-FX colours: every record colour's address, resolved through the import
/// pipeline's record sources, so the ROM oracles read, compare and look up colours by the words the cartridge stores.
/// </summary>
internal static class PaletteFxColorAddressAccess
{
    /// <summary>Every record colour, by its bank-$8D address.</summary>
    private static readonly Dictionary<ushort, PaletteFxColorKey> Keys = BuildKeys();

    /// <summary>Walks every record's written colours.</summary>
    private static Dictionary<ushort, PaletteFxColorKey> BuildKeys()
    {
        var keys = new Dictionary<ushort, PaletteFxColorKey>();
        for (int record = 0; record < PaletteFxRecordSourcesConstants.RecordCount; record++)
        {
            var shown = new PaletteFxRecord(record);
            int colors = PaletteFxRecordDefinitions.Layout(shown).Where(part => !part.Skip).Sum(part => part.Count);
            for (int color = 0; color < colors; color++)
            {
                var key = new PaletteFxColorKey(shown, color);
                keys.Add(PaletteFxRecordSources.ColorAddress(key), key);
            }
        }
        return keys;
    }

    /// <summary>The bank-$8D address of <paramref name="key"/>'s colour word.</summary>
    internal static ushort Address(PaletteFxColorKey key) => PaletteFxRecordSources.ColorAddress(key);

    /// <summary>The record colour at bank-$8D <paramref name="pointer"/>, when the word is one.</summary>
    internal static bool TryKeyAt(int pointer, out PaletteFxColorKey key) => Keys.TryGetValue(unchecked((ushort)pointer), out key);

    /// <summary>The record colour at bank-$8D <paramref name="pointer"/>.</summary>
    internal static PaletteFxColorKey KeyAt(int pointer) => TryKeyAt(pointer, out PaletteFxColorKey key)
        ? key
        : throw new InvalidDataException($"$8D:{pointer:X4} is no palette-FX record colour.");

    /// <summary><paramref name="colors"/> keyed by record colour.</summary>
    internal static Dictionary<PaletteFxColorKey, RgbColor> Keyed(IReadOnlyDictionary<ushort, RgbColor> colors) =>
        colors.ToDictionary(pair => KeyAt(pair.Key), pair => pair.Value);

    /// <summary>Colour sources by address.</summary>
    extension(IPaletteFxColorSource source)
    {
        /// <summary>The colour at bank-$8D <paramref name="pointer"/>; false when the word is no record colour.</summary>
        internal bool TryReadColor(int pointer, out RgbColor color)
        {
            color = RgbColor.Black;
            return TryKeyAt(pointer, out PaletteFxColorKey key) && source.TryReadColor(key, out color);
        }
    }

    /// <summary>Keyed colour tables by address.</summary>
    extension(IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors)
    {
        /// <summary>Whether the table holds the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal bool ContainsKey(ushort pointer) => TryKeyAt(pointer, out PaletteFxColorKey key) && colors.ContainsKey(key);

        /// <summary>The table's colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal bool TryGetValue(ushort pointer, out RgbColor value)
        {
            value = RgbColor.Black;
            return TryKeyAt(pointer, out PaletteFxColorKey key) && colors.TryGetValue(key, out value);
        }

        /// <summary>The bank-$8D addresses of the table's colours.</summary>
        internal IEnumerable<ushort> Pointers => colors.Keys.Select(Address);
    }
}

