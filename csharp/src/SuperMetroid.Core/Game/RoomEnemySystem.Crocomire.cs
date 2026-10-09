using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Even-valued indexes consumed by Crocomire's 21-entry fight-AI table.</summary>
public enum CrocomireFightFunction : ushort
{
    /// <summary>Table offset $00 selects <c>$A4:86DE FightAI_Crocomire_0_LockUp_SetInitialInstList</c>, which restarts the initial animation with timer one.</summary>
    ResetAnimation = 0x00,
    /// <summary>Table offset $02 selects <c>$A4:86E8 FightAI_Crocomire_2_StepForwardUntilOnScreen_StepForward</c>, installing the forward-step list and returning the fight index to sleeping.</summary>
    StepForward = 0x02,
    /// <summary>Table offset $04 selects <c>$A4:86F2 FightAI_Crocomire_4_Asleep</c>, waking into the first-damage wait when Samus is within 224 horizontal pixels.</summary>
    Sleeping = 0x04,
    /// <summary>Table offset $06 selects <c>$A4:8717 FightAI_Crocomire_6_SteppingForward</c>, responding to queued damage steps or selecting a charge below room X=$300.</summary>
    SteppingForward = 0x06,
    /// <summary>Table offset $08 selects <c>$A4:876C FightAI_Crocomire_8_ProjectileAttack</c>, spawning shots with successive even parameters through $12 unless damage interrupts with a backward step.</summary>
    ProjectileAttack = 0x08,
    /// <summary>Table offset $0A selects <c>$A4:87B2 FightAI_Crocomire_A_NearSpikeWallCharge</c>, retaining the charge list until a damage latch selects backward stepping.</summary>
    NearSpikeWallCharge = 0x0a,
    /// <summary>Table offset $0C selects <c>$A4:87CA FightAI_Crocomire_C_SteppingBack</c>, consuming one queued step per instruction dispatch before returning to forward stepping.</summary>
    SteppingBack = 0x0c,
    /// <summary>Table offset $0E selects <c>$A4:87E9 FightAI_Crocomire_E_BackOffFromSpikeWall</c>, continuing the retreat until room X reaches $300.</summary>
    BackingOffSpikeWall = 0x0e,
    /// <summary>Unused offset $10 selects <c>$A4:87FB UNUSED_FightAI_Crocomire_10_RoarAndStepForwards_A487FB</c>, installing a roar and selecting forward stepping.</summary>
    RoarAndStepForwardUnused = 0x10,
    /// <summary>Table offset $12 selects <c>$A4:8812 FightAI_Crocomire_12_WaitForFirstDamage</c>, looping the roar until damage installs a backward step and advances to the second-damage wait.</summary>
    WaitingForFirstDamage = 0x12,
    /// <summary>Table offset $14 selects <c>$A4:8836 FightAI_Crocomire_14_WaitForSecondDamage</c>, looping the roar until a second damage latch enters ordinary backward stepping.</summary>
    WaitingForSecondDamage = 0x14,
    /// <summary>Unused offset $16 selects <c>$A4:885A UNUSED_FightAI_Crocomire_16_WaitForSecondDamage_A4885A</c>, a separate native copy of the offset-$14 damage wait.</summary>
    WaitingForSecondDamageUnused = 0x16,
    /// <summary>Table offset $18 selects <c>$A4:887E FightAI_Crocomire_18_PowerBombedCharge</c>, decrementing the reaction step count until fewer than two remain, then restoring forward stepping.</summary>
    PowerBombCharge = 0x18,
    /// <summary>Unused offset $1A selects <c>$A4:889A UNUSED_FightAI_Crocomire_1A_DoNearSpikeWallCharge_A4889A</c>, converting damage into a one-step retreat or otherwise installing the charge roar.</summary>
    NearSpikeWallChargeUnused = 0x1a,
    /// <summary>Unused offset $1C selects <c>$A4:88D2 FightAI_Crocomire_1C_UnusedSequence_SetInitialInstList</c>, restarting art with a 32-count setup for the unused attack branch.</summary>
    ResetAnimationUnused = 0x1c,
    /// <summary>Unused offset $1E selects <c>$A4:891B UNUSED_FightAI_Crocomire_1E_ChooseForwardMovingAttack_A4891B</c>, choosing a charge or move-until-claw-contact branch from fight bit $0100.</summary>
    ChooseAttackUnused = 0x1e,
    /// <summary>Unused offset $20 selects <c>$A4:8940 UNUSED_FightAI_Crocomire_20_DoNothingAndStepForward_A48940</c>, retaining initial art until the step count reaches zero and begins the claw sequence.</summary>
    StepForwardUnused = 0x20,
    /// <summary>Unused offset $22 selects <c>$A4:895E UNUSED_FightAI_Crocomire_22_MoveForwardUntilHitSamus_A4895E</c>, branching on room X=$2A0 and claw-contact bit $4000; one native branch selects the unwritten $2A entry.</summary>
    MoveUntilSamusUnused = 0x22,
    /// <summary>Unused offset $24 selects <c>$A4:89A8 UNUSED_FightAI_Crocomire_24_MoveClaws_StepForward_A489A8</c>, counting down claw cycles while resetting the tongue word and publishing fight bit $0400.</summary>
    MoveClawsUnused = 0x24,
    /// <summary>Unused offset $26 selects <c>$A4:89DE UNUSED_FightAI_Crocomire_26_StepForward_A489DE</c>, adjusting attack flags and installing a forward step before the moving-claws branch.</summary>
    StepForwardVariantUnused = 0x26,
    /// <summary>Unused offset $28 selects <c>$A4:89F9 UNUSED_FightAI_Crocomire_28_MovingClaws_A489F9</c>, consuming claw-contact cycles or restoring the saved fight index from the projectile word.</summary>
    MovingClawsUnused = 0x28,
}

/// <summary>
/// Named view of the six generic words in Crocomire's physical body slot. These names come
/// from the bank-$A4 disassembly; the raw flag word deliberately remains a <see cref="ushort"/>
/// because several bits are overloaded by unused authored fight states.
/// </summary>
public sealed class CrocomireEnemyState
{
    internal CrocomireEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical $DDBF body record owning the fight/death state words and room-pixel coordinates; BG2 body artwork follows this actor.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Physical $DDFF tongue record, attached during initialization; bridge collapse sleeps and hides it, while later melting phases reuse its actor.</summary>
    public RoomEnemySlot? Tongue { get; internal set; }

    /// <summary>Native variable A: even byte offset into the bank-$A4 main/death dispatcher; zero runs the fight/bridge checks, two begins collapse, and $54 displays an already-defeated corpse.</summary>
    public ushort DeathSequenceIndex
    {
        get => Body.VariableA;
        internal set => Body.VariableA = value;
    }

    /// <summary>Native variable B: raw fight latches, including damage bit $0800 and a saturating low-nibble hit count; unused attack branches assign additional overlapping bit meanings.</summary>
    public ushort FightFlags
    {
        get => Body.VariableB;
        internal set => Body.VariableB = value;
    }

    /// <summary>Native variable C: even byte offset into the 21-word table at $A4:86B3, consumed when instruction $A4:86A6 runs rather than once per main-AI frame.</summary>
    public CrocomireFightFunction FightFunction
    {
        get => (CrocomireFightFunction)Body.VariableC;
        internal set => Body.VariableC = (ushort)value;
    }

    /// <summary>Native variable D: queued backward-step count during combat; death reuses it as a frame countdown, rumble-table offset, or 8.8 vertical acceleration word.</summary>
    public ushort StepCounter
    {
        get => Body.VariableD;
        internal set => Body.VariableD = value;
    }

    /// <summary>Native variable E: shot/power-bomb reaction word initialized to ten; death reuses it for 8.8 vertical speed or the fractional low word of skeleton 16.16 motion.</summary>
    public ushort ReactionTimer
    {
        get => Body.VariableE;
        internal set => Body.VariableE = value;
    }

    /// <summary>Native variable F: even projectile parameter advancing by two through $12, or saved fight index in unused claw AI; death reuses it for subposition or the whole word paired with <see cref="ReactionTimer"/>.</summary>
    public ushort ProjectileCounter
    {
        get => Body.VariableF;
        internal set => Body.VariableF = value;
    }
}

/// <summary>Literal fight-phase translation for enemy $DDBF and tongue $DDFF.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort CrocomireDefinition = 0xddbf;
    internal const ushort CrocomireTongueDefinition = 0xddff;

    private const ushort CrocomireBridgeThreshold = 0x0640;
    private const ushort CrocomireSpikeWallThreshold = 0x0300;
    private readonly List<CrocomirePlmRequest> _crocomirePlmRequests = new();
    private ushort _crocomireCameraX;

    /// <summary>Debugger-visible Crocomire owner while the current room contains $DDBF.</summary>
    public CrocomireEnemyState? Crocomire { get; private set; }

    /// <summary>Typed WRAM extension used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath { get; private set; }

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
        Crocomire = null;
        CrocomireDeath = null;
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
        Crocomire = state;
        CrocomireDeath = new CrocomireDeathState();
        ClearCrocomireBg2WorkingTilemap();

        if (RequireAreaMiniBossDefeated())
        {
            // The dead-room state keeps only Crocomire's skeleton display actor. Property
            // mask $7BFF and these radii/coordinates are literal writes at $A4:8AEA-$8B35.
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.SolidToSamus |
                    EnemyProperties.IgnoreSamusCollision,
                EnemyProperties.IgnoreSamusCollision);
            state.DeathSequenceIndex = 0x0054;
            InstallCrocomireInstructionList(
                slot,
                CrocomireInstructionProgramDefinitions.Dead);
            slot.XPosition = 0x0240;
            slot.YPosition = 0x0090;
            slot.XRadius = 0x0028;
            slot.YRadius = 0x001c;
            RequireSetRoomScrollState(0, RoomScrollState.Blue);
            RequireSetRoomScrollState(1, RoomScrollState.Blue);
            RequireSetRoomScrollState(2, RoomScrollState.Blue);
            RequireSetRoomScrollState(3, RoomScrollState.Blue);
            PublishCrocomirePlm(0x20, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(0x1e, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(0x61, 0x0b, RoomPlmHeaders.ClearCrocomireBridge);
            TransferCrocomireBg2Words(0, 1024);
            return;
        }

        state.DeathSequenceIndex = 0;
        state.ReactionTimer = 0;
        state.FightFunction = CrocomireFightFunction.Sleeping;
        // $A4:8ABA-8ABD, on the living branch only.
        CameraDistanceIndex = CameraDistanceMode.BossTracking;
        InstallCrocomireInstructionList(
            slot,
            CrocomireInstructionProgramDefinitions.Initial);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        RequireSetRoomScrollState(0, RoomScrollState.RedBoundary);
        RequireSetRoomScrollState(1, RoomScrollState.RedBoundary);

        // The initializer copies seventeen words, not sixteen: X starts at $20 and reaches
        // zero inclusively. Preserve that palette-boundary write because later fades compare
        // the exact target image produced by the cartridge.
        (TileArtwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire requires installed color artwork.")).ApplyInitial(_cgram!);
    }

    /// <summary>Ports <c>InitAI_CrocomireTongue</c> at $A4:F67A.</summary>
    private void InitializeCrocomireTongue(RoomEnemySlot slot)
    {
        if (RequireAreaMiniBossDefeated())
        {
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.Deleted |
                    EnemyProperties.Invisible,
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        InstallCrocomireInstructionList(
            slot,
            CrocomireTongueInstructionProgramDefinitions.Fight);
        slot.ExtraProperties = slot.ExtraProperties
            .With(EnemyExtraProperties.UsesExtendedSpritemap)
            .WithUntranslatedExtraBits(
                EnemyExtraPropertyRawBits.CrocomireInitializerBit0400,
                "Crocomire initializer");
        slot.VariableA = 23;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (Crocomire is not null)
            Crocomire.Tongue = slot;
    }

    /// <summary>Runs the live-fight phase of <c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    private void RunCrocomireMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort cameraX)
    {
        CrocomireEnemyState state = RequireCrocomire(slot);
        _crocomireCameraX = cameraX;

        if (state.DeathSequenceIndex == 0)
        {
            HandleCrocomireBridgeThreshold(state);
            // The native word write runs even on the frame that starts bridge collapse.
            // Later death phases retain these zones until the skeleton enters the river.
            RequireSetRoomScrollState(CrocomireCameraDefinitions.BridgeLeftScreen,
                samus is not null && unchecked((short)(samus.XPosition - CrocomireCameraDefinitions.BridgeVisibleSamusX)) >= 0
                    ? RoomScrollState.RedBoundary : RoomScrollState.Blue);
            RequireSetRoomScrollState(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.Blue);
            // Main state zero always finishes through $A4:8B5B, including the frame in
            // which $8D5E changes the death index to two.
            UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        }
        else
        {
            RunCrocomireDeathSequence(state, samus);
        }

        HandleCrocomireInvisibleWall(slot, state, samus);
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
        InstallCrocomireInstructionList(
            body,
            CrocomireInstructionProgramDefinitions.BridgeCollapsed);
        body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
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
            tongue.CurrentInstruction =
                CrocomireTongueInstructionProgramDefinitions.Sleep;
            tongue.Properties = tongue.Properties.With(EnemyProperties.Invisible);
        }
    }

    /// <summary>Ports the artificial left-side wall in <c>$A4:8C95</c>.</summary>
    private void HandleCrocomireInvisibleWall(
        RoomEnemySlot body,
        CrocomireEnemyState state,
        SamusState? samus)
    {
        if (samus is null || state.DeathSequenceIndex != 0)
            return;
        ushort leftEdge = unchecked((ushort)(
            body.XPosition - body.XRadius - samus.Kinematics.XRadius));
        if (unchecked((short)(leftEdge - samus.XPosition)) >= 0)
            return;

        ResolveNormalEnemyTouch(body, samus);
        samus.XPosition = leftEdge;
        samus.Kinematics.ExtraXDisplacement = unchecked((ushort)-4);
        samus.Kinematics.ExtraYDisplacement = 0xffff;
    }

    /// <summary>Ports Crocomire's eight-color body hurt flash at $A4:8CCB.</summary>
    private void ApplyCrocomireHurtPalette(RoomEnemySlot body)
    {
        bool white = body.FlashTimer != 0 && (_randomEnemyCounter & 2) != 0;
        if (!white)
        {
            (TileArtwork?.CrocomireColors ?? throw new InvalidDataException(
                "Crocomire hurt palette requires installed artwork.")).ApplyFightBody(_cgram!);
            return;
        }
        for (int color = 0; color < CrocomirePaletteRomData.FightBodyCount; color++)
            _cgram!.SetColor(CrocomirePaletteRomData.FightBodyDestination + color,
                (ushort)0x7fff);
    }

    private static void InstallCrocomireInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private CrocomireEnemyState RequireCrocomire(RoomEnemySlot slot) =>
        Crocomire is { } state && ReferenceEquals(state.Body, slot)
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Crocomire body state.");

    private CrocomireDeathState RequireCrocomireDeath() =>
        CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire death extension is not initialized for the current room.");
}
