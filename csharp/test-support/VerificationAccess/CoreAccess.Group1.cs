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
    /// <summary>Slot occupancy sampled before the pass so newly allocated projectile slots can be identified afterward.</summary>
    private readonly bool[] _occupied;
    /// <summary>Live explosion-family state sampled before the pass so projectile-to-explosion conversions can be detected.</summary>
    private readonly bool[] _explosion;

    /// <summary>Captures occupancy and active explosion state for each ordinary projectile slot.</summary>
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

    /// <summary>Ensures the supplied projectile system has the same slot layout used by this snapshot.</summary>
    private void RequireSameSlots(SamusProjectileSystem projectiles)
    {
        ArgumentNullException.ThrowIfNull(projectiles);
        if (projectiles.Slots.Count != _occupied.Length)
            throw new InvalidOperationException(
                $"Projectile observation captured {_occupied.Length} slots but the system has " +
                $"{projectiles.Slots.Count}.");
    }

    /// <summary>Applies the cartridge allocation convention that a nonzero damage word marks a used projectile slot.</summary>
    private static bool IsOccupied(SamusProjectileSlot slot) => slot.Damage != 0;

    /// <summary>Identifies active beam or missile explosion objects produced when a projectile collides.</summary>
    private static bool IsLiveExplosion(SamusProjectileSlot slot) =>
        slot.IsActive && slot.PackedType.Family is
            SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion;
}

internal static partial class CoreAccess
{
    /// <summary>Stores one frame-to-frame projectile allocation observer per game instance without extending its lifetime.</summary>
    private static readonly ConditionalWeakTable<SuperMetroidGame, GameplayProjectileFireObserver>
        GameplayProjectileFireObservers = new();

    /// <summary>Provides projectile-slot snapshot helpers for verification code without changing the production API.</summary>
    extension(SamusProjectileSystem projectiles)
    {
        /// <summary>Captures slot occupancy and families before an alpha pass.</summary>
        internal SamusProjectileSlotObservation ObserveSlots() => new(projectiles);
    }

    /// <summary>Exposes the latest ordinary projectile allocation inferred from consecutive game-frame observations.</summary>
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

    /// <summary>Attributes newly occupied projectile slots to a game frame and rejects gaps that make attribution ambiguous.</summary>
    private sealed class GameplayProjectileFireObserver
    {
        /// <summary>Projectile system associated with the last accepted snapshot.</summary>
        private SamusProjectileSystem? _projectiles;
        /// <summary>Game frame number represented by the last accepted snapshot.</summary>
        private ushort _frameNumber;
        /// <summary>Slot occupancy and explosion-family state captured for the last frame.</summary>
        private SamusProjectileSlotObservation? _slots;
        /// <summary>Allocated slot attributed to the last frame, reused by repeated observations of that frame.</summary>
        private int? _firedSlot;

        /// <summary>Returns the slot newly occupied since the prior consecutive frame, or null when no shot was allocated.</summary>
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
