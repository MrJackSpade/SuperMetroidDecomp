using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's body: it shows the Torizo's colouring, starts its body animation and drips its low-health drool.</summary>
[RoomLifetime]
internal sealed class TorizoBody
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    private readonly TorizoAttacks _attacks;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoBody(EnemyRoomBinding room, TorizoAttacks attacks)
    {
        _room = room;
        _attacks = attacks;
    }

    /// <summary>Shows <paramref name="look"/> on <paramref name="torizo"/>'s body and belly at once (#1512).</summary>
    internal static void ShowLook(RoomEnemyPlace torizo, TorizoEnemyState state, TorizoLook look)
    {
        state.Colours.Show(look);
        torizo.Shading = state.Colours.Shading;
    }

    /// <summary>Uses the room RNG to emit drool only while a living Torizo is below its gut's blow-up health (<see cref="TorizoDecisionDefinitions.GutBlowUpHealth"/>).</summary>
    internal void MaybeSpawnLowHealthDrool(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        if (torizo.Health is 0 or >= TorizoDecisionDefinitions.GutBlowUpHealth)
            return;
        if (_room.Random.Read() is int random &&
            TorizoRandomDefinitions.LowHealthDrool.IsTakenBy(random))
        {
            SpawnInitialDrool(occupant, torizo, state);
        }
    }

    /// <summary>Requests the drool he drips as he wakes, its position and fall speed jittered at random.</summary>
    internal void SpawnInitialDrool(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState state) =>
        _attacks.SpawnTorizoInitialDrool(occupant, torizo, state);

    /// <summary>Installs a body playback; its play resumes on the next frame.</summary>
    internal static void StartTorizoBodyAnimation(RoomEnemyPlace torizo, TorizoEnemyState state, AnimationPlayback<TorizoBodySegment> playback)
    {
        state.BodyPlayback = playback;
        torizo.AnimationClock.StepOnNextUpdate();
    }
}
