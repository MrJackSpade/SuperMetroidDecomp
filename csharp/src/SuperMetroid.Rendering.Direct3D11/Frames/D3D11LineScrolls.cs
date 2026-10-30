using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// Packs a pass's described line scrolls (#1468) into the constant rows after the per-line rows, for Tiles.hlsl's
/// <c>LineScroll</c>: four slots, each one axis of one plane (a pass's own plane horizontally and vertically, then the
/// plane an X-ray pass blends with), and each slot's data rows. A slot holds a sine wave, a waveform of samples, a
/// stretch, or bands of scroll (one scroll on every line when there are none); the shader evaluates each line's scroll
/// from it, so no table of lines is uploaded.
/// </summary>
internal static class D3D11LineScrolls
{
    /// <summary>The slot of a pass's own plane's horizontal scroll.</summary>
    internal const int Horizontal = 0;
    /// <summary>The slot of a pass's own plane's vertical scroll.</summary>
    internal const int Vertical = 1;
    /// <summary>The slot of the horizontal scroll of the plane an X-ray pass blends with.</summary>
    internal const int BlendPlaneHorizontal = 2;
    /// <summary>The slot of the vertical scroll of the plane an X-ray pass blends with.</summary>
    internal const int BlendPlaneVertical = 3;
    /// <summary>The slots, each a row: its kind with its base scroll in the high half; its band's first line with the line after its last in the high half; its scale; its shape word.</summary>
    private const int Slots = 4;
    /// <summary>Words in a constant row.</summary>
    private const int RowWords = 4;
    /// <summary>The data rows each slot owns after the slots' rows: a waveform's samples, or a list of bands.</summary>
    private const int DataRowsPerSlot = 5;
    /// <summary>The data words each slot owns.</summary>
    private const int DataWords = DataRowsPerSlot * RowWords;
    /// <summary>A waveform's samples packed in one data word, a signed byte each from the low byte up.</summary>
    private const int SamplesPerWord = 4;
    /// <summary>The bits of one packed sample.</summary>
    private const int SampleBits = 8;
    /// <summary>The bit the high half of a word starts at.</summary>
    private const int HighHalf = 16;
    /// <summary>The words the slots and their data rows take, which must fit after the per-line rows.</summary>
    internal const int Words = (Slots + Slots * DataRowsPerSlot) * RowWords;

    /// <summary>Packs one slot: <paramref name="shape"/>, or <paramref name="ownScroll"/> on every line when it is null.</summary>
    /// <param name="data">The pass's constant words.</param>
    /// <param name="slot">The slot to pack.</param>
    /// <param name="shape">The axis's described line scroll, or null.</param>
    /// <param name="ownScroll">The plane's own scroll on the axis, which every line shows without a shape.</param>
    /// <param name="bias">A scroll added to every absolute scroll the slot holds, as the room planes' one-line offset.</param>
    /// <exception cref="NotSupportedException">A waveform or band list is larger than a slot's data rows hold.</exception>
    internal static void Pack(Span<uint> data, int slot, LineScroll? shape, int ownScroll, int bias)
    {
        Span<uint> record = data.Slice(D3D11ShaderLayout.LineScrollsWordOffset + slot * RowWords, RowWords);
        Span<uint> words = data.Slice(D3D11ShaderLayout.LineScrollsWordOffset + (Slots + slot * DataRowsPerSlot) * RowWords, DataWords);
        switch (shape)
        {
            case null:
                record[0] = Kind(D3D11LineScrollKind.Bands, ownScroll + bias);
                break;
            case ScrollWave wave:
                record[0] = Kind(D3D11LineScrollKind.Wave, wave.BaseScroll + bias);
                record[1] = Band(wave.FirstLine, wave.EndLine);
                record[2] = (uint)wave.Amplitude;
                record[3] = D3D11ConstantPacking.Halves(wave.Phase.RawValue, checked((ushort)wave.Period));
                break;
            case ScrollStretch stretch:
                record[0] = Kind(D3D11LineScrollKind.Stretch, stretch.BaseScroll + bias);
                record[1] = Band(stretch.FirstLine, stretch.EndLine);
                record[2] = (uint)stretch.Slope;
                record[3] = unchecked((ushort)(stretch.ScrollAbove - stretch.BaseScroll));
                break;
            case SampledScrollWave sampled:
                if (sampled.Period > DataWords * SamplesPerWord)
                    throw new NotSupportedException($"A waveform of {sampled.Period} samples is longer than a line-scroll slot holds.");
                record[0] = Kind(D3D11LineScrollKind.Sampled, sampled.BaseScroll + bias);
                record[1] = Band(sampled.FirstLine, sampled.EndLine);
                record[2] = (uint)sampled.Amplitude;
                record[3] = D3D11ConstantPacking.Halves(checked((ushort)sampled.Phase), checked((ushort)sampled.Period));
                for (int index = 0; index < sampled.Period; index++)
                    words[index / SamplesPerWord] |= (uint)unchecked((byte)checked((sbyte)sampled.Samples[index]))
                        << (index % SamplesPerWord * SampleBits);
                break;
            case ScrollBands bands:
                if (bands.Bands.Length > DataWords)
                    throw new NotSupportedException($"{bands.Bands.Length} bands are more than a line-scroll slot holds.");
                record[0] = Kind(D3D11LineScrollKind.Bands, bands.BaseScroll + bias);
                record[3] = (uint)bands.Bands.Length;
                for (int index = 0; index < bands.Bands.Length; index++)
                    words[index] = D3D11ConstantPacking.Halves(unchecked((ushort)(bands.Bands[index].Scroll + bias)),
                        checked((ushort)bands.Bands[index].FirstLine));
                break;
            default:
                throw new NotSupportedException($"Line scroll {shape.GetType().Name} has no shader evaluation.");
        }
    }

    /// <summary>A slot's kind word: the kind in the low byte, the base scroll wrapped to sixteen bits in the high half.</summary>
    private static uint Kind(D3D11LineScrollKind kind, int baseScroll) => (uint)kind | (uint)unchecked((ushort)baseScroll) << HighHalf;

    /// <summary>A slot's band word: the first line, then the line after the last in the high half.</summary>
    private static uint Band(int firstLine, int endLine) => D3D11ConstantPacking.Halves(checked((ushort)firstLine), checked((ushort)endLine));
}
