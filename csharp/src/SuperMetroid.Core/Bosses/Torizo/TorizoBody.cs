using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, program start and the low-health drool it triggers.</summary>
[RoomLifetime]
internal sealed class TorizoBody
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function their branches test.</summary>
    private readonly TorizoEncounterAccess _access;
    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    private readonly TorizoAttacks _attacks;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoBody(EnemyRoomBinding room, TorizoEncounterAccess access, TorizoAttacks attacks)
    {
        _room = room;
        _access = access;
        _attacks = attacks;
    }

    /// <summary>Shows <paramref name="look"/> on <paramref name="torizo"/>'s body and belly at once (#1512).</summary>
    internal static void LoadTorizoBodyColours(RoomEnemySlot torizo, TorizoEnemyState state, TorizoLook look)
    {
        state.Colours.Show(look);
        torizo.Shading = state.Colours.Shading;
    }

    /// <summary>Uses the room RNG to emit drool only while a living Torizo is below the head-break threshold.</summary>
    internal void MaybeSpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo)
    {
        if (torizo.Health is 0 or >= BombTorizoBehavior.BombTorizoHeadExplosionHealth)
            return;
        if (_room.Random.Read() is int random &&
            TorizoRandomDefinitions.LowHealthDrool.AllClear(random))
        {
            SpawnBombTorizoInitialDrool(torizo);
        }
    }

    /// <summary>Whether Torizo has latched <paramref name="flag"/>.</summary>
    internal bool HasBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flag) => _access.RequireBombTorizoState(torizo).Behavior.HasFlag(flag);

    /// <summary>Requests the awakening drool variant whose position and speed use wrap-sensitive jitter.</summary>
    internal void SpawnBombTorizoInitialDrool(RoomEnemySlot torizo) =>
        _attacks.SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);

    /// <summary>Installs a body playback; its play resumes on the next frame.</summary>
    internal static void StartTorizoBodyProgram(RoomEnemySlot torizo, TorizoEnemyState state, AnimationPlayback<TorizoBodyProgram> playback)
    {
        state.BodyPlayback = playback;
        torizo.FrameHold = WrappingCountdown.Start(1);
    }
}
