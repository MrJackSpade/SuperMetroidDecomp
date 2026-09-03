using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact state, tile DMA, and one-OBJ renderer for Samus's opening arm cannon at
/// <c>$90:C5C4-$90:C790</c>.
/// </summary>
/// <remarks>
/// The cannon is not baked into every Samus body frame. Missile, Super Missile, and
/// grapple HUD selections open a four-state cover, then the draw routine places tile $1F
/// separately and uploads one direction/frame-specific 4bpp tile to VRAM word $61F0.
/// Keeping this independent from pose art preserves its native before/after-body priority.
/// </remarks>
public sealed class SamusArmCannonState
{
    private ushort _previousSelectedHudItem;

    /// <summary>Low byte of WRAM `$0AA6`: zero closed, one open/opening.</summary>
    public byte OpenFlag { get; private set; }

    /// <summary>High byte at WRAM `$0AA7`: one while either transition is incomplete.</summary>
    public byte CloseFlag { get; private set; }

    /// <summary>WRAM `$0AA8`; zero is invisible, one/two transition, three fully open.</summary>
    public ushort Frame { get; private set; }

    /// <summary>WRAM `$0AAA`; the HUD selection producer saturates this at two.</summary>
    public ushort ToggleFlag { get; private set; }

    /// <summary>Pose-record byte one copied to WRAM `$0AAC` on every draw pass.</summary>
    public ushort DrawingMode { get; private set; }

    /// <summary>
    /// Low-nibble dispatch value consumed by `$90:EB55/$90:EBA3`. The full source byte is
    /// retained above because `$90:C5E0-$C5E6` stores it unmasked; only the drawing handler
    /// applies `$000F` before choosing mode zero, one, or two.
    /// </summary>
    public ushort EffectiveDrawingMode => unchecked((ushort)(DrawingMode & 0x000f));

    /// <summary>
    /// Runs the HUD selection stability update and `HandleArmCannonOpenState`. Calling once
    /// per gameplay frame reproduces `$90:C519-$C534` followed later by `$90:C5C4`.
    /// </summary>
    public SamusArmCannonUpdateResult Update(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.SelectedHudItem > 5)
            throw new InvalidDataException($"HUD item {samus.SelectedHudItem} is outside native range 0..5.");

        bool itemChanged = samus.SelectedHudItem != _previousSelectedHudItem;
        if (itemChanged)
        {
            ToggleFlag = 1;
            _previousSelectedHudItem = samus.SelectedHudItem;
        }
        else
        {
            ushort incremented = unchecked((ushort)(ToggleFlag + 1));
            ToggleFlag = incremented < 3 ? incremented : (ushort)2;
        }

        ushort frameBefore = Frame;
        bool transitionStarted = false;
        if (CloseFlag != 0 || (transitionStarted = TryStartTransition(bus, samus.SelectedHudItem)))
            AdvanceFrame();

        ushort drawingData = ReadWord(
            bus,
            SamusRenderingRomData.ArmCannon.PoseDrawingDataPointers + samus.Pose * 2);
        DrawingMode = bus.ReadByte(
            (int)new SnesAddress(
                SamusRenderingRomData.Banks.MovementNumber,
                unchecked((ushort)(drawingData + 1))));
        return new SamusArmCannonUpdateResult(
            itemChanged,
            transitionStarted,
            frameBefore,
            Frame,
            OpenFlag,
            CloseFlag,
            DrawingMode,
            drawingData);
    }

    /// <summary>
    /// Draws the direction-specific small OBJ and queues its exact 32-byte bank-$9A tile
    /// upload. The caller chooses before/after-body order from <see cref="DrawingMode"/>.
    /// </summary>
    public SamusArmCannonDrawResult Draw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        VramWriteQueue vramWrites,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(vramWrites);
        ArgumentNullException.ThrowIfNull(samus);

        // The cannon shares Samus's invincibility flicker, and a closed frame performs
        // neither OAM nor DMA work. Native returns before even consulting pose data.
        if (Frame == 0 || (samus.InvincibilityTimer != 0 && (nmiFrameCounter & 1) != 0))
            return new SamusArmCannonDrawResult(false, false, Frame);

        ushort drawingData = ReadWord(
            bus,
            SamusRenderingRomData.ArmCannon.PoseDrawingDataPointers + samus.Pose * 2);
        byte firstSelector = bus.ReadByte((int)new SnesAddress(
            SamusRenderingRomData.Banks.MovementNumber,
            drawingData));
        bool frameDependentSelector = (firstSelector & 0x80) != 0;
        byte selector = frameDependentSelector && samus.AnimationFrame != 0
            ? unchecked((byte)(bus.ReadByte((int)new SnesAddress(
                SamusRenderingRomData.Banks.MovementNumber,
                unchecked((ushort)(drawingData + 2)))) & 0x7f))
            : unchecked((byte)(firstSelector & 0x7f));
        if (selector >= SamusRenderingRomData.ArmCannon.DirectionCount)
            throw new InvalidDataException($"Arm-cannon direction selector {selector} is outside 0..9.");

        ushort offsetsBase = unchecked((ushort)(drawingData +
            (frameDependentSelector ? 4 : 2)));
        int offsetAddress = SamusRenderingRomData.Banks.Movement | unchecked((ushort)(
            offsetsBase + samus.AnimationFrame * 2));
        sbyte xOffset = unchecked((sbyte)bus.ReadByte(offsetAddress));
        sbyte yOffset = unchecked((sbyte)bus.ReadByte(offsetAddress + 1));
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        short screenX = unchecked((short)(samus.XPosition + xOffset - layer1X));
        short screenY = unchecked((short)(
            samus.YPosition + yOffset - graphicsYOffset - layer1Y));
        bool spriteWritten = screenX >= 0 && screenX < 256 && screenY >= 0 && screenY < 256;
        ushort attributes = ReadWord(
            bus,
            SamusRenderingRomData.ArmCannon.SpriteAttributes + selector * 2);
        if (spriteWritten)
        {
            oam.AddRawSmallSprite(
                unchecked((ushort)screenX),
                unchecked((ushort)screenY),
                attributes);
        }

        // Selector -> one of four orientation lists -> current cover frame -> bank-$9A
        // source. Entry zero is intentionally null but cannot be reached because Frame zero
        // returned above. Destination `$61F0` is the tile-$1F slot used by the OAM word.
        ushort tileList = ReadWord(
            bus,
            SamusRenderingRomData.ArmCannon.TileListPointers + selector * 2);
        ushort tileSource = ReadWord(
            bus,
            SamusRenderingRomData.Banks.Movement |
                unchecked((ushort)(tileList + Frame * 2)));
        vramWrites.Enqueue(
            sizeInBytes: SamusRenderingRomData.ArmCannon.TileUploadByteCount,
            sourceAddress: SamusRenderingRomData.Banks.CharacterData | tileSource,
            encodedVramDestination: SamusRenderingRomData.ArmCannon.TileVramDestination);

        return new SamusArmCannonDrawResult(
            spriteWritten,
            TileUploadQueued: true,
            Frame,
            selector,
            attributes,
            tileSource,
            screenX,
            screenY);
    }

    private bool TryStartTransition(ISnesAddressSpace bus, ushort selectedHudItem)
    {
        if (ToggleFlag < 2)
            return false;

        byte desiredOpenFlag = bus.ReadByte(
            SamusRenderingRomData.ArmCannon.OpenFlags + selectedHudItem);
        if (OpenFlag == desiredOpenFlag)
            return false;

        // The packed native word write changes the low open byte and sets high close byte
        // one simultaneously. Closing begins from synthetic frame four; the same call then
        // advances it to visible frame three.
        Frame = desiredOpenFlag != 0 ? (ushort)0 : (ushort)4;
        OpenFlag = desiredOpenFlag;
        CloseFlag = 1;
        return true;
    }

    private void AdvanceFrame()
    {
        if (OpenFlag != 0)
        {
            ushort incremented = unchecked((ushort)(Frame + 1));
            if (unchecked((short)(incremented - 3)) < 0)
            {
                Frame = incremented;
                return;
            }

            Frame = 3;
        }
        else
        {
            ushort decremented = unchecked((ushort)(Frame - 1));
            if (decremented != 0 && unchecked((short)decremented) >= 0)
            {
                Frame = decremented;
                return;
            }

            Frame = 0;
        }

        // `$90:C658` stores the low byte as a 16-bit word, clearing only the transition
        // byte while retaining the desired open state.
        CloseFlag = 0;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address)
    {
        SnesAddress source = SnesAddress.FromBusAddress(address);
        return unchecked((ushort)(
            bus.ReadByte((int)source) |
            (bus.ReadByte((int)source.AddWithinBank(1)) << 8)));
    }
}

/// <summary>Debugger witness from the once-per-frame arm-cannon state update.</summary>
public readonly record struct SamusArmCannonUpdateResult(
    bool HudItemChanged,
    bool TransitionStarted,
    ushort FrameBefore,
    ushort FrameAfter,
    byte OpenFlag,
    byte CloseFlag,
    ushort DrawingMode,
    ushort DrawingDataPointer);

/// <summary>Debugger witness from one optional arm-cannon OBJ/tile upload.</summary>
public readonly record struct SamusArmCannonDrawResult(
    bool SpriteWritten,
    bool TileUploadQueued,
    ushort Frame,
    byte DirectionSelector = 0,
    ushort Attributes = 0,
    ushort TileSource = 0,
    short ScreenX = 0,
    short ScreenY = 0);
