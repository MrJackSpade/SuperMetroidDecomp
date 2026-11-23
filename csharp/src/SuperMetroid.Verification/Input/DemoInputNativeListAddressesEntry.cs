using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// One demo recording's native list: the recording, by the name of its id and the start of its playback, its entry, and
/// the cue a signalling recording signals as it ends.
/// </summary>
/// <param name="Name">The name of the recording's id.</param>
/// <param name="Start">The start of the recording's playback.</param>
/// <param name="Entry">The entry.</param>
/// <param name="Signal">The cue the recording signals as it ends, or null for a recording that does not signal.</param>
internal readonly record struct DemoInputNativeListAddressesEntry(string Name, DemoPlaybackPosition Start, ushort Entry, DemoInputCue? Signal)
{
    /// <summary>The recording.</summary>
    internal DemoRecording Program => Start.Recording;
}
