using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
[RoomLifetime]
internal sealed class TorizoEncounterAccess
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoEncounterAccess()
    {
    }

    /// <summary>Current statue state, cleared when the room reloads.</summary>
    internal TorizoEnemyState? TorizoState { get; set; }

    /// <summary>Torizo's posture bits from its enemy parameter 1.</summary>
    internal TorizoPostureFlags Posture(RoomEnemySlot torizo) => RequireBombTorizoState(torizo).Posture;

    /// <summary>Whether Torizo faces right.</summary>
    internal bool FacesRight(RoomEnemySlot torizo) => Posture(torizo).HasFlag(TorizoPostureFlags.FacingRight);

    /// <summary>Rejects a slot unless it is the statue currently bound to this room owner.</summary>
    internal TorizoEnemyState RequireBombTorizoState(RoomEnemySlot torizo) =>
        TorizoState is not null && ReferenceEquals(TorizoState.Slot, torizo)
            ? TorizoState
            : throw new InvalidOperationException(
                $"Enemy slot {torizo.SlotIndex} has no initialized Bomb Torizo state.");

    /// <summary>Tests the native parameter-one XOR enemy-X minus Samus-X sign used by attack branches.</summary>
    internal bool BombTorizoFunction12IsNonNegative(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // $AA:C58B calls Torizo_Func_12, whose operand is the facing bit XOR the sign of
        // enemyX-SamusX, rather than the SamusX-enemyX test used by the walkers.
        return FacesRight(torizo) == unchecked((short)(torizo.Position.X.Whole - samus.Position.X.Whole)) < 0;
    }
}
