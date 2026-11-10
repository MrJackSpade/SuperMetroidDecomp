using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
[RoomLifetime]
internal sealed class KraidHead
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidHead(EnemyRoomBinding room, EnemyProjectileEngine projectiles, KraidParts parts, KraidEncounterAccess access, KraidBackground background)
    {
        _room = room;
        _projectiles = projectiles;
        _parts = parts;
        _access = access;
        _background = background;
    }

    /// <summary>Advances the active body palette program and health-dependent colors.</summary>
    internal void RunKraidPaletteHandling(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health == 0)
        {
            state.HurtFrame = 0;
            UpdateKraidHealthPalettes(body, state);
            return;
        }
        if (state.HurtFrame == 0)
            return;
        if (--state.HurtFrameTimer == 0)
        {
            state.HurtFrameTimer = 2;
            state.StepHurtFrameDown();
            UpdateKraidHealthPalettes(body, state);
        }
    }

    /// <summary>
    /// Shows Kraid's colours for his current health band, or the hurt flash on an odd hurt frame (#1512): his body and
    /// every sprite part take the band's shade, and his eyes the band's colours at once.
    /// </summary>
    internal void UpdateKraidHealthPalettes(RoomEnemySlot body, KraidEnemyState state)
    {
        int band = (state.HurtFrame & 1) != 0 ? HurtBand : HealthBand(body, state);
        state.Colours.ShowBand(band, BandEyes(band), _background.EyeColours.Shaded);
        PictureShading parts = state.Colours.Parts;
        foreach (RoomEnemySlot part in _parts.All)
            part.Shading = parts;
    }

    /// <summary>The eye colours of health band <paramref name="band"/>, or of the hurt flash for minus one.</summary>
    internal ReadOnlySpan<RgbColor> BandEyes(int band)
    {
        KraidEyeColours eyes = _background.EyeColours;
        return band == HurtBand ? eyes.Hurt : eyes.HealthBand(band);
    }

    /// <summary>The health band his colours start in, before any update: the band of his health as he initializes.</summary>
    internal static int InitialBand(RoomEnemySlot body, KraidEnemyState state) => HealthBand(body, state);

    /// <summary>The band a hurt flash shows, which shows the hurt flash's colours in place of a health band's.</summary>
    private const int HurtBand = -1;

    /// <summary>
    /// The health band Kraid's colours show: the highest of the eight bands whose threshold his health has reached,
    /// or the lowest band.
    /// </summary>
    private static int HealthBand(RoomEnemySlot body, KraidEnemyState state)
    {
        int band = KraidHealthThresholdDefinitions.EighthCount - 1;
        while (band != 0 && body.Health < state.HealthEighthThreshold(band))
            band--;
        return band;
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    internal void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.BodyState.Function = KraidAiFunction.MainloopThinking;
        _parts.TopLint.Position = _parts.TopLint.Position.WithWrappedWholeY(body.Position.Y.Whole - 20);
        _parts.MiddleLint.Position = _parts.MiddleLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 46);
        _parts.BottomLint.Position = _parts.BottomLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 112);
        state.ThinkingTimer = ReadKraidThinkingTimer();
    }

    /// <summary>Advances the open-mouth attack and its head instruction timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(state) == KraidHeadProgramResult.ProgramEnded)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.BodyState.HeadTimer = 90;

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the original game arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.BodyState.Function = KraidAiFunction.HandleFunctionTimer;
                    state.BodyState.FunctionTimer = 64;
                    state.BodyState.NextFunction = KraidAiFunction.InitializeEyeGlow;
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after the open-mouth setup reloads it.
        if (state.DisplayedHeadFrame is { MouthStage: KraidHeadFrame.FullyOpenMouthStage } &&
            state.BodyState.HeadTimer % 16 == 0)
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Counts the displayed head frame down and runs the head program on when it expires. A cleared timer holds the
    /// head, as does a timer still counting; a timer reaching zero gives the program's result.
    /// </summary>
    internal KraidHeadProgramResult ProcessKraidHeadInstruction(KraidEnemyState state)
    {
        KraidBodyState part = state.BodyState;
        if (part.HeadTimer == 0)
            return KraidHeadProgramResult.PoseHeld;
        return --part.HeadTimer == 0 ? ExecuteKraidHeadInstruction(state) : KraidHeadProgramResult.PoseHeld;
    }

    /// <summary>
    /// Runs Kraid's head program to its next pose: cries on the way are announced, and a pose installs its duration as
    /// the head timer and shows its image; a program's end leaves the cursor there.
    /// </summary>
    internal KraidHeadProgramResult ExecuteKraidHeadInstruction(KraidEnemyState state)
    {
        AnimationProgramCursor<KraidHeadProgram> cursor = state.HeadProgram ?? throw new InvalidDataException(
            "Kraid's head program has not been started.");
        WrappingCountdown unusedLoopCounter = default;
        AnimationProgramAdvance<KraidHeadFrame> advance = KraidHeadAnimations.Programs.AdvanceTo(
            ref cursor, ref unusedLoopCounter, (cue, _) =>
            {
                PublishKraidHeadCue(cue, state);
                return null;
            });
        state.HeadProgram = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<KraidHeadFrame>.Hold:
                return KraidHeadProgramResult.ProgramEnded;

            case AnimationProgramAdvance<KraidHeadFrame>.Show shown:
                state.BodyState.HeadTimer = shown.Duration;
                // His body layer shows the pose over his body from here (#1515).
                state.DisplayedHeadFrame = shown.Frame;
                return KraidHeadProgramResult.PoseShown;

            case AnimationProgramAdvance<KraidHeadFrame>.Wait or AnimationProgramAdvance<KraidHeadFrame>.Stop:
                throw new InvalidOperationException($"Kraid's head programs never {advance}.");

            default:
                throw new InvalidOperationException($"Undefined animation program advance {advance}.");
        }
    }

    /// <summary>Plays a head program's cry; a roar is also counted.</summary>
    private void PublishKraidHeadCue(KraidHeadCue cue, KraidEnemyState state)
    {
        switch (cue)
        {
            case KraidHeadCue.Roar:
                _access.RequestKraidSound(KraidSoundDefinitions.HeadRoar);
                state.RoarRequestCount++;
                return;

            case KraidHeadCue.DyingCry:
                _access.RequestKraidSound(KraidSoundDefinitions.HeadDyingCry);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidHeadCue)} {cue}.");
        }
    }

    /// <summary>Updates the mouth-open reaction after a damaging shot.</summary>
    internal void RunKraidMouthOpenReaction(KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(state) != KraidHeadProgramResult.ProgramEnded)
            return;
        state.BodyState.Function = KraidAiFunction.MainloopThinking;
        state.BodyState.HeadTimer = 90;
        if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.BodyState.Function = KraidAiFunction.HandleFunctionTimer;
                state.BodyState.FunctionTimer = 64;
                state.BodyState.NextFunction = KraidAiFunction.InitializeEyeGlow;
                return;
            }
        }
        state.ClearMouthReactions();
    }

    /// <summary>Seeds eye-glow palette state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(KraidEnemyState state)
    {
        state.BodyState.Function = KraidAiFunction.GlowEye;
        state.HeadProgram = KraidHeadAnimations.EyeGlowAfterEntryFrame;
        state.BodyState.HeadTimer = KraidHeadAnimations.EyeGlowEntryDuration;
        GlowKraidEye(state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(state);
        // The glow's peak, read back from the eyes' colours, ends the glow (#1512).
        if (state.Colours.GlowEyes())
            state.BodyState.Function = KraidAiFunction.UnglowEye;
    }

    /// <summary>Restores the eye palette after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        // The eyes' colours, read back, reaching the band's ends the fade and starts the roar (#1512).
        bool unglowed = state.Colours.UnglowEyes(BandEyes(HealthBand(body, state)));
        if (unglowed)
        {
            state.BodyState.Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.BodyState.HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Emits a mouth rock when a projectile slot is available.</summary>
    internal bool SpawnKraidSpitRock(RoomEnemySlot body)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + 16), CoordinateAxis.Wrap(body.Position.Y.Whole - 96)));
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.FromRandom(random));
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.FromSignedRaw(-1024));
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
        return true;
    }

    /// <summary>Derives a nonzero multiple-of-64 thinking delay from the room RNG.</summary>
    internal int ReadKraidThinkingTimer()
    {
        int random = _room.Random.Read();
        int value = KraidRandomDefinitions.ThinkingDelaySteps.Of(random);
        if (value == 0)
            value = 2;
        return value << 6;
    }
}
