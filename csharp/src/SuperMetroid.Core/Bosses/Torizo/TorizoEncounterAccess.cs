using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the phase their branches test.</summary>
[RoomLifetime]
internal sealed class TorizoEncounterAccess
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoEncounterAccess()
    {
    }

    /// <summary>Current statue state, cleared when the room reloads.</summary>
    internal TorizoEnemyState? TorizoState { get; set; }

    /// <summary>Torizo's posture.</summary>
    internal TorizoPostureFlags Posture(RoomEnemySlot torizo) => RequireBombTorizoState(torizo).Posture;

    /// <summary>Whether Torizo faces right.</summary>
    internal bool FacesRight(RoomEnemySlot torizo) => Posture(torizo).HasFlag(TorizoPostureFlags.FacingRight);

    /// <summary>Rejects a slot unless it is the statue currently bound to this room owner.</summary>
    internal TorizoEnemyState RequireBombTorizoState(RoomEnemySlot torizo) =>
        TorizoState is not null && ReferenceEquals(TorizoState.Slot, torizo)
            ? TorizoState
            : throw new InvalidOperationException(
                $"Enemy slot {torizo.SlotIndex} has no initialized Bomb Torizo state.");

    /// <summary>Tests whether Torizo faces the side Samus is on, as its attack branches do.</summary>
    internal bool FacesSamus(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // The awakening facing check compares the facing bit with whether
        // the enemy's X minus Samus's is negative, rather than the reverse difference the walkers test.
        return FacesRight(torizo) == CoordinateAxis.Difference(torizo.Position.X.Whole, samus.Position.X.Whole) < 0;
    }
}
