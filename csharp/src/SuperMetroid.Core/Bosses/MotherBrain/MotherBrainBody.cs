using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's body and tank: their setup, programs, movement, footsteps and the first phase.</summary>
[RoomLifetime]
internal sealed class MotherBrainBody
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;

    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;
    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainBody(MotherBrainBodyDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _presentation = dependencies.Presentation;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _roomActors = dependencies.RoomActors;

        _projectileAttacks = dependencies.ProjectileAttacks;
        _access = dependencies.Access;
    }

    /// <summary>
    /// Runs a falling tube's program where the enemy instruction interpreter would: its pose shows on the update the
    /// native timer expires, raising the new-frame flag, and the native sleep then keeps it.
    /// </summary>
    internal static void AdvanceMotherBrainTubeAnimation(RoomEnemySlot tube)
    {
        CountdownStep instructionTimer = tube.InstructionTimer.Decrement();
        tube.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            tube.ExtraProperties = tube.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        AnimationProgramCursor<MotherBrainTubePiece> cursor = tubeState.Cursor;
        WrappingCountdown counter = tube.Timer;
        AnimationProgramAdvance<MotherBrainTubeFrame> advance = MotherBrainTubeAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => throw new InvalidOperationException($"Falling-tube programs publish no cue {cue}."));
        tube.Timer = counter;
        tubeState.Cursor = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<MotherBrainTubeFrame>.Show shown:
                tube.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                tube.Frame = MotherBrainVisualDefinitions.TubeFrames.Of(shown.Frame);
                tube.ExtraProperties = tube.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<MotherBrainTubeFrame>.Hold:
                // The native sleep: the timer wraps from zero and the pose stays.
                return;
            default:
                throw new InvalidOperationException($"Falling-tube programs have no {advance} step.");
        }
    }

    /// <summary>Ports <c>InitAI_MotherBrainBody</c> at <c>$A9:8687</c>.</summary>
    internal void InitializeMotherBrainBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain's body requires native slot zero, not slot {body.SlotIndex}.");
        }

        body.InstructionTimer = WrappingCountdown.Start(1);
        body.TileIndexBase = EnemyTileBaseDefinitions.Fixed[EnemyDefinitionId.MotherBrainBody];
        body.Properties = body.Properties.With(
            EnemyProperties.BlocksPlasmaBeam |
            EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.Invisible);
        body.Colouring = SharedSpriteColouring.Common;

        // Both source labels include transparent color zero. `$A9:86B3/$86C0` deliberately
        // begin at +2 and copy only colors 1..15 into the otherwise unrelated room palette
        // ranges used by glass shards and tube projectiles.
        LoadMotherBrainRoomEntryColors();

        _motherBrainEncounter.MotherBrain = new MotherBrainEnemyState(body)
        {
            Form = MotherBrainForm.Glass,
            EnableUnpauseHook = false,
            HitboxesEnabled = MotherBrainHitboxParts.Brain,
            BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn,
            Function = MotherBrainBodyFunction.FirstPhase,
            // Her body is staged on a cleared canvas, without replacing the visible pipe background; her phase-two
            // transition shows it.
            StagedBody = new BossBodyCanvas(_bossArtwork.Body(BossBodyOwner.MotherBrain), BossBodyDefinitions.SharedBodyRows),
        };
        // The first LoadFxEntry, which the room loader applies after enemy initialization.
        _frameResults.MotherBrainFxEntry = 1;
        SpawnMotherBrainInitialTurrets();
    }

    /// <summary>Applies the installed room-entry palette for glass and tube artwork.</summary>
    internal void LoadMotherBrainRoomEntryColors()
    {
        (_bossArtwork.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room requires installed color artwork."))
            .ApplyRoomEntry(_presentation.Colours);
    }

    /// <summary>Updates the glass-bound first phase and advances its scripted reaction to Samus.</summary>
    internal void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState? samus)
    {
        // Before PLM `$D6DE` finishes the nineteen missile increments and its 48-frame tail,
        // event two is clear and `$A9:87E1` performs no phase mutation. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body-function dispatcher.
        bool glassDestroyed = _room.Services.Events.Has(EventNumber.MotherBrainGlassDestroyed);
        if (glassDestroyed)
        {
            state.BrainMainShakeTimer = WrappingCountdown.FromRaw(_earthquake.EarthquakeTimer.RawValue);
            if (samus is not null && samus.Position.X.Whole < 236 && state.Head.Health == 0)
            {
                // This is the exact first-phase exit at `$A9:87FE-$8814`. The following
                // function remains represented by its native pointer so the next frame
                // enters the real timed descent rather than jumping to standing form.
                state.DeleteTurretsAndRinkas = true;
                // Mother Brain and Rinka alias native slot-zero variable $1D. Publish the
                // shared word before later enemy slots can keep drawing replaced tile art.
                _roomActors.TerminateRinkas();
                state.Form = MotherBrainForm.FakeDeath;
                state.RequestMusic(MusicRequest.SelectTrack(6), MusicQueueDelay.Shortest);

                // MotherBrain_SealWall at `$AD:E396` creates both dust puffs before its
                // two identical hardcoded PLMs. Requests remain ordered because the bank
                // $84 allocator searches the same descending pool for each call.
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 72), MiscDustAnimation.SmallDustCloud);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 152), MiscDustAnimation.SmallDustCloud);
                _access.RequestMotherBrainPlm(new(15, 4), PlmHeaderId.FillMotherBrainsWall);
                _access.RequestMotherBrainPlm(new(15, 9), PlmHeaderId.FillMotherBrainsWall);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentInitialPause;
            }
        }

        // `$A9:87E1` always falls through to the encounter's custom rectangle walker,
        // including the frame that arms fake death. This collision is not represented by
        // either enemy header's ordinary radius and must stay in the body dispatcher.
        if (samus is not null)
            ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>
    /// Runs the body's program, or the head slot's dummy, where the enemy instruction interpreter would: the frame
    /// expires on the update its native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceMotherBrainSlotAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        MotherBrainEnemyState state = _access.RequireCompleteMotherBrainState(slot);
        bool body = ReferenceEquals(slot, state.Body);
        CountdownStep instructionTimer = slot.InstructionTimer.Decrement();
        slot.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<MotherBrainBodyProgram> cursor = body ? state.BodyCursor : state.HeadSlotCursor;
        WrappingCountdown counter = slot.Timer;
        AnimationProgramAdvance<MotherBrainBodyFrame> advance = MotherBrainAnimations.Body.Advance(ref cursor, ref counter,
            (cue, continuation) =>
            {
                ApplyMotherBrainBodyCue(state, samus, cue);
                return null;
            });
        slot.Timer = counter;
        if (body)
            state.BodyCursor = cursor;
        else
            state.HeadSlotCursor = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<MotherBrainBodyFrame>.Show shown:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                slot.Frame = MotherBrainBodyVisualDefinitions.ShownFrames.Of(shown.Frame);
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<MotherBrainBodyFrame>.Hold:
                // The native sleep: the timer wraps from zero and the last frame stays.
                return;
            default:
                throw new InvalidOperationException($"Mother Brain's body programs have no {advance} step.");
        }
    }

    /// <summary>Carries out one cue of a body program.</summary>
    private void ApplyMotherBrainBodyCue(MotherBrainEnemyState state, SamusState? samus, MotherBrainBodyCue cue)
    {
        switch (cue)
        {
            // Pose is shared encounter state, not a property of the displayed map. Body AI
            // waits on these exact values while the program continues independently.
            case MotherBrainBodyCue.SetPose setPose:
                state.Pose = setPose.Pose;
                return;
            // The walk-cycle comments in the historical disassembly describe visual
            // motion and are occasionally opposite the literal signed Y operand. The
            // programs keep the actual additions performed by `$9579`, not the labels.
            case MotherBrainBodyCue.Move move:
                MoveMotherBrainBody(move.Offset);
                return;
            case MotherBrainBodyCue.StepAndMove stepAndMove:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(stepAndMove.Offset);
                return;
            // `$95B6-$95F2` are the six posture-transition displacements. Each moves the body
            // vertically, counter-scrolls BG2 by the opposite amount, and derives BG2 X from
            // the body's new origin plus its authored horizontal bias.
            case MotherBrainBodyCue.MoveWithScrollBias moveWithScrollBias:
                MoveMotherBrainBodyWithScrollBias(moveWithScrollBias.Y, moveWithScrollBias.ScrollBias);
                return;
            case MotherBrainBodyCue.SpawnHandDust spawnHandDust:
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    new(unchecked((ushort)(state.Body.Position.X.Whole + spawnHandDust.Offset.X)), unchecked((ushort)(state.Body.Position.Y.Whole + spawnHandDust.Offset.Y))),
                    spawnHandDust.Dust);
                return;
            case MotherBrainBodyCue.Command { Kind: MotherBrainBodyCueKind.FireHandBeam }:
            {
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain hand-beam aiming requires the active Samus actor.");
                state.LastSoundEffect = MotherBrainSoundDefinitions.HandBeam;
                SpawnMotherBrainHandBeamCharging(state, target);
                return;
            }
            case MotherBrainBodyCue.Command { Kind: MotherBrainBodyCueKind.AdvanceHandBeamPhase }:
                state.HandBeamPhase = MotherBrainHandBeamPhases.Next(state.HandBeamPhase);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyCueKind)} {cue}.");
        }
    }

    /// <summary>
    /// Ports <c>$A9:B3B6-$B454</c>. Mother Brain owns a bit-selected set of asymmetric
    /// rectangles whose side extents are interpreted relative to Samus, rather than an
    /// ordinary enemy radius. Phase one enables only the brain list; later phases also
    /// admit the body and three independently positioned neck segments.
    /// </summary>
    internal static bool ResolveMotherBrainSamusCollision(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        MotherBrainHitboxParts enabled = state.HitboxesEnabled;
        if ((enabled & MotherBrainHitboxParts.Body) != MotherBrainHitboxParts.None && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Body,
                state.Body.Position.Whole))
        {
            return true;
        }

        RoomEnemySlot head = state.Head;
        if ((enabled & MotherBrainHitboxParts.Brain) != MotherBrainHitboxParts.None && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Brain,
                head.Position.Whole))
        {
            return true;
        }

        if ((enabled & MotherBrainHitboxParts.Neck) != MotherBrainHitboxParts.None)
        {
            // Native tests only middle joints one, two, and three. Joint zero is buried in
            // the body and joint four is already covered by the separate brain rectangles.
            Position16<RoomPixels>[] collisionSegments = state.NeckJoints[MotherBrainNeckJointDefinitions.CollisionJoints];
            foreach (Position16<RoomPixels> segment in collisionSegments)
            {
                if (ResolveMotherBrainSamusCollisionPart(
                        state,
                        samus,
                        MotherBrainContactPart.Neck,
                        segment))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>Checks a single head or body contact region and applies the phase-specific collision response.</summary>
    internal static bool ResolveMotherBrainSamusCollisionPart(
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainContactPart contactPart,
        Position16<RoomPixels> origin)
    {
        Offset16<RoomPixels> fromOrigin = samus.Position.Whole - origin;
        foreach (EnemyHitboxRectangle hitbox in
            MotherBrainContacts.Get(contactPart))
        {
            bool belowOrigin = fromOrigin.Y >= 0;
            int yDistance = Math.Abs(fromOrigin.Y);
            int yExtent = belowOrigin ? hitbox.Bottom : hitbox.Top;
            int yOverlap = samus.Kinematics.Radius.Y + Math.Abs(yExtent) - yDistance;
            if (yOverlap < 0)
                continue;

            bool rightOfOrigin = fromOrigin.X >= 0;
            int xDistance = Math.Abs(fromOrigin.X);
            int xExtent = rightOfOrigin ? hitbox.Right : hitbox.Left;
            int xOverlap = samus.Kinematics.Radius.X + Math.Abs(xExtent) - xDistance;
            if (xOverlap < 0)
                continue;

            // Native clamps shallow contact UP to four pixels. The resulting words are
            // consumed by Samus movement later in the frame, so overwrite rather than add.
            samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(unchecked((short)Math.Max(xOverlap, 4)), 4));
            samus.InvincibilityTimer = WrappingCountdown.Start(96);
            samus.KnockbackTimer = WrappingCountdown.Start(5);
            samus.KnockbackXDirection = KnockbackHorizontalDirection.Right;
            if (unchecked((short)(samus.Position.Y.Whole - 192)) < 0)
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;

            // Only contact to the right of body X+24 calls MotherBrain_HurtSamus. Contact
            // elsewhere still installs the displacement and timers above without damage.
            if (unchecked((short)(state.Body.Position.X.Whole + 24 - samus.Position.X.Whole)) < 0)
                DamageSamusFromMotherBrainBody(state.Body, samus);
            return true;
        }
        return false;
    }

    /// <summary>Applies body contact damage while preserving Samus's invulnerability timing.</summary>
    private static void DamageSamusFromMotherBrainBody(RoomEnemySlot body, SamusState samus)
    {
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? body.Definition.Damage >> 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? body.Definition.Damage >> 1
                : body.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = WrappingCountdown.Start(96);
        samus.KnockbackTimer = WrappingCountdown.Start(5);
        samus.KnockbackXDirection = samus.Position.X.Whole >= body.Position.X.Whole ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left;
    }

    /// <summary>
    /// Ports charging initializer <c>$86:C605</c>. The body record—not the articulated
    /// head—is the source: its extended map puts the hand at (+$40,-$30).
    /// </summary>
    internal void SpawnMotherBrainHandBeamCharging(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemyProjectileSlot? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
            graphics: new SpriteGraphics(EnemyTileBaseDefinitions.MotherBrainObjectCharacters, SharedSpriteColouring.Common));
        beam.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        beam.Position = beam.Position.WithFractionX(0).WithFractionY(0);
        beam.Graphics = SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(MotherBrainColourDefinitions.PhaseTwoAttack));
        beam.Position = beam.Position.WithWhole(new(unchecked((ushort)(state.Body.Position.X.Whole + 64)), unchecked((ushort)(state.Body.Position.Y.Whole - 48))));

        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWhole(beam.Position.Whole);

        ByteAngle angle = EnemyMath.CalculateBearing(samus.Position.Whole - beam.Position.Whole)
            .ToTableAngle().AddTableUnits(ByteAngle.QuarterTurn.RawValue);
        state.HandBeamNextAngle = angle.RawValue;
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            new SignedEightEight(EnemyMath.MultiplySineSample(3072, angle)),
            new SignedEightEight(EnemyMath.MultiplySineSample(3072, angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue))));

        beam.StartAnimation(new MotherBrainHandBeamAnimation());
    }

    /// <summary>
    /// Literal body/BG2 movement helper at <c>$A9:9579</c>. The signed Y operand is added
    /// to the actor and subtracted from the background, keeping the large BG2 body art
    /// rigidly attached to its small OBJ hitbox record. X movement happens before this call
    /// in the cartridge, so the derived scroll must sample the already-updated body X.
    /// </summary>
    private void MoveMotherBrainBody(Offset16<RoomPixels> displacement)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain body bytecode ran without its multipart encounter state.");
        state.Body.Position += displacement;
        PlaceMotherBrainBodyLayer(state, new(
            unchecked((ushort)(34 - state.Body.Position.X.Whole)),
            unchecked((ushort)(state.BodyLayerPlacement.Y - displacement.Y))));
    }

    /// <summary>
    /// Posture-transition variant at <c>$A9:9552</c>. Its X register is a scroll bias, not
    /// actor displacement; only Y changes the physical body coordinate.
    /// </summary>
    private void MoveMotherBrainBodyWithScrollBias(
        int yDisplacement,
        int horizontalScrollBias)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain posture bytecode ran without its multipart encounter state.");
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(state.Body.Position.Y.Whole + yDisplacement)));
        PlaceMotherBrainBodyLayer(state, new(
            unchecked((ushort)(34 + horizontalScrollBias - state.Body.Position.X.Whole)),
            unchecked((ushort)(state.BodyLayerPlacement.Y - yDisplacement))));
    }

    /// <summary>Shared four-frame earthquake produced by each planted walking frame.</summary>
    private void RunMotherBrainFootstep()
    {
        _earthquake.EarthquakeType = 1;
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(4);

        // Form three additionally queues library-three sound $16. Phase two normally never
        // reaches that branch, but retaining it here makes the shared gait opcode complete.
        if (_motherBrainEncounter.MotherBrain?.Form == MotherBrainForm.DrainedByBabyMetroid)
            _motherBrainEncounter.MotherBrain.LastSoundEffectLibrary3 = Library3Sound.MotherBrainFootstep;
    }

    /// <summary>Places her body layer for the current motion; the accepted frame publishes it with her body (#1356).</summary>
    internal static void PlaceMotherBrainBodyLayer(
        MotherBrainEnemyState state,
        Position16<BackgroundPixels> placement)
    {
        state.BodyLayerPlacement = placement;
        state.PlacesBodyLayer = true;
    }

    /// <summary>
    /// Ports the twelve calls to <c>SpawnEprojWithRoomGfx($C17E, A=0..$B)</c> made by
    /// <c>InitAI_MotherBrainBody</c>. Allocation scans from native index $22 down, so request
    /// zero lands in host slot 17 and request $B lands in slot 6.
    /// </summary>
    private void SpawnMotherBrainInitialTurrets()
    {
        for (int parameter = 0; parameter < 12; parameter++)
            SpawnMotherBrainTurret(parameter);
    }

    /// <summary>Ports <c>InitAI_EnemyProjectile_MotherBrainsTurrets</c> at $86:BE4F.</summary>
    internal void SpawnMotherBrainTurret(int parameter)
    {
        if (parameter is < 0 or >= 12)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        RoomEnemyProjectileSlot? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            turret,
            RoomEnemyProjectileKind.MotherBrainRoomTurret,
            MotherBrainTurretDefinitions.Graphics);

        MotherBrainTurretDefinition definition =
            MotherBrainTurrets.ForTurret(parameter);
        MotherBrainTurretDirection direction = definition.InitialDirection;
        turret.Position = turret.Position.WithWhole(definition.Position);
        turret.State = new MotherBrainTurretState(parameter, MotherBrainTurretDefinitions.Ring.PositionOf(direction));
        turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretProgram.TurretPose, direction));

        // The initializer makes two independent calls to the cartridge RNG for the turret's waits and clamps their low
        // bytes upward.
        _projectileAttacks.ResetMotherBrainTurretRotationTimer(turret);
        _projectileAttacks.ResetMotherBrainTurretCooldown(turret);
    }
}
