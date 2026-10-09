using SuperMetroid.Core.Assets;
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
    /// <summary>Initial instruction entry for the independently animated arm actor.</summary>
    private const ushort KraidInitialArmInstruction =
        KraidArmInstructionProgramDefinitions.RisingOrSinking;

    /// <summary>Initial instruction entry shared by Kraid's lint actors.</summary>
    private const ushort KraidInitialLintInstruction =
        KraidLintInstructionLists.InitialLint;

    /// <summary>Initial instruction entry for the foot actor's native animation loop.</summary>
    private const ushort KraidInitialFootInstruction =
        KraidFootInstructionProgramDefinitions.Initial;

    /// <summary>Most recent Kraid-private sound request in the current enemy frame.</summary>
    public KraidSoundRequest? LastKraidSoundEffect { get; private set; }

    /// <summary>Initializes Kraid's slot-zero body, arena state, palette, and either the live or defeated room setup.</summary>
    /// <param name="body">Native body slot, which must be slot zero.</param>
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
            LoadKraidColorBand(KraidPaletteSource.RoomBackdrop, 96);
            state.BackgroundTilemapWords.AsSpan().Fill(KraidBackgroundRomData.BlankTile);
            state.BackgroundTilemapsPrepared = true;
            UploadKraidRoomBackgroundTiles();
            _kraidPlmRequests.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body is the owner of `$A7:C715-$C815`; deleting it here skips the two
            // BG2 clears and four standard-BG3 restoration DMAs. The other seven physical
            // records take their own dead initializer and remain deleted as on cartridge.
            body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;
            return;
        }

        // $A7:A9E4-A9E7.
        CameraDistanceIndex = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(grown: false);
        state.MinimumYPositionForEjection = 324;
        state.InitialHealth = body.Health;

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
        LoadKraidColorBand(KraidPaletteSource.InitialTarget, 176);
        EarthquakeType = 5;
    }

    /// <summary>Installs the per-screen scroll thresholds for Kraid's initial or grown arena layout.</summary>
    /// <param name="grown">Selects the post-growth thresholds when <see langword="true"/>.</param>
    private void ApplyKraidScrolls(bool grown)
    {
        for (int index = 0; index < KraidCameraDefinitions.ScreenCount; index++)
            RequireSetRoomScrollState(index, grown
                ? KraidCameraDefinitions.GrownScroll(index)
                : KraidCameraDefinitions.InitialScroll(index));
    }

    /// <summary>Initializes Kraid's arm actor in slot one or marks it dead when the boss is defeated.</summary>
    /// <param name="arm">Physical arm slot to configure.</param>
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

    /// <summary>Initializes one lint actor with its slot-specific offset and compiled instruction list.</summary>
    /// <param name="lint">Physical lint slot to configure.</param>
    /// <param name="expectedSlot">Native slot identity, used to select the lint's horizontal offset.</param>
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
        lint.SpritemapPointer = KraidLintVisualDefinitions.InitialFrame;
        lint.VariableA = (ushort)KraidAiFunction.LintInactive;
        lint.VariableC = expectedSlot == 2 ? (ushort)0 : (ushort)0xfff0;
    }

    /// <summary>Initializes Kraid's foot actor in slot five or marks it dead for a defeated boss.</summary>
    /// <param name="foot">Physical foot slot to configure.</param>
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

    /// <summary>
    /// Ports <c>InitAI_KraidNail_Common</c> ($A7:BCF2). Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once slot zero's health reads below one ($A7:BD34).
    /// </summary>
    private void InitializeKraidNail(RoomEnemySlot nail, int expectedSlot)
    {
        KraidEnemyState state = RequireKraidState(nail);
        EnsureKraidSlot(nail, expectedSlot, "fingernail");
        nail.PaletteIndex = _slots[0].PaletteIndex;
        nail.VariableB = 40;
        nail.Properties = nail.Properties.With(EnemyProperties.Invisible);
        nail.InstructionTimer = 0x7fff;
        nail.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
        nail.SpritemapPointer = KraidVisualDefinitions.InitialNailFrame;
        state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
        nail.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
        nail.VariableF = 64;
    }

    /// <summary>Validates that a Kraid body part occupies the native physical slot assigned to it.</summary>
    /// <param name="slot">Actor whose slot identity is checked.</param>
    /// <param name="expectedSlot">Required native slot index.</param>
    /// <param name="part">Part label included in the invalid-data diagnostic.</param>
    /// <exception cref="InvalidDataException">The actor occupies a different slot from the required one.</exception>
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

    /// <summary>
    /// Ports <c>MainAI_Kraid</c> ($A7:AC21): mouth-projectile collision, palette handling,
    /// body-projectile collision and body-vs-Samus collision precede the function. A
    /// killing mouth hit therefore reaches palette handling, which zeroes the hurt frame,
    /// before the death initializer runs in this same frame.
    /// </summary>
    private void RunKraidBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY,
        VramWriteQueue? vramWriteQueue,
        SamusProjectileSystem? samusProjectiles,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        KraidEnemyState state = RequireKraidState(body);
        KraidShotSlots? shots = samusProjectiles is not null && sharedProjectiles is not null
            ? new KraidShotSlots(samusProjectiles, sharedProjectiles)
            : null;
        if (shots is { } mouthShots)
            ResolveKraidMouthProjectileHits(body, state, mouthShots);
        RunKraidPaletteHandling(body, state);
        if (shots is { } bodyShots)
            ResolveKraidBodyProjectileHits(body, state, bodyShots);
        if (samus is not null)
            ResolveKraidBodyContact(body, state, samus);
        // `$A7:AC21` makes BG2 follow Kraid rather than the room. X radius is the native
        // body's right-edge origin adjustment; 152 is the authored vertical anchor.
        state.Bg2HorizontalScroll = unchecked((ushort)(
            cameraX - body.XPosition + body.XRadius));
        state.Bg2VerticalScroll = unchecked((ushort)(cameraY - body.YPosition + 152));
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
        if (function is >= KraidAiFunction.RestrictSamusToFirstScreen and
            <= KraidAiFunction.RaiseBody)
        {
            RunKraidRiseFunction(body, state, samus);
            return;
        }
        RunKraidCombatFunction(body, state, vramWriteQueue, cameraX);
    }

    /// <summary>Advances Kraid's hurt-flash cadence and refreshes palettes when health or flash state changes.</summary>
    /// <param name="body">Body slot supplying health for palette selection.</param>
    /// <param name="state">Encounter state containing hurt timers and health thresholds.</param>
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

    /// <summary>Writes the health-band colors and hurt-flash colors into Kraid's two CGRAM palettes.</summary>
    /// <param name="body">Body slot whose current health selects a palette band.</param>
    /// <param name="state">Encounter state providing health thresholds and active hurt-frame selection.</param>
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
                    body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
            {
                thresholdWordOffset -= 2;
            }
        }
        int sourceColor = 8 * (thresholdWordOffset + 2);
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(
                112 + color,
                ReadKraidColor(KraidPaletteSource.Health, sourceColor + color));
            _cgram.SetColor(
                240 + color,
                ReadKraidColor(KraidPaletteSource.Secondary, sourceColor + color));
        }
    }
}
