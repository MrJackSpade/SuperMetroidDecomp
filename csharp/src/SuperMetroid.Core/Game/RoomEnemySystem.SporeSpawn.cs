using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five bank-$A5 functions stored in Spore Spawn's native variable A.
/// </summary>
public enum SporeSpawnFunction : ushort
{
    /// <summary><c>RTS_A5EB1A</c> at <c>$A5:EB1A</c>; performs no main-AI movement while instruction lists control the idle or open phase.</summary>
    Idle = 0xeb1a,
    /// <summary><c>Function_SporeSpawn_Descent</c> at <c>$A5:EB1B</c>; lowers the body one room pixel per AI update until Y reaches 624 and installs the fight-start list.</summary>
    Descending = 0xeb1b,
    /// <summary><c>Function_SporeSpawn_Moving</c> at <c>$A5:EB52</c>; computes the phase-driven body sway about its spawn center and updates stalk segment positions.</summary>
    Moving = 0xeb52,
    /// <summary><c>Function_SporeSpawn_SetupDeath</c> at <c>$A5:EB9B</c>; computes the fixed death-drift angle and velocity toward room position (128,624), without moving the body.</summary>
    SetupDeath = 0xeb9b,
    /// <summary><c>Function_SporeSpawn_Dying</c> at <c>$A5:EBEE</c>; applies death drift and ceiling dust, returning to idle when both axis distances from (128,624) are below eight pixels.</summary>
    Dying = 0xebee,
}

/// <summary>
/// Debugger-visible state that the cartridge stores beyond Spore Spawn's common enemy words.
/// Naming these fields keeps the boss's angle, stalk anchor, and death velocity from being
/// hidden in unrelated generic variables while retaining their native 16-bit arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    private readonly Bgr555[] _targetPalette = new Bgr555[256];
    private readonly bool[] _targetPaletteWritten = new bool[256];

    internal SporeSpawnEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Live common enemy slot owned by the room enemy system; shared by movement, combat, instruction, and stalk-projectile owners.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Current bank-$A5 main-AI pointer, corresponding to native variable A at WRAM $0FA8.</summary>
    public SporeSpawnFunction Function { get; internal set; }
    /// <summary>Fixed stalk interpolation anchor X in room pixels, captured from the spawn body; native <c>stalkXOrigin</c> at $7E:7808.</summary>
    public ushort StalkAnchorX { get; internal set; }
    /// <summary>Fixed stalk interpolation anchor Y in room pixels, initialized 72 pixels above the spawn body; native <c>stalkYOrigin</c> at $7E:780A.</summary>
    public ushort StalkAnchorY { get; internal set; }
    /// <summary>Original body X in room pixels, used as the horizontal sway center; native <c>XOrigin</c> at WRAM $0FAC.</summary>
    public ushort MovementCenterX { get; internal set; }
    /// <summary>Original body Y in room pixels, retained before the live spawn is raised 128 pixels; native <c>YOrigin</c> at WRAM $0FAE.</summary>
    public ushort MovementCenterY { get; internal set; }
    /// <summary>Byte-wrapped sway phase stored in a word at $7E:7814, with 256 units per cycle; X samples cosine and Y samples the doubled, shifted phase.</summary>
    public ushort Angle { get; internal set; }
    /// <summary>Horizontal sway amplitude in room pixels, native $7E:7816; vertical amplitude is this value minus sixteen.</summary>
    public ushort MaximumXRadius { get; internal set; }
    /// <summary>Signed angular increment per movement update, native $7E:7818; hits may reverse its sign or increase its magnitude from one to two below 400 health, with movement using the low byte.</summary>
    public ushort AngleDelta { get; internal set; }
    /// <summary>Native $7E:9000 emitter gate: zero permits ceiling spawner countdowns, while nonzero suppresses generation during protected instruction phases.</summary>
    public ushort SporeGenerationFlag { get; internal set; }
    /// <summary>Native $7E:801E one-hit guard for an open phase; nonzero prevents repeated direction reversal and close-list installation until the clear-damaged opcode resets it.</summary>
    public ushort DamagedFlag { get; internal set; }
    /// <summary>Health value last recorded by the guarded damaging-hit palette update, native $7E:8800; used to avoid reloading an unchanged health palette.</summary>
    public ushort PreviousHealth { get; internal set; }
    /// <summary>Fixed byte-sized death-drift direction toward (128,624), stored at $7E:8806; zero is right, $40 up, $80 left, and $C0 down.</summary>
    public ushort DeathAngle { get; internal set; }
    /// <summary>Unsigned horizontal 16.16 pixels-per-update magnitude combining native death speed/subspeed words $7E:8010/$8012; <see cref="DeathAngle"/> supplies the movement sign.</summary>
    public int DeathXVelocityMagnitude { get; internal set; }
    /// <summary>Unsigned vertical 16.16 pixels-per-update magnitude combining native death speed/subspeed words $7E:8014/$8016; <see cref="DeathAngle"/> supplies the movement sign.</summary>
    public int DeathYVelocityMagnitude { get; internal set; }
    /// <summary>Whether the live fight's scrolling-finished hook still enforces the arena's minimum camera Y; cleared at death and never installed for an already defeated load.</summary>
    public bool ScrollClampHookActive { get; internal set; }
    /// <summary>Host guard recording that zero health has already triggered projectile cleanup, the defeated flag, death instructions, and the ceiling-crumble request.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the death instruction has published its sixteen boss item-drop attempts; separate from individual destroyed-spore drops and actual drop-pool allocation success.</summary>
    public bool DeathDropRequested { get; internal set; }
    /// <summary>Defeated-area miniboss flag sampled at room initialization; selects the solid dead body and cleared ceiling instead of the live fight setup.</summary>
    public bool LoadedAsDefeated { get; internal set; }

    internal void WriteTargetColor(int index, Bgr555 value)
    {
        _targetPalette[index] = value;
        _targetPaletteWritten[index] = true;
    }

    internal void ConsumeTargetColors(Action<int, Bgr555> write)
    {
        for (int index = 0; index < _targetPalette.Length; index++)
        {
            if (!_targetPaletteWritten[index]) continue;
            write(index, _targetPalette[index]);
            _targetPaletteWritten[index] = false;
        }
    }
}

/// <summary>One hardcoded bank-$84 ceiling mutation published by Spore Spawn.</summary>
/// <param name="BlockX">Zero-based room block column in 16-pixel units; the native ceiling request uses seven.</param>
/// <param name="BlockY">Zero-based room block row in 16-pixel units; the native ceiling request uses thirty.</param>
/// <param name="Header">Bank-relative PLM header offset in bank $84, selecting the ceiling clear or crumble operation.</param>
public readonly record struct SporeSpawnPlmRequest(byte BlockX, byte BlockY, PlmHeaderId Header);

/// <summary>Frame-local marker for one item-drop attempt from a destroyed spore or the boss's death instruction.</summary>
/// <remarks>Contains no position or table selector: the publisher also invokes drop spawning immediately, using stalk header $DF7F for a spore or body header $DF3F for one of the sixteen death drops.</remarks>
public readonly record struct SporeSpawnDropRequest();

public sealed partial class RoomEnemySystem
{

    private const ushort SporeSpawnDeathCenterX = 128;
    private const ushort SporeSpawnDeathCenterY = 624;
    private const ushort SporeSpawnCeilingY = 560;

    private readonly List<SporeSpawnDropRequest> _sporeSpawnDropRequests = new();

    /// <summary>Frame-local hardcoded ceiling PLM request.</summary>
    public SporeSpawnPlmRequest? LastSporeSpawnPlm { get; private set; }

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
    /// <param name="camera">Live room camera whose layer-one Y is raised to the arena minimum of 464 room pixels when the hook is active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
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
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            Bgr555 value = (TileArtwork?.SporeSpawnColors ?? throw new InvalidDataException(
                "Spore Spawn spore palette requires installed artwork.")).ResolveSpore(color);
            state.WriteTargetColor(SporeSpawnColorRomData.SporeDestination + color, value);
            _cgram!.SetColor(SporeSpawnColorRomData.SporeDestination + color, value);
        }

        // SpawnEprojWithGfx searches the eighteen-slot pool downward. Four calls therefore
        // occupy physical slots 17..14, which SporeSpawn_Func_5 later addresses directly.
        for (ushort argument = 0; argument < 4; argument++)
            SpawnSporeSpawnStalk(body, argument);

        state.LoadedAsDefeated = RequireAreaMiniBossDefeated();
        if (state.LoadedAsDefeated)
        {
            body.CurrentInstruction = SporeSpawnInstructionProgramDefinitions.InitialDead;
            body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
            UpdateSporeSpawnStalks(state);
            PublishSporeSpawnPlm(header: PlmHeaderId.ClearSporeSpawnCeiling);
            return;
        }

        body.CurrentInstruction = SporeSpawnInstructionProgramDefinitions.InitialAlive;
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
        SporeSpawnEnemyState state)
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
                RunSporeSpawnDying(body, state);
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
            body.CurrentInstruction = SporeSpawnInstructionProgramDefinitions.FightStarted;
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
            ReadEightBitNegativeSineProduct(
                unchecked((ushort)(2 * (state.Angle - 64))),
                unchecked((ushort)(state.MaximumXRadius - 16)))));

        // Only the low byte of the signed delta participates, and the resulting angle is
        // truncated to a byte before being stored back to the native word.
        state.Angle = unchecked((byte)(state.Angle + (byte)state.AngleDelta));
    }

    /// <summary>Ports <c>SporeSpawn_Func_3</c> at $A5:EB9B.</summary>
    private static void SetupSporeSpawnDeathVelocity(RoomEnemySlot body, SporeSpawnEnemyState state)
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
        SporeSpawnEnemyState state)
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
        // $A5:E9F7 tests the 16-bit NMI_FrameCounter, not the 8-bit $05B5 copy.
        if ((_enemyFrameNmiFrameCounter & 0x0f) == 0)
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

    private void PublishSporeSpawnPlm(PlmHeaderId header) =>
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
