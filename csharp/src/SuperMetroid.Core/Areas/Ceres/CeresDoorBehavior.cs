using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres door species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CeresDoorBehavior : IEnemySpecies, IEnemyEntityFactory
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
    /// <summary>Provides slot zero's frame counter for the shared palette cycle.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Owns VRAM and CGRAM destinations for Ceres door artwork.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Spawns smoke or explosion effects around the shaking door.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies Ridley's escape status and receives the completed-escape marker.</summary>
    private readonly ICeresEscapeStatus _ridleyEncounter;
    /// <summary>The session's installed area artwork.</summary>
    private readonly AreaArtworkBinding _areaArtwork;


    /// <summary>Captures the room systems used by the Ceres door's variants and escape sequence.</summary>
    internal CeresDoorBehavior(CeresDoorBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _projectiles = dependencies.Projectiles;
        _ridleyEncounter = dependencies.CeresEscape;
        _areaArtwork = dependencies.AreaArtwork;
    }

    /// <summary>Each door slot's typed program cursor in the loaded room.</summary>
    internal readonly Dictionary<int, AnimationProgramCursor<CeresDoorProgram>> DoorPrograms = [];

    /// <summary>Runs a door's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceCeresDoorAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<CeresDoorProgram> cursor = DoorPrograms[slot.SlotIndex];
        EnemyProgramAnimation.Run(slot, CeresDoorAnimations.Programs, ref cursor, CeresDoorFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyCeresDoorCue(slot, samus, cue));
        DoorPrograms[slot.SlotIndex] = cursor;
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

                // `$A6:F63E` subtracts the two unsigned position words, interprets the
                // wrapped result as signed, then takes its absolute value independently
                // on each axis. If either distance is at least 48 pixels, the door branches.
                int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
                int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
                return xDistance >= 48 || yDistance >= 48 ? cue.Target : null;
            }
            case CeresDoorCueKind.GoToIfAreaBossAlive:
                // `$A6:F66A-$F676` samples bit zero of the current area's SRAM-mirror
                // boss byte. Ceres begins with that bit clear, so the facing-right door
                // loops as a tangible actor during Ridley's fight. `$A6:C117` publishes
                // the boss bit together with status two; the next pass must then run on
                // past the branch to `$F68B`'s intangible setup.
                // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                // X=$0008 and clipped Samus at X=$001D after the getaway cutscene.
                return _room.Services.Bosses.IsDefeated(BossBits.AreaBoss) ? null : cue.Target;
            case CeresDoorCueKind.GoToIfRidleyNotEscaped:
                return _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ? null : cue.Target;
            case CeresDoorCueKind.MakeIntangible:
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return null;
            case CeresDoorCueKind.MakeTangible:
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                return null;
            case CeresDoorCueKind.HandDrawingToRidley:
                slot.RequireSpeciesState<CeresDoorEnemyState>().HandsDrawingToRidley = true;
                return null;
            case CeresDoorCueKind.MakeInvisible:
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.MakeVisibleAndTakeDrawingBack:
                slot.RequireSpeciesState<CeresDoorEnemyState>().HandsDrawingToRidley = false;
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.MakeVisible:
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.PlayOpeningSound:
                _audio.QueueEnemySound(SoundEffectLibrary3Sounds.CeresDoorOpening, maximumQueued: 6);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDoorCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initial countdown whose underflow ends the 49-update destruction sequence.</summary>
    private const ushort CeresDoorRumbleDuration = 48;
    /// <summary>Reload value for the interval between destruction effects.</summary>
    private const ushort CeresDoorRumbleInterval = 4;
    /// <summary>Room graphics animation selected for the smoke outcome.</summary>
    private const MiscDustAnimation CeresDoorSmokeAnimation = MiscDustAnimation.SmallExplosion;
    /// <summary>Room graphics animation selected for the explosion outcome.</summary>
    private const MiscDustAnimation CeresDoorExplosionAnimation = MiscDustAnimation.Smoke;

    /// <summary>
    /// Most recent library-two sound queued by Ceres-door destruction during this enemy frame.
    /// The view keeps its exact cadence observable to ROM-backed audits; active playback
    /// consumes the lossless <see cref="EnemyAudioQueue.SoundRequests"/> list.
    /// </summary>
    public SoundEffectId? LastCeresDoorSoundEffect { get; private set; }

    /// <summary>Gets the installed Ceres door artwork required by initialization and animation.</summary>
    private CeresDoorVisualCatalog SelectedCeresDoorVisual =>
        _areaArtwork.Artwork?.CeresDoorVisual ?? throw new InvalidOperationException(
            "Ceres door requires installed visual artwork.");

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for every retail population variant.</summary>
    internal void InitializeCeresDoor(RoomEnemySlot slot)
    {
        // Native Enemy.init0 ($0FB4) is populated from the record's parameter-one word and
        // remains mutable after load. RoomEnemySlot.Spawn.Population.Parameter1 is that exact WRAM field.
        // The earlier population "init" word seeds Enemy.instList before initialization;
        // it is not the variant despite its host-side historical name.
        ushort variant = checked((ushort)slot.Spawn.Population.Parameter1);

        CeresDoorInitializationDefinition initialization =
            CeresDoorInitializationDefinitions.For(variant);

        slot.Frame = EnemyFrameId.CeresDoorInitial;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.VramTilesIndex = EnemyTileBaseDefinitions.Fixed[EnemyDefinitionId.CeresDoor];
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        var state = new CeresDoorEnemyState { Function = initialization.MainFunction };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        DoorPrograms[slot.SlotIndex] = AnimationProgramCursor<CeresDoorProgram>.Start(initialization.Program);

        // CeresDoor_Func_1 copies the door's characters over the OBJ characters only for variant two (#1406).
        if (variant == 2)
        {
            _ = SelectedCeresDoorVisual;
            _presentation.ShowCharacters(CeresDoorCharacters.Shown);
        }

        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped && variant == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            SelectedCeresDoorVisual.LoadNormalColors(CeresDoorColourDefinitions.Inactive.In(_presentation.Cgram));
            return;
        }

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
            SelectedCeresDoorVisual.LoadEscapeColors(CeresDoorColourDefinitions.Active.In(_presentation.Cgram));
        else
            SelectedCeresDoorVisual.LoadNormalColors(CeresDoorColourDefinitions.Active.In(_presentation.Cgram));
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

            case CeresDoorFunction.RidleyEscapeMode7Wall:
                // Ridley's room overlay begins hidden. Ridley's escape reveals it and swap
                // to enemy palette seven; the actor remains present so later Mode-7 drawing
                // can retain the native priority relationship with Samus and Ridley.
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyEscaped)
                {
                    slot.PaletteIndex = EnemyPaletteBits.Palette7;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case CeresDoorFunction.RotatingElevatorRoomDefault:
                RunCeresDoorPaletteAnimation();
                if (_ridleyEncounter.CeresStatus is CeresEscapeStatus.SelfDestructStarted or CeresEscapeStatus.ElevatorDoorDestroyed)
                {
                    // $A6:F7BD installs the destruction function and initializes all three
                    // actor-owned counters, but deliberately does not execute $F7DC until
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
                RunCeresDoorPaletteAnimation();
                return;

            default:
                throw new InvalidOperationException($"Undefined CeresDoorFunction {function}.");
        }
    }

    /// <summary>Ports the two entry points at $A6:F76B/$F770 without advancing the RNG.</summary>
    private void RunCeresDoorEarthquake(ushort baseEarthquakeType)
    {
        // The door cannot start a second quake while the shared timer is active. A separate
        // engine stage decrements that timer; this actor only chooses the next type/duration.
        if (_ridleyEncounter.CeresStatus is CeresEscapeStatus.RidleyNotEscaped or CeresEscapeStatus.RidleyEscaped || !_earthquake.EarthquakeTimer.IsZero)
            return;

        Func<int> readRandomNumber = _room.Random.Read;
        // One random word in 32 (its low twelve bits below 128) picks the stronger quake.
        ushort lowTwelveBits = unchecked((ushort)(readRandomNumber() % 4096));
        if (lowTwelveBits < 128)
        {
            // Rarely select the stronger sibling quake six entries after the base type.
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(4);
            _earthquake.EarthquakeType = unchecked((ushort)(baseEarthquakeType + 6));
            return;
        }

        _earthquake.EarthquakeTimer = WrappingCountdown.Start(2);
        _earthquake.EarthquakeType = baseEarthquakeType;
    }

    /// <summary>Ports the 49-call destruction state at $A6:F7DC exactly.</summary>
    internal void RunCeresDoorRumbleAndExplosions(RoomEnemySlot slot)
    {
        CeresDoorEnemyState state = slot.RequireSpeciesState<CeresDoorEnemyState>();
        CountdownStep rumble = WrappingCounter.Decrement(checked((ushort)state.RumbleCallsLeft));
        state.RumbleCallsLeft = rumble.Value;
        if (rumble.IsNegative)
        {
            // The 49th call underflows $0000 to $FFFF. Native code hides the door, returns
            // it to the perpetual elevator-animation function, and publishes its destruction so the
            // room's Mode-7 controller begins the rotation after Ridley's escape.
            slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
            state.Function = CeresDoorFunction.ElevatorAnimation;
            if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;
            return;
        }

        CountdownStep interval = WrappingCounter.Decrement(checked((ushort)state.ExplosionIntervalLeft));
        state.ExplosionIntervalLeft = interval.Value;
        if (!interval.IsNegative)
            return;

        state.ExplosionIntervalLeft = CeresDoorRumbleInterval;
        CountdownStep burst = WrappingCounter.Decrement(checked((ushort)state.ExplosionPositionIndex));
        state.ExplosionPositionIndex = burst.IsNegative ? CeresDoorRumbleGeometryDefinitions.Count - 1 : burst.Value;

        (short xOffset, short yOffset) = CeresDoorRumbleGeometryDefinitions.Offset(state.ExplosionPositionIndex);
        ushort explosionX = unchecked((ushort)(slot.Position.X.Whole + xOffset));
        ushort explosionY = unchecked((ushort)(slot.Position.Y.Whole + yOffset));

        // GenerateRandomNumber is called exactly once per explosion. Values below $4000
        // select animation $0C; all other values select animation three (smoke).
        int randomNumber = _room.Random.Generate();
        MiscDustAnimation animation = randomNumber < 16384
            ? CeresDoorExplosionAnimation
            : CeresDoorSmokeAnimation;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(explosionX, explosionY), animation);

        // QueueSound_Lib2_Max6 runs even if the finite enemy-projectile pool was full and
        // could not allocate the visual effect, so publish the request unconditionally.
        // The request list carries the sound to the audio queue; this property is its debugger and test view.
        LastCeresDoorSoundEffect = SoundEffectLibrary2Sounds.CeresDoorExplosion;
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.CeresDoorExplosion, maximumQueued: 6);
    }

    /// <summary>Cycles door colors and the room's Mode-7 elevator frame from slot zero's frame counter.</summary>
    internal void RunCeresDoorPaletteAnimation()
    {
        // $A6:F850 selects six colors by NMI counter bits 3..5. Enemy FrameCounter advances
        // at the same accepted-frame cadence in this runtime, so slot zero is the shared
        // timebase for the room-owned palette cycle.
        int frame = _slotTable.Slots[0].FrameCounter;

        // AnimateCeresElevatorPlatform at $A6:F8F1 does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating these four Mode-7 tilemap bytes forever. Omitting this
        // queue made the moving OBJ pad flash correctly, then left the landed tile platform
        // frozen on whichever frame happened to be present at deletion.
        _room.Services.DrawMode7Map(CeresDoorVisualRomData.Mode7MapCell, SelectedCeresDoorVisual.Mode7DoorFrame((frame & 2) >> 1));

        // Bits 3..5 of the frame counter: a new row of six colors every eight frames.
        int colorRow = (frame >> 3) % 8;
        SelectedCeresDoorVisual.LoadAnimationColors(_presentation.BackgroundSets, colorRow);
    }

    /// <summary>Advances CeresDoor's typed animation.</summary>
    internal void AdvanceCeresDoorTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceCeresDoorAnimation(slot, context.Samus);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCeresDoorSoundEffect = null;
    }

}
