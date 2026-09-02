namespace SuperMetroid.Core.Audio;

/// <summary>
/// Sound IDs written by <c>Cancel_Sound_Effects</c> at <c>$82:BE17</c>. These are SPC
/// sequence commands, not silent host-side queue clears: each library consumes its own
/// stop command and preserves the cartridge acknowledgement protocol.
/// </summary>
public static class AudioCancellationCommands
{
    /// <summary>Bank-$82 cancellation command for SFX library one.</summary>
    public const byte Library1 = 0x02;

    /// <summary>Bank-$82 cancellation command for SFX library two.</summary>
    public const byte Library2 = 0x71;

    /// <summary>Bank-$82 cancellation command for SFX library three.</summary>
    public const byte Library3 = 0x01;
}
