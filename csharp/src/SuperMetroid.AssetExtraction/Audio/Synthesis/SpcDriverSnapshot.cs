using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The byte layout of a driver-state snapshot (#1305): each driver byte as one byte and each driver word low byte
/// first, as audio RAM holds them; a value with no driver width, such as DSP state, as four bytes. Loop detection
/// compares these snapshots, so a value outside its driver width is a defect in the managed player and fails here
/// rather than producing a different snapshot.
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

    /// <summary>Appends each of <paramref name="values"/> as four little-endian bytes, whatever its range (#1503).</summary>
    public static void AddValues(List<byte> state, ReadOnlySpan<int> values)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        foreach (int value in values)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
            foreach (byte part in bytes)
                state.Add(part);
        }
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
