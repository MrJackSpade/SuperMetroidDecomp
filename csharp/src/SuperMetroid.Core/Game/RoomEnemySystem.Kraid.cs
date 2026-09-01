namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail initialization and dispatcher foundation for Kraid's eight physical enemy slots.
/// Kraid's body uses BG2 while the arm, foot, lints, and nails use ordinary/extended enemy
/// spritemaps; preserving those independent records is required for native layer and damage
/// ordering later in the encounter.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort KraidInitialArmInstruction = 0x8aa4;
    private const ushort KraidInitialLintInstruction = 0x8afe;
    private const ushort KraidInitialLintSpritemap = 0xa5df;
    private const ushort KraidInitialFootInstruction = 0x86e7;
    private const ushort KraidNailInstruction = 0x8b0a;

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
            // marks every physical actor invisible/deleted/non-interactive.
            _cgram!.LoadFromBus(_bus!, 0xa786c7, colorCount: 16, destinationIndex: 96);
            MarkKraidPartDead(body);
            return;
        }

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

        // `$A7:AAC6` decompresses two cartridge tilemaps and clears priority bits before
        // the first rise frame. The renderer-facing tilemap transfer will consume this
        // state in the dedicated BG2 slice; initialization still records the authored seam.
        state.BackgroundTilemapsPrepared = true;
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
        nail.SpritemapPointer = ReadWord(_bus!, 0xa78b0c);
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
        slot.Properties = unchecked((ushort)((slot.Properties & 0x50ff) | 0x0700));

    private void RunKraidBodyMain(RoomEnemySlot body, SamusState? samus)
    {
        KraidEnemyState state = RequireKraidState(body);
        RunKraidPaletteHandling(body, state);
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
        if (function is >= KraidAiFunction.RestrictSamusToFirstScreen and
            <= KraidAiFunction.RaiseBody)
        {
            RunKraidRiseFunction(body, state, samus);
            return;
        }
        RunKraidCombatFunction(body, state);
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
                ReadWord(_bus!, 0xa7b3d3 + (sourceColor + color) * 2));
            _cgram.SetColor(
                240 + color,
                ReadWord(_bus!, 0xa7b513 + (sourceColor + color) * 2));
        }
    }
}
