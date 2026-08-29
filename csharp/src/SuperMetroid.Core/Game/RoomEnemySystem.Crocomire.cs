using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Even-valued indexes consumed by Crocomire's 21-entry fight-AI table.</summary>
public enum CrocomireFightFunction : ushort
{
    ResetAnimation = 0x00,
    StepForward = 0x02,
    Sleeping = 0x04,
    SteppingForward = 0x06,
    ProjectileAttack = 0x08,
    NearSpikeWallCharge = 0x0a,
    SteppingBack = 0x0c,
    BackingOffSpikeWall = 0x0e,
    RoarAndStepForwardUnused = 0x10,
    WaitingForFirstDamage = 0x12,
    WaitingForSecondDamage = 0x14,
    WaitingForSecondDamageUnused = 0x16,
    PowerBombCharge = 0x18,
    NearSpikeWallChargeUnused = 0x1a,
    ResetAnimationUnused = 0x1c,
    ChooseAttackUnused = 0x1e,
    StepForwardUnused = 0x20,
    MoveUntilSamusUnused = 0x22,
    MoveClawsUnused = 0x24,
    StepForwardVariantUnused = 0x26,
    MovingClawsUnused = 0x28,
}

/// <summary>
/// Named view of the six generic words in Crocomire's physical body slot. These names come
/// from the bank-$A4 disassembly; the raw flag word deliberately remains a <see cref="ushort"/>
/// because several bits are overloaded by unused authored fight states.
/// </summary>
public sealed class CrocomireEnemyState
{
    private readonly RoomEnemySlot _body;

    internal CrocomireEnemyState(RoomEnemySlot body) => _body = body;

    public RoomEnemySlot Body => _body;
    public RoomEnemySlot? Tongue { get; internal set; }

    public ushort DeathSequenceIndex
    {
        get => _body.VariableA;
        internal set => _body.VariableA = value;
    }

    public ushort FightFlags
    {
        get => _body.VariableB;
        internal set => _body.VariableB = value;
    }

    public CrocomireFightFunction FightFunction
    {
        get => (CrocomireFightFunction)_body.VariableC;
        internal set => _body.VariableC = (ushort)value;
    }

    public ushort StepCounter
    {
        get => _body.VariableD;
        internal set => _body.VariableD = value;
    }

    public ushort ReactionTimer
    {
        get => _body.VariableE;
        internal set => _body.VariableE = value;
    }

    public ushort ProjectileCounter
    {
        get => _body.VariableF;
        internal set => _body.VariableF = value;
    }
}

/// <summary>Literal fight-phase translation for enemy $DDBF and tongue $DDFF.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort CrocomireDefinition = 0xddbf;
    internal const ushort CrocomireTongueDefinition = 0xddff;

    private const ushort CrocomireInitialInstructionList = 0xbade;
    private const ushort CrocomireTongueInstructionList = 0xbe56;
    private const ushort CrocomireDeadInstructionList = 0xe1cc;
    private const ushort CrocomireBridgeCollapseInstructionList = 0xbfb0;
    private const ushort CrocomireTongueSleepInstructionList = 0xbf62;
    private const ushort CrocomireBridgeThreshold = 0x0640;
    private const ushort CrocomireSpikeWallThreshold = 0x0300;
    private const int CrocomireFightPaletteSource = 0xa4b89d;

    private CrocomireEnemyState? _crocomire;
    private CrocomireDeathState? _crocomireDeath;
    private readonly List<CrocomirePlmRequest> _crocomirePlmRequests = new();
    private ushort _crocomireCameraX;

    /// <summary>Debugger-visible Crocomire owner while the current room contains $DDBF.</summary>
    public CrocomireEnemyState? Crocomire => _crocomire;

    /// <summary>Typed WRAM extension used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath => _crocomireDeath;

    /// <summary>Hardcoded bank-$84 arena mutations published during the current frame.</summary>
    public IReadOnlyList<CrocomirePlmRequest> CrocomirePlmRequests => _crocomirePlmRequests;

    /// <summary>Delayed music publication from the current Crocomire frame.</summary>
    public CrocomireMusicRequest? LastCrocomireMusicRequest { get; private set; }

    /// <summary>The boss-specific item-drop request emitted after the skeleton collapses.</summary>
    public CrocomireDropRequest? LastCrocomireDropRequest { get; private set; }

    /// <summary>Literal BG2 scroll owned by Crocomire while its extended tilemap is active.</summary>
    public ushort CrocomireBg2HorizontalScroll { get; private set; }

    /// <summary>Literal BG2 vertical scroll owned by Crocomire while its extended tilemap is active.</summary>
    public ushort CrocomireBg2VerticalScroll { get; private set; }

    /// <summary>Most recent bank-$A4 library-two sound publication.</summary>
    public ushort? LastCrocomireSoundEffect { get; private set; }

    /// <summary>True after the $640 X threshold starts the native bridge-collapse phase.</summary>
    public bool CrocomireBridgeCollapseStarted { get; private set; }

    private void ResetCrocomireRoomState()
    {
        _crocomire = null;
        _crocomireDeath = null;
        _crocomirePlmRequests.Clear();
        _crocomireCameraX = 0;
        LastCrocomireSoundEffect = null;
        LastCrocomireMusicRequest = null;
        LastCrocomireDropRequest = null;
        CrocomireBg2HorizontalScroll = 0;
        CrocomireBg2VerticalScroll = 0;
        CrocomireBridgeCollapseStarted = false;
    }

    /// <summary>Ports <c>InitAI_Crocomire</c> at $A4:8A5A.</summary>
    private void InitializeCrocomire(RoomEnemySlot slot)
    {
        BossId = 6;
        var state = new CrocomireEnemyState(slot);
        _crocomire = state;
        _crocomireDeath = new CrocomireDeathState();
        ClearCrocomireBg2WorkingTilemap();

        if (_isAreaMiniBossDefeated?.Invoke() ?? false)
        {
            // The dead-room state keeps only Crocomire's skeleton display actor. Property
            // mask $7BFF and these radii/coordinates are literal writes at $A4:8AEA-$8B35.
            slot.Properties = unchecked((ushort)((slot.Properties & 0x7bff) | 0x0400));
            state.DeathSequenceIndex = 0x0054;
            InstallCrocomireInstructionList(slot, CrocomireDeadInstructionList);
            slot.XPosition = 0x0240;
            slot.YPosition = 0x0090;
            slot.XRadius = 0x0028;
            slot.YRadius = 0x001c;
            _setRoomScrollByte?.Invoke(0, 1);
            _setRoomScrollByte?.Invoke(1, 1);
            _setRoomScrollByte?.Invoke(2, 1);
            _setRoomScrollByte?.Invoke(3, 1);
            PublishCrocomirePlm(0x20, 0x03, 0xb753);
            PublishCrocomirePlm(0x1e, 0x03, 0xb753);
            PublishCrocomirePlm(0x61, 0x0b, 0xb747);
            TransferCrocomireBg2Words(0, 1024);
            return;
        }

        state.DeathSequenceIndex = 0;
        state.ReactionTimer = 0;
        state.FightFunction = CrocomireFightFunction.Sleeping;
        InstallCrocomireInstructionList(slot, CrocomireInitialInstructionList);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        _setRoomScrollByte?.Invoke(0, 0);
        _setRoomScrollByte?.Invoke(1, 0);

        // The initializer copies seventeen words, not sixteen: X starts at $20 and reaches
        // zero inclusively. Preserve that palette-boundary write because later fades compare
        // the exact target image produced by the cartridge.
        _cgram!.LoadFromBus(_bus!, 0xa4b8bd, colorCount: 17, destinationIndex: 160);
        _cgram.LoadFromBus(_bus!, 0xa4b8dd, colorCount: 17, destinationIndex: 208);
    }

    /// <summary>Ports <c>InitAI_CrocomireTongue</c> at $A4:F67A.</summary>
    private void InitializeCrocomireTongue(RoomEnemySlot slot)
    {
        if (_isAreaMiniBossDefeated?.Invoke() ?? false)
        {
            slot.Properties = unchecked((ushort)((slot.Properties & 0xdcff) | 0x0300));
            return;
        }

        InstallCrocomireInstructionList(slot, CrocomireTongueInstructionList);
        slot.ExtraProperties = unchecked((ushort)(slot.ExtraProperties | 0x0404));
        slot.VariableA = 23;
        slot.PaletteIndex = 0x0e00;
        if (_crocomire is not null)
            _crocomire.Tongue = slot;
    }

    /// <summary>Runs the live-fight phase of <c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    private void RunCrocomireMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level,
        ushort cameraX)
    {
        CrocomireEnemyState state = RequireCrocomire(slot);
        _crocomireCameraX = cameraX;

        if (state.DeathSequenceIndex == 0)
        {
            HandleCrocomireBridgeThreshold(state);
            // Main state zero always finishes through $A4:8B5B, including the frame in
            // which $8D5E changes the death index to two.
            UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        }
        else
        {
            RunCrocomireDeathSequence(state, samus);
        }

        HandleCrocomireInvisibleWall(slot, state, samus, controllerInput);
        ApplyCrocomireHurtPalette(slot);
    }

    /// <summary>
    /// Ports the state transition at $A4:8D5E. The individual bridge PLMs and subsequent
    /// melting sequence remain a separate translation boundary, but the actor/tongue state
    /// changes occur on the exact native X threshold.
    /// </summary>
    private void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        HandleCrocomireBridgeApproach(body);
        if (unchecked((short)(body.XPosition - CrocomireBridgeThreshold)) < 0)
            return;

        CrocomireBridgeCollapseStarted = true;
        state.DeathSequenceIndex = 2;
        InstallCrocomireInstructionList(body, CrocomireBridgeCollapseInstructionList);
        body.Properties = unchecked((ushort)(body.Properties | 0x8000));
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        state.StepCounter = 0x0800;
        body.YRadius = 16;
        LastCrocomireSoundEffect = 0x003b;
        CrocomireDeathState death = RequireCrocomireDeath();
        death.BridgeFragmentCursor = 0;
        death.AcidSmokeTimer = 1;
        death.AcidSoundTimer = 1;

        PublishCrocomireBridgeCollapsePlms();

        if (state.Tongue is { } tongue)
        {
            tongue.InstructionTimer = 0x7fff;
            tongue.CurrentInstruction = CrocomireTongueSleepInstructionList;
            tongue.Properties = tongue.Properties.With(EnemyProperties.Invisible);
        }
    }

    /// <summary>Ports the artificial left-side wall in <c>$A4:8C95</c>.</summary>
    private void HandleCrocomireInvisibleWall(
        RoomEnemySlot body,
        CrocomireEnemyState state,
        SamusState? samus,
        ushort controllerInput)
    {
        if (samus is null || state.DeathSequenceIndex != 0)
            return;
        ushort leftEdge = unchecked((ushort)(
            body.XPosition - body.XRadius - samus.Kinematics.XRadius));
        if (unchecked((short)(leftEdge - samus.XPosition)) >= 0)
            return;

        ResolveNormalEnemyTouch(body, samus, controllerInput);
        samus.XPosition = leftEdge;
        samus.Kinematics.ExtraXDisplacement = unchecked((ushort)-4);
        samus.Kinematics.ExtraYDisplacement = 0xffff;
    }

    /// <summary>Ports Crocomire's eight-color body hurt flash at $A4:8CCB.</summary>
    private void ApplyCrocomireHurtPalette(RoomEnemySlot body)
    {
        bool white = body.FlashTimer != 0 && (_randomEnemyCounter & 2) != 0;
        for (int color = 0; color < 8; color++)
        {
            ushort value = white
                ? (ushort)0x7fff
                : ReadWord(_bus!, CrocomireFightPaletteSource + color * 2);
            _cgram!.SetColor(112 + color, value);
        }
    }

    private static void InstallCrocomireInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private CrocomireEnemyState RequireCrocomire(RoomEnemySlot slot) =>
        _crocomire is { } state && ReferenceEquals(state.Body, slot)
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Crocomire body state.");

    private CrocomireDeathState RequireCrocomireDeath() =>
        _crocomireDeath ?? throw new InvalidOperationException(
            "Crocomire death extension is not initialized for the current room.");
}
