namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How many shots Samus has out, as firing counts them (#2051). The count is kept apart from the shot places the
/// projectiles hold, and the two drift: a beam is counted before it takes its place and taken back when Samus's pose
/// cannot fire it, and with every place held it replaces the last place's shot, which is never uncounted
/// (<see cref="BeamFiredOverLastShot"/>); a special attack sets the count to its particle count whatever its fifth place holds; a Super
/// Missile counts its collision link; a deleted projectile takes one off, but a deletion at zero leaves zero. The count,
/// not occupancy, admits firing (no beam, missile or Super Missile is fired while it is at the limit) and the
/// shinespark crash's echoes (each echo is released only while the count leaves room for it), and Kraid's and Ridley's
/// shot passes read it, so its drift is gameplay-observable.
/// </summary>
/// <remarks>
/// Consulted by every firing admission and by a finishing crash's echo release; changed by firing, a refused shot, a
/// special attack, a Super Missile's link, an echo's release, every projectile deletion and the clearing of Samus's
/// shots. Lifetime: one room's shot pool, created with it. Evidence: the original keeps the projectile count in its own
/// word, written by the fire routines and the projectile deletion separately from the projectile slots; the special
/// attacks store their particle count into it directly.
/// </remarks>
[CompatibilityService("Samus's shot count, which admits firing and the crash echoes, drifts from the places her shots hold: a special attack sets it to its particle count and a deletion at zero leaves zero")]
public sealed class SamusShotCount
{
    /// <summary>The shots counted out, zero or more.</summary>
    public int Count
    {
        get;
        private set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>A shot is fired or released, or a Super Missile's link takes a place: one more is counted.</summary>
    internal void CountShot() => Count++;

    /// <summary>A beam counted before its place was taken is refused, as Samus's pose cannot fire it: the count is taken back.</summary>
    internal void TakeBackRefusedShot() => Count--;

    /// <summary>A projectile is deleted: one less is counted, but a count of zero stays zero.</summary>
    internal void CountDeletion() => Count = Count == 0 ? 0 : Count - 1;

    /// <summary>A special attack's <paramref name="particles"/> replace Samus's shots: the count becomes their number.</summary>
    internal void CountSpecialAttack(int particles) => Count = particles;

    /// <summary>Samus's shots are cleared all at once: nothing is counted.</summary>
    internal void Clear() => Count = 0;
}
