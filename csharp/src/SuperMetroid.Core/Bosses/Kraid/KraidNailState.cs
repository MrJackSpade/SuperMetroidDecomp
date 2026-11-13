namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's fingernails' own state beside the function every part runs: its velocity, whether it has launched,
/// and which spawn its last launch took, which its sibling reads.
/// </summary>
public sealed class KraidNailState : KraidPartState
{
    /// <summary>
    /// Gets whether this fingernail's last launch was the fixed-left spawn rather than the body-side one. A nail
    /// launches body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public bool AlternateSpawn { get; internal set; }

    /// <summary>Whether this fingernail has launched at least once (its parameter one); the game sets it and nothing reads it back.</summary>
    public bool NailLaunched { get; internal set; }

    /// <summary>A fingernail's signed 16.16 velocity in pixels per update.</summary>
    public FixedVector<RoomPixels> NailVelocity { get; internal set; }
}
