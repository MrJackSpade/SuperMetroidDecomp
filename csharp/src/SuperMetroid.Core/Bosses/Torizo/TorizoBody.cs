using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, program start and the low-health drool it triggers.</summary>
[RoomLifetime]
internal sealed class TorizoBody
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Exposes the live body and belly colours Torizo's body colouring loads.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    private readonly TorizoEncounterAccess _access;
    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    private readonly TorizoAttacks _attacks;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoBody(EnemyRoomBinding room, EnemyPresentation presentation, TorizoEncounterAccess access, TorizoAttacks attacks)
    {
        _room = room;
        _presentation = presentation;
        _access = access;
        _attacks = attacks;
    }

    /// <summary>Loads <paramref name="bodyAndBelly"/>, the body's colours then the belly's, into the live Torizo body colours.</summary>
    internal void LoadTorizoBodyColours(ReadOnlySpan<RgbColor> bodyAndBelly) =>
        _presentation.BossColours().TorizoBodyAndBelly.CopyFrom(bodyAndBelly);

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

    /// <summary>Requests the awakening drool variant whose position and speed use carry-sensitive jitter.</summary>
    internal void SpawnBombTorizoInitialDrool(RoomEnemySlot torizo) =>
        _attacks.SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);

    /// <summary>Installs a body program cursor and runs its first instruction on the next frame.</summary>
    internal static void StartTorizoBodyProgram(RoomEnemySlot torizo, TorizoEnemyState state, AnimationProgramCursor<TorizoBodyProgram> cursor)
    {
        state.BodyProgram = cursor;
        torizo.InstructionTimer = WrappingCountdown.Start(1);
    }
}
