using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The byte the sound driver's music port holds for each control (#2556): the one place the port's control bytes are known.</summary>
[EncodingOwner("sound driver music port control bytes")]
internal static partial class MusicPortCommands
{
    /// <summary>The port byte of each control.</summary>
    private static readonly Dictionary<int, MusicPortCommand> commands = new()
    {
        [0xF0] = MusicPortCommand.Pause,
        [0xF1] = MusicPortCommand.Resume,
        [0xFF] = MusicPortCommand.None,
    };

    /// <summary>Distinguishes a control byte from a track number.</summary>
    internal static bool TryDecode(int port, out MusicPortCommand command) => commands.TryGetValue(port, out command);

    /// <summary>The port byte of <paramref name="command"/>.</summary>
    /// <exception cref="InvalidOperationException">The control has no byte.</exception>
    internal static byte Encode(MusicPortCommand command) => checked((byte)commands.Single(entry => entry.Value == command).Key);
}
