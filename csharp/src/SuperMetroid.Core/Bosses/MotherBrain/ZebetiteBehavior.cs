using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetite species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ZebetiteBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zebetite];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new ZebetiteEntity(this, occupant, place, common, linkedTo: null));

    /// <summary>Initializes newly spawned Zebetite places through the common enemy lifecycle.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Supplies room artwork, data, and event services used by Zebetites.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clears actor shaking when the room earthquake ends.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The room's enemies, whose places are reused across Zebetite generations.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Samus's passage through a door, which keeps newly initialized Zebetites inert.</summary>
    private readonly DoorPassage _doorPassage;
    /// <summary>Tracks the room kill count as Zebetite halves are removed.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Resolves touch reactions through the shared enemy collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives the Zebetite's shot sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The fade step count every gradual fade shares, past whose start the pulse holds.</summary>
    private readonly SharedFadeStepCount _fadeStepCount;
    /// <summary>The enemy deaths, whose death explosion a dying half leaves.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;


    /// <summary>Captures the shared systems used by Zebetite initialization, AI, and reactions.</summary>
    internal ZebetiteBehavior(ZebetiteBehaviorDependencies dependencies)
    {
        _audio = dependencies.Audio;
        _collision = dependencies.Collision;
        _doorPassage = dependencies.DoorPassage;
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _places = dependencies.Places;
        _roomStatus = dependencies.RoomStatus;
        _fadeStepCount = dependencies.FadeStepCount;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>
    /// The Zebetites' shared pulse (#2259), advanced by every primary Zebetite in the room through its cycle (#2470); it
    /// starts at its first frame with the room. It selects only the pulse the Zebetites show.
    /// </summary>
    private ListStep<ZebetitePulse> SharedPulse { get; set; } = ListStep<ZebetitePulse>.FirstOf(ZebetitePulse.All);

    /// <summary>The pulse frame last shown, or null before the pulse's first step in the room (#1512).</summary>
    private ZebetitePulse? PulseShown { get; set; }

    /// <summary>The shading every Zebetite draws in: the pulse frame last shown, or the sheet's own colours before it.</summary>
    internal PictureShading PulseShading => PulseShown is { } pulse
        ? PictureShading.Shaded(ZebetiteShades.Pulse(pulse))
        : PictureShading.Standard;

    /// <summary>
    /// Sets a Zebetite up when the room spawns it: a linked half spawned by its primary is linked to the primary's place
    /// (<paramref name="linkedTo"/>) as it is set up (#2554).
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal ZebetiteEnemyState InitializeZebetite(RoomEnemyOccupant occupant, RoomEnemyPlace place, RoomEnemyPlace? linkedTo)
    {
        var state = new ZebetiteEnemyState(occupant.Spawn, place.Frame) { LinkedHalf = linkedTo };

        // A Zebetite animates and is solid to Samus.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();
        place.Shading = PulseShading;

        int generation = _room.Services.ZebetiteProgress.ZebetitesDestroyed;
        state.Generation = generation;
        if (generation == PersistentProgress.ZebetiteCount)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return state;
        }

        ZebetiteGenerationDefinition definition = Zebetites.Generation(generation);
        state.Form = definition.Form;
        place.Radius = place.Radius.WithY(definition.HalfHeight);
        // A new barrier shows its form's unhurt picture.
        state.Picture = HealthPicture(definition.Form, ZebetiteDefinitions.MaximumHealth);
        place.Position = place.Position.WithWholeX(definition.X);
        place.Position = place.Position.WithWholeY(Zebetites.YPosition(definition, state.Half));
        return state;
    }

    /// <summary>Runs the main step.</summary>
    private void RunZebetiteMain(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        RoomEnemyPlace place = zebetite.Place;
        if (!_earthquake.IsShaking)
            place.Shake.End();

        switch (state.Action)
        {
            case ZebetiteAction.SpawnLinkedHalf:
                RunZebetiteSpawnLinkedHalf(zebetite, state);
                break;
            case ZebetiteAction.WaitForDoorTransition:
                RunZebetiteWaitForDoorTransition(zebetite, state);
                break;
            case ZebetiteAction.Active:
                RunZebetiteActive(zebetite, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Zebetite has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Creates a linked half when this generation requires one, then enters the door-transition wait.</summary>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        RoomEnemyPlace place = zebetite.Place;
        if (state.Form == ZebetiteBarrierForm.SplitPair)
        {
            // The linked half is linked to this half as it is set up.
            if (SpawnZebetite(ZebetiteHalf.Linked, linkedTo: place) is { } secondary)
                state.LinkedHalf = secondary;
        }

        state.Action = ZebetiteAction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(zebetite, state);
    }

    /// <summary>Starts active AI on the first update after Samus's passage through the door ends.</summary>
    private void RunZebetiteWaitForDoorTransition(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        _ = zebetite.Place;
        // Ordinary doors and elevators alike hold the barrier inert during the passage, and it enters its
        // active body on the first update after.
        if (_doorPassage.IsUnderWay)
            return;
        state.Action = ZebetiteAction.Active;
        RunZebetiteActive(zebetite, state);
    }

    /// <summary>Regenerates living halves and advances the primary generation when its health reaches zero.</summary>
    private void RunZebetiteActive(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        RoomEnemyPlace place = zebetite.Place;
        CycleZebetiteColours(state);
        // Every Zebetite shows the shared pulse; each primary half steps it as it runs (#1512).
        place.Shading = PulseShading;
        ChooseZebetiteHealthPicture(place, state);

        if (place.Health != 0)
        {
            place.Health = Math.Min(ZebetiteDefinitions.MaximumHealth, place.Health + 1);
            return;
        }

        if (state.Half == ZebetiteHalf.Linked)
        {
            FinishZebetiteDeath(zebetite);
            return;
        }

        int nextGeneration = state.Generation + 1;
        state.Generation = nextGeneration;
        _room.Services.ZebetiteProgress.ZebetitesDestroyed = nextGeneration;
        FinishZebetiteDeath(zebetite);
        if (nextGeneration < PersistentProgress.ZebetiteCount)
            _ = SpawnZebetite(ZebetiteHalf.Primary, linkedTo: null);
    }

    /// <summary>
    /// Steps the shared pulse, which holds while the shared fade count is past a fade's start: the room's enemies run
    /// between a door fade's steps, and the pulse waits for the fade.
    /// </summary>
    private void CycleZebetiteColours(ZebetiteEnemyState zebetiteEnemyState)
    {
        if (_fadeStepCount.IsPastStart || zebetiteEnemyState.Half == ZebetiteHalf.Linked)
            return;

        ListStep<ZebetitePulse> pulse = SharedPulse.NextInCycle;
        SharedPulse = pulse;
        PulseShown = pulse.Entry;
    }

    /// <summary>
    /// Chooses the picture the half's form and current health show (#2250), which the animation pass shows on this update.
    /// </summary>
    private static void ChooseZebetiteHealthPicture(
        RoomEnemyPlace place,
        ZebetiteEnemyState state)
    {
        state.Picture = HealthPicture(state.Form, place.Health);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>The picture of <paramref name="form"/> at <paramref name="health"/>: the first its catalog shows at that health.</summary>
    /// <exception cref="InvalidOperationException">The health is below every picture's, which no Zebetite's is.</exception>
    private static EnemyFrameId HealthPicture(ZebetiteBarrierForm form, int health)
    {
        foreach (ZebetiteHealthPicture picture in ZebetiteHealthPictureDefinitions.ByForm[form])
        {
            if (health >= picture.MinimumHealth)
                return picture.Picture;
        }
        throw new InvalidOperationException($"A {form} Zebetite has no picture at health {health}.");
    }

    /// <summary>
    /// Finds a free place in the room's enemies and initializes a primary or linked half from its population entry; a full
    /// pool spawns no half.
    /// </summary>
    private RoomEnemyPlace? SpawnZebetite(ZebetiteHalf half, RoomEnemyPlace? linkedTo)
    {
        // The spawn takes the first free place. In particular, a primary's death vacates
        // its place before spawning the next generation; surviving linked halves keep
        // naming that reused place.
        if (_places.FindFreePlace() is not { } spawned)
            return null;

        RoomEnemyPlacement population = Zebetites.SpawnPopulation(half, _room.Services.ZebetiteProgress.ZebetitesDestroyed);

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        var spawn = new OrdinaryEnemySpawn<EnemySpawnSettings>(population, population.SpawnSettings, (occupant, _, place, common) =>
            EnemyOccupant.Running(new ZebetiteEntity(this, occupant, place, common, linkedTo)));
        _lifecycle.SetUpSpawnedEnemy(_lifecycle.InitializePlaceFromDefinition(spawned, spawn, definition), spawn);
        _places.CoverSpawn(spawned);
        return spawned;
    }

    /// <summary>Finishes a Zebetite's reaction to a shot, after its common damage.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemyPlace struck, ZebetiteEnemyState zebetiteEnemyState)
    {
        _audio.QueueEnemySound(ZebetiteDefinitions.ShotSound, SoundRequestPriority.Normal);
        RoomEnemyPlace linked = ZebetiteShotShare.Recipient(_places.InUpdateOrder, zebetiteEnemyState.LinkedHalf);
        linked.Health = struck.Health;
        linked.HurtFlash.Restart(struck.HurtFlash.UpdatesLeft);
    }

    /// <summary>Removes the half in its small death explosion, never to respawn, and records its room kill.</summary>
    private void FinishZebetiteDeath(RoomEnemyOccupant zebetite)
    {
        _ = zebetite.Place;
        _deathsAndDrops.RemoveKilledEnemy(zebetite, EnemyDeathExplosion.Small, respawns: false);
        _roomStatus.RecordKill();
    }

    /// <summary>
    /// Shows a half's chosen picture where the shared enemy animation runs (#2250): a still picture, which shows for good on
    /// the update its clock runs out, until the half chooses again.
    /// </summary>
    internal static void AdvanceZebetiteAnimation(RoomEnemyPlace place, ZebetiteEnemyState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        place.Frame = state.Picture;
        place.AnimationClock.ShowForGood();
    }

    /// <summary>Lets a bomb finish as a shot does on the Zebetite, without the generic death.</summary>
    /// <param name="state">The state of the enemy the reaction is built for, which it holds from its setup.</param>
    internal EnemyBombReaction ZebetiteBombReaction(ZebetiteEnemyState state) => new()
    {
        RunsGenericDeath = false,
        // The bomb's common damage shows no hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy, state),
    };

    /// <summary>Runs Zebetite's main AI.</summary>
    internal void RunZebetiteMainAi(RoomEnemyOccupant zebetite, ZebetiteEnemyState zebetiteEnemyState)
    {
        RoomEnemyPlace place = zebetite.Place;
        RunZebetiteMain(zebetite, zebetiteEnemyState);
    }

    /// <summary>Runs shot damage, then shares it across the linked Zebetites' health, with no generic death.</summary>
    /// <param name="state">The state of the enemy the reaction is built for, which it holds from its setup.</param>
    internal EnemyShotReaction ZebetiteShotReaction(ZebetiteEnemyState state) => new()
    {
        EndsWithoutImpact = true,
        ExemptFromGenericDeath = true,
        // The Zebetite's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy, state),
    };

    /// <summary>Routes Zebetite contact through normal touch collision while skipping generic death animation.</summary>
    internal void ReactToZebetiteTouch(EnemyTouchContext context) =>
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
}
