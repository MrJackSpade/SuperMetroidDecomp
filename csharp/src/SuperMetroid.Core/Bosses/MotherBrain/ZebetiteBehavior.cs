using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ZebetiteEntity(this, slot, common);

    /// <summary>Initializes newly spawned Zebetite slots through the common enemy lifecycle.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Supplies room artwork, data, and event services used by Zebetites.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the explosion projectile when a Zebetite half dies.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Clears actor shaking when the room earthquake ends.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The room's enemies, whose places are reused across Zebetite generations.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Exposes the door transition that keeps newly initialized Zebetites inert.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;
    /// <summary>Tracks the room kill count as Zebetite halves are removed.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Resolves touch reactions through the shared enemy collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives the Zebetite's shot sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The fade step count every gradual fade shares, past whose start the pulse holds.</summary>
    private readonly SharedFadeStepCount _fadeStepCount;


    /// <summary>Captures the shared systems used by Zebetite initialization, AI, and reactions.</summary>
    internal ZebetiteBehavior(ZebetiteBehaviorDependencies dependencies)
    {
        _audio = dependencies.Audio;
        _collision = dependencies.Collision;
        _elevatorBehavior = dependencies.ElevatorBehavior;
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _slotTable = dependencies.SlotTable;
        _roomStatus = dependencies.RoomStatus;
        _fadeStepCount = dependencies.FadeStepCount;
    }

    /// <summary>Health ceiling applied while an active Zebetite regenerates each update.</summary>
    private const int ZebetiteMaximumHealth = 1000;
    /// <summary>The sound a Zebetite makes when a shot hits it.</summary>
    private static readonly SoundEffectId ZebetiteShotSound = RoomSound.MissileDoorHit;

    /// <summary>
    /// How far ahead of Samus the camera aims, shared by every boss that retargets the camera.
    /// The new room's special-effect setup clears it on every room load.
    /// </summary>
    public CameraDistanceMode CameraDistance { get; internal set; }

    /// <summary>
    /// The Zebetites' shared pulse counter, advanced by every primary Zebetite in the room; it starts at zero with the
    /// room. It selects only the pulse the Zebetites show.
    /// </summary>
    public int SharedPaletteCycle { get; internal set; }

    /// <summary>The pulse frame last shown, or null before the pulse's first step in the room (#1512).</summary>
    internal int? PulseShown { get; private set; }

    /// <summary>The shading every Zebetite draws in: the pulse frame last shown, or the sheet's own colours before it.</summary>
    internal PictureShading PulseShading => PulseShown is int frame
        ? PictureShading.Shaded(ZebetiteShades.Pulse(frame))
        : PictureShading.Standard;

    /// <summary>Sets a Zebetite up when the room spawns it.</summary>
    internal void InitializeZebetite(RoomEnemySlot slot)
    {
        var state = new ZebetiteEnemyState(slot.Spawn.Population);
        slot.SetSpeciesState(state);

        // One more processing property is still unnamed: its wider meaning is not proven. Keep it as
        // loaded while naming only the verified animating property.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.SolidToSamus);
        slot.AnimationClock.Restart();
        slot.Shading = PulseShading;
        state.Action = state.IsSecondaryHalf
            ? ZebetiteAction.WaitForDoorTransition
            : ZebetiteAction.SpawnLinkedHalf;

        int generation = _room.Services.ZebetiteProgress.ZebetitesDestroyed;
        state.Generation = generation;
        if (generation == PersistentProgress.ZebetiteCount)
        {
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
            return;
        }

        ZebetiteGenerationDefinition definition = Zebetites.Generation(generation);
        state.GenerationFlags = definition.GenerationFlags;
        slot.Radius = slot.Radius.WithY(definition.YRadius);
        state.Playback = ZebetiteAnimations.Starting(definition.Animation);
        slot.Position = slot.Position.WithWholeX(definition.XPosition);
        slot.Position = slot.Position.WithWholeY(Zebetites.YPosition(definition, state.IsSecondaryHalf));
    }

    /// <summary>Runs the main step.</summary>
    private void RunZebetiteMain(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if (!_earthquake.IsShaking)
            slot.ShakeTimer = 0;

        switch (state.Action)
        {
            case ZebetiteAction.SpawnLinkedHalf:
                RunZebetiteSpawnLinkedHalf(slot, state);
                break;
            case ZebetiteAction.WaitForDoorTransition:
                RunZebetiteWaitForDoorTransition(slot, state);
                break;
            case ZebetiteAction.Active:
                RunZebetiteActive(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Zebetite has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Creates a linked half when this generation requires one, then enters the door-transition wait.</summary>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if ((state.GenerationFlags & ZebetiteGenerationFlags.LinkedPair) != ZebetiteGenerationFlags.None)
        {
            RoomEnemySlot secondary = SpawnZebetite(ZebetiteHalf.Linked);
            secondary.RequireSpeciesState<ZebetiteEnemyState>().LinkedHalf = slot;
            state.LinkedHalf = secondary;
        }

        state.Action = ZebetiteAction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(slot, state);
    }

    /// <summary>Starts active AI on the first update after the shared elevator door transition clears.</summary>
    private void RunZebetiteWaitForDoorTransition(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        // The door-transition flag is shared with normal elevator transitions. Holds the barrier
        // inert during door loading and enters its active body on the first clear frame.
        if (_elevatorBehavior.ElevatorDoorTransitionActive)
            return;
        state.Action = ZebetiteAction.Active;
        RunZebetiteActive(slot, state);
    }

    /// <summary>Regenerates living halves and advances the primary generation when its health reaches zero.</summary>
    private void RunZebetiteActive(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        CycleZebetitePalette(slot);
        // Every Zebetite shows the shared pulse; each primary half steps it as it runs (#1512).
        slot.Shading = PulseShading;
        SelectZebetiteHealthAnimation(slot, state);

        if (slot.Health != 0)
        {
            slot.Health = Math.Min(ZebetiteMaximumHealth, slot.Health + 1);
            return;
        }

        if (state.IsSecondaryHalf)
        {
            FinishZebetiteDeath(slot);
            return;
        }

        int nextGeneration = state.Generation + 1;
        state.Generation = nextGeneration;
        _room.Services.ZebetiteProgress.ZebetitesDestroyed = nextGeneration;
        FinishZebetiteDeath(slot);
        if (nextGeneration < PersistentProgress.ZebetiteCount)
            SpawnZebetite(ZebetiteHalf.Primary);
    }

    /// <summary>
    /// Steps the shared pulse, which holds while the shared fade count is past a fade's start: the room's enemies run
    /// between a door fade's steps, and the pulse waits for the fade.
    /// </summary>
    internal void CycleZebetitePalette(RoomEnemySlot slot)
    {
        if (_fadeStepCount.IsPastStart || slot.RequireSpeciesState<ZebetiteEnemyState>().IsSecondaryHalf)
            return;

        int paletteCycle = (SharedPaletteCycle + 1) % ZebetiteDefinitions.PaletteCycleLength;
        SharedPaletteCycle = paletteCycle;
        PulseShown = paletteCycle;
    }

    /// <summary>Restarts the animation chosen from the half's pairing and current health.</summary>
    internal static void SelectZebetiteHealthAnimation(
        RoomEnemySlot slot,
        ZebetiteEnemyState state)
    {
        state.Playback = ZebetiteAnimations.Starting(ZebetiteAnimations.HealthAnimation(
            linkedPair: (state.GenerationFlags & ZebetiteGenerationFlags.LinkedPair) != ZebetiteGenerationFlags.None,
            slot.Health));
        slot.AnimationClock.Restart();
    }

    /// <summary>Finds a free place in the room's enemies and initializes a primary or linked half from its population record.</summary>
    internal RoomEnemySlot SpawnZebetite(ZebetiteHalf half)
    {
        // The spawn takes the first free slot. In particular, a primary's death vacates
        // its place before spawning the next generation; surviving linked halves keep
        // naming that reused place.
        RoomEnemySlot spawned = _slotTable.FindFreeSlot() ??
            throw new InvalidOperationException("Zebetite progression filled every enemy slot.");

        RoomEnemyPlacement population =
            Zebetites.SpawnPopulation(half);
        if (population.DefinitionId != EnemyDefinitionId.Zebetite)
        {
            throw new InvalidDataException(
                $"Zebetite {half} spawn record names enemy " +
                $"{population.DefinitionId}.");
        }

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionId);
        _lifecycle.InitializeSlotFromDefinition(spawned, population, definition);
        _lifecycle.RunInitializationAi(spawned, EnemySpawnSurroundings.None);
        _slotTable.CoverSpawn(spawned);
        return spawned;
    }

    /// <summary>Resolves the private tail of a shot on a Zebetite.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemySlot struck)
    {
        _audio.QueueEnemySound(ZebetiteShotSound, SoundRequestPriority.Normal);
        RoomEnemySlot linked = ZebetiteShotShare.Recipient(_slotTable.Slots, struck.RequireSpeciesState<ZebetiteEnemyState>().LinkedHalf);
        linked.Health = struck.Health;
        linked.FlashTimer = struck.FlashTimer;
    }

    /// <summary>Starts the death explosion, frees the half's slot, and records its room kill.</summary>
    private void FinishZebetiteDeath(RoomEnemySlot slot)
    {
        RoomEnemySpawnSnapshot survivingSpawn = slot.Spawn;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            MotherBrainProjectileStarts.Initialize(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphics: default);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.DropDefinitionId = slot.DefinitionId;
            projectile.KilledEnemy = KilledEnemy.Of(slot, respawns: false);
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallExplosion));
        }

        slot.Clear();
        slot.Spawn = survivingSpawn;
        _roomStatus.RecordKill();
    }

    /// <summary>
    /// Runs a half's health animation where the shared enemy animation runs: its pose shows on the update the
    /// timer expires, raising the new-frame flag, and the script's sleep then keeps it.
    /// </summary>
    internal static void AdvanceZebetiteAnimation(RoomEnemySlot slot)
    {
        ZebetiteEnemyState state = slot.RequireSpeciesState<ZebetiteEnemyState>();
        if (!slot.AnimationClock.Expire())
            return;
        // A held pose is the script's sleep: the timer wraps from zero and the pose stays.
        AnimationPlayback<ZebetiteSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, ZebetiteAnimations.Segments, ref playback, ZebetiteVisualDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException($"Zebetite animations raise no event {cue}."));
        state.Playback = playback;
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
    internal void RunZebetiteMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunZebetiteMain(slot, slot.RequireSpeciesState<ZebetiteEnemyState>());
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
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
}
