using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible state that extends Bomb Torizo's ordinary $40-byte enemy slot. The
/// original actor aliases the bank-$AA boss variable arrays at $7800-$781F; spelling those
/// words out here keeps the state inspectable without pretending unrelated common slot
/// fields have the same meaning.
/// </summary>
public sealed class TorizoEnemyState
{
    internal TorizoEnemyState(RoomEnemySlot slot, bool isGolden)
    {
        Slot = slot;
        IsGolden = isGolden;
    }

    public RoomEnemySlot Slot { get; }
    public bool IsGolden { get; }

    /// <summary>Native <c>toriz_var_00</c>: instruction-list return/landing target.</summary>
    public ushort ReturnInstruction { get; internal set; }

    /// <summary>Native <c>toriz_var_01</c>: interrupted list restored after head damage.</summary>
    public ushort InterruptedInstruction { get; internal set; }

    /// <summary>Native <c>toriz_var_03</c>: delayed in-air transition counter.</summary>
    public ushort AirTransitionTimer { get; internal set; }

    /// <summary>
    /// Native <c>toriz_var_04</c>. Nonzero frames reject shots before common damage AI.
    /// The awakening list writes the cartridge sentinel $7777 and later clears it.
    /// </summary>
    public ushort ShotGuard { get; internal set; }

    /// <summary>Native <c>toriz_var_09</c>: animation/decision repetition counter.</summary>
    public ushort DecisionCounter { get; internal set; }

    /// <summary>Native <c>toriz_var_05</c>: last missile-family word seen by Golden Torizo.</summary>
    public ushort CapturedProjectileFamily { get; internal set; }

    /// <summary>Native <c>toriz_var_06</c>: Golden Torizo egg/attack-list flag word.</summary>
    public ushort AttackFlags { get; internal set; }

    /// <summary>Native <c>toriz_var_07</c>: consecutive frames Samus has space-jumped.</summary>
    public ushort SamusSpaceJumpFrames { get; internal set; }

    /// <summary>Signed 8.8 jump velocity or signed whole-pixel walk delta, by caller.</summary>
    public ushort HorizontalVelocity { get; internal set; }

    /// <summary>Signed 8.8 vertical velocity.</summary>
    public ushort VerticalVelocity { get; internal set; }

    /// <summary>Signed 8.8 vertical acceleration.</summary>
    public ushort VerticalAcceleration { get; internal set; }

    /// <summary>Native <c>toriz_var_E</c> main-function dispatcher pointer.</summary>
    public ushort Function { get; internal set; }

    /// <summary>Native <c>toriz_var_F</c> active-state pre-instruction pointer.</summary>
    public ushort PreInstruction { get; internal set; }

    public bool AwakeningReleased { get; internal set; }
    public bool DeathStarted { get; internal set; }
    public bool BossBitSet { get; internal set; }
    public bool ItemDropRequested { get; internal set; }
}

/// <summary>Music request produced by Bomb Torizo's ROM instruction stream.</summary>
public readonly record struct BombTorizoMusicRequest(byte Track, byte DelayFrames);

/// <summary>
/// Cartridge-faithful translation of enemy definition $EEFF. Animation duration, extended
/// spritemaps, branches, and attack selection remain in the original bank-$AA instruction
/// lists; this file translates only the native callbacks those lists invoke.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BombTorizoDefinition = 0xeeff;
    internal const ushort GoldenTorizoDefinition = 0xef7f;
    internal const ushort BombTorizoTouchAi = 0xc977;
    internal const ushort BombTorizoShotAi = 0xc97c;
    internal const ushort GoldenTorizoShotAi = 0xd667;

    private const ushort BombTorizoInitialInstruction = 0xb879;
    private const ushort BombTorizoLowHealthInterruptInstruction = 0xb0e5;
    private const ushort BombTorizoLowHealthRecoveryInstruction = 0xb155;
    private const ushort BombTorizoDeathInstruction = 0xb1c8;
    private const ushort BombTorizoInitialExtendedSpritemap = 0x87d0;
    private const ushort BombTorizoHandTriggerPlm = 0xd6ea;
    private const ushort BombTorizoShotGuardValue = 0x7777;
    private const ushort BombTorizoHeadExplosionHealth = 350;
    private const ushort BombTorizoCoreExplosionHealth = 100;
    private const ushort BombTorizoRawTangibleProperty = 0x8000;

    private const ushort TorizoFunctionIdle = 0xc6ab;
    private const ushort TorizoFunctionFalling = 0xc6bf;
    private const ushort TorizoFunctionWaitForHandTrigger = 0xc6c6;
    private const ushort TorizoFunctionActive = 0xc6ff;
    private const ushort GoldenTorizoFunctionWaitForSamus = 0xd5c2;
    private const ushort GoldenTorizoFunctionGravity = 0xd5df;
    private const ushort GoldenTorizoFunctionPreInstruction = 0xd5e6;

    private const ushort TorizoPreInstructionIdle = 0xc95e;
    private const ushort TorizoPreInstructionAirTransition = 0xc752;
    private const ushort TorizoPreInstructionGravity = 0xc828;
    private const ushort TorizoPreInstructionJump = 0xc82c;
    private const ushort GoldenTorizoPreInstructionAirTransition = 0xd5f1;

    private TorizoEnemyState? _torizoState;
    private Func<ushort, bool>? _isRoomPlmPresent;
    private Func<bool>? _isAreaTorizoDefeated;
    private Action? _setAreaTorizoDefeated;

    /// <summary>Bomb Torizo's state while definition $EEFF owns slot zero.</summary>
    public TorizoEnemyState? BombTorizo =>
        _torizoState is { IsGolden: false } ? _torizoState : null;

    /// <summary>Golden Torizo's state while definition $EF7F owns slot zero.</summary>
    public TorizoEnemyState? GoldenTorizo =>
        _torizoState is { IsGolden: true } ? _torizoState : null;

    /// <summary>Last library-two sound requested by the current Torizo frame.</summary>
    public ushort? LastBombTorizoSoundEffect { get; private set; }

    /// <summary>Last delayed music request emitted by awakening/death bytecode.</summary>
    public BombTorizoMusicRequest? LastBombTorizoMusicRequest { get; private set; }

    private void ResetBombTorizoRoomState()
    {
        _torizoState = null;
        LastBombTorizoSoundEffect = null;
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>
    /// Ports shared <c>Torizo_Init</c> at $AA:C87F for the area-zero Bomb and area-two
    /// Golden definitions. The cartridge indexes parallel two-word tables by area; using
    /// the concrete definition here is equivalent and prevents an unrelated host area enum
    /// from becoming a second authority for actor identity.
    /// </summary>
    private void InitializeBombTorizo(RoomEnemySlot torizo)
    {
        bool isGolden = torizo.EnemyDefinitionPointer == GoldenTorizoDefinition;
        var state = new TorizoEnemyState(torizo, isGolden);
        _torizoState = state;

        // Boss bit four is shared by the Bomb and Golden Torizo encounters, but it is stored
        // per area. The room loader supplies the current area's accessor, keeping the actor
        // independent from save-memory layout and avoiding a room-number special case.
        if (_isAreaTorizoDefeated?.Invoke() == true)
        {
            torizo.Properties = torizo.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            return;
        }

        // Entries zero/one are Bomb/Golden respectively. Every value remains ROM-backed;
        // only the table index is selected from the already-validated enemy definition.
        int tableOffset = isGolden ? 2 : 0;
        torizo.XPosition = ReadWord(_bus!, 0xaac95f + tableOffset);
        torizo.YPosition = ReadWord(_bus!, 0xaac963 + tableOffset);
        torizo.CurrentInstruction = ReadWord(_bus!, 0xaac967 + tableOffset);
        torizo.Properties = unchecked((ushort)(
            torizo.Properties | ReadWord(_bus!, 0xaac96b + tableOffset)));
        torizo.ExtraProperties = torizo.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        torizo.XRadius = ReadWord(_bus!, 0xaac96f + tableOffset);
        torizo.YRadius = ReadWord(_bus!, 0xaac973 + tableOffset);
        torizo.InstructionTimer = 1;
        torizo.Timer = 0;
        torizo.PaletteIndex = 0;
        torizo.SpritemapPointer = BombTorizoInitialExtendedSpritemap;

        state.Function = TorizoFunctionFalling;
        state.PreInstruction = TorizoPreInstructionIdle;
        state.HorizontalVelocity = 0;
        state.VerticalVelocity = 0x0100;

        // Init writes two shared target rows and then selects the encounter-specific body
        // pair. This runtime exposes live CGRAM rather than separate current/target buffers,
        // so install those final visible rows directly at their native palette indexes.
        LoadTorizoSharedPaletteRows();
        if (isGolden)
            LoadGoldenTorizoBasePalette();
        else
            LoadBombTorizoPalette();
    }

    private void LoadTorizoSharedPaletteRows()
    {
        ReadOnlySpan<ushort> rowEleven =
        [
            0x3800, 0x03ff, 0x033b, 0x0216, 0x0113, 0x6b1e, 0x4a16, 0x3591,
            0x20e9, 0x1580, 0x1580, 0x1580, 0x1580, 0x1580, 0x1580, 0x1580,
        ];
        ReadOnlySpan<ushort> rowFifteen =
        [
            0x3800, 0x02df, 0x01d7, 0x00ac, 0x5a73, 0x41ad, 0x2d08, 0x1863,
            0x1486, 0x0145, 0x0145, 0x0145, 0x7fff, 0x0145, 0x0145, 0x0000,
        ];
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(176 + color, rowEleven[color]);
            _cgram.SetColor(240 + color, rowFifteen[color]);
        }
    }

    private void LoadBombTorizoPalette()
    {
        // Exact words from kTorizo_Palettes_2/_3 in bank $AA. Palette rows nine and ten
        // are used by the two extended-spritemap halves of the Bomb Torizo body.
        ReadOnlySpan<ushort> rowNine =
        [
            0x3800, 0x679f, 0x5299, 0x252e, 0x14aa, 0x5efc, 0x4657, 0x35b2,
            0x2d70, 0x5b7f, 0x3df8, 0x2d0e, 0x5f5f, 0x5e1a, 0x5d35, 0x0c63,
        ];
        ReadOnlySpan<ushort> rowTen =
        [
            0x3800, 0x4aba, 0x35b2, 0x0847, 0x0003, 0x4215, 0x2970, 0x18cb,
            0x1089, 0x463a, 0x28b3, 0x1809, 0x6f7f, 0x51fd, 0x4113, 0x0c63,
        ];
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(144 + color, rowNine[color]);
            _cgram.SetColor(160 + color, rowTen[color]);
        }
    }

    private void LoadGoldenTorizoBasePalette()
    {
        // Torizo_C280 is the Golden encounter's initial body pair. Later hurt frames use
        // bank $84's health-indexed gradient; keeping this initial write separate mirrors
        // the cartridge's target-palette setup before the first live damage callback.
        ReadOnlySpan<ushort> rowNine =
        [
            0x3800, 0x6ab5, 0x49b0, 0x1c45, 0x0c01, 0x5613, 0x416d, 0x2cc9,
            0x2066, 0x5714, 0x31cc, 0x14e3, 0x5630, 0x3569, 0x1883, 0x0c66,
        ];
        ReadOnlySpan<ushort> rowTen =
        [
            0x3800, 0x5610, 0x350b, 0x0800, 0x0000, 0x416e, 0x2cc8, 0x1823,
            0x0c01, 0x6a31, 0x4caa, 0x2406, 0x7f7b, 0x75f4, 0x4d10, 0x0c63,
        ];
        LoadTorizoBodyPalette(rowNine, rowTen);
    }

    private void LoadTorizoDeathPalette()
    {
        // Torizo_C268 is shared by the late death bytecode for both encounters.
        ReadOnlySpan<ushort> rowNine =
        [
            0x3800, 0x56ba, 0x41b2, 0x1447, 0x0403, 0x4e15, 0x3570, 0x24cb,
            0x1868, 0x6f7f, 0x51f8, 0x410e, 0x031f, 0x01da, 0x00f5, 0x0c63,
        ];
        ReadOnlySpan<ushort> rowTen =
        [
            0x3800, 0x4215, 0x2d0d, 0x0002, 0x0000, 0x3970, 0x20cb, 0x0c26,
            0x0403, 0x463a, 0x28b3, 0x1809, 0x6f7f, 0x51fd, 0x4113, 0x0c63,
        ];
        LoadTorizoBodyPalette(rowNine, rowTen);
    }

    private void LoadGoldenTorizoFinalPalette()
    {
        // Torizo_C298 is selected by Golden Torizo's late encounter instruction $CADE.
        ReadOnlySpan<ushort> rowNine =
        [
            0x3800, 0x4bbe, 0x06b9, 0x00a8, 0x0000, 0x173a, 0x0276, 0x01f2,
            0x014d, 0x73e0, 0x4f20, 0x2a20, 0x7fe0, 0x5aa0, 0x5920, 0x0043,
        ];
        ReadOnlySpan<ushort> rowTen =
        [
            0x3800, 0x3719, 0x0214, 0x0003, 0x0000, 0x0295, 0x01d1, 0x014d,
            0x00a8, 0x4b40, 0x25e0, 0x00e0, 0x6b40, 0x4600, 0x4480, 0x0000,
        ];
        LoadTorizoBodyPalette(rowNine, rowTen);
    }

    private void LoadGoldenTorizoHealthPalette(ushort health)
    {
        // $84:8000 selects one of eight 16-color rows using bits 11..14 of health, clamping
        // the upper half to row seven. Deriving the row start algebraically preserves the
        // original descending copy while keeping the loop natural for debugger inspection.
        int selector = (health >> 8) & 0x78;
        if ((selector & 0x40) != 0)
            selector = 56;
        int sourceWord = selector * 2;
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(144 + color, ReadWord(_bus!, 0x848032 + (sourceWord + color) * 2));
            _cgram.SetColor(160 + color, ReadWord(_bus!, 0x848132 + (sourceWord + color) * 2));
        }
    }

    private void LoadTorizoBodyPalette(
        ReadOnlySpan<ushort> rowNine,
        ReadOnlySpan<ushort> rowTen)
    {
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(144 + color, rowNine[color]);
            _cgram.SetColor(160 + color, rowTen[color]);
        }
    }

    private void BlackOutBombTorizoPalette()
    {
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(144 + color, 0);
            _cgram.SetColor(160 + color, 0);
        }
    }

    /// <summary>Ports <c>Torizo_Main</c> and its <c>toriz_var_E</c> dispatcher.</summary>
    private void RunBombTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        LastBombTorizoSoundEffect = null;
        switch (state.Function)
        {
            case TorizoFunctionIdle:
                return;

            case TorizoFunctionFalling:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                ApplyBombTorizoGravity(torizo, state, RequireBombTorizoLevel(level));
                return;

            case TorizoFunctionWaitForHandTrigger:
                // $AA:C6C6 scans all forty PLM headers for $D6EA. A supplied PLM owner is
                // authoritative; standalone enemy audits omit it and therefore model the
                // already-collected item/removed hand trigger.
                torizo.Properties = unchecked((ushort)(
                    torizo.Properties | BombTorizoRawTangibleProperty));
                if (_isRoomPlmPresent?.Invoke(BombTorizoHandTriggerPlm) == true)
                    return;

                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(6, 8);
                torizo.Properties = unchecked((ushort)(
                    torizo.Properties & ~BombTorizoRawTangibleProperty));
                torizo.CurrentInstruction = unchecked((ushort)(torizo.CurrentInstruction + 2));
                torizo.InstructionTimer = 1;
                state.AwakeningReleased = true;
                return;

            case TorizoFunctionActive:
                RunBombTorizoActiveState(torizo, state, level);
                return;

            case GoldenTorizoFunctionWaitForSamus:
                // $AA:D5C2 advances the list operand only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus is not null && samus.YPosition > 0x0140 && samus.XPosition > 0x0170)
                {
                    torizo.CurrentInstruction = unchecked((ushort)(torizo.CurrentInstruction + 2));
                    torizo.InstructionTimer = 1;
                }
                return;

            case GoldenTorizoFunctionGravity:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                ApplyBombTorizoGravity(torizo, state, RequireBombTorizoLevel(level));
                return;

            case GoldenTorizoFunctionPreInstruction:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                RunBombTorizoPreInstruction(torizo, state, level);
                return;

            default:
                throw new NotSupportedException(
                    $"Torizo main function $AA:{state.Function:X4} is not translated.");
        }
    }

    private void RunGoldenTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // GoldTorizo_Main counts consecutive space-jump poses for its anti-stall leap
        // decision, then calls the same toriz_var_E dispatcher as Bomb Torizo.
        if (samus?.Pose is 0x1b or 0x1c)
            state.SamusSpaceJumpFrames = unchecked((ushort)(state.SamusSpaceJumpFrames + 1));
        else
            state.SamusSpaceJumpFrames = 0;
        RunBombTorizoMain(torizo, state, samus, level);
    }

    private void RunBombTorizoActiveState(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        MaybeSpawnBombTorizoLowHealthDrool(torizo);

        // The two high parameter-two bits are death-sequence latches. Before they are set,
        // health thresholds interrupt the current authored list with the core/head bursts.
        if ((torizo.Parameter2 & 0x8000) == 0 &&
            torizo.Health < BombTorizoHeadExplosionHealth)
        {
            state.InterruptedInstruction = torizo.CurrentInstruction;
            torizo.CurrentInstruction = BombTorizoLowHealthInterruptInstruction;
            torizo.InstructionTimer = 1;
            return;
        }

        if ((torizo.Parameter2 & 0x4000) == 0 &&
            torizo.Health < BombTorizoCoreExplosionHealth)
        {
            state.ReturnInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? (ushort)0xbd0e
                : (ushort)0xc188;
            torizo.CurrentInstruction = BombTorizoLowHealthRecoveryInstruction;
            torizo.InstructionTimer = 1;
            return;
        }

        RunBombTorizoPreInstruction(torizo, state, level);
    }

    private void RunBombTorizoPreInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        switch (state.PreInstruction)
        {
            case TorizoPreInstructionIdle:
                return;
            case TorizoPreInstructionAirTransition:
                RunBombTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstructionGravity:
                ApplyBombTorizoGravity(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstructionJump:
                RunBombTorizoJump(torizo, state, RequireBombTorizoLevel(level));
                return;
            case GoldenTorizoPreInstructionAirTransition:
                RunGoldenTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            default:
                throw new NotSupportedException(
                    $"Bomb Torizo pre-instruction $AA:{state.PreInstruction:X4} is not translated.");
        }
    }

    private void ApplyBombTorizoGravity(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        int displacement = unchecked((short)state.VerticalVelocity) << 8;
        if (MoveEnemyVertically(level, torizo, displacement))
        {
            short velocity = unchecked((short)state.VerticalVelocity);
            if (velocity >= 0 && velocity != 0x0100)
            {
                EarthquakeType = 4;
                EarthquakeTimer = 32;
                state.VerticalVelocity = 0x0100;
            }
            return;
        }

        state.VerticalVelocity = unchecked((ushort)(state.VerticalVelocity + 40));
    }

    private void RunBombTorizoJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            torizo,
            unchecked((short)state.HorizontalVelocity) << 8);
        AlignEnemyYWithNonSquareSlope(level, torizo);

        if (MoveEnemyVertically(
                level,
                torizo,
                unchecked((short)state.VerticalVelocity) << 8))
        {
            torizo.CurrentInstruction = state.ReturnInstruction;
            torizo.InstructionTimer = 1;
            state.VerticalVelocity = 0x0100;
            EarthquakeType = 4;
            EarthquakeTimer = 32;
            return;
        }

        state.VerticalVelocity = unchecked((ushort)(
            state.VerticalVelocity + state.VerticalAcceleration));
    }

    private void RunBombTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        bool alternateLanding = (torizo.Parameter2 & 0x4000) != 0;
        if (state.AirTransitionTimer != 0)
        {
            state.AirTransitionTimer = unchecked((ushort)(state.AirTransitionTimer - 1));
            if (state.AirTransitionTimer == 0)
            {
                torizo.CurrentInstruction = alternateLanding
                    ? ((torizo.Parameter1 & 0x8000) != 0 ? (ushort)0xbd0e : (ushort)0xc188)
                    : ((torizo.Parameter1 & 0x8000) != 0 ? (ushort)0xb962 : (ushort)0xbdd8);
                torizo.InstructionTimer = 1;
                return;
            }
        }

        int fallingSpeed = Math.Min(Math.Abs(unchecked((short)state.HorizontalVelocity)) + 1, 15);
        if (!MoveEnemyVertically(level, torizo, fallingSpeed << 16))
        {
            // $AA:C752 swaps to the authored in-air list while the downward probe remains
            // clear. Landing itself is handled by the list-installed gravity/jump callback.
            torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? (ushort)0xc0f2
                : (ushort)0xbc78;
            torizo.InstructionTimer = 1;
            state.VerticalVelocity = 0x0100;
            state.HorizontalVelocity = 0;
        }
    }

    private void RunGoldenTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        // $AA:D5F1 is the Golden analogue of $C752. Its only behavioral difference is the
        // sixteen-frame facing transition and Golden landing lists selected when it expires.
        if (state.AirTransitionTimer != 0)
        {
            state.AirTransitionTimer = unchecked((ushort)(state.AirTransitionTimer - 1));
            if (state.AirTransitionTimer == 0)
            {
                torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                    ? (ushort)0xd203
                    : (ushort)0xd2bf;
                torizo.InstructionTimer = 1;
                return;
            }
        }

        int fallingSpeed = Math.Min(
            Math.Abs(unchecked((short)state.HorizontalVelocity)) + 1,
            15);
        if (!MoveEnemyVertically(level, torizo, fallingSpeed << 16))
        {
            torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? (ushort)0xc0f2
                : (ushort)0xbc78;
            torizo.InstructionTimer = 1;
            state.VerticalVelocity = 0x0100;
            state.HorizontalVelocity = 0;
        }
    }

    private void MaybeSpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo)
    {
        if (torizo.Health == 0 || torizo.Health >= BombTorizoHeadExplosionHealth)
            return;
        if ((_readRandomNumber?.Invoke() ?? _nextRandom!()) is ushort random &&
            (random & 0x8142) == 0)
        {
            SpawnBombTorizoInitialDrool(torizo);
        }
    }

    /// <summary>Ports Bomb Torizo's custom hurt callback $AA:C67E.</summary>
    private void ApplyBombTorizoHurt(RoomEnemySlot torizo)
    {
        MaybeSpawnBombTorizoLowHealthDrool(torizo);
        if ((torizo.FlashTimer & 1) == 0)
        {
            if (torizo.EnemyDefinitionPointer == GoldenTorizoDefinition)
                LoadGoldenTorizoHealthPalette(torizo.Health);
            else
                LoadTorizoDeathPalette();
        }
        else
        {
            for (int color = 0; color < 16; color++)
            {
                _cgram!.SetColor(144 + color, 0x7fff);
                _cgram.SetColor(160 + color, 0x7fff);
            }
        }
    }

    private void BeginBombTorizoDeath(RoomEnemySlot torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Function = TorizoFunctionIdle;
        torizo.CurrentInstruction = BombTorizoDeathInstruction;
        torizo.InstructionTimer = 1;
        torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0xc000));
        torizo.Properties = unchecked((ushort)(
            torizo.Properties | BombTorizoRawTangibleProperty));
    }

    private void FinishBombTorizoDeath(TorizoEnemyState state)
    {
        _setAreaTorizoDefeated?.Invoke();
        state.BossBitSet = true;
        state.ItemDropRequested = true;
        LastBombTorizoMusicRequest = new BombTorizoMusicRequest(3, 8);
    }

    private static RoomLevelData RequireBombTorizoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Bomb Torizo movement requires the active room collision layer.");

    private TorizoEnemyState RequireBombTorizoState(RoomEnemySlot torizo) =>
        _torizoState is not null && ReferenceEquals(_torizoState.Slot, torizo)
            ? _torizoState
            : throw new InvalidOperationException(
                $"Enemy slot {torizo.SlotIndex} has no initialized Bomb Torizo state.");
}
