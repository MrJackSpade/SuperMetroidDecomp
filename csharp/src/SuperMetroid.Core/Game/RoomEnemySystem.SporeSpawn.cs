using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five bank-$A5 functions stored in Spore Spawn's native variable A.
/// </summary>
public enum SporeSpawnFunction : ushort
{
    Idle = 0xeb1a,
    Descending = 0xeb1b,
    Moving = 0xeb52,
    SetupDeath = 0xeb9b,
    Dying = 0xebee,
}

/// <summary>
/// Debugger-visible state that the cartridge stores beyond Spore Spawn's common enemy words.
/// Naming these fields keeps the boss's angle, stalk anchor, and death velocity from being
/// hidden in unrelated generic variables while retaining their native 16-bit arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    private readonly ushort[] _targetPalette = new ushort[256];

    internal SporeSpawnEnemyState(RoomEnemySlot body) => Body = body;

    public RoomEnemySlot Body { get; }
    public SporeSpawnFunction Function { get; internal set; }
    public ushort StalkAnchorX { get; internal set; }
    public ushort StalkAnchorY { get; internal set; }
    public ushort MovementCenterX { get; internal set; }
    public ushort MovementCenterY { get; internal set; }
    public ushort Angle { get; internal set; }
    public ushort MaximumXRadius { get; internal set; }
    public ushort AngleDelta { get; internal set; }
    public ushort SporeGenerationFlag { get; internal set; }
    public ushort DamagedFlag { get; internal set; }
    public ushort PreviousHealth { get; internal set; }
    public ushort DeathAngle { get; internal set; }
    public int DeathXVelocityMagnitude { get; internal set; }
    public int DeathYVelocityMagnitude { get; internal set; }
    public bool ScrollClampHookActive { get; internal set; }
    public bool DeathStarted { get; internal set; }
    public bool DeathDropRequested { get; internal set; }
    public bool LoadedAsDefeated { get; internal set; }

    /// <summary>
    /// The global target-palette words written by $A5:EA2A/$E91C. The runtime currently
    /// presents completed room fades directly in CGRAM, but retaining this buffer makes the
    /// cartridge's separate target/current ownership inspectable and testable.
    /// </summary>
    public ReadOnlyMemory<ushort> TargetPalette => _targetPalette;
    internal Span<ushort> MutableTargetPalette => _targetPalette;
}

/// <summary>One hardcoded bank-$84 ceiling mutation published by Spore Spawn.</summary>
public readonly record struct SporeSpawnPlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>One destroyed spore's request to use enemy $DF3F's item-drop table.</summary>
public readonly record struct SporeSpawnDropRequest(ushort X, ushort Y);

public sealed partial class RoomEnemySystem
{
    public const ushort SporeSpawnDefinition = 0xdf3f;

    private const ushort SporeSpawnInitialDeadInstruction = 0xe6b9;
    private const ushort SporeSpawnInitialAliveInstruction = 0xe6c7;
    private const ushort SporeSpawnFightStartedInstruction = 0xe6d5;
    private const ushort SporeSpawnDeathInstruction = 0xe77d;
    private const ushort SporeSpawnDeathCenterX = 128;
    private const ushort SporeSpawnDeathCenterY = 624;
    private const ushort SporeSpawnCeilingY = 560;
    private const int SporeSpawnInitialPaletteSource = 0xa5e359;
    private const int SporeSpawnInitialPaletteDestination = 240;

    private readonly List<SporeSpawnDropRequest> _sporeSpawnDropRequests = new();

    /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
    public SporeSpawnEnemyState? SporeSpawn => _sporeSpawn;

    /// <summary>Frame-local hardcoded ceiling PLM request.</summary>
    public SporeSpawnPlmRequest? LastSporeSpawnPlm { get; private set; }

    /// <summary>Frame-local drops requested by destroyed free-floating spores.</summary>
    public IReadOnlyList<SporeSpawnDropRequest> SporeSpawnDropRequests =>
        _sporeSpawnDropRequests;

    /// <summary>Last library-two sound requested by the boss or one of its instructions.</summary>
    public ushort? LastSporeSpawnSoundEffectLibrary2 { get; private set; }

    private void ResetSporeSpawnRoomState()
    {
        _sporeSpawn = null;
        LastSporeSpawnPlm = null;
        LastSporeSpawnSoundEffectLibrary2 = null;
        _sporeSpawnDropRequests.Clear();
    }

    private void BeginSporeSpawnFrame()
    {
        LastSporeSpawnPlm = null;
        LastSporeSpawnSoundEffectLibrary2 = null;
        _sporeSpawnDropRequests.Clear();
    }

    /// <summary>
    /// Runs the global scrolling-finished callback installed by live Spore Spawn at
    /// <c>$A5:EAD7-$EADA</c>. Bank $90 invokes it after both ordinary camera axes have
    /// finished and before background streaming observes the final layer-one words.
    /// </summary>
    public void RunScrollingFinishedHook(ScrollBoundaryCamera camera)
    {
        ArgumentNullException.ThrowIfNull(camera);
        if (_sporeSpawn?.ScrollClampHookActive != true)
            return;

        // `$90:9589` loads $01D0, compares it with layer1_y_pos, and returns on BCC.
        // Equality deliberately performs the harmless native store; a larger camera Y is
        // already below the arena ceiling and is therefore left untouched.
        if (SporeSpawnScrollingHooks.FightMinimumLayerOneY < camera.YPosition)
            return;

        camera.SetLayerOneYFromScrollingFinishedHook(
            SporeSpawnScrollingHooks.FightMinimumLayerOneY);
    }

    private SporeSpawnEnemyState RequireSporeSpawnState(RoomEnemySlot slot)
    {
        SporeSpawnEnemyState state = _sporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn main AI ran without an initialized boss extension.");
        if (!ReferenceEquals(state.Body, slot))
            throw new InvalidOperationException("Spore Spawn extension belongs to another enemy slot.");
        return state;
    }

    /// <summary>Ports <c>SporeSpawn_Init</c> at $A5:EA2A.</summary>
    private void InitializeSporeSpawn(RoomEnemySlot body)
    {
        if (_sporeSpawn is not null)
            throw new InvalidDataException("A room population contains more than one Spore Spawn body.");

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            StalkAnchorX = body.XPosition,
            StalkAnchorY = unchecked((ushort)(body.YPosition - 72)),
            MovementCenterX = body.XPosition,
            MovementCenterY = body.YPosition,
        };
        _sporeSpawn = state;

        // Native writes these sixteen colors to target-palette row F. Room loading currently
        // presents completed fades directly, so install the identical final words in CGRAM
        // while retaining the independent target copy above.
        for (int color = 0; color < 16; color++)
        {
            ushort value = ReadWord(_bus!, SporeSpawnInitialPaletteSource + color * 2);
            state.MutableTargetPalette[SporeSpawnInitialPaletteDestination + color] = value;
            _cgram!.SetColor(SporeSpawnInitialPaletteDestination + color, value);
        }

        // SpawnEprojWithGfx searches the eighteen-slot pool downward. Four calls therefore
        // occupy physical slots 17..14, which SporeSpawn_Func_5 later addresses directly.
        for (ushort argument = 0; argument < 4; argument++)
            SpawnSporeSpawnStalk(body, argument);

        state.LoadedAsDefeated = RequireAreaMiniBossDefeated();
        if (state.LoadedAsDefeated)
        {
            body.CurrentInstruction = SporeSpawnInitialDeadInstruction;
            body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
            UpdateSporeSpawnStalks(state);
            PublishSporeSpawnPlm(header: RoomPlmHeaders.ClearSporeSpawnCeiling);
            return;
        }

        body.CurrentInstruction = SporeSpawnInitialAliveInstruction;
        body.YPosition = unchecked((ushort)(body.YPosition - 128));

        // `flag_process_all_enemies = FFFF` is a global bank-$A0 activity override, not an
        // enemy property. Preserve that distinction so all room actors use the same scan.
        _processAllEnemies = true;
        state.ScrollClampHookActive = true;
        for (ushort argument = 0; argument < 4; argument++)
            SpawnSporeSpawnSpawner(body, argument);
        UpdateSporeSpawnStalks(state);
    }

    /// <summary>Dispatches <c>SporeSpawn_Main</c> at $A5:EB13.</summary>
    private void RunSporeSpawnMain(
        RoomEnemySlot body,
        SporeSpawnEnemyState state,
        byte nmiFrameCounter8)
    {
        switch (state.Function)
        {
            case SporeSpawnFunction.Idle:
                return;

            case SporeSpawnFunction.Descending:
                RunSporeSpawnDescent(body, state);
                return;

            case SporeSpawnFunction.Moving:
                RunSporeSpawnMovement(body, state);
                return;

            case SporeSpawnFunction.SetupDeath:
                SetupSporeSpawnDeathVelocity(body, state);
                return;

            case SporeSpawnFunction.Dying:
                RunSporeSpawnDying(body, state, nmiFrameCounter8);
                return;

            default:
                throw new InvalidDataException(
                    $"Spore Spawn function $A5:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>SporeSpawn_Func_1</c> at $A5:EB1B.</summary>
    private void RunSporeSpawnDescent(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.YPosition = unchecked((ushort)(body.YPosition + 1));
        if (unchecked((short)(body.YPosition - SporeSpawnDeathCenterY)) >= 0)
        {
            body.CurrentInstruction = SporeSpawnFightStartedInstruction;
            body.InstructionTimer = 1;
        }

        state.MaximumXRadius = 48;
        state.AngleDelta = 1;
        state.Angle = 192;
    }

    /// <summary>Ports the table-backed ellipse in <c>SporeSpawn_Func_2</c> at $A5:EB52.</summary>
    private void RunSporeSpawnMovement(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.XPosition = unchecked((ushort)(
            state.MovementCenterX +
            ReadEightBitCosineProduct(state.Angle, state.MaximumXRadius)));
        body.YPosition = unchecked((ushort)(
            state.MovementCenterY +
            ReadEightBitSineProduct(
                unchecked((ushort)(2 * (state.Angle - 64))),
                unchecked((ushort)(state.MaximumXRadius - 16)))));

        // Only the low byte of the signed delta participates, and the resulting angle is
        // truncated to a byte before being stored back to the native word.
        state.Angle = unchecked((byte)(state.Angle + (byte)state.AngleDelta));
    }

    /// <summary>Ports <c>SporeSpawn_Func_3</c> at $A5:EB9B.</summary>
    private void SetupSporeSpawnDeathVelocity(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        byte direction = unchecked((byte)(64 - CalculateCartridgeAngle(
            unchecked((short)(SporeSpawnDeathCenterX - body.XPosition)),
            unchecked((short)(SporeSpawnDeathCenterY - body.YPosition)))));
        state.DeathAngle = direction;
        state.DeathXVelocityMagnitude = ReadUnsignedSineMagnitudeProduct(direction, 1, 0x40);
        state.DeathYVelocityMagnitude = ReadUnsignedSineMagnitudeProduct(direction, 1, 0x80);
    }

    /// <summary>Ports <c>SporeSpawn_Func_4</c> at $A5:EBEE.</summary>
    private void RunSporeSpawnDying(
        RoomEnemySlot body,
        SporeSpawnEnemyState state,
        byte nmiFrameCounter8)
    {
        int xDisplacement = ((state.DeathAngle + 64) & 0x80) != 0
            ? -state.DeathXVelocityMagnitude
            : state.DeathXVelocityMagnitude;
        int yDisplacement = ((state.DeathAngle + 128) & 0x80) != 0
            ? -state.DeathYVelocityMagnitude
            : state.DeathYVelocityMagnitude;
        (body.XPosition, body.XSubposition) = AddSporeSpawnFixed(
            body.XPosition,
            body.XSubposition,
            xDisplacement);
        (body.YPosition, body.YSubposition) = AddSporeSpawnFixed(
            body.YPosition,
            body.YSubposition,
            yDisplacement);

        if (WrappedMagnitude(unchecked((ushort)(body.XPosition - SporeSpawnDeathCenterX))) < 8 &&
            WrappedMagnitude(unchecked((ushort)(body.YPosition - SporeSpawnDeathCenterY))) < 8)
        {
            state.Function = SporeSpawnFunction.Idle;
        }

        UpdateSporeSpawnStalks(state);
        if ((nmiFrameCounter8 & 0x0f) == 0)
        {
            ushort random = _nextRandom!();
            _ = SpawnRoomSpriteObject(
                unchecked((ushort)((random & 0x003f) + 96)),
                unchecked((ushort)(((random & 0x0f00) >> 8) + 480)),
                RoomSpriteObjectKind.DustCloud,
                graphicsIndex: 0);
        }
    }

    /// <summary>Ports the raw slot interpolation in <c>SporeSpawn_Func_5</c> at $A5:EC49.</summary>
    private void UpdateSporeSpawnStalks(SporeSpawnEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnDeathCenterX,
            anchor: state.StalkAnchorX,
            end: body.XPosition,
            horizontal: true);
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnCeilingY,
            anchor: state.StalkAnchorY,
            end: unchecked((ushort)(body.YPosition - 40)),
            horizontal: false);
    }

    private void WriteSporeSpawnStalkAxis(
        ushort firstPosition,
        ushort anchor,
        ushort end,
        bool horizontal)
    {
        ushort signedDelta = unchecked((ushort)(end - anchor));
        bool negative = (signedDelta & 0x8000) != 0;
        ushort magnitude = negative
            ? unchecked((ushort)(anchor - end))
            : signedDelta;
        ushort quarter = unchecked((ushort)(magnitude >> 2));
        ushort half = unchecked((ushort)(magnitude >> 1));
        ushort threeQuarters = unchecked((ushort)(quarter + half));
        Span<ushort> positions = stackalloc ushort[4]
        {
            firstPosition,
            unchecked((ushort)(anchor + (negative ? -quarter : quarter))),
            unchecked((ushort)(anchor + (negative ? -half : half))),
            unchecked((ushort)(anchor + (negative ? -threeQuarters : threeQuarters))),
        };

        // The boss writes physical projectile slots 14,15,16,17 in this order even though
        // its four initializer calls allocated them in reverse order.
        for (int segment = 0; segment < positions.Length; segment++)
        {
            RoomEnemyProjectileSlot stalk = _enemyProjectiles[14 + segment];
            if (horizontal)
                stalk.XPosition = positions[segment];
            else
                stalk.YPosition = positions[segment];
        }
    }

    private void PublishSporeSpawnPlm(ushort header) =>
        LastSporeSpawnPlm = new SporeSpawnPlmRequest(7, 30, header);

    private static (ushort Position, ushort Subposition) AddSporeSpawnFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }
}
