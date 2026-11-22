namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The pre-Phantoon room's setup holds one enemy-projectile place for its first projectile updates: it takes the first
/// place a spawn would take, holds it for <see cref="HeldUpdates"/> projectile updates and frees it on the update after
/// them. No projectile stands in the place, which keeps what its previous projectile left there for the next spawn. While it holds its place, a projectile spawned in the room (a Work Robot's laser in the powered room) takes
/// the place below it, so it runs, and is scanned against Samus, after projectiles spawned once the place is free again;
/// the pool also has one place fewer.
/// </summary>
/// <remarks>
/// Consulted by the room's setup, when it takes the place, and by the projectile pass at the place's turn, which counts
/// the hold and frees the place on the update after it runs out. Evidence: the room's setup (<c>$8F:C8C8</c>) spawns its projectile
/// into the pool its load has just cleared, and the projectile's animation (<c>$86:A3AA</c>) shows a blank frame for 32
/// updates and deletes it on the update after them; spawns search the pool from the place it takes downward. The original's projectile
/// also held the room's background at its resting height while it lived, a presentation effect the port does not keep
/// (#1851).
/// </remarks>
public sealed partial class PrePhantoonRoomHeldProjectilePlace
{
    /// <summary>The projectile updates the place is held before the update that frees it.</summary>
    public const int HeldUpdates = 32;

    /// <summary>The hold while the place is taken: over before the room's setup takes it, and its last count frees it.</summary>
    private UpdateCountdown hold = UpdateCountdown.Ended();

    /// <summary>Starts the hold as the room's setup takes the place.</summary>
    internal void Take() => hold = UpdateCountdown.Lasting(HeldUpdates);

    /// <summary>Counts one projectile update of the place: one of the hold's while it runs, the freeing one once it is over.</summary>
    /// <returns>Whether this update frees the place.</returns>
    /// <remarks>Only the holding projectile, which the room's setup spawned as it took the place, counts it.</remarks>
    internal bool FreesOnThisUpdate()
    {
        if (hold.IsRunning)
        {
            hold.Count();
            return false;
        }
        return true;
    }
}
