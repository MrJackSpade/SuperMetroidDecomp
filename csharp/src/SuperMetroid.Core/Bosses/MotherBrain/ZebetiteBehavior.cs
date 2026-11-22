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
internal sealed partial class ZebetiteBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
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
    internal ListStep<ZebetitePulse> SharedPulse { get; set; } = ListStep<ZebetitePulse>.FirstOf(ZebetitePulse.All);

    /// <summary>The pulse frame last shown, or null before the pulse's first step in the room (#1512).</summary>
    internal ZebetitePulse? PulseShown { get; set; }

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
        var state = new ZebetiteEnemyState(occupant.Spawn) { LinkedHalf = linkedTo };

        // A Zebetite animates and is solid to Samus.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();
        place.Shading = PulseShading;

        int generation = _room.Services.ZebetiteProgress.ZebetitesDestroyed;
        state.Generation = generation;
        if (Zebetites.StandingGeneration(generation) is not { } definition)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return state;
        }

        state.Form = definition.Form;
        place.Radius = place.Radius.WithY(definition.HalfHeight);
        // A new barrier shows its form's unhurt picture.
        state.Picture = HealthPicture(definition.Form, ZebetiteDefinitions.MaximumHealth);
        place.Position = place.Position.WithWholeX(definition.X);
        place.Position = place.Position.WithWholeY(Zebetites.YPosition(definition, state.Half));
        return state;
    }

    /// <summary>What <see cref="RunZebetiteMain"/> does for each <see cref="ZebetiteAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<ZebetiteAction, Action<RoomEnemyOccupant, RoomEnemyPlace, ZebetiteEnemyState>> RunZebetiteMainForZebetiteAction => field ??= new(new Dictionary<ZebetiteAction, Action<RoomEnemyOccupant, RoomEnemyPlace, ZebetiteEnemyState>>
    {
        [ZebetiteAction.SpawnLinkedHalf] = (zebetite, place, state) => RunZebetiteSpawnLinkedHalf(zebetite, state),
        [ZebetiteAction.WaitForDoorTransition] = (zebetite, place, state) => RunZebetiteWaitForDoorTransition(zebetite, state),
        [ZebetiteAction.Active] = (zebetite, place, state) => RunZebetiteActive(zebetite, state),
    });

    /// <summary>Runs the main step.</summary>
    private void RunZebetiteMain(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        RoomEnemyPlace place = zebetite.Place;
        if (!_earthquake.IsShaking)
            place.Shake.End();

        RunZebetiteMainForZebetiteAction[state.Action](zebetite, place, state);
    }

    /// <summary>Creates a linked half when this generation requires one, then enters the door-transition wait.</summary>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemyOccupant zebetite, ZebetiteEnemyState state)
    {
        RoomEnemyPlace place = zebetite.Place;
        // A standing primary's generation is the one its setup found standing.
        if (state.Form == ZebetiteBarrierForm.SplitPair && Zebetites.StandingGeneration(state.Generation) is { } standing)
        {
            // The linked half is linked to this half as it is set up.
            if (SpawnZebetite(ZebetiteHalf.Linked, standing, linkedTo: place) is { } secondary)
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
        if (Zebetites.StandingGeneration(nextGeneration) is { } next)
            _ = SpawnZebetite(ZebetiteHalf.Primary, next, linkedTo: null);
    }

    /// <summary>
    /// Steps the shared pulse, which holds while the shared fade count is past a fade's start: the room's enemies run
    /// between a door fade's steps, and the pulse waits for the fade.
    /// </summary>
    internal void CycleZebetiteColours(ZebetiteEnemyState zebetiteEnemyState)
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
    internal static void ChooseZebetiteHealthPicture(
        RoomEnemyPlace place,
        ZebetiteEnemyState state)
    {
        state.Picture = HealthPicture(state.Form, place.Health);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// The picture a Zebetite of <paramref name="form"/> shows at <paramref name="health"/>: the first of its pictures whose
    /// least health the health reaches, the last of them covering every health below the others' down to none.
    /// </summary>
    private static EnemyFrameId HealthPicture(ZebetiteBarrierForm form, int health)
    {
        IReadOnlyList<ZebetiteHealthPicture> pictures = ZebetiteHealthPictureDefinitions.ByForm[form];
        for (int index = 0; index < pictures.Count - 1; index++)
        {
            if (health >= pictures[index].MinimumHealth)
                return pictures[index].Picture;
        }
        return pictures[^1].Picture;
    }

    /// <summary>
    /// Finds a free place in the room's enemies and initializes a primary or linked half from its population entry; a full
    /// pool spawns no half.
    /// </summary>
    internal RoomEnemyPlace? SpawnZebetite(ZebetiteHalf half, ZebetiteGenerationDefinition generation, RoomEnemyPlace? linkedTo)
    {
        // The spawn takes the first free place. In particular, a primary's death vacates
        // its place before spawning the next generation; surviving linked halves keep
        // naming that reused place.
        if (_places.FindFreePlace() is not { } spawned)
            return null;

        RoomEnemyPlacement population = Zebetites.SpawnPopulation(half, generation);

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        var spawn = new OrdinaryEnemySpawn<EnemySpawnSettings>(population, population.SpawnSettings, new LinkedSpawn(this, linkedTo).CreateZebetite);
        _lifecycle.SetUpSpawnedEnemy(_lifecycle.InitializePlaceFromDefinition(spawned, spawn, definition), spawn);
        _places.CoverSpawn(spawned);
        return spawned;
    }

    /// <summary>The creation of a spawned half (#2554): a Zebetite linked to the half it shares its health with, or to none.</summary>
    /// <param name="species">The Zebetite species owner.</param>
    /// <param name="linkedTo">The half the spawned one shares its health with, when it has one.</param>
    private sealed partial class LinkedSpawn(ZebetiteBehavior species, RoomEnemyPlace? linkedTo)
    {
        /// <summary>The spawned half, linked to <c>linkedTo</c>.</summary>
        internal EnemyOccupant CreateZebetite(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(new ZebetiteEntity(species, occupant, place, common, linkedTo));
    }

    /// <summary>Finishes a Zebetite's reaction to a shot, after its common damage.</summary>
    internal void ResolveZebetiteShotAfterCommon(RoomEnemyPlace struck, ZebetiteEnemyState zebetiteEnemyState)
    {
        _audio.QueueEnemySound(ZebetiteDefinitions.ShotSound, SoundRequestPriority.Normal);
        RoomEnemyPlace linked = ZebetiteShotShare.Recipient(_places.InUpdateOrder[0], zebetiteEnemyState.LinkedHalf);
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
        // A half with no barrier left to show, which its setup deleted, shows nothing.
        if (state.Picture is not { } picture || !place.AnimationClock.Expire())
            return;
        place.Frame = picture;
        place.AnimationClock.ShowForGood();
    }

    /// <summary>Lets a bomb finish as a shot does on the Zebetite, without the generic death.</summary>
    /// <param name="afterCommon">The half's own method sharing the damage with its linked half.</param>
    internal static EnemyBombReaction ZebetiteBombReaction(Action<EnemyBombHitContext> afterCommon) => new()
    {
        RunsGenericDeath = false,
        // The bomb's common damage shows no hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = afterCommon,
    };

    /// <summary>Runs Zebetite's main AI.</summary>
    internal void RunZebetiteMainAi(RoomEnemyOccupant zebetite, ZebetiteEnemyState zebetiteEnemyState)
    {
        RoomEnemyPlace place = zebetite.Place;
        RunZebetiteMain(zebetite, zebetiteEnemyState);
    }

    /// <summary>Runs shot damage, then shares it across the linked Zebetites' health, with no generic death.</summary>
    /// <param name="afterCommon">The half's own method sharing the damage with its linked half.</param>
    internal static EnemyShotReaction ZebetiteShotReaction(Action<EnemyShotHitContext> afterCommon) => new()
    {
        EndsWithoutImpact = true,
        Lethal = EnemyShotLethal.SpeciesOwned,
        // The Zebetite's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = afterCommon,
    };

    /// <summary>Routes Zebetite contact through normal touch collision while skipping generic death animation.</summary>
    internal void ReactToZebetiteTouch(EnemyTouchContext context) =>
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
}
