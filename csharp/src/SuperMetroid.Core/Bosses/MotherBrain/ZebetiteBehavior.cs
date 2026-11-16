using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetite species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ZebetiteBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zebetite];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ZebetiteEntity(this, place, common);

    /// <summary>Initializes newly spawned Zebetite places through the common enemy lifecycle.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Supplies room artwork, data, and event services used by Zebetites.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the explosion projectile when a Zebetite half dies.</summary>
    private readonly EnemyProjectileEngine _projectiles;
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
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _places = dependencies.Places;
        _roomStatus = dependencies.RoomStatus;
        _fadeStepCount = dependencies.FadeStepCount;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>Health ceiling applied while an active Zebetite regenerates each update.</summary>
    private const int ZebetiteMaximumHealth = 1000;
    /// <summary>The sound a Zebetite makes when a shot hits it.</summary>
    private static readonly SoundEffectId ZebetiteShotSound = RoomSound.MissileDoorHit;

    /// <summary>
    /// The Zebetites' shared pulse counter, advanced by every primary Zebetite in the room; it starts at zero with the
    /// room. It selects only the pulse the Zebetites show.
    /// </summary>
    public int SharedColourCycle { get; internal set; }

    /// <summary>The pulse frame last shown, or null before the pulse's first step in the room (#1512).</summary>
    internal int? PulseShown { get; private set; }

    /// <summary>The shading every Zebetite draws in: the pulse frame last shown, or the sheet's own colours before it.</summary>
    internal PictureShading PulseShading => PulseShown is int frame
        ? PictureShading.Shaded(ZebetiteShades.Pulse(frame))
        : PictureShading.Standard;

    /// <summary>Sets a Zebetite up when the room spawns it.</summary>
    internal void InitializeZebetite(RoomEnemyPlace place)
    {
        var state = new ZebetiteEnemyState(place.Spawn.Population);
        place.SetSpeciesState(state);

        // One more processing property is unnamed; it keeps its loaded value, and only the animating property is set.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();
        place.Shading = PulseShading;

        int generation = _room.Services.ZebetiteProgress.ZebetitesDestroyed;
        state.Generation = generation;
        if (generation == PersistentProgress.ZebetiteCount)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        ZebetiteGenerationDefinition definition = Zebetites.Generation(generation);
        state.Form = definition.Form;
        place.Radius = place.Radius.WithY(definition.HalfHeight);
        // A new barrier shows its form's unhurt picture.
        state.Picture = HealthPicture(definition.Form, ZebetiteMaximumHealth);
        place.Position = place.Position.WithWholeX(definition.X);
        place.Position = place.Position.WithWholeY(Zebetites.YPosition(definition, state.IsSecondaryHalf));
    }

    /// <summary>Runs the main step.</summary>
    private void RunZebetiteMain(RoomEnemyPlace place, ZebetiteEnemyState state)
    {
        if (!_earthquake.IsShaking)
            place.ShakeTimer = 0;

        switch (state.Action)
        {
            case ZebetiteAction.SpawnLinkedHalf:
                RunZebetiteSpawnLinkedHalf(place, state);
                break;
            case ZebetiteAction.WaitForDoorTransition:
                RunZebetiteWaitForDoorTransition(place, state);
                break;
            case ZebetiteAction.Active:
                RunZebetiteActive(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Zebetite has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Creates a linked half when this generation requires one, then enters the door-transition wait.</summary>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemyPlace place, ZebetiteEnemyState state)
    {
        if (state.Form == ZebetiteBarrierForm.SplitPair)
        {
            RoomEnemyPlace secondary = SpawnZebetite(ZebetiteHalf.Linked);
            secondary.RequireSpeciesState<ZebetiteEnemyState>().LinkedHalf = place;
            state.LinkedHalf = secondary;
        }

        state.Action = ZebetiteAction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(place, state);
    }

    /// <summary>Starts active AI on the first update after Samus's passage through the door ends.</summary>
    private void RunZebetiteWaitForDoorTransition(RoomEnemyPlace place, ZebetiteEnemyState state)
    {
        // Ordinary doors and elevators alike hold the barrier inert during the passage, and it enters its
        // active body on the first update after.
        if (_doorPassage.IsUnderWay)
            return;
        state.Action = ZebetiteAction.Active;
        RunZebetiteActive(place, state);
    }

    /// <summary>Regenerates living halves and advances the primary generation when its health reaches zero.</summary>
    private void RunZebetiteActive(RoomEnemyPlace place, ZebetiteEnemyState state)
    {
        CycleZebetiteColours(place);
        // Every Zebetite shows the shared pulse; each primary half steps it as it runs (#1512).
        place.Shading = PulseShading;
        ChooseZebetiteHealthPicture(place, state);

        if (place.Health != 0)
        {
            place.Health = Math.Min(ZebetiteMaximumHealth, place.Health + 1);
            return;
        }

        if (state.IsSecondaryHalf)
        {
            FinishZebetiteDeath(place);
            return;
        }

        int nextGeneration = state.Generation + 1;
        state.Generation = nextGeneration;
        _room.Services.ZebetiteProgress.ZebetitesDestroyed = nextGeneration;
        FinishZebetiteDeath(place);
        if (nextGeneration < PersistentProgress.ZebetiteCount)
            SpawnZebetite(ZebetiteHalf.Primary);
    }

    /// <summary>
    /// Steps the shared pulse, which holds while the shared fade count is past a fade's start: the room's enemies run
    /// between a door fade's steps, and the pulse waits for the fade.
    /// </summary>
    internal void CycleZebetiteColours(RoomEnemyPlace place)
    {
        if (_fadeStepCount.IsPastStart || place.RequireSpeciesState<ZebetiteEnemyState>().IsSecondaryHalf)
            return;

        int colourCycle = (SharedColourCycle + 1) % ZebetiteDefinitions.ColourCycleLength;
        SharedColourCycle = colourCycle;
        PulseShown = colourCycle;
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

    /// <summary>Finds a free place in the room's enemies and initializes a primary or linked half from its population entry.</summary>
    internal RoomEnemyPlace SpawnZebetite(ZebetiteHalf half)
    {
        // The spawn takes the first free place. In particular, a primary's death vacates
        // its place before spawning the next generation; surviving linked halves keep
        // naming that reused place.
        RoomEnemyPlace spawned = _places.FindFreePlace() ??
            throw new InvalidOperationException("Zebetite progression filled every enemy place.");

        RoomEnemyPlacement population =
            Zebetites.SpawnPopulation(half);
        if (population.DefinitionId != EnemyDefinitionId.Zebetite)
        {
            throw new InvalidDataException(
                $"Zebetite {half} population entry names enemy " +
                $"{population.DefinitionId}.");
        }

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        _lifecycle.InitializePlaceFromDefinition(spawned, population, definition);
        _lifecycle.RunInitializationAi(spawned, EnemySpawnSurroundings.None);
        _places.CoverSpawn(spawned);
        return spawned;
    }

    /// <summary>Resolves the private tail of a shot on a Zebetite.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemyPlace struck)
    {
        _audio.QueueEnemySound(ZebetiteShotSound, SoundRequestPriority.Normal);
        RoomEnemyPlace linked = ZebetiteShotShare.Recipient(_places.InUpdateOrder, struck.RequireSpeciesState<ZebetiteEnemyState>().LinkedHalf);
        linked.Health = struck.Health;
        linked.FlashTimer = struck.FlashTimer;
    }

    /// <summary>Starts the death explosion, frees the half's place, and records its room kill.</summary>
    private void FinishZebetiteDeath(RoomEnemyPlace place)
    {
        RoomEnemySpawnSnapshot survivingSpawn = place.Spawn;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                _deathsAndDrops.DeathProjectiles,
                graphics: default);
            projectile.Position = projectile.Position.WithWhole(place.Position.Whole);
            projectile.DropDefinitionId = place.DefinitionId;
            projectile.KilledEnemy = KilledEnemy.Of(place, respawns: false);
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallExplosion));
        }

        place.Clear();
        place.Spawn = survivingSpawn;
        _roomStatus.RecordKill();
    }

    /// <summary>
    /// Shows a half's chosen picture where the shared enemy animation runs (#2250): a still picture, which shows for good on
    /// the update its clock runs out, raising the new-frame flag, until the half chooses again.
    /// </summary>
    /// <exception cref="InvalidOperationException">The half shows a picture before its setup chose one.</exception>
    internal static void AdvanceZebetiteAnimation(RoomEnemyPlace place)
    {
        ZebetiteEnemyState state = place.RequireSpeciesState<ZebetiteEnemyState>();
        if (!place.AnimationClock.Expire())
            return;
        place.Frame = state.Picture ?? throw new InvalidOperationException("A Zebetite shows its picture before its setup chose one.");
        place.AnimationClock.ShowForGood();
    }

    /// <summary>Lets bombs share the Zebetite shot tail without generic death handling.</summary>
    internal EnemyBombReaction ZebetiteBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // The Zebetite's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs Zebetite's main AI.</summary>
    internal void RunZebetiteMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunZebetiteMain(place, place.RequireSpeciesState<ZebetiteEnemyState>());
    }

    /// <summary>Runs shot damage with the Zebetite-specific linked-health tail and no generic death.</summary>
    internal EnemyShotReaction ZebetiteShotReaction => field ??= new()
    {
        AlwaysUsesCollisionPrelude = true,
        ExemptFromGenericDeath = true,
        // The Zebetite's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy),
    };

    /// <summary>Routes Zebetite contact through normal touch collision while skipping generic death animation.</summary>
    internal void ReactToZebetiteTouch(EnemyTouchContext context) =>
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
}
