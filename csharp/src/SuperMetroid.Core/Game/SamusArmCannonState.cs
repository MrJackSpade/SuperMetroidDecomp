using SuperMetroid.Core.Assets;
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
    /// <summary>HUD selection observed on the preceding update, used to restart the native stability counter on change.</summary>
    private ushort _previousSelectedHudItem;

    /// <summary>Host-owned visual data; rebound after a restored debugger state.</summary>
    [field: NonSerialized]
    public SamusArmCannonArtworkCatalog? Artwork { get; set; }

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
        if (samus.SelectedHudItem >= SamusArmCannonDefinitions.HudItemCount)
        {
            throw new InvalidDataException(
                $"HUD item {samus.SelectedHudItem} is outside native range " +
                $"0..{SamusArmCannonDefinitions.HudItemCount - 1}.");
        }

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
        if (CloseFlag != 0 || (transitionStarted = TryStartTransition(samus.SelectedHudItem)))
            AdvanceFrame();

        ushort drawingData = PoseDrawingData(bus, samus.Pose);
        DrawingMode = ReadDrawingByte(bus, unchecked((ushort)(drawingData + 1)));
        return new SamusArmCannonUpdateResult();
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
            return new SamusArmCannonDrawResult();

        ushort drawingData = PoseDrawingData(bus, samus.Pose);
        byte firstSelector = ReadDrawingByte(bus, drawingData);
        bool frameDependentSelector = (firstSelector & 0x80) != 0;
        byte selector = frameDependentSelector && samus.AnimationFrame != 0
            ? unchecked((byte)(ReadDrawingByte(bus,
                unchecked((ushort)(drawingData + 2))) & 0x7f))
            : unchecked((byte)(firstSelector & 0x7f));
        if (selector >= SamusRenderingRomData.ArmCannon.DirectionCount)
            throw new InvalidDataException($"Arm-cannon direction selector {selector} is outside 0..9.");

        ushort offsetsBase = unchecked((ushort)(drawingData +
            (frameDependentSelector ? 4 : 2)));
        ushort offsetAddress = unchecked((ushort)(offsetsBase + samus.AnimationFrame * 2));
        sbyte xOffset = unchecked((sbyte)ReadDrawingByte(bus, offsetAddress));
        sbyte yOffset = unchecked((sbyte)ReadDrawingByte(bus,
            unchecked((ushort)(offsetAddress + 1))));
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        short screenX = unchecked((short)(samus.XPosition + xOffset - layer1X));
        short screenY = unchecked((short)(
            samus.YPosition + yOffset - graphicsYOffset - layer1Y));
        bool spriteWritten = screenX >= 0 && screenX < 256 && screenY >= 0 && screenY < 256;
        SamusArmCannonArtworkCatalog artwork = Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed artwork.");
        ushort attributes = artwork.SpriteAttributes(selector);
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
        ushort tileSource = artwork.TileSource(selector, Frame);
        vramWrites.Enqueue(
            sizeInBytes: SamusRenderingRomData.ArmCannon.TileUploadByteCount,
            sourceAddress: SamusRenderingRomData.Banks.CharacterData | tileSource,
            encodedVramDestination: SamusRenderingRomData.ArmCannon.TileVramDestination);

        return new SamusArmCannonDrawResult();
    }

    /// <summary>Starts opening or closing the cover once the HUD selection is stable and its desired state differs.</summary>
    /// <param name="selectedHudItem">Validated HUD selection whose cover policy should be applied.</param>
    /// <returns><see langword="true"/> when a new transition was initialized.</returns>
    private bool TryStartTransition(ushort selectedHudItem)
    {
        if (ToggleFlag < 2)
            return false;

        byte desiredOpenFlag = SamusArmCannonDefinitions.DesiredOpenFlag(selectedHudItem);
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

    /// <summary>Gets the installed bank-$90 arm-cannon drawing descriptor for Samus's current pose.</summary>
    /// <param name="bus">Gameplay address space passed by the caller; the pointer is selected from installed artwork data.</param>
    /// <param name="pose">Samus body pose whose descriptor is required.</param>
    /// <returns>The bank-relative pointer to the pose's drawing descriptor.</returns>
    private ushort PoseDrawingData(ISnesAddressSpace bus, int pose) =>
        (Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed drawing definitions.")).PoseDrawingData(pose);

    /// <summary>Resolves a byte from the installed bank-$90 arm-cannon drawing descriptor data.</summary>
    /// <param name="bus">Gameplay address space passed by the caller; the byte is resolved from installed artwork data.</param>
    /// <param name="address">Bank-relative drawing-data address.</param>
    /// <returns>The descriptor byte selected by the artwork catalog.</returns>
    private byte ReadDrawingByte(ISnesAddressSpace bus, ushort address) =>
        (Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed drawing definitions.")).ReadDrawingByte(address);

    /// <summary>Advances the cover animation toward its open or closed endpoint and clears the transition flag at completion.</summary>
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

}

/// <summary>Debugger witness from the once-per-frame arm-cannon state update.</summary>
public readonly record struct SamusArmCannonUpdateResult();

/// <summary>Debugger witness from one optional arm-cannon OBJ/tile upload.</summary>
public readonly record struct SamusArmCannonDrawResult();
