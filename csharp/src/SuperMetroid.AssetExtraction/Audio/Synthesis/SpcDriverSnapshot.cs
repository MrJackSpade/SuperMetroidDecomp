using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The byte layout of a driver-state snapshot (#1305): each driver byte as one byte and each driver word low byte
/// first, as audio RAM holds them. Loop detection compares these snapshots, so a value outside its driver width is a
/// defect in the managed player and fails here rather than producing a different snapshot.
/// </summary>
[EncodingOwner("sound driver state snapshot bytes")]
internal static class SpcDriverSnapshot
{
    /// <summary>Appends each driver byte in <paramref name="values"/>, in order.</summary>
    public static void AddBytes(List<byte> state, ReadOnlySpan<int> values)
    {
        foreach (int value in values)
            state.Add(checked((byte)value));
    }

    /// <summary>Appends each driver word in <paramref name="words"/>, in order, low byte first.</summary>
    public static void AddWords(List<byte> state, ReadOnlySpan<int> words)
    {
        foreach (int word in words)
        {
            ushort value = checked((ushort)word);
            state.Add(LittleEndianBytes.LowByte(value));
            state.Add(LittleEndianBytes.HighByte(value));
        }
    }
}
