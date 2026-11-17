using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's body: its behaviour flags, colours, animation start and the low-health drool it triggers.</summary>
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
    internal static void LoadTorizoBodyColours(RoomEnemyPlace torizo, TorizoEnemyState state, TorizoLook look)
    {
        state.Colours.Show(look);
        torizo.Shading = state.Colours.Shading;
    }

    /// <summary>Uses the room RNG to emit drool only while a living Torizo is below the head-break threshold.</summary>
    internal void MaybeSpawnLowHealthDrool(RoomEnemyPlace torizo)
    {
        if (torizo.Health is 0 or >= TorizoDecisionDefinitions.GutBlowUpHealth)
            return;
        if (_room.Random.Read() is int random &&
            TorizoRandomDefinitions.LowHealthDrool.IsTakenBy(random))
        {
            SpawnInitialDrool(torizo);
        }
    }

    /// <summary>Whether Torizo has latched <paramref name="flag"/>.</summary>
    internal static bool HasBehavior(RoomEnemyPlace torizo, TorizoBehaviorFlags flag) => torizo.RequireSpeciesState<TorizoEnemyState>().Behavior.HasFlag(flag);

    /// <summary>Requests the drool he drips as he wakes, its position and fall speed jittered at random.</summary>
    internal void SpawnInitialDrool(RoomEnemyPlace torizo) =>
        _attacks.SpawnDrool(torizo, RoomEnemyProjectileKind.TorizoInitialDrool);

    /// <summary>Installs a body playback; its play resumes on the next frame.</summary>
    internal static void StartTorizoBodyAnimation(RoomEnemyPlace torizo, TorizoEnemyState state, AnimationPlayback<TorizoAnimation> playback)
    {
        state.BodyPlayback = playback;
        torizo.AnimationClock.StepOnNextUpdate();
    }
}
