using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's head and brain: their setup, neck, programs, cues and drawing.</summary>
[RoomLifetime]
internal sealed class MotherBrainHead
{
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Applies the encounter's damage and death/drop rules.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;
    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
    private readonly MotherBrainEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainHead(MotherBrainHeadDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _roomActors = dependencies.RoomActors;
        _projectileAttacks = dependencies.ProjectileAttacks;
        _access = dependencies.Access;
    }

    /// <summary>Ports <c>InitAI_MotherBrainHead</c> at <c>$A9:8705</c>.</summary>
    internal void InitializeMotherBrainHead(RoomEnemySlot head)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain's head appeared before its body record.");
        if (head.SlotIndex != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain's head requires native slot one, not slot {head.SlotIndex}.");
        }

        // The cartridge seeds the complete future corpse effect during room load, not when
        // Mother Brain dies. Keeping this eager call preserves the exact WRAM state visible
        // throughout every earlier phase and prevents the death sequence from inventing it.
        state.CorpseRotting.Initialize(_room.Staging, _room.MotherBrainCorpseArtwork);

        head.Health = 3000;
        // `$A9:8714` gives the head enemy the dummy program for life (the encounter state's
        // head-slot cursor starts there): its $A320 frame is the head's collision hitbox.
        // The visible brain runs a separate program (`$A9:8734`).
        head.InstructionTimer = NativeCountdown.Start(1);
        head.VramTilesIndex = 0;
        head.Properties = head.Properties.With(
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.Invisible);
        head.PaletteIndex = EnemyPaletteBits.Palette1;
        state.Head = head;
        state.NeckPalette = SnesObjPalettes.Palette(1);
        state.BrainPalette = SnesObjPalettes.Palette(1);
        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.Initial);
    }

    /// <summary>Ports the head main/hurt entry and its first-phase draw-hook selector.</summary>
    internal void RunMotherBrainHeadMain(RoomEnemySlot head, SamusState? samus)
    {
        MotherBrainEnemyState state = _access.RequireCompleteMotherBrainState(head);

        // `$A9:878B` first restores the global enemy-graphics-drawn hook to RTL. Only an
        // intentionally invisible head dispatches the shared brain function and replaces it.
        state.DrawBrain = false;
        state.DrawNeck = false;
        if (!head.Properties.HasAny(EnemyProperties.Invisible))
            return;

        switch (state.BrainFunction)
        {
            case MotherBrainBrainFunction.SetupBrainToBeDrawn:
                state.DrawBrain = true;
                return;
            case MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn:
                StepMotherBrainNeck(
                    state,
                    samus ?? throw new InvalidOperationException(
                        "Mother Brain's articulated neck requires the live Samus actor."));
                Position16<RoomPixels> brainAnchor = state.NeckJoints[MotherBrainNeckJointDefinitions.BrainAnchor];
                head.Position = head.Position.WithWhole(new(brainAnchor.X, unchecked((ushort)(brainAnchor.Y - 21))));
                state.DrawBrain = true;
                state.DrawNeck = true;
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain brain function $A9:{(ushort)state.BrainFunction:X4} is not translated.");
        }
    }

    /// <summary>
    /// Executes the hook installed by <c>$A9:87D0</c> after the ordinary enemy queues have
    /// been written. The source is an ordinary bank-$A9 spritemap even though the population
    /// record carries extended-spritemap bit <c>$0004</c>; routing it through the generic
    /// extended parser is precisely the corruption the cartridge hook avoids.
    /// </summary>
    internal void DrawMotherBrainHook(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState? state = _motherBrainEncounter.MotherBrain;
        RoomEnemySlot? head = state?.Head;
        if (state?.DrawBrain != true || head is null)
            return;
        // `$A9:9357-$936B` samples (never advances) the RNG before reading the brain frame,
        // so the small breath can spawn even on a draw that then shows no brain.
        if (state.SmallPurpleBreathGenerationEnabled && !state.SmallPurpleBreathActive &&
            unchecked((short)_room.Random.Read()) >= 0)
            SpawnMotherBrainPurpleBreathSmall(state);
        // `$A9:936F` advances the brain list here, after every enemy's AI, and returns out
        // of the whole draw when the list is not live.
        if (AdvanceMotherBrainBrain(state, _deathsAndDrops.SamusForEnemyDrops) is not MotherBrainHeadFrame brainFrame)
            return;

        Offset16<RoomPixels> brainShake = GetMotherBrainBrainShake(state, head);
        // The caller must cull before the nine-bit OAM coordinate loses world-space bits.
        // Otherwise a brain half a room to the left reappears in the approach hallway.
        if (unchecked((short)(head.Position.X.Whole + brainShake.X +
                MotherBrainDrawData.BrainLeftCullMargin - camera.X)) >= 0)
        DrawMotherBrainWorldSpritemap(
            sprites,
            objects,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain),
            new MotherBrainWorldFrame(
                MotherBrainVisualDefinitions.HeadFrames.Of(brainFrame),
                head.Position.Whole + brainShake,
                // The custom draw hook bypasses ordinary enemy palette selection. Retail
                // still selects the shared white hurt palette on odd flash-timer frames.
                (head.FlashTimer.RawValue & 1) != 0 ? (ushort)0 : state.BrainPalette.PaletteBits,
                checked((ushort)head.VramTilesIndex)),
            camera);

        if (!state.DrawNeck || state.Body.Properties.HasAny(EnemyProperties.Invisible))
            return;

        // `$A9:930C-$9354` emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same drawing precedence.
        int shakeIndex = (head.FlashTimer.RawValue & 6) >> 1;
        Offset16<RoomPixels> neckShake = MotherBrainBehavior.MotherBrainShakeOffsets[shakeIndex];
        Position16<RoomPixels>[] segments = state.NeckJoints;
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            Position16<RoomPixels> segment = segments[index];
            DrawMotherBrainWorldSpritemap(
                sprites,
                objects,
                new SpriteSource(SpriteSourceKind.MotherBrainNeck, segments.Length - 1 - index),
                new MotherBrainWorldFrame(MotherBrainVisualDefinitions.NeckJointFrame, segment + neckShake,
                    state.NeckPalette.PaletteBits, checked((ushort)head.VramTilesIndex)),
                camera);
        }
    }

    /// <summary>Publishes a selected world-space Mother Brain frame with palette and camera offsets.</summary>
    internal void DrawMotherBrainWorldSpritemap(
        SpriteFrameBuffer sprites,
        SpriteArtwork objects,
        SpriteSource source,
        MotherBrainWorldFrame frame,
        Position16<RoomPixels> camera)
    {
        // Mother Brain's private `$A9:93EE` writer rejects an entire map whose center begins
        // above the screen. The generic writer intentionally wraps, so preserve that unique
        // vertical gate here before sharing its byte-exact map decoder.
        Position16<ScreenPixels> screen = frame.World.ToScreen(camera);
        if (unchecked((short)screen.Y) < 0)
            return;

        _presentation.PublishEnemySpritemap(sprites, objects, source, new EnemySpritemapDraw { Frame = frame.Frame, Origin = screen.ToOam(), PaletteBits = frame.PaletteIndex, BaseTileIndex = frame.VramTilesIndex });
    }

    /// <summary>Derives the head's current shake offset from the encounter's animation state.</summary>
    private static Offset16<RoomPixels> GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemySlot head)
    {
        ushort shake;
        if (!state.BrainMainShakeTimer.IsZero)
        {
            state.BrainMainShakeTimer = state.BrainMainShakeTimer.Decrement().Remaining;
            shake = state.BrainMainShakeTimer.RawValue;
        }
        else
        {
            shake = !head.FlashTimer.IsZero ? head.FlashTimer.RawValue : head.ShakeTimer.RawValue;
        }

        int index = (shake & 6) >> 1;
        return MotherBrainBehavior.MotherBrainShakeOffsets[index];
    }

    /// <summary>Moves neck segments toward the head and updates their attachment to Samus's position.</summary>
    private static void StepMotherBrainNeck(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.NeckMovementEnabled)
        {
            SnesAngle lowerAngle = state.LowerNeckAngle;
            SnesAngle upperAngle = state.UpperNeckAngle;
            MotherBrainNeckMovement lowerIndex = state.LowerNeckMovementIndex;
            MotherBrainNeckMovement upperIndex = state.UpperNeckMovementIndex;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerIndex,
                ref upperIndex,
                state.NeckAngleDelta,
                state.Head.Position.Y.Whole,
                samus.Position.Y.Whole);
            state.LowerNeckAngle = lowerAngle;
            state.UpperNeckAngle = upperAngle;
            state.LowerNeckMovementIndex = lowerIndex;
            state.UpperNeckMovementIndex = upperIndex;
        }

        MotherBrainNeckKinematics.CalculateGeometry(state.Body.Position.Whole, state.LowerNeckAngle, state.UpperNeckAngle, state.NeckJointDistances)
            .CopyTo(state.NeckJoints);
    }

    /// <summary>
    /// Ports <c>GetMotherBrainHeadSpritemapPointerInY</c> ($A9:92AF): returns the brain frame
    /// to draw, or null when the brain is not live. With time frozen it only reads the current
    /// frame. Otherwise a frame holds while its duration minus the timer is not negative
    /// (CMP/BPL), the timer counting up; past it, cues run until the next frame, which
    /// resets the timer to one.
    /// </summary>
    internal MotherBrainHeadFrame? AdvanceMotherBrainBrain(MotherBrainEnemyState state, SamusState? samus)
    {
        if (state.BrainCursor is not { } cursor)
            return null;
        var programs = MotherBrainAnimations.Head;
        AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, MotherBrainHeadCue> step = programs.Steps(cursor.Program)[cursor.Position];
        if (_frameContext.TimeIsFrozen)
        {
            return step is AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, MotherBrainHeadCue>.ShowStep frozen ? frozen.Frame : throw new InvalidOperationException(
                $"Mother Brain's frozen brain rests on {step} step {cursor.Position} of {cursor.Program}, not a frame.");
        }

        if (step is AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, MotherBrainHeadCue>.ShowStep show)
        {
            if (state.BrainFrameTimer <= show.Duration)
            {
                state.BrainFrameTimer++;
                return show.Frame;
            }
            cursor = cursor with { Position = cursor.Position + 1 };
        }

        NativeCountdown unusedCounter = default;
        AnimationProgramAdvance<MotherBrainHeadFrame> advance = programs.Advance(ref cursor, ref unusedCounter,
            (cue, continuation) => ApplyMotherBrainHeadCue(state, samus, cue));
        if (advance is not AnimationProgramAdvance<MotherBrainHeadFrame>.Show shown)
            throw new InvalidOperationException($"Mother Brain's brain programs have no {advance} step.");
        // The native pointer rests on the frame it shows, so the cursor steps back onto it.
        state.BrainCursor = cursor with { Position = cursor.Position - 1 };
        state.BrainFrameTimer = 1;
        return shown.Frame;
    }

    /// <summary>Carries out one cue of a brain program.</summary>
    /// <returns>The program a random hold or drool cue redirects to; otherwise null.</returns>
    /// <remarks>
    /// The brain program advances in the <c>$A9:87D0</c> draw hook, after EnemyMain has collected the frame's legacy
    /// sound slots, so its sounds go straight to the enemy audio queue at their native limits (#1391).
    /// </remarks>
    private MotherBrainHeadProgram? ApplyMotherBrainHeadCue(MotherBrainEnemyState state, SamusState? samus, MotherBrainHeadCue cue)
    {
        switch (cue)
        {
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.ShakeBrain }:
                state.BrainMainShakeTimer = NativeCountdown.Start(50);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnDrool }:
                SpawnMotherBrainDrool(state);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnBigPurpleBreath }:
                SpawnMotherBrainPurpleBreathBig(state);
                return null;
            // `QueueSoundX_Lib2_Max6` / `QueueSoundX_Lib3_Max6`.
            case MotherBrainHeadCue.QueueSound queueSound:
                _audio.QueueEnemySound(queueSound.Sound, maximumQueued: 6);
                return null;
            case MotherBrainHeadCue.QueueLibrary3Sound queueLibrary3Sound:
                _audio.QueueEnemySound(queueLibrary3Sound.Sound, maximumQueued: 6);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.DisableNeckMovement }:
                state.NeckMovementEnabled = false;
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.EnableNeckMovement }:
                state.NeckMovementEnabled = true;
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.AimOnionRingsAtSamus }:
            {
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain onion-ring aiming requires the active Samus actor.");
                AimMotherBrainOnionRingsAt(state, target.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid }:
            {
                BabyMetroidCutsceneState baby = state.BabyMetroid ?? throw new InvalidOperationException(
                    "Mother Brain's Baby-targeting cue ran without the Baby actor.");
                AimMotherBrainOnionRingsAt(state, baby.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnOnionRing }:
                SpawnMotherBrainOnionRing(state, unchecked(new SnesByteAngle((byte)state.OnionRingsTargetAngle)));
                return null;
            case MotherBrainHeadCue.SpawnBomb bomb:
                SpawnMotherBrainBomb(state, (ushort)bomb.Afterburns);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnLaser }:
                SpawnMotherBrainMouthLaser(state);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.CountBabyMetroidAttack }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).IncrementLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.ResetBabyMetroidAttacks }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).ResetLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.QueueBabyMetroidAttackSound }:
                // `$A9:9DF7` skips the attack cry once the attack counter reaches eleven.
                if (MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).Revival.BabyMetroidAttackCounter != 11)
                    _audio.QueueEnemySound(MotherBrainSoundDefinitions.AttackCry, maximumQueued: 6);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.PrepareRainbowBeamCharge }:
                state.SmallPurpleBreathGenerationEnabled = false;
                _audio.QueueEnemySound(MotherBrainSoundDefinitions.RainbowBeamCharge, maximumQueued: 6);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle }:
                SpawnMotherBrainRainbowChargingProjectile();
                return null;
            // `$A9:9CAD` holds the phase-two neutral pose again unless the random word reaches $F000 (15 in 16).
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase2 }:
                return _room.Random.Read() < 61440 ? MotherBrainHeadProgram.NeutralPhase2Hold : null;
            // Retail contains an unconditional BRA where the adjacent commentary might suggest a
            // carry branch: low-twelve random values below $EC0 hold the phase-three pose again.
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase3 }:
                return (_room.Random.Read() & 4095) < 3776 ? MotherBrainHeadProgram.NeutralPhase3Hold : null;
            // `$A9:9C65-$9C76` returns its destination in X: only low-twelve random values
            // $FE0-$FFF burst into more drool; every other value keeps the one-frame loop.
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeDroolAgain }:
                return (_room.Random.Read() & 4095) >= 4064 ? MotherBrainHeadProgram.DyingDroolBurst : null;
            default:
                throw new InvalidOperationException($"Undefined Mother Brain head cue {cue}.");
        }
    }

    /// <summary>Fires the mouth laser at <c>$A9:9F46</c>, freezing the neck.</summary>
    private void SpawnMotherBrainMouthLaser(MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head;
        state.NeckMovementEnabled = false;

        // `$A9:9F4E-$9F66` writes an explicit mouth coordinate into the common
        // bank-$86 spawn scratch words, selects direction one (right), and invokes
        // definition `$A17B`. The source header remains the head record, so shared
        // initializer `$86:A009` obtains Mother Brain's authored contact damage and
        // her parameter-one speed flag without any encounter-specific substitute.
        RoomEnemyProjectileSlot? laser = _roomActors.SpawnPirateLaser(
            head,
            head.Position.Whole + new Offset16<RoomPixels>(16, 4),
            direction: HorizontalDirection.Right);
        if (laser is not null)
            _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.PirateMotherBrainLaserSound), maximumQueued: 6);
    }

    /// <summary>
    /// Shared `$A9:9E37/$9E5B` tail. Both target selectors subtract the same mouth offsets,
    /// call the cartridge angle routine, rotate by `$80`, and clamp the launch arc to
    /// `$10..$48`; only the source coordinate differs.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        Position16<RoomPixels> target)
    {
        RoomEnemySlot head = state.Head;
        short deltaX = unchecked((short)(target.X - head.Position.X.Whole - 10));
        short deltaY = unchecked((short)(target.Y - head.Position.Y.Whole - 16));
        byte angle = EnemyMath.CalculateCartridgeAngle(new(deltaX, deltaY)).ToTableAngle().AddTableUnits(SnesByteAngle.QuarterTurn.RawValue).RawValue;

        // `$9E86-$9E98` performs two signed-flag comparisons in 8-bit mode. The
        // wraparound half maps $C0..FF and $00..0F to $10; the opposite half maps
        // $48..BF to $48. Only $10..47 survives unchanged.
        state.OnionRingsTargetAngle = angle switch
        {
            >= 16 and < 72 => angle,
            >= 72 and < 192 => 72,
            _ => 16,
        };
    }

    /// <summary>Ports initializer <c>$86:C482-C4C7</c>.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        ushort afterburnCount)
    {
        RoomEnemyProjectileSlot? bomb = _projectiles.AllocateEnemyProjectile();
        if (bomb is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            bomb,
            RoomEnemyProjectileKind.MotherBrainBomb,
            graphicsIndex: 1024);
        RoomEnemySlot head = state.Head;
        bomb.Position = bomb.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 12)), unchecked((ushort)(head.Position.Y.Whole + 16))));

        // The initializer's 8-bit store writes parameter LOW into the low byte of X
        // subposition. Every 8.8 motion delta has a zero low byte, so that value naturally
        // survives fractional motion and becomes the terminal Ridley-afterburn count.
        bomb.Position = bomb.Position.WithFractionX(unchecked((byte)afterburnCount)).WithFractionY(0);
        bomb.Velocity = new(new SnesSignedEightEight(224), new SnesSignedEightEight(256));
        bomb.Variable0 = 112; // Horizontal speed restored after each floor bounce.
        bomb.Variable1 = 0;      // Byte offset into the ten-word acceleration table.
        bomb.DirectionParameter = afterburnCount;
        state.RecordBombSpawned();
    }

    /// <summary>
    /// Ports head opcode <c>$A9:9E29</c> and initializer <c>$86:C2F3</c>. The shared
    /// allocator deliberately runs after the twelve room turrets and all lingering drool,
    /// so pool saturation drops the ring exactly as it does on hardware.
    /// </summary>
    internal void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        SnesByteAngle angle)
    {
        RoomEnemyProjectileSlot? ring = _projectiles.AllocateEnemyProjectile();
        if (ring is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            ring,
            RoomEnemyProjectileKind.MotherBrainOnionRing,
            graphicsIndex: 1024);
        ring.Variable0 = 8;
        ring.Variable1 = 0;
        ring.DirectionParameter = angle.RawValue;
        ring.Velocity = ring.Velocity.WithX(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(1104, angle)));
        ring.Velocity = ring.Velocity.WithY(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
            1104,
            angle.AddTableUnits(SnesByteAngle.QuarterTurn.RawValue))));

        // `$C2F3` falls through into `$C320`, making the mouth position observable before
        // the projectile scheduler performs the first of eight delayed attachment frames.
        MotherBrainProjectiles.PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Ports head opcode <c>$A9:9B3C</c> and initializer <c>$86:C843</c>.</summary>
    internal void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        int parameter = state.DroolProjectileParameter + 1;
        if (parameter >= 6)
            parameter = 0;
        state.DroolProjectileParameter = parameter;

        RoomEnemyProjectileSlot? drool = _projectiles.AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckAngleDelta < 128
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(drool, kind, graphicsIndex: 0);
        drool.Variable0 = (ushort)parameter;

        // SpawnEnemyProjectile runs initializer C843 immediately. It falls through into
        // the attached pre-instruction, so debugger-visible coordinates are valid before
        // the later global projectile pass repeats the same attachment calculation.
        _projectileAttacks.RunMotherBrainAttachedDroolPreInstruction(drool);
    }

    /// <summary>Ports head opcode <c>$A9:9B6D</c> and initializer <c>$86:CA6A</c>.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head;
        breath.Position = breath.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 6)), unchecked((ushort)(head.Position.Y.Whole + 16))));
    }

    /// <summary>Ports the small-breath spawn at <c>$A9:9368</c> and initializer <c>$86:CA83</c>.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head;
        breath.Position = breath.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 6)), unchecked((ushort)(head.Position.Y.Whole + 16))));
        state.SmallPurpleBreathActive = true;
    }

    /// <summary>Ports `$86:C80A-$C828`, including the initializer's same-call pin.</summary>
    internal void SpawnMotherBrainRainbowChargingProjectile()
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
            graphicsIndex: 0);
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.GraphicsIndex = 0;
        _projectileAttacks.RunMotherBrainRainbowChargingPreInstruction(projectile);
    }
}
