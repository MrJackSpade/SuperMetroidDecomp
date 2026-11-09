using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres door species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CeresDoorBehavior : IEnemySpecies, IEnemyEntityFactory, IGameplayShadingSource
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CeresDoor];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CeresDoorEntity(this, slot, common);

    /// <summary>Provides Ceres room artwork, boss status, and the gameplay RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the quake type and duration selected during the escape.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the door's opening and destruction sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Shows the pre-explosion overlay's characters over the room's sprite characters.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Spawns smoke or explosion effects around the shaking door.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies Ridley's escape status and receives the completed-escape marker.</summary>
    private readonly ICeresEscapeStatus _ridleyEncounter;


    /// <summary>Captures the room systems used by the Ceres door's variants and escape sequence.</summary>
    internal CeresDoorBehavior(CeresDoorBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _projectiles = dependencies.Projectiles;
        _ridleyEncounter = dependencies.CeresEscape;
    }

    /// <summary>
    /// The step of the elevator room's six-colour glow the rotating-room door cycles, which the room's tiles show as
    /// <see cref="CeresDoorShades.ElevatorRoomGlow"/> (#1512); null before the door first cycles it.
    /// </summary>
    internal int? ElevatorRoomGlowStep { get; private set; }

    /// <summary>Lays the elevator room's glow step, once the door has cycled one, over the room's pictures (#1512).</summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (ElevatorRoomGlowStep is int step)
            frame.Shade(GameplayPictures.Room, CeresDoorShades.ElevatorRoomGlow(step));
    }

    /// <summary>Runs a door's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceCeresDoorAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        CeresDoorEnemyState state = slot.RequireSpeciesState<CeresDoorEnemyState>();
        AnimationProgramCursor<CeresDoorProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, CeresDoorAnimations.Programs, ref cursor, CeresDoorFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyCeresDoorCue(slot, samus, cue));
        state.Program = cursor;
    }

    /// <summary>Carries out one cue of a door's program, returning the program a branch continues at, or null to run on.</summary>
    private CeresDoorProgram? ApplyCeresDoorCue(RoomEnemySlot slot, SamusState? samus, CeresDoorCue cue)
    {
        switch (cue.Kind)
        {
            case CeresDoorCueKind.GoToIfSamusDistant:
            {
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres door proximity instruction requires the active Samus actor.");
                }

                // The distance test subtracts the two unsigned position words, interprets the
                // wrapped result as signed, then takes its absolute value independently
                // on each axis. If either distance is at least 48 pixels, the door branches.
                int xDistance = CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole);
                int yDistance = CoordinateAxis.Distance(slot.Position.Y.Whole, samus.Position.Y.Whole);
                return xDistance >= 48 || yDistance >= 48 ? cue.Target : null;
            }
            case CeresDoorCueKind.GoToIfAreaBossAlive:
                // The test samples bit zero of the current area's saved
                // boss byte. Ceres begins with that bit clear, so the facing-right door
                // loops as a tangible actor during Ridley's fight. Ridley's escape publishes
                // the boss bit together with status two; the next pass must then run on
                // past the branch to the intangible setup.
                // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                // X=8 and clipped Samus at X=29 after the getaway cutscene.
                return _room.Services.Bosses.IsDefeated(BossBits.AreaBoss) ? null : cue.Target;
            case CeresDoorCueKind.GoToIfRidleyNotEscaped:
                return _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ? null : cue.Target;
            case CeresDoorCueKind.MakeIntangible:
                slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.MakeTangible:
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.HandDrawingToRidley:
                slot.RequireSpeciesState<CeresDoorEnemyState>().HandsDrawingToRidley = true;
                return null;
            case CeresDoorCueKind.MakeInvisible:
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.MakeVisibleAndTakeDrawingBack:
                slot.RequireSpeciesState<CeresDoorEnemyState>().HandsDrawingToRidley = false;
                slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.MakeVisible:
                slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.PlayOpeningSound:
                _audio.QueueEnemySound(RoomSound.CeresDoorOpening, priority: SoundRequestPriority.Normal);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDoorCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initial countdown whose underflow ends the 49-update destruction sequence.</summary>
    private const int CeresDoorRumbleDuration = 48;
    /// <summary>Reload value for the interval between destruction effects.</summary>
    private const int CeresDoorRumbleInterval = 4;
    /// <summary>Room graphics animation selected for the smoke outcome.</summary>
    private const MiscDustAnimation CeresDoorSmokeAnimation = MiscDustAnimation.SmallExplosion;
    /// <summary>Room graphics animation selected for the explosion outcome.</summary>
    private const MiscDustAnimation CeresDoorExplosionAnimation = MiscDustAnimation.Smoke;

    /// <summary>Initializes the Ceres door for every retail population variant.</summary>
    internal void InitializeCeresDoor(RoomEnemySlot slot)
    {
        CeresDoorVariant variant = slot.Spawn.Population.SpawnSettingsOf<CeresDoorSpawnSettings>().Variant;
        CeresDoorInitializationDefinition initialization = CeresDoorInitializations.For(variant);

        slot.Frame = EnemyFrameId.CeresDoorInitial;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        var state = new CeresDoorEnemyState
        {
            Function = initialization.MainFunction,
            Program = AnimationProgramCursor<CeresDoorProgram>.Start(initialization.Program),
        };
        slot.SetSpeciesState(state);

        // Only the pre-explosion overlay shows the door's own look (#1406, #1538).
        if (variant == CeresDoorVariant.RotatingElevatorPreExplosionOverlay)
            _presentation.ShowLook(CeresDoorLookDefinitions.OverlayLook);

        // The game loads the door's colours into one of two palettes, the inactive door's or the active door's, whose
        // completed fade target this runtime shows directly: the escaping colours once Ridley has escaped, otherwise the
        // normal ones. The door shows them as its shade (#1512); which palette held them is not kept.
        slot.Shading = _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped
            ? CeresDoorShades.InEscapingColours
            : CeresDoorShades.InNormalColours;
    }

    /// <summary>Dispatches the door's main-AI function.</summary>
    internal void RunCeresDoorMain(RoomEnemySlot slot)
    {
        CeresDoorEnemyState state = slot.RequireSpeciesState<CeresDoorEnemyState>();
        CeresDoorFunction function = state.Function
            ?? throw new InvalidDataException("A Ceres door has no main function.");
        switch (function)
        {
            case CeresDoorFunction.HandleEarthquakeDuringEscape:
                RunCeresDoorEarthquake(baseEarthquakeType: 20);
                return;
            case CeresDoorFunction.HandleEarthquakeDuringEscapeInRidleysRoom:
                RunCeresDoorEarthquake(baseEarthquakeType: 29);
                return;

            case CeresDoorFunction.RidleyEscapeFloorWall:
                // A getaway wall actor only holds its place: the getaway scene shows the chamber's walls (#1545).
                return;

            case CeresDoorFunction.RotatingElevatorRoomDefault:
                RunCeresDoorPaletteAnimation(slot);
                if (_ridleyEncounter.CeresStatus is CeresEscapeStatus.SelfDestructStarted or CeresEscapeStatus.ElevatorDoorDestroyed)
                {
                    // Entering the destruction state installs the destruction function and initializes all three
                    // actor-owned counters, but deliberately does not run the destruction step until
                    // the following enemy frame.
                    state.Function = CeresDoorFunction.RotatingElevatorRumble;
                    state.RumbleCallsLeft = CeresDoorRumbleDuration;
                    state.ExplosionIntervalLeft = 0;
                    state.ExplosionPositionIndex = 0;
                }
                return;

            case CeresDoorFunction.RotatingElevatorRumble:
                RunCeresDoorRumbleAndExplosions(slot);
                return;

            case CeresDoorFunction.ElevatorAnimation:
                RunCeresDoorPaletteAnimation(slot);
                return;

            default:
                throw new InvalidOperationException($"Undefined CeresDoorFunction {function}.");
        }
    }

    /// <summary>Runs the earthquake shared by both escape behaviours without advancing the RNG.</summary>
    private void RunCeresDoorEarthquake(int baseEarthquakeType)
    {
        // The door cannot start a second quake while the shared timer is active. A separate
        // engine stage decrements that timer; this actor only chooses the next type/duration.
        if (_ridleyEncounter.CeresStatus is CeresEscapeStatus.RidleyNotEscaped or CeresEscapeStatus.RidleyEscaped || !_earthquake.EarthquakeTimer.IsZero)
            return;

        // One random word in 32 (its strength roll below 128) picks the stronger quake.
        int strengthRoll = CeresDoorQuakeDefinitions.StrengthRoll.Of(_room.Random.Read());
        if (strengthRoll < 128)
        {
            // Rarely select the stronger sibling quake six entries after the base type.
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(4);
            _earthquake.EarthquakeType = baseEarthquakeType + 6;
            return;
        }

        _earthquake.EarthquakeTimer = WrappingCountdown.Start(2);
        _earthquake.EarthquakeType = baseEarthquakeType;
    }

    /// <summary>Runs the 49-call destruction state exactly.</summary>
    internal void RunCeresDoorRumbleAndExplosions(RoomEnemySlot slot)
    {
        CeresDoorEnemyState state = slot.RequireSpeciesState<CeresDoorEnemyState>();
        CountdownStep rumble = WrappingCounter.Decrement(Ensure.BetweenInclusive(0, ushort.MaxValue, state.RumbleCallsLeft));
        state.RumbleCallsLeft = rumble.Remaining.RemainingFrames;
        if (rumble.IsNegative)
        {
            // The 49th call underflows zero to -1. The door hides, returns
            // it to the perpetual elevator-animation function, and publishes its destruction so the
            // room's turning-floor controller begins the rotation after Ridley's escape.
            slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
            state.Function = CeresDoorFunction.ElevatorAnimation;
            if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;
            return;
        }

        CountdownStep interval = WrappingCounter.Decrement(Ensure.BetweenInclusive(0, ushort.MaxValue, state.ExplosionIntervalLeft));
        state.ExplosionIntervalLeft = interval.Remaining.RemainingFrames;
        if (!interval.IsNegative)
            return;

        state.ExplosionIntervalLeft = CeresDoorRumbleInterval;
        CountdownStep burst = WrappingCounter.Decrement(Ensure.BetweenInclusive(0, ushort.MaxValue, state.ExplosionPositionIndex));
        state.ExplosionPositionIndex = burst.IsNegative ? CeresDoorRumbleGeometryDefinitions.Count - 1 : burst.Remaining.RemainingFrames;

        Offset16<RoomPixels> burstOffset = CeresDoorRumble.Offset(state.ExplosionPositionIndex);
        int xOffset = burstOffset.X;
        int yOffset = burstOffset.Y;
        Position16<RoomPixels> explosion = new(
            CoordinateAxis.Wrap(slot.Position.X.Whole + xOffset), CoordinateAxis.Wrap(slot.Position.Y.Whole + yOffset));

        // The shared random sequence advances exactly once per explosion. Values below 16384
        // select the large explosion; all other values select animation three (smoke).
        int randomNumber = _room.Random.Generate();
        MiscDustAnimation animation = randomNumber < 16384
            ? CeresDoorExplosionAnimation
            : CeresDoorSmokeAnimation;
        _projectiles.SpawnRoomGraphicsDustExplosion(explosion, animation);

        // The explosion sound is requested even when the finite enemy-projectile pool was full and
        // could not allocate the visual effect.
        _audio.QueueEnemySound(EnemySound.BigExplosion, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Cycles the elevator room's glow and the room's turning-floor elevator frame by <paramref name="door"/>'s own update
    /// count. The glow and the platform strip are only drawn, so the door clocks them itself; the original game read the
    /// room's first enemy's count, which only shifts the drawn phase where another enemy is listed first.
    /// </summary>
    internal void RunCeresDoorPaletteAnimation(RoomEnemySlot door)
    {
        int frame = Ensure.NotNull(door).FrameCounter;

        // The elevator platform's animation does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating the turning floor's platform frames forever. Omitting this
        // queue made the moving sprite pad flash correctly, then left the landed floor platform
        // frozen on whichever frame happened to be present at deletion. The import pictures both strips (#1467).
        _room.Services.DrawFloor(CeresFloorAnimation.PlatformFrame((frame & 2) >> 1));

        // Bits 3..5 of the frame counter: a new step of six colors every eight frames.
        int colorRow = (frame >> 3) % 8;
        ElevatorRoomGlowStep = colorRow;
    }

    /// <summary>Advances CeresDoor's typed animation.</summary>
    internal void AdvanceCeresDoorTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceCeresDoorAnimation(slot, context.Samus);
    }
}
