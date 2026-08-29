using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible state that extends Bomb Torizo's ordinary $40-byte enemy slot. The
/// original actor aliases the bank-$AA boss variable arrays at $7800-$781F; spelling those
/// words out here keeps the state inspectable without pretending unrelated common slot
/// fields have the same meaning.
/// </summary>
public sealed class BombTorizoEnemyState
{
    internal BombTorizoEnemyState(RoomEnemySlot slot) => Slot = slot;

    public RoomEnemySlot Slot { get; }

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
    internal const ushort BombTorizoTouchAi = 0xc977;
    internal const ushort BombTorizoShotAi = 0xc97c;

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

    private const ushort TorizoPreInstructionIdle = 0xc95e;
    private const ushort TorizoPreInstructionAirTransition = 0xc752;
    private const ushort TorizoPreInstructionGravity = 0xc828;
    private const ushort TorizoPreInstructionJump = 0xc82c;

    private BombTorizoEnemyState? _bombTorizoState;
    private Func<ushort, bool>? _isRoomPlmPresent;
    private Func<bool>? _isAreaTorizoDefeated;
    private Action? _setAreaTorizoDefeated;

    /// <summary>Bomb Torizo's state while definition $EEFF owns slot zero.</summary>
    public BombTorizoEnemyState? BombTorizo => _bombTorizoState;

    /// <summary>Last library-two sound requested by the current Torizo frame.</summary>
    public ushort? LastBombTorizoSoundEffect { get; private set; }

    /// <summary>Last delayed music request emitted by awakening/death bytecode.</summary>
    public BombTorizoMusicRequest? LastBombTorizoMusicRequest { get; private set; }

    private void ResetBombTorizoRoomState()
    {
        _bombTorizoState = null;
        LastBombTorizoSoundEffect = null;
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>Ports <c>Torizo_Init</c> at $AA:C87F for area-zero definition $EEFF.</summary>
    private void InitializeBombTorizo(RoomEnemySlot torizo)
    {
        var state = new BombTorizoEnemyState(torizo);
        _bombTorizoState = state;

        // Boss bit four is shared by the Bomb and Golden Torizo encounters, but it is stored
        // per area. The room loader supplies the current area's accessor, keeping the actor
        // independent from save-memory layout and avoiding a room-number special case.
        if (_isAreaTorizoDefeated?.Invoke() == true)
        {
            torizo.Properties = torizo.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            return;
        }

        // These are the area-zero entries of $AA:C95F-$C973. Read the cartridge words
        // directly so a different supported ROM revision remains authoritative.
        torizo.XPosition = ReadWord(_bus!, 0xaac95f);
        torizo.YPosition = ReadWord(_bus!, 0xaac963);
        torizo.CurrentInstruction = ReadWord(_bus!, 0xaac967);
        torizo.Properties = unchecked((ushort)(
            torizo.Properties | ReadWord(_bus!, 0xaac96b)));
        torizo.ExtraProperties = torizo.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        torizo.XRadius = ReadWord(_bus!, 0xaac96f);
        torizo.YRadius = ReadWord(_bus!, 0xaac973);
        torizo.InstructionTimer = 1;
        torizo.Timer = 0;
        torizo.PaletteIndex = 0;
        torizo.SpritemapPointer = BombTorizoInitialExtendedSpritemap;

        state.Function = TorizoFunctionFalling;
        state.PreInstruction = TorizoPreInstructionIdle;
        state.HorizontalVelocity = 0;
        state.VerticalVelocity = 0x0100;

        // Init writes two shared Torizo target palettes and then the Bomb-specific body
        // palettes. This runtime currently exposes live CGRAM rather than the original
        // current/target pair, so install the final visible colors at the native indexes.
        LoadBombTorizoPalette();
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
        BombTorizoEnemyState state,
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

            default:
                throw new NotSupportedException(
                    $"Bomb Torizo main function $AA:{state.Function:X4} is not translated.");
        }
    }

    private void RunBombTorizoActiveState(
        RoomEnemySlot torizo,
        BombTorizoEnemyState state,
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
        BombTorizoEnemyState state,
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
            default:
                throw new NotSupportedException(
                    $"Bomb Torizo pre-instruction $AA:{state.PreInstruction:X4} is not translated.");
        }
    }

    private void ApplyBombTorizoGravity(
        RoomEnemySlot torizo,
        BombTorizoEnemyState state,
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
        BombTorizoEnemyState state,
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
        BombTorizoEnemyState state,
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
            LoadBombTorizoPalette();
        else
        {
            for (int color = 0; color < 16; color++)
            {
                _cgram!.SetColor(144 + color, 0x7fff);
                _cgram.SetColor(160 + color, 0x7fff);
            }
        }
    }

    private void BeginBombTorizoDeath(RoomEnemySlot torizo, BombTorizoEnemyState state)
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

    private void FinishBombTorizoDeath(BombTorizoEnemyState state)
    {
        _setAreaTorizoDefeated?.Invoke();
        state.BossBitSet = true;
        state.ItemDropRequested = true;
        LastBombTorizoMusicRequest = new BombTorizoMusicRequest(3, 8);
    }

    private static RoomLevelData RequireBombTorizoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Bomb Torizo movement requires the active room collision layer.");

    private BombTorizoEnemyState RequireBombTorizoState(RoomEnemySlot torizo) =>
        _bombTorizoState is not null && ReferenceEquals(_bombTorizoState.Slot, torizo)
            ? _bombTorizoState
            : throw new InvalidOperationException(
                $"Enemy slot {torizo.SlotIndex} has no initialized Bomb Torizo state.");
}
