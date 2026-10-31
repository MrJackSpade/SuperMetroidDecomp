using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Validated decoding of top-level music track control words.</summary>
[EncodingOwner("music track control words")]
internal static class MusicTrackControls
{
    /// <summary>The track word naming each control.</summary>
    private static readonly Dictionary<int, MusicTrackControl> controls = new()
    {
        [0x0080] = MusicTrackControl.FastForwardOn,
        [0x0081] = MusicTrackControl.FastForwardOff,
    };

    /// <summary>Distinguishes a control word from a phrase pointer, end marker or repeat count.</summary>
    internal static bool TryDecode(int record, out MusicTrackControl control) => controls.TryGetValue(record, out control);

    /// <summary>True when <paramref name="record"/> is exactly <paramref name="control"/>.</summary>
    internal static bool Is(int record, MusicTrackControl control) =>
        TryDecode(record, out MusicTrackControl decoded) && decoded == control;
}
