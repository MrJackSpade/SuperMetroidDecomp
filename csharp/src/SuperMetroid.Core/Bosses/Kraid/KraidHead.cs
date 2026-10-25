using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
[RoomLifetime]
internal sealed class KraidHead
{
    /// <summary>Room scratch memory, services, and tilemap state used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions, top and bottom tilemap transfers and the ceiling rocks.</summary>
    private readonly KraidBackground _background;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidHead(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyPresentation presentation, KraidEncounterAccess access, KraidBackground background)
    {
        _room = room;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _presentation = presentation;
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

    /// <summary>Updates Kraid body and background colors from the current health band.</summary>
    internal void UpdateKraidHealthPalettes(RoomEnemySlot body, KraidEnemyState state)
    {
        int thresholdWordOffset = 14;
        if ((state.HurtFrame & 1) != 0)
        {
            thresholdWordOffset = -2;
        }
        else
        {
            while (thresholdWordOffset != 0 &&
                unchecked((short)(
                    body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
            {
                thresholdWordOffset -= 2;
            }
        }
        int sourceColor = 8 * (thresholdWordOffset + 2);
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossColours().KraidBody.Write(
                color,
                _background.ReadKraidColor(KraidPaletteSource.Health, sourceColor + color));
            _presentation.BossColours().KraidSecondary.Write(
                color,
                _background.ReadKraidColor(KraidPaletteSource.Secondary, sourceColor + color));
        }
    }

    /// <summary>
    /// Draws the head pose Kraid's private eight-byte instruction record at <c>$A7:AF3D</c> selected over the upper
    /// page. <c>$A7:AF5D</c> sends it straight to the display; mirroring it into the working map would change later
    /// growth and sink uploads, which is not native behavior.
    /// </summary>
    internal static void TransferKraidHeadTilemap(KraidEnemyState state, KraidHeadFrame frame)
    {
        state.DisplayedBody = state.DisplayedBody with { Head = frame };
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    internal void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.MainloopThinking;
        _slotTable.Slots[2].Position = _slotTable.Slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable.Slots[3].Position = _slotTable.Slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable.Slots[4].Position = _slotTable.Slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
        state.ThinkingTimer = WrappingCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Advances the open-mouth attack and its head instruction timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        int result = ProcessKraidHeadInstruction(body, state);
        if (result == ushort.MaxValue)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.Parts[body.SlotIndex].HeadTimer = 90;

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the cartridge arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                    state.Parts[body.SlotIndex].FunctionTimer = 64;
                    state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                    state.Unknown2 = 2;
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after `$AF3D` reloads it.
        if (state.DisplayedHeadFrame is { MouthStage: KraidHeadFrame.FullyOpenMouthStage } &&
            state.Parts[body.SlotIndex].HeadTimer % 16 == 0)
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            _access.LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Counts the displayed head frame down and runs the next head command when it expires. A cleared timer holds the
    /// head (the result is zero); otherwise the result is the remaining time before this update's decrement, or the
    /// executed command's result when the decrement reached zero.
    /// </summary>
    internal ushort ProcessKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        WrappingCountdown headTimer = WrappingCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].HeadTimer));
        if (headTimer.IsZero)
            return 0;
        CountdownStep step = headTimer.Decrement();
        state.Parts[body.SlotIndex].HeadTimer = step.Remaining.RawValue;
        return step.IsZero ? ExecuteKraidHeadInstruction(body, state) : headTimer.RawValue;
    }

    /// <summary>
    /// Runs Kraid's head program to its next pose: cries on the way are announced, a pose installs its duration as the
    /// head timer and uploads its image (the result is one), and a program's end leaves the cursor there (the result is
    /// <see cref="ushort.MaxValue"/>).
    /// </summary>
    internal ushort ExecuteKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
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
                state.Parts[body.SlotIndex].HeadTimer = checked((ushort)shown.Duration);
                state.DisplayedHeadFrame = shown.Frame;
                TransferKraidHeadTilemap(state, shown.Frame);
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
                _access.LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.HeadRoar);
                state.RoarRequestCount++;
                return;

            case KraidHeadCue.DyingCry:
                _access.LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.HeadDyingCry);
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
        state.Parts[body.SlotIndex].HeadTimer = 90;
        if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                state.Parts[body.SlotIndex].FunctionTimer = 64;
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
        state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.EyeGlowEntryDuration;
        GlowKraidEye(body, state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        int completedChannels = 0;
        ColourRun eyeColors = _presentation.BossColours().KraidEye;
        for (int colorIndex = 0; colorIndex < eyeColors.Count; colorIndex++)
        {
            RgbColor color = eyeColors[colorIndex];
            int red = color.Red;
            int green = color.Green;
            if (red >= 30)
            {
                red = 31;
                completedChannels++;
            }
            else
            {
                red++;
            }
            if (green >= 30)
            {
                green = 31;
                completedChannels++;
            }
            else
            {
                green++;
            }
            eyeColors.Write(colorIndex, new RgbColor(red, green, color.Blue));
        }
        if (completedChannels >= 6)
            state.Parts[body.SlotIndex].Function = KraidAiFunction.UnglowEye;
    }

    /// <summary>Restores the eye palette after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        int thresholdWordOffset = 14;
        while (thresholdWordOffset != 0 &&
            unchecked((short)(
                body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
        {
            thresholdWordOffset -= 2;
        }
        int sourceColor = 8 * (thresholdWordOffset + 2) + 1;
        int changedChannels = 0;
        ColourRun eyeColors = _presentation.BossColours().KraidEye;
        for (int eye = 0; eye < eyeColors.Count; eye++)
        {
            RgbColor current = eyeColors[eye];
            RgbColor target = _background.ReadKraidColor(KraidPaletteSource.Health, sourceColor + eye);
            int red = current.Red;
            int green = current.Green;
            // Native decrements toward any differing target and masks each field, so a
            // channel below its target wraps within its five bits without borrowing.
            if (red != target.Red)
            {
                red = (red - 1) & RgbColor.MaxChannel;
                changedChannels++;
            }
            if (green != target.Green)
            {
                green = (green - 1) & RgbColor.MaxChannel;
                changedChannels++;
            }
            eyeColors.Write(eye, new RgbColor(red, green, current.Blue));
        }
        if (changedChannels == 0)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
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
            graphicsIndex: EnemyPaletteBits.Palette3);
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + 16)), unchecked((ushort)(body.Position.Y.Whole - 96))));
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.FromRandom(random));
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.FromSignedRaw(-1024));
        rock.GraphicsIndex = EnemyPaletteBits.Palette3;
        return true;
    }

    /// <summary>Derives a nonzero multiple-of-64 thinking delay from the room RNG.</summary>
    internal ushort ReadKraidThinkingTimer()
    {
        int random = _room.Random.Read();
        int value = random & 7;
        if (value == 0)
            value = 2;
        return unchecked((ushort)(value << 6));
    }
}
