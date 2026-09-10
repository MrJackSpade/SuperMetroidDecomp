using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail initialization and dispatcher foundation for Kraid's eight physical enemy slots.
/// Kraid's body uses BG2 while the arm, foot, lints, and nails use ordinary/extended enemy
/// spritemaps; preserving those independent records is required for native layer and damage
/// ordering later in the encounter.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort KraidInitialArmInstruction = KraidInstructionLists.InitialArm;
    private const ushort KraidInitialLintInstruction = KraidInstructionLists.InitialLint;
    private const ushort KraidInitialLintSpritemap = 0xa5df;
    private const ushort KraidInitialFootInstruction = KraidInstructionLists.InitialFoot;
    private const ushort KraidNailInstruction = KraidInstructionLists.Nail;

    /// <summary>Most recent Kraid-private sound request in the current enemy frame.</summary>
    public KraidSoundRequest? LastKraidSoundEffect { get; private set; }

    private void InitializeKraidBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Kraid body requires native slot zero, not slot {body.SlotIndex}.");
        }

        _kraidState = new KraidEnemyState();
        KraidEnemyState state = _kraidState;
        if (RequireAreaBossDefeated())
        {
            // `$A7:A959` installs the dead-room BG palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            _cgram!.LoadFromBus(_bus!, 0xa786c7, colorCount: 16, destinationIndex: 96);
            state.BackgroundTilemapWords.AsSpan().Fill(KraidBackgroundRomData.BlankTile);
            state.BackgroundTilemapsPrepared = true;
            _vram!.ExecuteQueuedWrite(
                _bus!,
                KraidBackgroundRomData.RoomBackgroundTileAddress,
                KraidBackgroundRomData.RoomBackgroundTileBytes,
                KraidBackgroundRomData.RoomBackgroundTileVramWord);
            _kraidPlmRequests.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body is the owner of `$A7:C715-$C815`; deleting it here skips the two
            // BG2 clears and four standard-BG3 restoration DMAs. The other seven physical
            // records take their own dead initializer and remain deleted as on cartridge.
            body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;
            return;
        }

        state.CameraDistanceIndex = KraidCameraDefinitions.CameraDistanceIndex;
        ApplyKraidScrolls(KraidCameraDefinitions.InitialScrolls);
        state.MinimumYPositionForEjection = 324;
        ushort oneEighth = unchecked((ushort)(body.Health >> 3));
        for (int index = 0; index < state.HealthEighthThresholds.Length; index++)
        {
            state.HealthEighthThresholds[index] = unchecked((ushort)(
                oneEighth * (index + 1)));
        }
        ushort oneQuarter = unchecked((ushort)(body.Health >> 2));
        for (int index = 0; index < state.HealthQuarterThresholds.Length; index++)
        {
            state.HealthQuarterThresholds[index] = unchecked((ushort)(
                oneQuarter * (index + 1)));
        }

        // `$A7:AAC6` constructs the private WRAM tilemap which subsequent rise, head,
        // growth, and death functions upload in independently timed slices.
        InitializeKraidBackground(state);
        state.HurtFrame = 0;
        state.HurtFrameTimer = 0;

        body.XPosition = 176;
        body.YPosition = 592;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        body.VariableA = (ushort)KraidAiFunction.RestrictSamusToFirstScreen;
        body.VariableF = 300;
        body.VariableC = 64;
        state.Parts[0].NextFunction = KraidAiFunction.RaiseKraidThroughFloor;

        // Background palette three's target colors are written at CGRAM palette eleven in
        // native target-palette storage. The software renderer exposes that buffer directly.
        _cgram!.LoadFromBus(_bus!, 0xa7aaa6, colorCount: 16, destinationIndex: 176);
        EarthquakeType = 5;
    }

    private void ApplyKraidScrolls(ReadOnlySpan<RoomScrollState> scrolls)
    {
        for (int index = 0; index < scrolls.Length; index++)
            RequireSetRoomScrollState(index, scrolls[index]);
    }

    private void InitializeKraidArm(RoomEnemySlot arm)
    {
        KraidEnemyState state = RequireKraidState(arm);
        if (RequireAreaBossDefeated())
        {
            MarkKraidPartDead(arm);
            return;
        }
        EnsureKraidSlot(arm, 1, "arm");
        arm.PaletteIndex = _slots[0].PaletteIndex;
        arm.VariableA = (ushort)KraidAiFunction.NoOperation;
        arm.CurrentInstruction = KraidInitialArmInstruction;
        arm.InstructionTimer = 1;
        arm.VariableB = 0;
        state.Parts[arm.SlotIndex].NextFunction = KraidAiFunction.NoOperation;
    }

    private void InitializeKraidLint(RoomEnemySlot lint, int expectedSlot)
    {
        _ = RequireKraidState(lint);
        if (RequireAreaBossDefeated())
        {
            MarkKraidPartDead(lint);
            return;
        }
        EnsureKraidSlot(lint, expectedSlot, "lint");
        lint.PaletteIndex = _slots[0].PaletteIndex;
        lint.InstructionTimer = 0x7fff;
        lint.CurrentInstruction = KraidInitialLintInstruction;
        lint.SpritemapPointer = KraidInitialLintSpritemap;
        lint.VariableA = (ushort)KraidAiFunction.LintInactive;
        lint.VariableC = expectedSlot == 2 ? (ushort)0 : (ushort)0xfff0;
    }

    private void InitializeKraidFoot(RoomEnemySlot foot)
    {
        KraidEnemyState state = RequireKraidState(foot);
        if (RequireAreaBossDefeated())
        {
            MarkKraidPartDead(foot);
            return;
        }
        EnsureKraidSlot(foot, 5, "foot");
        foot.PaletteIndex = _slots[0].PaletteIndex;
        foot.CurrentInstruction = KraidInitialFootInstruction;
        foot.InstructionTimer = 1;
        foot.VariableA = (ushort)KraidAiFunction.NoOperation;
        state.Parts[foot.SlotIndex].NextFunction = 0;
    }

    private void InitializeKraidNail(RoomEnemySlot nail, int expectedSlot)
    {
        KraidEnemyState state = RequireKraidState(nail);
        if (RequireAreaBossDefeated())
        {
            MarkKraidPartDead(nail);
            return;
        }
        EnsureKraidSlot(nail, expectedSlot, "fingernail");
        nail.PaletteIndex = _slots[0].PaletteIndex;
        nail.VariableB = 40;
        nail.Properties = nail.Properties.With(EnemyProperties.Invisible);
        nail.InstructionTimer = 0x7fff;
        nail.CurrentInstruction = KraidNailInstruction;
        nail.SpritemapPointer = ReadWord(
            _bus!, EnemyRomTablePointers.Kraid.InitialNailSpritemapWord);
        state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
        nail.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
        nail.VariableF = 64;
    }

    private static void EnsureKraidSlot(RoomEnemySlot slot, int expectedSlot, string part)
    {
        if (slot.SlotIndex != expectedSlot)
        {
            throw new InvalidDataException(
                $"Kraid {part} requires native slot {expectedSlot}, not {slot.SlotIndex}.");
        }
    }

    /// <summary>Ports `$A7:A943`: preserve raw upper flags while installing `$0700`.</summary>
    private static void MarkKraidPartDead(RoomEnemySlot slot) =>
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible,
            EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);

    private void RunKraidBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY,
        VramWriteQueue? vramWriteQueue)
    {
        KraidEnemyState state = RequireKraidState(body);
        // `$A7:AC21` makes BG2 follow Kraid rather than the room. X radius is the native
        // body's right-edge origin adjustment; 152 is the authored vertical anchor.
        state.Bg2HorizontalScroll = unchecked((ushort)(
            cameraX - body.XPosition + body.XRadius));
        state.Bg2VerticalScroll = unchecked((ushort)(cameraY - body.YPosition + 152));
        RunKraidPaletteHandling(body, state);
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
        if (function is >= KraidAiFunction.RestrictSamusToFirstScreen and
            <= KraidAiFunction.RaiseBody)
        {
            RunKraidRiseFunction(body, state, samus);
            return;
        }
        RunKraidCombatFunction(body, state, vramWriteQueue);
    }

    private void RunKraidPaletteHandling(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health == 0)
        {
            state.HurtFrame = 0;
            UpdateKraidHealthPalettes(body, state);
            return;
        }
        if (state.HurtFrame == 0)
            return;
        state.HurtFrameTimer = unchecked((ushort)(state.HurtFrameTimer - 1));
        if (state.HurtFrameTimer == 0)
        {
            state.HurtFrameTimer = 2;
            state.HurtFrame = unchecked((ushort)(state.HurtFrame - 1));
            UpdateKraidHealthPalettes(body, state);
        }
    }

    private void UpdateKraidHealthPalettes(RoomEnemySlot body, KraidEnemyState state)
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
                    body.Health - state.HealthEighthThresholds[thresholdWordOffset / 2])) < 0)
            {
                thresholdWordOffset -= 2;
            }
        }
        int sourceColor = 8 * (thresholdWordOffset + 2);
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(
                112 + color,
                ReadWord(
                    _bus!,
                    EnemyRomTablePointers.Kraid.HealthPaletteWords +
                        (sourceColor + color) * 2));
            _cgram.SetColor(
                240 + color,
                ReadWord(
                    _bus!,
                    EnemyRomTablePointers.Kraid.SecondaryPaletteWords +
                        (sourceColor + color) * 2));
        }
    }
}
