namespace SuperMetroid.Core.Audio;

/// <summary>Validated decoding of top-level music track control words.</summary>
internal static class MusicTrackControls
{
    /// <summary>Distinguishes a control word from a phrase pointer, end marker or repeat count.</summary>
    internal static bool TryDecode(ushort record, out MusicTrackControl control)
    {
        if (!Enum.IsDefined((MusicTrackControl)record))
        {
            control = default;
            return false;
        }
        control = (MusicTrackControl)record;
        return true;
    }

    /// <summary>True when <paramref name="record"/> is exactly <paramref name="control"/>.</summary>
    internal static bool Is(ushort record, MusicTrackControl control) =>
        TryDecode(record, out MusicTrackControl decoded) && decoded == control;
}
