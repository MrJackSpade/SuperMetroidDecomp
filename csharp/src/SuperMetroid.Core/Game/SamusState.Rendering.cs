using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Samus animation, tile-transfer, and OAM rendering behavior.
/// </summary>
public sealed partial class SamusState
{
    /// <summary>Last top-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort TopSpritemapIndex { get; private set; }

    /// <summary>Last bottom-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort BottomSpritemapIndex { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapXPosition { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapYPosition { get; private set; }

    /// <summary>The exact pending bank-$92 definitions consumed by accepted NMI.</summary>
    public SamusTileTransferState TileTransfers { get; } = new();

    /// <summary>
    /// Copies <c>SamusPalettes_PowerSuit</c> at <c>$9B:9400</c> to palette-buffer/CGRAM
    /// entries 192–207, porting <c>Samus_LoadSuitPalette</c>'s no-suit branch.
    /// </summary>
    public static void LoadPowerSuitPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        // OBJ palettes begin at CGRAM 128. Spritemap palette 4 therefore resolves to 192,
        // exactly matching CopyToSamusSuitPalette's &palette_buffer[192] destination.
        cgram.LoadFromBus(bus, PowerSuitPalette, colorCount: 16, destinationIndex: 192);
    }

    /// <summary>
    /// Ports <c>Samus_LoadSuitPalette</c> at <c>$91:DEBA</c>, giving Gravity priority over
    /// Varia and falling back to Power Suit when neither equipment bit is enabled.
    /// </summary>
    public void LoadSuitPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        int paletteAddress = EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? GravitySuitPalette
            : EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? VariaSuitPalette
                : PowerSuitPalette;
        cgram.LoadFromBus(bus, paletteAddress, colorCount: 16, destinationIndex: 192);
    }

    /// <summary>
    /// Seeds pose/frame-zero graphics before the first NMI, as room/game setup must do
    /// before a visible normal-gameplay Samus can be constructed.
    /// </summary>
    public void PrimeGraphics(ISnesAddressSpace bus) =>
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);

    /// <summary>
    /// Seeds the animation frame timer as <c>Set_Samus_AnimationFrame_if_PoseChanged</c>
    /// at <c>$91:FB08</c> does, including its bottom-minus-one liquid boundary test.
    /// </summary>
    public void InitializeAnimation(ISnesAddressSpace bus, ushort initialFrame = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);

        AnimationFrame = initialFrame;
        // `$91:FB08` does not simply reuse the previous animation pass's `$0A9A`. It
        // recomputes water/lava delay from the NEW pose radius at Y + radius - 1, while
        // Gravity Suit takes the ordinary speed-divisor path.
        AnimationFrameBuffer = LiquidPhysics.DeterminePoseChangeAnimationBuffer(this);
        AnimationDelayListAddress = ResolveAnimationDelayList(bus);
        byte initialDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((initialDelay & 0x80) != 0)
        {
            throw new InvalidOperationException(
                $"Samus pose ${Pose:X2} cannot initialize directly on delay command ${initialDelay:X2}.");
        }

        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + initialDelay));
        LastAnimationDelayCommand = null;
        PendingTransitionalPose = null;
    }

    /// <summary>
    /// Rebinds the delay-list pointer after a bank-$91 scripted controller writes Pose and
    /// literal animation words without calling ordinary pose initialization.
    /// </summary>
    /// <remarks>
    /// Drained-controller functions one and four do exactly that at `$91:E571/$91:E60C`.
    /// The cartridge's animation pass resolves the pointer from Pose every call, whereas
    /// this C# port caches it as a consistency check. This narrow method updates only that
    /// cache plus the controller's explicit writes; it intentionally does not recompute the
    /// liquid animation buffer or silently refresh collision radius.
    /// </remarks>
    internal void SetPoseAndAnimationFromScriptedController(
        ISnesAddressSpace bus,
        byte pose,
        ushort frame,
        ushort timer,
        bool refreshRadius)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Pose = pose;
        if (refreshRadius)
            RefreshCollisionRadii(bus);
        AnimationDelayListAddress = ResolveAnimationDelayList(bus);
        AnimationFrame = frame;
        AnimationFrameTimer = timer;
        LastAnimationDelayCommand = null;
        PendingTransitionalPose = null;
    }

    /// <summary>
    /// Publishes the angle-selected grapple art exactly as $9B:BD95 does before the normal
    /// bank-$91 animation pass. The latter will decrement this fifteen to fourteen later in
    /// the same gameplay frame; exposing this narrow writer prevents grapple code from
    /// acquiring a general-purpose escape hatch around animation invariants.
    /// </summary>
    public void SetGrappleSwingAnimationFrame(ushort frame)
    {
        if (Pose is not (SamusPoseIds.GrappleSwingRightPose or SamusPoseIds.GrappleSwingLeftPose))
            throw new InvalidOperationException($"Grapple swing art cannot be assigned to pose ${Pose:X2}.");
        AnimationFrame = frame;
        AnimationFrameTimer = 15;
    }

    /// <summary>
    /// Ports <c>AnimateSamus</c> at <c>$90:8000</c>, including movement-visible FX delay state.
    /// </summary>
    /// <remarks>
    /// The FX dispatcher publishes water/lava delay buffering, remembered medium, native
    /// atmospheric slots, sound requests, and periodic-damage words before the timer
    /// decrement. The historical name remains as a source-compatible debugger API.
    /// </remarks>
    public void AnimateNoFx(
        ISnesAddressSpace bus,
        ushort controllerInput = 0,
        ushort nmiFrameCounter = 0,
        Bank80SystemState? system = null,
        bool beginLiquidSoundRequestFrame = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureAnimationInitialized(bus);

        // `$90:8000` dispatches the active room-FX animation handler before touching the
        // frame timer. This also updates remembered `$0AD2`, which the next Space Jump gate
        // consumes independently of its current top-boundary submersion check.
        LiquidPhysics.PrepareAnimationFrame(
            bus,
            this,
            nmiFrameCounter,
            system,
            beginLiquidSoundRequestFrame);

        // $90:8032 keeps neutral-jump frame one alive in four-tick chunks while Samus is
        // still rising. This is intentionally tested before DEC and applies only when the
        // timer is exactly one; release/apex changes YDirection to two and lets it expire.
        if (Pose is SamusPoseIds.NeutralJumpRightPose or SamusPoseIds.NeutralJumpLeftPose &&
            Kinematics.YDirection != 2 &&
            AnimationFrame == 1 &&
            AnimationFrameTimer == 1)
        {
            AnimationFrameTimer = 4;
        }

        // DEC is 16-bit. A timer accidentally initialized to zero becomes $FFFF; BMI then
        // advances just like BEQ does for an ordinary 1 -> 0 expiration.
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer - 1));
        if (AnimationFrameTimer != 0 && (AnimationFrameTimer & 0x8000) == 0)
            return;

        AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
        HandleAnimationDelay(bus, controllerInput);
    }

    /// <summary>
    /// Ports the animation-only core of <c>Draw_Samus_Starting_Death_Animation</c> at
    /// `$90:8976`. Unlike ordinary gameplay animation, this call deliberately skips liquid-
    /// FX delay buffering, neutral-jump hacks, and controller-dependent Dash interception.
    /// </summary>
    internal void AnimateDeathFrame(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureAnimationInitialized(bus);
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer - 1));
        if (AnimationFrameTimer != 0 && (AnimationFrameTimer & 0x8000) == 0)
            return;
        AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
        HandleAnimationDelay(bus, controllerInput: 0);
    }

    /// <summary>
    /// Publishes an animation frame/timer pair written directly by an installed special
    /// movement handler instead of by the generic delay-program interpreter.
    /// </summary>
    /// <remarks>
    /// Crystal Flash writes `$0A94/$0A96` at `$90:D689` and `$90:D74E`. This narrow internal
    /// seam preserves the native ordering: movement writes the pair, then animation later
    /// in beta immediately decrements the newly written timer.
    /// </remarks>
    internal void SetAnimationFrameFromSpecialHandler(ushort frame, ushort timer)
    {
        AnimationFrame = frame;
        AnimationFrameTimer = timer;
    }

    private void HandleAnimationDelay(ISnesAddressSpace bus, ushort controllerInput)
    {
        byte delayOrCommand = ReadAnimationByte(bus, AnimationFrame);
        if ((delayOrCommand & 0x80) == 0)
        {
            LastAnimationDelayCommand = null;

            // `$90:8554-$8568` replaces an ordinary running pose's per-pose delay list
            // with the pointer stored at `$91:B5D1` whenever momentum flag `$0B3C` is set.
            // Notice that this branch does NOT test the Dash button: releasing B retains
            // the default running cadence until a transition/collision cancels momentum.
            byte runningOrPoseDelay = HorizontalSpeed.HasRunningMomentum &&
                ReadMovementKind(bus) == SamusMovementType.Running
                ? EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster)
                    ? HorizontalSpeed.ReadSpeedBoosterAnimationByte(bus, AnimationFrame)
                    : ReadDefaultRunningAnimationByte(bus, AnimationFrame)
                : delayOrCommand;
            AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + runningOrPoseDelay));
            return;
        }

        bool activeDashAnimation = HorizontalSpeed.HasRunningMomentum &&
            ReadMovementKind(bus) == SamusMovementType.Running &&
            (controllerInput & (ushort)SnesButton.B) != 0;
        if (activeDashAnimation)
        {
            // `$90:852C-$8543` intercepts a command byte before the generic command table.
            // The no-Speed-Booster route restarts frame zero and uses byte zero from the
            // default running-delay stream. Returning here is important: native returns
            // command number zero, whose handler does no further timer selection.
            if (EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            {
                ushort stagedFrame = AnimationFrame;
                if (HorizontalSpeed.TryAdvanceSpeedBoosterAnimationStage(
                    bus,
                    movementType: SamusMovementType.Running,
                    controllerInput,
                    AnimationFrameBuffer,
                    ref stagedFrame,
                    out ushort stagedTimer))
                {
                    AnimationFrame = stagedFrame;
                    AnimationFrameTimer = stagedTimer;
                    LastAnimationDelayCommand = delayOrCommand;
                    return;
                }

                // A nonzero low counter byte makes `$90:852C` return the original command,
                // so normal `$FE/$FF/...` dispatch below proceeds against the pose stream.
            }
            else
            {
                AnimationFrame = 0;
                LastAnimationDelayCommand = delayOrCommand;
                AnimationFrameTimer = unchecked((ushort)(
                    AnimationFrameBuffer + ReadDefaultRunningAnimationByte(bus, byteIndex: 0)));
                return;
            }
        }

        LastAnimationDelayCommand = delayOrCommand;
        switch (delayOrCommand & 0x0f)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                // `$90:8324-$8345` points all six instruction slots at the same CLC/RTS
                // handler. Carry clear tells the caller to return immediately: native does
                // not change the animation frame and, crucially, does not reload its timer.
                // The outer routine has already advanced onto this command after a 1 -> 0
                // expiration, so the timer remains zero for the rest of this gameplay frame.
                // On the next frame DEC wraps it to `$FFFF`; BMI then advances once more,
                // stepping over the no-op command and interpreting the following delay.
                // `$F0` is used by the aimed-falling sequences `$6D-$70`; accepting it as a
                // one-frame command is therefore live game behavior, not defensive parsing.
                return;

            case 6:
                // $90:8346, command $F6: healthy Samus loops to zero; below 30 energy she
                // advances past the command into the alternate breathing sequence.
                AnimationFrame = Health < 30
                    ? unchecked((ushort)(AnimationFrame + 1))
                    : (ushort)0;
                break;

            case 7:
                // `$90:8360`, command `$F7`: install `$90:94CB`, then increment once more
                // past the command byte. The outer animation routine already performed its
                // ordinary pre-dispatch increment, so this second increment is essential.
                Drained.InstallFallingMovementHandler(this);
                AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
                break;

            case 8:
                // $90:8370 falls through to command $FD's one-byte pose operand. For the
                // grounded $25/$26 sequences this publishes $02/$01 through command three;
                // it does NOT select a new delay or advance the visible animation frame.
                // The runtime consumes this after AnimateNoFx, where Samus_HandleTransitions
                // runs in the native frame. `$BF-$C4` use the same command to start their
                // literal `$19/$1A` spin-jump operand after the grounded turn art finishes.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 9:
                // $90:839A, command $F9 eeee gg aa GG AA. The mask is a little-endian
                // item word; unequipped/equipped each choose a grounded or airborne target
                // according to BOTH halves of Y speed. `$37/$38` test Spring Ball bit $0002
                // and use this command to finish ordinary morph entry without guessing
                // whether the transition walked off a ledge.
                ushort itemMask = unchecked((ushort)(
                    ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1))) |
                    (ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 2))) << 8)));
                bool itemEquipped = (EquippedItems & itemMask) != 0;
                bool movingVertically = Kinematics.YSpeed != 0 || Kinematics.YSubspeed != 0;
                ushort targetOffset = itemEquipped
                    ? movingVertically ? (ushort)6 : (ushort)5
                    : movingVertically ? (ushort)4 : (ushort)3;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + targetOffset)));
                return;

            case 10:
                // `$90:83F6`, unused command `$FA gg aa`: select one transitional pose
                // when both halves of Y speed are zero and the other when either half is
                // nonzero. Retail has no reachable active pose using this instruction, but
                // preserving it completes the actual sixteen-entry interpreter instead of
                // treating valid cartridge bytecode as corrupt data.
                bool faMovingVertically = Kinematics.YSpeed != 0 || Kinematics.YSubspeed != 0;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + (faMovingVertically ? 2 : 1))));
                return;

            case 11:
                // `$90:841D`, command `$FB`, first checks TOP-boundary submersion. A fully
                // submerged non-Gravity body is forced to the ordinary one-byte sequence;
                // otherwise Screw Attack has priority over Space Jump. This top-edge test
                // deliberately differs from jump launch/gravity's bottom-edge test.
                AnimationFrame = unchecked((ushort)(AnimationFrame + (
                    !EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
                    LiquidPhysics.IsTopBoundarySubmerged(this) ? 1 :
                    EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack) ? 0x15 :
                    EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ? 0x0b : 1)));
                break;

            case 12:
                // `$90:848B`, unused command `$FC eeee gg aa`: the little-endian item
                // mask is followed by unequipped/equipped pose bytes. Poses `$3F/$40` are
                // the only retail users (Spring Ball-aware Morph Ball selection), but the
                // command still publishes through the ordinary transitional-pose seam.
                ushort fcItemMask = unchecked((ushort)(
                    ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1))) |
                    (ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 2))) << 8)));
                bool fcItemEquipped = (EquippedItems & fcItemMask) != 0;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + (fcItemEquipped ? 4 : 3))));
                return;

            case 13:
                // $90:83A0, command $FD pp: publish pose pp through the same command-three
                // seam as $F8. Unlike F8, FD has no auto-jump special case before it falls
                // through here. The one-frame $4B/$4C neutral-jump transitions use it.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 14:
                // $90:84C7, command $FE nn: move backward nn byte positions. The operand
                // remains in the same delay stream and is not itself an animation frame.
                byte backwardCount = ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1)));
                AnimationFrame = unchecked((ushort)(AnimationFrame - backwardCount));
                break;

            case 15:
                // $90:84DB, command $FF: unconditional loop to the start of the sequence.
                AnimationFrame = 0;
                break;

        }

        byte selectedDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((selectedDelay & 0x80) != 0)
        {
            throw new InvalidDataException(
                $"Samus animation command ${delayOrCommand:X2} selected another command ${selectedDelay:X2}.");
        }
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + selectedDelay));
    }

    private void EnsureAnimationInitialized(ISnesAddressSpace bus)
    {
        int expectedList = ResolveAnimationDelayList(bus);
        if (AnimationDelayListAddress != expectedList)
        {
            throw new InvalidOperationException(
                "Samus animation was not initialized for the current pose. Call InitializeAnimation after changing Pose.");
        }
    }

    private int ResolveAnimationDelayList(ISnesAddressSpace bus)
    {
        ushort pointer = ReadWord(
            bus,
            AddWithinBank(SamusMovementRomData.Poses.AnimationDelayListPointers, Pose * 2));
        return SamusMovementRomData.Banks.Pose | pointer;
    }

    private byte ReadAnimationByte(ISnesAddressSpace bus, ushort byteIndex) =>
        bus.ReadByte(AddWithinBank(AnimationDelayListAddress, byteIndex));

    /// <summary>
    /// Reads the shared ordinary-Dash delay list selected indirectly through `$91:B5D1`.
    /// Keeping the pointer lookup live means the ROM, not a duplicated C# byte array,
    /// remains authoritative for both cadence and command position.
    /// </summary>
    private static byte ReadDefaultRunningAnimationByte(ISnesAddressSpace bus, ushort byteIndex)
    {
        ushort listPointer = ReadWord(
            bus,
            SamusMovementRomData.Poses.DefaultRunningAnimationDelayListPointer);
        return bus.ReadByte((int)new SnesAddress(0x91, listPointer).AddWithinBank(byteIndex));
    }

    /// <summary>
    /// Ports the standing and ordinary-running portions of <c>Samus_Draw</c> at
    /// <c>$90:85E2</c>.
    /// </summary>
    /// <remarks>
    /// Movement type zero uses the standing position selector, `$0F/$1B` use their explicit
    /// table-backed selectors, and every other native slot through `$1B` uses the usual
    /// pose offset. The complete `$90:864E` lower-half dispatcher is retained, including
    /// all unused-but-valid movement types. Morph types `$04/$08` draw only their complete
    /// top spritemap; spin-jump `$03` retains its own conditional bottom rule. Draygon's
    /// ten type-`$1A` bodies have ordinary split top/bottom spritemaps and no draw-time offset.
    /// </remarks>
    public bool Draw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter = 0,
        SamusMode7Transform? mode7Transform = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        int poseDefinition = AddWithinBank(
            SamusMovementRomData.Poses.Definitions,
            Pose * SamusMovementRomData.Poses.DefinitionByteCount);
        SamusMovementType movementType = ReadMovementType(bus);

        // `$90:85E2-$90:85FC` applies invincibility flicker to the body spritemaps, not to
        // animation or graphics streaming as a whole. An odd NMI hides ordinary invincible
        // Samus. Active knockback and any nonzero stored/active shine timer independently
        // force the body visible, even though the separately drawn arm cannon follows its
        // stricter `$90:C663` rule and may still disappear on that same odd frame.
        bool bodyVisible = KnockbackTimer != 0 ||
            InvincibilityTimer == 0 ||
            Shinespark.ShineTimer != 0 ||
            (nmiFrameCounter & 1) == 0;
        if (!bodyVisible)
        {
            // Native branches around every position/index/OAM write, then still falls
            // through to `$92:8000`. Preserve the previous visible spritemap indices for
            // echo drawing while selecting this frame's tile definitions for the next NMI.
            TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);
            return false;
        }

        // `$90:8C1F` temporarily overwrites Samus's integer world point only while Ceres's
        // high status bit is set. Every position selector below therefore consumes the
        // rotated point, but public physics coordinates remain completely untouched.
        SamusMode7Point renderPoint = mode7Transform is { } transform
            ? transform.Transform(XPosition, YPosition)
            : new SamusMode7Point(XPosition, YPosition);
        ushort renderX = renderPoint.X;
        ushort renderY = renderPoint.Y;

        // $90:8C94 sign-extends the byte at pose-definition offset four. Pose $01 stores
        // +6, moving the art origin six pixels above Samus's world-space center.
        sbyte graphicsYOffset = unchecked((sbyte)bus.ReadByte(AddWithinBank(poseDefinition, 4)));
        SpritemapXPosition = unchecked((ushort)(renderX - layer1X));
        if (movementType == SamusMovementType.Standing &&
            Pose is SamusPoseIds.ForwardFacingPowerSuitPose or SamusPoseIds.ForwardFacingSuitedPose &&
            AnimationFrame >= 2)
        {
            // `$90:8D07-$90:8D27` uses the ordinary pose graphics offset for front-facing
            // frames zero/one, then pins every later frame exactly one pixel above Samus's
            // center. This is independent of `$00`'s extra power-suit chest-cover OBJ.
            SpritemapYPosition = unchecked((ushort)(renderY - 1 - layer1Y));
        }
        else if (movementType == SamusMovementType.Standing && Pose >= SamusPoseIds.NormalLandingRightPose &&
            Pose <= SamusPoseIds.SpinLandingLeftPose)
        {
            // `$90:8CDC-$90:8CF6` indexes sixteen packed bytes, but performs a 16-bit
            // unaligned LDA. The following landing frame's byte therefore becomes the high
            // byte of the subtraction. OAM ultimately displays only low Y, yet the complete
            // wrapped `$0AFA` spritemap-position word is observable and is preserved here.
            int landingOffsetIndex = (Pose - SamusPoseIds.NormalLandingRightPose) * 4 +
                AnimationFrame;
            ushort landingOffset = ReadWord(
                bus,
                AddWithinBank(0x908d28, landingOffsetIndex));
            SpritemapYPosition = unchecked((ushort)(
                renderY - landingOffset - layer1Y));
        }
        else if (movementType == SamusMovementType.PostureTransition &&
            Pose >= SamusPoseIds.CrouchingTransitionRightPose &&
            Pose < SamusPoseIds.MorphBallGroundLeftPose)
        {
            // `$90:8D3C` indexes a signed byte by `2*(pose-$35)+animation frame` instead
            // of using the pose-definition graphics offset. Reading the cartridge table
            // directly retains ordinary crouch/morph/stand/unmorph values and the four
            // valid-but-unused zero records `$39/$3A/$3F/$40`. Animation commands replace
            // retail poses before a command/operand index can escape this two-byte record.
            int transitionOffsetAddress = AddWithinBank(
                0x908d80,
                (Pose - SamusPoseIds.CrouchingTransitionRightPose) * 2 + AnimationFrame);
            sbyte transitionOffset = unchecked((sbyte)bus.ReadByte(transitionOffsetAddress));
            SpritemapYPosition = unchecked((ushort)(renderY + transitionOffset - layer1Y));
        }
        else if (Pose is SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose)
        {
            // `$90:8DC1` indexes the shared 32-byte table at `$90:8DEF` directly with the
            // animation byte index. Several indices intentionally name command operands,
            // because the external controller can publish those literal indices for a
            // visible frame. Reading ROM keeps that odd layout authoritative.
            sbyte drainedOffset = unchecked((sbyte)bus.ReadByte(
                AddWithinBank(0x908def, AnimationFrame)));
            SpritemapYPosition = unchecked((ushort)(renderY + drainedOffset - layer1Y));
        }
        else if ((Pose is SamusPoseIds.DrainedStandingRightPose or SamusPoseIds.DrainedStandingLeftPose) &&
                 AnimationFrame >= 5)
        {
            // `$90:8DB1-$8DBC` replaces the usual pose graphics offset with -3 after
            // standing drained art reaches byte index five.
            SpritemapYPosition = unchecked((ushort)(renderY - 3 - layer1Y));
        }
        else
        {
            SpritemapYPosition = unchecked((ushort)(renderY - graphicsYOffset - layer1Y));
        }

        ushort topBase = ReadWord(bus, AddWithinBank(TopSpritemapBaseIndexTable, Pose * 2));
        TopSpritemapIndex = unchecked((ushort)(topBase + AnimationFrame));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, SpritemapXPosition, SpritemapYPosition);

        // `$90:868D-$90:86C4` writes one small OBJ directly between the top and bottom
        // spritemap calls when unsuited pose `$00` faces the screen. This is not a visor:
        // the disassembly identifies tile `$021` as a cover for the left side of the power-
        // suit chest. The suited `$9B` body has that shape in its ordinary spritemap and
        // deliberately skips this write. Coordinates use Samus's world center directly,
        // not SpritemapYPosition (which has already applied pose graphics offset `$08`).
        if (Pose == SamusPoseIds.ForwardFacingPowerSuitPose)
        {
            ushort chestX = unchecked((ushort)(XPosition - 7 - layer1X));
            ushort chestY = unchecked((ushort)(YPosition - 0x11 - layer1Y));
            oam.AddRawSmallSprite(chestX, chestY, attributes: 0x3821);
        }

        // $90:8686 suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. The admitted Screw/Space Jump records use their
        // separate native rule and always draw the bottom half at every animation frame.
        bool ordinarySpinBottom = movementType != SamusMovementType.SpinJumping ||
            Pose is SamusPoseIds.SpaceJumpRightPose or SamusPoseIds.SpaceJumpLeftPose or
                SamusPoseIds.ScrewAttackRightPose or SamusPoseIds.ScrewAttackLeftPose ||
            AnimationFrame == 0 || AnimationFrame >= 0x0b;

        // `$90:86EE` is movement type `$0A`'s only exception. The first three frames of
        // the `$D7/$D8` Crystal-Flash-end/fatal-damage body are complete top spritemaps;
        // all other knockback-family poses and later frames retain an ordinary lower half.
        bool knockbackBottom = movementType != SamusMovementType.Knockback ||
            Pose is not (SamusPoseIds.DeathSequenceRightPose or SamusPoseIds.DeathSequenceLeftPose) ||
            AnimationFrame >= 3;

        // `$90:870C-$90:874B` is a pose-and-frame dispatcher, not a blanket type-$0F
        // policy. Basic crouch/stand transitions always use a lower half. Morph/unmorph
        // bodies below `$DB` never do. `$DB/$DC` draw it only on frame zero, `$DD-$F0`
        // only on frame two, and the aimed transition family `$F1+` always draws it.
        bool transitionBottom = movementType != SamusMovementType.PostureTransition ||
            Pose >= SamusPoseIds.CrouchingTransitionAimUpRightPose ||
            Pose is SamusPoseIds.CrouchingTransitionRightPose or SamusPoseIds.CrouchingTransitionLeftPose or
                SamusPoseIds.StandingTransitionRightPose or SamusPoseIds.StandingTransitionLeftPose ||
            (Pose is SamusPoseIds.UnusedPoseDb or SamusPoseIds.UnusedPoseDc &&
                AnimationFrame == 0) ||
            (Pose >= SamusPoseIds.UnusedPoseDd &&
                Pose < SamusPoseIds.CrouchingTransitionAimUpRightPose && AnimationFrame == 2);

        // The otherwise-unused movement type `$0D` still has executable cartridge logic
        // at `$90:874C`: poses `$65/$66` draw a lower half only on frame zero, while every
        // other type-`$0D` pose follows the ordinary always-split return.
        bool unusedTypeDBottom = movementType != SamusMovementType.Unused0D ||
            Pose is not (SamusPoseIds.UnusedPose65 or SamusPoseIds.UnusedPose66) ||
            AnimationFrame < 1;

        bool wallJumpBottom = movementType != SamusMovementType.WallJumping ||
            AnimationFrame < 3 || AnimationFrame >= 0x0d;
        bool damageBoostBottom = movementType != SamusMovementType.DamageBoost ||
            AnimationFrame < 2 || AnimationFrame >= 9;
        // `$90:8790` suppresses the lower half for vertical shinesparks and for drained
        // crouch/fall byte indices zero and one. Every other type-$1B record draws it.
        bool specialType1BBottom = movementType != SamusMovementType.Special ||
            (Pose is not (SamusPoseIds.ShinesparkVerticalRightPose or SamusPoseIds.ShinesparkVerticalLeftPose) &&
             (Pose is not (SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose) ||
              AnimationFrame >= 2));
        bool drawBottom = movementType is not (
            SamusMovementType.MorphBallGround or
            SamusMovementType.UnusedGlitchBall or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling) &&
            ordinarySpinBottom && knockbackBottom && transitionBottom && unusedTypeDBottom &&
            wallJumpBottom && damageBoostBottom && specialType1BBottom;
        // The native bottom selector clears this word when a complete top-half frame does
        // not need a bottom. Clearing it here also prevents the following echo renderer
        // from reusing a bottom spritemap left by an earlier animation frame.
        BottomSpritemapIndex = 0;
        if (drawBottom)
        {
            ushort bottomBase = ReadWord(bus, AddWithinBank(BottomSpritemapBaseIndexTable, Pose * 2));
            BottomSpritemapIndex = unchecked((ushort)(bottomBase + AnimationFrame));
            oam.AddSamusSpritemap(bus, BottomSpritemapIndex, SpritemapXPosition, SpritemapYPosition);
        }

        // Native Samus_Draw always performs this selection after its conditional OAM work.
        // Those flags drive the following accepted NMI, so stepping exposes the authentic
        // one-main-loop/one-NMI producer-consumer relationship.
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);
        return true;
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes from the positions captured by
    /// <c>Samus_UpdateSpeedEchoPos</c> at <c>$90:EEE7</c>.
    /// </summary>
    /// <remarks>
    /// This preserves both branches of <c>Samus_DrawEchoes</c> at <c>$90:87BD</c>. A
    /// nonnegative index draws stationary trailing snapshots only at boost stage four.
    /// Cancellation changes the index to <c>$FFFF</c>; drawing then advances each body's
    /// native ±8 X / ±2 Y convergence before deciding whether it crossed the live Samus.
    /// </remarks>
    public void DrawSpeedBoosterEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        if ((HorizontalSpeed.SpeedEchoIndex & 0x8000) != 0)
        {
            // `$90:87D3` walks slot one before slot zero. Movement is a draw-handler side
            // effect in the retail game, so do not advance it earlier in StepFrame: frames
            // whose Samus draw handler is suppressed must also freeze these copies.
            if (HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 1,
                    XPosition,
                    YPosition))
            {
                DrawActiveSpeedBoosterEcho(
                    bus,
                    oam,
                    HorizontalSpeed.SecondSpeedEchoXPosition,
                    HorizontalSpeed.SecondSpeedEchoYPosition,
                    layer1X,
                    layer1Y);
            }

            if (HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 0,
                    XPosition,
                    YPosition))
            {
                DrawActiveSpeedBoosterEcho(
                    bus,
                    oam,
                    HorizontalSpeed.FirstSpeedEchoXPosition,
                    HorizontalSpeed.FirstSpeedEchoYPosition,
                    layer1X,
                    layer1Y);
            }

            HorizontalSpeed.FinishDepartingSpeedEchoFrame();
            return;
        }

        if ((HorizontalSpeed.SpeedBoostCounter & 0xff00) != 0x0400)
            return;

        // `$90:87C7` draws slot one before slot zero. OAM order is observable when their
        // opaque pixels overlap, so retain that otherwise-surprising reverse order.
        DrawActiveSpeedBoosterEcho(
            bus,
            oam,
            HorizontalSpeed.SecondSpeedEchoXPosition,
            HorizontalSpeed.SecondSpeedEchoYPosition,
            layer1X,
            layer1Y);
        DrawActiveSpeedBoosterEcho(
            bus,
            oam,
            HorizontalSpeed.FirstSpeedEchoXPosition,
            HorizontalSpeed.FirstSpeedEchoYPosition,
            layer1X,
            layer1Y);
    }

    /// <summary>
    /// Ports `$90:88BA/$90:EBF3`: on odd NMI frames, draw both crash-orbit copies after
    /// the real Samus body using the current pose/frame spritemaps.
    /// </summary>
    public void DrawShinesparkCrashEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if ((nmiFrameCounter & 1) == 0 ||
            Shinespark.Phase is not (ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle))
        {
            return;
        }

        // Slot one precedes slot zero just like native's X=2,0 loop.
        DrawActiveSpeedBoosterEcho(
            bus, oam,
            HorizontalSpeed.SecondSpeedEchoXPosition,
            HorizontalSpeed.SecondSpeedEchoYPosition,
            layer1X, layer1Y);
        DrawActiveSpeedBoosterEcho(
            bus, oam,
            HorizontalSpeed.FirstSpeedEchoXPosition,
            HorizontalSpeed.FirstSpeedEchoYPosition,
            layer1X, layer1Y);
    }

    /// <summary>
    /// Ports <c>Samus_DrawShinesparkCrashEchoProjectiles</c> at <c>$90:8953</c>. These
    /// copies are ordinary projectile-phase visuals after the centered crash circle ends.
    /// </summary>
    public void DrawReleasedShinesparkCrashEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if ((nmiFrameCounter & 1) == 0)
            return;

        // `$90:8953` tests/draws fixed slot four (speed echo index three) before fixed
        // slot three (index two). That ordering controls OAM priority when copies overlap.
        ShinesparkReleasedEcho second = Shinespark.SecondReleasedCrashEcho;
        if (second.Active)
        {
            DrawActiveSpeedBoosterEcho(
                bus, oam, second.XPosition, second.YPosition, layer1X, layer1Y);
        }

        ShinesparkReleasedEcho first = Shinespark.FirstReleasedCrashEcho;
        if (first.Active)
        {
            DrawActiveSpeedBoosterEcho(
                bus, oam, first.XPosition, first.YPosition, layer1X, layer1Y);
        }
    }

    private void DrawActiveSpeedBoosterEcho(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort echoX,
        ushort echoY,
        ushort layer1X,
        ushort layer1Y)
    {
        // Zero X is the cartridge's empty-slot sentinel, not merely an off-screen point.
        if (echoX == 0)
            return;

        int poseDefinition = AddWithinBank(
            SamusMovementRomData.Poses.Definitions,
            Pose * SamusMovementRomData.Poses.DefinitionByteCount);
        sbyte graphicsYOffset = unchecked((sbyte)bus.ReadByte(AddWithinBank(poseDefinition, 4)));
        short screenY = unchecked((short)(echoY - graphicsYOffset - layer1Y));

        // The original accepts screen Y 0..247. Horizontal clipping remains OAM/PPU work,
        // exactly as it is for the current Samus body.
        if (screenY < 0 || screenY >= 248)
            return;

        ushort screenX = unchecked((ushort)(echoX - layer1X));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, screenX, unchecked((ushort)screenY));
        if (BottomSpritemapIndex != 0)
            oam.AddSamusSpritemap(bus, BottomSpritemapIndex, screenX, unchecked((ushort)screenY));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

}
