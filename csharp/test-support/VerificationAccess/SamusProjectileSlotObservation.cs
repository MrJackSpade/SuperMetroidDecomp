using SuperMetroid.Core;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

/// <summary>
/// Occupancy and family of every ordinary projectile slot, captured before an alpha pass so a
/// test can observe what that pass allocated or converted from the slots themselves.
/// </summary>
/// <remarks>
/// Native allocation (<c>$90:AC39</c> and the Super Missile link producer) takes the lowest
/// slot whose damage word is zero, and the HUD producer runs before any pre-instruction can
/// allocate. The producer's shot is therefore the lowest slot that was free before the pass
/// and occupied after it. A collision converts a live shot into a beam- or missile-explosion
/// family in place, so a newly explosive slot is the observable trace of that collision.
/// </remarks>
internal sealed class SamusProjectileSlotObservation
{
    /// <summary>Stores occupied.</summary>
    private readonly bool[] _occupied;
    /// <summary>Stores explosion.</summary>
    private readonly bool[] _explosion;

    /// <summary>Initializes a new samus projectile slot observation instance.</summary>
    internal SamusProjectileSlotObservation(SamusProjectileSystem projectiles)
    {
        Ensure.NotNull(projectiles);
        int count = projectiles.Slots.Count;
        _occupied = new bool[count];
        _explosion = new bool[count];
        for (int index = 0; index < count; index++)
        {
            SamusProjectileSlot slot = projectiles.Slots[index];
            _occupied[index] = IsOccupied(slot);
            _explosion[index] = IsLiveExplosion(slot);
        }
    }

    /// <summary>True when any slot was occupied at capture time.</summary>
    internal bool AnyOccupied => Array.IndexOf(_occupied, true) >= 0;

    /// <summary>The slot the producer allocated since capture, or null when it fired nothing.</summary>
    internal int? FiredSlot(SamusProjectileSystem projectiles)
    {
        RequireSameSlots(projectiles);
        for (int index = 0; index < _occupied.Length; index++)
        {
            if (!_occupied[index] && IsOccupied(projectiles.Slots[index]))
                return index;
        }
        return null;
    }

    /// <summary>True when a slot became a live beam/missile explosion since capture.</summary>
    internal bool CollisionStartedExplosion(SamusProjectileSystem projectiles)
    {
        RequireSameSlots(projectiles);
        for (int index = 0; index < _explosion.Length; index++)
        {
            if (!_explosion[index] && IsLiveExplosion(projectiles.Slots[index]))
                return true;
        }
        return false;
    }

    /// <summary>Requires same slots.</summary>
    private void RequireSameSlots(SamusProjectileSystem projectiles)
    {
        Ensure.NotNull(projectiles);
        if (projectiles.Slots.Count != _occupied.Length)
            throw new InvalidOperationException(
                $"Projectile observation captured {_occupied.Length} slots but the system has " +
                $"{projectiles.Slots.Count}.");
    }

    /// <summary>Checks whether the projectile slot has nonzero damage.</summary>
    private static bool IsOccupied(SamusProjectileSlot slot) => slot.Damage != 0;

    /// <summary>Checks whether an active slot belongs to the beam- or missile-explosion family.</summary>
    private static bool IsLiveExplosion(SamusProjectileSlot slot) =>
        slot.IsActive && slot.PackedType.Family is
            SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion;
}
