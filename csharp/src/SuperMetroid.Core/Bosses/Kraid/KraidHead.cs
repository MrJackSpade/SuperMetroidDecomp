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
    /// <summary>Provides the body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidHead(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, KraidEncounterAccess access, KraidBackground background)
    {
        _room = room;
        _projectiles = projectiles;
        _slotTable = slotTable;
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
        state.HurtFrameTimer = state.HurtFrameTimer.Decrement().Remaining;
        if (state.HurtFrameTimer.IsZero)
        {
            state.HurtFrameTimer = WrappingCountdown.Start(2);
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
        state.Colours.ShowBand(band, BandEyes(band));
        PictureShading parts = state.Colours.Parts;
        for (int part = 0; part < state.Parts.Length; part++)
            _slotTable.Slots[part].Shading = parts;
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
        while (band != 0 && WordWrap.Signed(body.Health - state.HealthEighthThreshold(band)) < 0)
            band--;
        return band;
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    internal void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.MainloopThinking;
        _slotTable.Slots[2].Position = _slotTable.Slots[2].Position.WithWrappedWholeY(body.Position.Y.Whole - 20);
        _slotTable.Slots[3].Position = _slotTable.Slots[3].Position.WithWrappedWholeY(body.Position.Y.Whole + 46);
        _slotTable.Slots[4].Position = _slotTable.Slots[4].Position.WithWrappedWholeY(body.Position.Y.Whole + 112);
        state.ThinkingTimer = WrappingCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Advances the open-mouth attack and its head instruction timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        int result = ProcessKraidHeadInstruction(body, state);
        if (result == ushort.MaxValue)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(90);

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the original game arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                    state.Parts[body.SlotIndex].FunctionTimer = WrappingCountdown.Start(64);
                    state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                    state.Unknown2 = 2;
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after the open-mouth setup reloads it.
        if (state.DisplayedHeadFrame is { MouthStage: KraidHeadFrame.FullyOpenMouthStage } &&
            state.Parts[body.SlotIndex].HeadTimer.IsOnMultipleOf(16))
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Counts the displayed head frame down and runs the next head command when it expires. A cleared timer holds the
    /// head (the result is zero); otherwise the result is the remaining time before this update's decrement, or the
    /// executed command's result when the decrement reached zero.
    /// </summary>
    internal int ProcessKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        WrappingCountdown headTimer = state.Parts[body.SlotIndex].HeadTimer;
        if (headTimer.IsZero)
            return 0;
        CountdownStep step = headTimer.Decrement();
        state.Parts[body.SlotIndex].HeadTimer = step.Remaining;
        return step.IsZero ? ExecuteKraidHeadInstruction(body, state) : Ensure.BetweenInclusive(0, ushort.MaxValue, headTimer.RemainingFrames);
    }

    /// <summary>
    /// Runs Kraid's head program to its next pose: cries on the way are announced, a pose installs its duration as the
    /// head timer and uploads its image (the result is one), and a program's end leaves the cursor there (the result is
    /// <see cref="ushort.MaxValue"/>).
    /// </summary>
    internal int ExecuteKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
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
                return ushort.MaxValue;

            case AnimationProgramAdvance<KraidHeadFrame>.Show shown:
                state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(shown.Duration);
                state.DisplayedHeadFrame = shown.Frame;
                // The head draw draws the pose straight over the upper half of his body; the next showing of that half covers it.
                state.ShowHead(shown.Frame);
                return 1;

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
    internal void RunKraidMouthOpenReaction(RoomEnemySlot body, KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(body, state) != ushort.MaxValue)
            return;
        state.Parts[body.SlotIndex].Function = KraidAiFunction.MainloopThinking;
        state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(90);
        if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                state.Parts[body.SlotIndex].FunctionTimer = WrappingCountdown.Start(64);
                state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                state.Unknown2 = 2;
                return;
            }
        }
        state.ClearMouthReactions();
    }

    /// <summary>Seeds eye-glow palette state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.GlowEye;
        state.HeadProgram = KraidHeadAnimations.EyeGlowAfterEntryFrame;
        state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(KraidHeadAnimations.EyeGlowEntryDuration);
        GlowKraidEye(body, state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        // The glow's peak, read back from the eyes' colours, ends the glow (#1512).
        if (state.Colours.EyeGlow.Glow())
            state.Parts[body.SlotIndex].Function = KraidAiFunction.UnglowEye;
    }

    /// <summary>Restores the eye palette after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        // The eyes' colours, read back, reaching the band's ends the fade and starts the roar (#1512).
        bool unglowed = state.Colours.EyeGlow.Unglow(BandEyes(HealthBand(body, state)));
        if (unglowed)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(KraidHeadAnimations.RoarEntryDuration);
        }
    }

    /// <summary>Emits a mouth rock when a projectile slot is available.</summary>
    internal bool SpawnKraidSpitRock(RoomEnemySlot body)
    {
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            graphics: SpriteGraphics.CharacterZeroIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + 16), CoordinateAxis.Wrap(body.Position.Y.Whole - 96)));
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.FromRandom(random));
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.FromSignedRaw(-1024));
        rock.Graphics = SpriteGraphics.CharacterZeroIn(KraidColours.RocksShading);
        return true;
    }

    /// <summary>Derives a nonzero multiple-of-64 thinking delay from the room RNG.</summary>
    internal int ReadKraidThinkingTimer()
    {
        int random = _room.Random.Read();
        int value = random & 7;
        if (value == 0)
            value = 2;
        return WordWrap.Unsigned(value << 6);
    }
}
