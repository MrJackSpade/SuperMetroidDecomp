using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Where each palette-FX record and colour lives in bank $8D: the import pipeline's map from keys to the cartridge.</summary>
internal static class PaletteFxRecordSources
{
    /// <summary>Each record's timer-word address, by record.</summary>
    internal static readonly Dictionary<PaletteFxFrame, ushort> Addresses = PaletteFxRecordSourceDefinitions.ByAddress.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The bank-$8D address of <paramref name="frame"/>'s timer word.</summary>
    internal static ushort Address(PaletteFxFrame frame) => Addresses[frame];

    /// <summary>
    /// The bank-$8D address of the colour word <paramref name="key"/> names: after the record's timer word, counting each
    /// written colour and each skip command as one word.
    /// </summary>
    internal static ushort ColorAddress(PaletteFxColorKey key)
    {
        int word = Address(key.Frame) + sizeof(ushort);
        int ordinal = 0;
        foreach (PaletteFxRecordPart part in PaletteFxRecordDefinitions.Layout(key.Frame))
        {
            if (part.Skip)
            {
                word += sizeof(ushort);
                continue;
            }
            if (key.Color < ordinal + part.Count)
                return checked((ushort)(word + (key.Color - ordinal) * sizeof(ushort)));
            word += part.Count * sizeof(ushort);
            ordinal += part.Count;
        }
        throw new ArgumentOutOfRangeException(nameof(key), key, "The record writes no such colour.");
    }
}
