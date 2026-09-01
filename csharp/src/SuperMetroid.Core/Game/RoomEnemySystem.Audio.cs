namespace SuperMetroid.Core.Game;

/// <summary>
/// One call to one of the cartridge's three <c>QueueSfx</c> entry points.
/// </summary>
/// <param name="Library">
/// APU input port/library number one through three. This is deliberately not an enum:
/// the three values are port identities, not combinable flags.
/// </param>
/// <param name="SoundId">The low-byte sound index supplied by the original 65816 caller.</param>
/// <param name="MaximumQueued">
/// The exact queue-cap variant selected by that caller (for example Max3 or Max6).
/// </param>
public readonly record struct EnemySoundRequest(
    byte Library,
    byte SoundId,
    byte MaximumQueued);

public sealed partial class RoomEnemySystem
{
    // Enemy AI can execute several independent actors in one frame. A single nullable
    // "last sound" loses calls when two actors publish during that pass, so new translations
    // use the same append-only-per-frame shape already proven by Samus and PLM audio.
    private readonly List<EnemySoundRequest> _soundRequests = [];

    /// <summary>
    /// Exact bank-$A0/$A6/$A9 sound-queue calls published by the current enemy frame.
    /// The frontend drains these only after all enemy AI and draw instructions have run.
    /// </summary>
    public IReadOnlyList<EnemySoundRequest> SoundRequests => _soundRequests;

    /// <summary>Begins the native EnemyMain publication window.</summary>
    private void BeginEnemySoundRequestFrame() => _soundRequests.Clear();

    /// <summary>
    /// Records an already-decoded cartridge queue call without assigning host-side meaning
    /// to its numeric sound ID. The bank-$80 queue remains the sole owner of arbitration.
    /// </summary>
    private void QueueEnemySound(byte library, ushort soundId, byte maximumQueued)
    {
        if (library is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(library), library, "SPC sound library must be 1..3.");
        if (soundId > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(soundId), soundId, "SPC sound ID must fit in one byte.");
        if (maximumQueued == 0)
            throw new ArgumentOutOfRangeException(nameof(maximumQueued), maximumQueued, "Queue capacity must be nonzero.");

        _soundRequests.Add(new EnemySoundRequest(
            library,
            unchecked((byte)soundId),
            maximumQueued));
    }
}
