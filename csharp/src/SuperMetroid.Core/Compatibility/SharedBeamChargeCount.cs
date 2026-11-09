namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The beam's charge count as the rest of Samus reads it (#1552). Her spin and wall jump become a pseudo Screw Attack at
/// a full charge, the bomb spread needs one, and her spin's end, a power bomb's end and the hurt flash's recovery resume
/// the charge sound from it. The projectile pass publishes the beam's own count into it at its end; switching weapons,
/// spawning a bomb spread, a pseudo Screw Attack's hit, a shinespark windup, a grapple wall jump and collecting Varia or
/// Gravity clear it.
/// </summary>
/// <remarks>
/// <para>
/// The original game keeps one charge count. This one holds what the rest of Samus reads of it; the projectile system's
/// charge keeps the count its firing and flare advance. Switching weapons, a bomb spread, a pseudo Screw Attack's hit and
/// a shinespark windup clear both; a grapple wall jump and a suit item clear only this one, so until the next projectile
/// pass publishes the beam's count again her movement, bombs and sounds read no charge while the beam keeps its count.
/// </para>
/// <para>
/// Published once per update at the end of the projectile pass, so the spin and wall-jump contact damage in the movement
/// that follows reads that update's count; read by the bomb spread in the next update's bomb pass, by the movement's
/// contact damage, by the shinespark's windup and by the charge-sound resumptions. Lifetime: Samus's; a new Samus starts
/// with no charge. Evidence: the original's charge counter, which the projectile handler advances and every one of these
/// owners reads or clears; the Morph Ball spread, spin contact damage and hurt-flash recovery checks.
/// </para>
/// </remarks>
[CompatibilityService("Samus's spin damage, bomb spread and charge-sound resumption read the beam charge her other sequences clear")]
public sealed class SharedBeamChargeCount
{
    /// <summary>The charge count, zero with no charge.</summary>
    public int Count { get; private set => field = Ensure.AtLeastZero(value); }

    /// <summary>Publishes the beam's own charge count, as the projectile pass does at its end.</summary>
    /// <param name="count">The beam's charge count.</param>
    public void Publish(int count) => Count = count;

    /// <summary>Clears the charge.</summary>
    public void Clear() => Count = 0;
}
