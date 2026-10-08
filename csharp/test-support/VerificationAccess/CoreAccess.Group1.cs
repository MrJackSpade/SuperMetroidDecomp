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
    private readonly bool[] _occupied;
    private readonly bool[] _explosion;

    internal SamusProjectileSlotObservation(SamusProjectileSystem projectiles)
    {
        ArgumentNullException.ThrowIfNull(projectiles);
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

    private void RequireSameSlots(SamusProjectileSystem projectiles)
    {
        ArgumentNullException.ThrowIfNull(projectiles);
        if (projectiles.Slots.Count != _occupied.Length)
            throw new InvalidOperationException(
                $"Projectile observation captured {_occupied.Length} slots but the system has " +
                $"{projectiles.Slots.Count}.");
    }

    private static bool IsOccupied(SamusProjectileSlot slot) => slot.Damage != 0;

    private static bool IsLiveExplosion(SamusProjectileSlot slot) =>
        slot.IsActive && slot.PackedType.Family is
            SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion;
}

internal static partial class CoreAccess
{
    private static readonly ConditionalWeakTable<SuperMetroidGame, GameplayProjectileFireObserver>
        GameplayProjectileFireObservers = new();

    extension(SamusProjectileSystem projectiles)
    {
        /// <summary>Captures slot occupancy and families before an alpha pass.</summary>
        internal SamusProjectileSlotObservation ObserveSlots() => new(projectiles);
    }

    extension(SuperMetroidGame game)
    {
        /// <summary>
        /// The ordinary projectile slot allocated during the most recent <c>Step</c>, observed
        /// by comparing live slot occupancy with the previous frame's observation. Callers
        /// must observe every frame (repeat calls within one frame return the same answer);
        /// a skipped frame or a first observation with shots already live fails loudly
        /// because no allocation can be attributed to it.
        /// </summary>
        internal int? GameplayLastFiredProjectileSlot =>
            GameplayProjectileFireObservers.GetValue(game, _ => new GameplayProjectileFireObserver())
                .Observe(game);
    }

    private sealed class GameplayProjectileFireObserver
    {
        private SamusProjectileSystem? _projectiles;
        private ushort _frameNumber;
        private SamusProjectileSlotObservation? _slots;
        private int? _firedSlot;

        internal int? Observe(SuperMetroidGame game)
        {
            SamusProjectileSystem? projectiles = game.RuntimeForVerification?.Projectiles;
            if (projectiles is null)
            {
                _projectiles = null;
                _slots = null;
                _firedSlot = null;
                return null;
            }

            if (ReferenceEquals(projectiles, _projectiles) && game.FrameNumber == _frameNumber)
                return _firedSlot;

            var current = new SamusProjectileSlotObservation(projectiles);
            if (!ReferenceEquals(projectiles, _projectiles))
            {
                if (current.AnyOccupied)
                    throw new InvalidOperationException(
                        $"First projectile observation at frame {game.FrameNumber} already has live " +
                        "ordinary projectiles; no prior frame exists to attribute their allocation.");
                _firedSlot = null;
            }
            else if (game.FrameNumber != unchecked((ushort)(_frameNumber + 1)))
            {
                throw new InvalidOperationException(
                    $"Projectile observation skipped from frame {_frameNumber} to {game.FrameNumber}; " +
                    "allocations in the unobserved frames cannot be attributed.");
            }
            else
            {
                _firedSlot = _slots!.FiredSlot(projectiles);
            }

            _projectiles = projectiles;
            _frameNumber = game.FrameNumber;
            _slots = current;
            return _firedSlot;
        }
    }
}
