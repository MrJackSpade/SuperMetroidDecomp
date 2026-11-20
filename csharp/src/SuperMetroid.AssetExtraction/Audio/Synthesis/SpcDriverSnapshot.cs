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

    /// <summary>The driver's byte for an on/off mark (#2560): the active byte when set, zero when clear.</summary>
    public static int Mark(bool set) => set ? SpcDriverDefinitions.SoundEffects.Active : 0;

    /// <summary>The driver's byte for the music's publication dirty marker (#2560): its high bit when set.</summary>
    public static int DirtyByte(bool dirty) => dirty ? SpcDriverDefinitions.Music.DirtyMarker : 0;

    /// <summary>The driver's byte for the form of a channel's last slide instruction: the legato opcode, or zero (#2560).</summary>
    public static int Byte(SoundEffectSlideForm form) => form switch
    {
        SoundEffectSlideForm.Plain => 0,
        SoundEffectSlideForm.Legato => SpcDriverDefinitions.SoundEffects.LegatoSlideOpcode,
        _ => throw new ArgumentOutOfRangeException(nameof(form), form, "Undefined slide form."),
    };

    /// <summary>The driver's byte for library three's request mode (#2560).</summary>
    public static int Byte(SoundLibraryMode mode) => mode switch
    {
        SoundLibraryMode.Normal => 0,
        SoundLibraryMode.LowHealth => SpcDriverDefinitions.SoundEffects.LowHealthMode,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined library mode."),
    };

    /// <summary>The driver's byte for a pitch envelope's direction (#2560).</summary>
    public static int Byte(PitchEnvelopeDirection direction) => direction switch
    {
        PitchEnvelopeDirection.From => 0,
        PitchEnvelopeDirection.To => SpcDriverDefinitions.Music.PitchEnvelopeToward,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined envelope direction."),
    };

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
