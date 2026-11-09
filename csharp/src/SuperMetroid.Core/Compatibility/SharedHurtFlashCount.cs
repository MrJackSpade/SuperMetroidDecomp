namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The count Samus's hurt flash runs on, which three unrelated sequences write (#1552): a knockback starts it at one, a
/// health loss found after drawing starts it at one only while it is idle, and an active shinespark holds it at eight on
/// every update while its crash's setup and its windup clear it. The hurt flash advances it once per palette update while
/// it runs: its first six counts alternate the hurt and suit colours, count two requests the damage sound, count forty
/// restores the movement sound a hurt interrupted, and the flash clears it at its end. A spark therefore shows no hurt
/// colours and starts no flash for a health loss while it moves, and a spark that stops without crashing lets the flash
/// run on from eight, so the restored movement sound is requested 32 updates later.
/// </summary>
/// <remarks>
/// <para>
/// Consulted and advanced by the hurt flash in Samus's palette update, after the special palettes; written by the
/// knockback's start, the post-draw health check, and the shinespark's windup, motion and crash. Lifetime: Samus's; a new
/// Samus starts idle.
/// </para>
/// <para>
/// Evidence: the original game keeps one hurt flash counter, which the knockback setup, the post-draw health-loss check
/// and the shinespark's handlers write, and which the hurt flash palette routine advances and tests; the Ridley full-movie
/// replay compares it on every update.
/// </para>
/// </remarks>
[CompatibilityService("a knockback, a health loss and a moving shinespark share the hurt flash's count, so a spark suppresses the flash and its sounds")]
public sealed class SharedHurtFlashCount
{
    /// <summary>The count the hurt flash shows next, zero while it is idle.</summary>
    public int Count { get; private set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the flash is idle.</summary>
    public bool IsIdle => Count == 0;

    /// <summary>Starts the flash at its first count, as a knockback and a health loss do.</summary>
    public void Start() => Count = 1;

    /// <summary>Holds the count at <paramref name="count"/>, as a moving shinespark does on every update.</summary>
    /// <param name="count">The count held.</param>
    public void HoldAt(int count) => Count = count;

    /// <summary>Clears the count, ending any flash.</summary>
    public void Clear() => Count = 0;

    /// <summary>Advances the count by one palette update and returns the count reached.</summary>
    public int Advance() => ++Count;
}
