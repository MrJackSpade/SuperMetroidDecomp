using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
[RoomLifetime]
internal sealed class DraygonParts
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the room shaking caused by Draygon's attacks and death.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Updates the body and eye palette bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing for the body and linked-part AI callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonParts(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, RoomSpriteObjectPool spriteObjects, EnemyPresentation presentation, EnemyFrameContext frameContext, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _spriteObjects = spriteObjects;
        _presentation = presentation;
        _frameContext = frameContext;
    }

    /// <summary>The typed encounter extension while the retail Draygon population is loaded.</summary>
    public DraygonEnemyState? Draygon { get; internal set; }

    /// <summary>Ports <c>InitAI_DraygonBody</c> at <c>$A5:8687</c>.</summary>
    internal void InitializeDraygonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
            throw new InvalidDataException("Draygon's body must own native enemy slot $0000.");

        // The native copy targets colors 144..168. This runtime has one concrete CGRAM
        // surface instead of a second fade-target array, so publish those exact 25 colors
        // immediately; the room's normal fade-in controls when they become visible.
        (_bossArtwork.Artwork?.DraygonColors ?? throw new InvalidOperationException(
            "Draygon requires installed color artwork.")).ApplyIntro(_presentation.Colours);

        // Draygon shows his body on a cleared body layer, both pages of it, not merely the one shown (#1354, #1466); his
        // body frames are drawn on it.
        _presentation.AttachBossBody(new BossBodyCanvas(_bossArtwork.Body(BossBodyOwner.Draygon), BossBodyDefinitions.SharedBodyRows));

        body.Colouring = new EncounterSpriteColouring(BossColourDefinitions.DraygonSprite);
        body.InstructionTimer = WrappingCountdown.Start(1);

        _frameContext.RequestFullSweep();
        Draygon = new DraygonEnemyState(body)
        {
            MinimapDisabledAndBossTilesExplored = true,
            Function = DraygonAiFunction.IntroInitialDelay,
        };
        body.SetSpeciesState(Draygon, Draygon.ClearWithRecord);
        Draygon.SetCursor(body, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.BodyFacingLeftIdle));
        Draygon.DisabledCannons.Add(DraygonWallCannon.UnusedBottom);
    }

    /// <summary>Ports the independent initializers at <c>$A5:C46B/C599/C5AD</c>.</summary>
    internal void InitializeDraygonPart(RoomEnemySlot part)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part appeared before the body record.");

        part.InstructionTimer = WrappingCountdown.Start(1);
        switch (DraygonPartDefinitions.Parts[part.DefinitionId])
        {
            case DraygonPart.Eye:
                if (part.SlotIndex != 1)
                    throw new InvalidDataException("Draygon's eye must own native slot $0040.");
                // Inert until the body publishes facing AI.
                var eyeState = new DraygonEyeState { Function = DraygonEyeFunction.Inert };
                part.SetSpeciesState(eyeState, eyeState.ClearWithRecord);
                state.Eye = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.EyeFacingLeftIdle));
                return;

            case DraygonPart.Tail:
                if (part.SlotIndex != 2)
                    throw new InvalidDataException("Draygon's tail must own native slot $0080.");
                part.Colouring = new EncounterSpriteColouring(BossColourDefinitions.DraygonSprite);
                state.Tail = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.TailFacingLeftInitialFakeWhip));
                return;

            case DraygonPart.Arms:
                if (part.SlotIndex != 3)
                    throw new InvalidDataException("Draygon's arms must own native slot $00C0.");
                // Body init briefly writes $9813 before this record exists. Retail part init
                // clears that lost write and installs the ordinary idle loop at $97E7.
                part.Colouring = new EncounterSpriteColouring(BossColourDefinitions.DraygonSprite);
                part.Layer = 2;
                state.Arms = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.ArmsFacingLeftIdle));
                return;

            case DraygonPart.Body:
            default:
                throw new InvalidDataException(
                    $"Enemy {part.DefinitionId} is not a Draygon part.");
        }
    }

    /// <summary>Ports the no-op tail/arms main AI and the active eye dispatcher.</summary>
    internal void RunDraygonPartMain(RoomEnemySlot part, SamusState? samus)
    {
        if (part.DefinitionId is EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms)
            return;
        if (part.DefinitionId != EnemyDefinitionId.DraygonEye)
            throw new InvalidDataException("Draygon part dispatcher received another family.");

        DraygonEyeFunction function = part.RequireSpeciesState<DraygonEyeState>().Function
            ?? throw new InvalidDataException("Draygon's eye has no function.");
        switch (function)
        {
            case DraygonEyeFunction.Inert:
                return;
            case DraygonEyeFunction.FacingLeft:
                TrackSamusWithDraygonEye(part, samus, facing: HorizontalDirection.Left);
                return;
            case DraygonEyeFunction.FacingRight:
                TrackSamusWithDraygonEye(part, samus, facing: HorizontalDirection.Right);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon eye function {function}.");
        }
    }

    /// <summary>Restarts a linked Draygon part's program on its next update, as a native list install does.</summary>
    internal void StartDraygonProgram(RoomEnemySlot part, DraygonProgram program)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon program started without encounter state.");
        state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(program));
        part.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Runs a Draygon part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, a new frame raises the new-frame flag, waits keep the frame, and a stop deletes the
    /// part. The part's loop counter is its native timer word.
    /// </summary>
    internal void AdvanceDraygonPartAnimation(RoomEnemySlot part, SamusState? samus)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part ran without encounter state.");
        CountdownStep instructionTimer = part.InstructionTimer.Decrement();
        part.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            part.ExtraProperties = part.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<DraygonProgram> cursor = state.Cursor(part);
        WrappingCountdown counter = part.Timer;
        AnimationProgramAdvance<DraygonFrame> advance = DraygonAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyDraygonCue(state, part, samus, cue));
        part.Timer = counter;
        switch (advance)
        {
            case AnimationProgramAdvance<DraygonFrame>.Show shown:
                state.SetCursor(part, cursor);
                part.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                part.Frame = DraygonFrameDefinitions.ShownFrames.Of(shown.Frame);
                part.ExtraProperties = part.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<DraygonFrame>.Wait waiting:
                state.SetCursor(part, cursor);
                part.InstructionTimer = WrappingCountdown.FromRaw((ushort)waiting.Duration);
                return;
            case AnimationProgramAdvance<DraygonFrame>.Hold:
                // The native sleep: the timer wraps from zero and the last frame stays.
                state.SetCursor(part, cursor);
                return;
            case AnimationProgramAdvance<DraygonFrame>.Stop:
                // The native stop deletes the part and leaves its instruction pointer unwritten.
                part.Properties = part.Properties.With(EnemyProperties.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Undefined animation program advance {advance}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a Draygon program run by <paramref name="part"/>. Program starts write the parts'
    /// stored cursors, so the running part's own write is replaced when its update ends, as the native interpreter's
    /// final instruction-pointer store replaces it.
    /// </summary>
    /// <returns>Always null: Draygon's cues never redirect the running program.</returns>
    private DraygonProgram? ApplyDraygonCue(DraygonEnemyState state, RoomEnemySlot part, SamusState? samus, DraygonCue cue)
    {
        switch (cue)
        {
            case DraygonCue.StartPartPrograms start:
                state.SetCursor(state.Body, AnimationProgramCursor<DraygonProgram>.Start(start.Body));
                state.SetCursor(RequirePart(state.Eye), AnimationProgramCursor<DraygonProgram>.Start(start.Eye));
                state.SetCursor(RequirePart(state.Tail), AnimationProgramCursor<DraygonProgram>.Start(start.Tail));
                state.SetCursor(RequirePart(state.Arms), AnimationProgramCursor<DraygonProgram>.Start(start.Arms));
                state.Arms.InstructionTimer = WrappingCountdown.Start(1);
                state.Tail.InstructionTimer = WrappingCountdown.Start(1);
                state.Eye.InstructionTimer = WrappingCountdown.Start(1);
                state.Body.InstructionTimer = WrappingCountdown.Start(1);
                return null;
            case DraygonCue.SetEyeFunction eyeFunction:
                (RequirePart(state.Eye)).RequireSpeciesState<DraygonEyeState>().Function = eyeFunction.Function;
                return null;
            case DraygonCue.SetOwnEyeFunction ownFunction:
                part.RequireSpeciesState<DraygonEyeState>().Function = ownFunction.Function;
                return null;
            case DraygonCue.DisplaceBodyGraphics displace:
                state.BodyGraphicsDisplacement = displace.Offset;
                return null;
            case DraygonCue.QueueSound { Sound.Library: SoundEffectLibrary.Library2 } sound:
                state.LastSoundLibrary2 = sound.Sound;
                return null;
            case DraygonCue.QueueSound { Sound.Library: SoundEffectLibrary.Library3 } sound:
                state.LastSoundLibrary3 = sound.Sound;
                return null;
            case DraygonCue.SpawnDyingObject dying:
                SpawnRandomDyingDraygonObject(state, dying.Kind);
                return null;
            case DraygonCue.ParalyseTailAndArms:
                StartDraygonProgram(RequirePart(state.Tail), DraygonProgram.Sleep);
                StartDraygonProgram(RequirePart(state.Arms), DraygonProgram.Sleep);
                return null;
            case DraygonCue.SetBodyIntangible:
                state.Body.Properties = state.Body.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return null;
            case DraygonCue.SetBodyFunction bodyFunction:
                state.Function = bodyFunction.Function;
                return null;
            case DraygonCue.TailWhipHit:
                ApplyDraygonTailWhipHit(state, samus);
                return null;
            case DraygonCue.SpawnGoop goop:
                SpawnDraygonGoop(state, goop.Direction == HorizontalDirection.Right);
                return null;
            default:
                throw new InvalidOperationException($"Undefined Draygon cue {cue}.");
        }
    }

    /// <summary>A linked Draygon part, failing if its record never loaded.</summary>
    internal static RoomEnemySlot RequirePart(RoomEnemySlot? part) =>
        part ?? throw new InvalidDataException("A Draygon program ran before all four parts loaded.");

    /// <summary>Ports the eye direction partition at <c>$A5:C48D/C513</c>.</summary>
    internal void TrackSamusWithDraygonEye(RoomEnemySlot eye, SamusState? samus, HorizontalDirection facing)
    {
        // Native emission precedes the angle-change early return, so a stationary
        // target must not suppress the periodic particle. It shares the real pool.
        if ((eye.FrameCounter & DraygonEyeEffects.CadenceMask) == 0)
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(unchecked((ushort)(eye.Position.X.Whole + (facing == HorizontalDirection.Right
                    ? DraygonEyeEffects.HorizontalOffset : -DraygonEyeEffects.HorizontalOffset))), unchecked((ushort)(eye.Position.Y.Whole + DraygonEyeEffects.VerticalOffset))),
                DraygonEyeEffects.DustVariant);
        if (samus is null)
            return;

        Position16<RoomPixels> gaze = eye.Position.Whole +
            new Offset16<RoomPixels>((short)(facing == HorizontalDirection.Right ? 24 : -24), -32);
        int angle = EnemyMath.CalculateBearing(samus.Position.Whole - gaze).RawValue;
        DraygonEyeState eyeState = eye.RequireSpeciesState<DraygonEyeState>();
        if (angle == (byte)eyeState.LookAngle)
            return;

        eyeState.LookAngle = angle;
        DraygonProgram look = angle switch
        {
            < 32 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingUp
                : DraygonProgram.EyeFacingLeftLookingUp,
            < 96 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingRight
                : DraygonProgram.EyeFacingLeftLookingRight,
            < 160 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingDown
                : DraygonProgram.EyeFacingLeftLookingDown,
            < 224 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingLeft
                : DraygonProgram.EyeFacingLeftLookingLeft,
            _ => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingUp
                : DraygonProgram.EyeFacingLeftLookingUp,
        };
        StartDraygonProgram(eye, look);
        eye.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Places a death-animation sprite at a room-RNG offset from the body.</summary>
    private void SpawnRandomDyingDraygonObject(
        DraygonEnemyState state,
        RoomSpriteObjectKind kind)
    {
        int random = _room.Random.Generate();
        Offset16<RoomPixels> scatter = new((short)((random & 127) - 64), (short)((SharedRandomState.UpperByte(random) & 127) - 64));
        if (_spriteObjects.SpawnRoomSpriteObject(
                state.Body.Position.Whole + scatter,
                kind,
                graphics: default) is not null)
        {
            state.DeathAnimationObjectsSpawned++;
        }
    }

    /// <summary>Ports tail instruction <c>$A5:9B9A</c>, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon's tail-hit instruction requires Samus state.");

        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
        int rawDamage = state.Body.Definition.Damage;
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? rawDamage >> 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? rawDamage >> 1
                : rawDamage;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;

        // `$A5:9B9A` also publishes the same 32-frame, type-seven quake used by the
        // cartridge's grabbed-tail impact. Keeping it on the shared room-enemy surface lets
        // the ordinary camera renderer observe the hit instead of reducing it to HP loss.
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
        _earthquake.EarthquakeType = 7;

        state.TailWhipHits++;
        _spriteObjects.SpawnRoomSpriteObject(
            new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + 16))),
            RoomSpriteObjectKind.DustCloud,
            graphics: default);
    }

    /// <summary>Ports the two body instruction producers at <c>$A5:9F7C/9FAE</c>.</summary>
    internal void SpawnDraygonGoop(DraygonEnemyState state, bool movingRight)
    {
        RoomEnemyProjectileSlot? goop = _projectiles.AllocateEnemyProjectile();
        if (goop is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            graphics: SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(BossColourDefinitions.DraygonGoop)));
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.Goop));
        goop.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(state.Body.Position.X.Whole + (movingRight ? 24 : -28))), unchecked((ushort)(state.Body.Position.Y.Whole - 16))));

        int random = _room.Random.Generate();
        ByteAngle angle = new(unchecked((byte)((random & 63) + (movingRight ? 192 : 128))));
        SetDraygonProjectileVelocity(goop, angle, DraygonProjectileSpeeds.Goop);
        state.GoopProjectilesSpawned++;
    }

    /// <summary>Launches a Draygon projectile along <paramref name="angle"/> at <paramref name="speed"/>.</summary>
    internal static void SetDraygonProjectileVelocity(
        RoomEnemyProjectileSlot projectile,
        ByteAngle angle,
        int speed) =>
        projectile.State = AngledFlightState.Launch(angle, speed);
}
