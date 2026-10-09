using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The four packed atmospheric-graphics slots at WRAM <c>$0AD4-$0AF3</c> and their
/// bank-$90 producer/animation/draw logic.
/// </summary>
/// <remarks>
/// These are not host particles. Water entry, bubbles, lava spray, footsteps, and boost
/// dust all share four fixed cartridge slots. Each slot stores its animation frame in the
/// low byte and its type in the high byte; the deliberately strange <c>$8002</c> timers are
/// signed delay sentinels. Preserving those words makes the one-frame staggering and slot
/// contention visible in the debugger exactly as they are in WRAM.
/// </remarks>
public sealed class SamusAtmosphericEffectsState
{
    /// <summary>Four shared slots, represented by four words in each native timer, X, Y, and packed frame/type array.</summary>
    public const int SlotCount = 4;

    /// <summary>Mutable models of the four native atmospheric-effect records, indexed in WRAM slot order.</summary>
    private readonly SamusAtmosphericEffectSlot[] _slots =
    [
        new(),
        new(),
        new(),
        new(),
    ];

    /// <summary>Read-only debugger view of the four native slots, in WRAM order.</summary>
    public IReadOnlyList<SamusAtmosphericEffectSlot> Slots => _slots;

    /// <summary>Clears every slot as room/Samus initialization clears the backing WRAM.</summary>
    public void Clear()
    {
        foreach (SamusAtmosphericEffectSlot slot in _slots)
            slot.Clear();
    }

    /// <summary>
    /// Installs a literal native slot. Public visibility is intentional: deterministic tests
    /// and debugger experiments can seed the exact packed state without a fake water room.
    /// </summary>
    /// <param name="slotIndex">Zero-based slot index, 0..3, corresponding to native byte offsets 0, 2, 4, and 6.</param>
    /// <param name="type">Native graphics type 0..7; a zero packed frame/type word is inactive.</param>
    /// <param name="animationFrame">Low-byte animation index; interpreted against the selected type when updated.</param>
    /// <param name="animationTimer">Literal native countdown word, including signed delayed-start sentinels such as $8002.</param>
    /// <param name="worldX">Room-space horizontal anchor in whole pixels, stored with native 16-bit wrapping.</param>
    /// <param name="worldY">Room-space vertical anchor in whole pixels, stored with native 16-bit wrapping.</param>
    public void SetSlot(
        int slotIndex,
        byte type,
        byte animationFrame,
        ushort animationTimer,
        ushort worldX,
        ushort worldY)
    {
        ValidateSlotIndex(slotIndex);
        if (type > 7)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Atmospheric type must fit the native 0..7 table.");

        SamusAtmosphericEffectSlot slot = _slots[slotIndex];
        slot.FrameAndType = unchecked((ushort)((type << 8) | animationFrame));
        slot.AnimationTimer = animationTimer;
        slot.XPosition = worldX;
        slot.YPosition = worldY;
    }

    /// <summary>
    /// Clears only a slot's packed frame/type word, exactly like the landing-graphics delete
    /// paths at <c>$91:F0BE/$91:F1CC</c>. The cartridge deliberately leaves the timer and
    /// coordinates behind; retaining that stale debugger-visible state matters when proving
    /// that a later producer really initialized every field it owns.
    /// </summary>
    /// <param name="slotIndex">Zero-based slot index, 0..3; its timer and coordinates are not cleared.</param>
    public void ClearFrameAndType(int slotIndex)
    {
        ValidateSlotIndex(slotIndex);
        _slots[slotIndex].FrameAndType = 0;
    }

    /// <summary>
    /// Ports <c>Handle_AtmosphericEffects</c> at <c>$90:8A4C-$8C1E</c>, including timer
    /// underflow, reverse slot order, per-type motion, clipping, and the two OAM paths.
    /// </summary>
    /// <param name="bus">Address space providing mirrored WRAM for type two attributes and the Samus spritemap path.</param>
    /// <param name="oam">OAM buffer to which visible sprites are appended, with slot three processed first.</param>
    /// <param name="cameraX">Room-space camera X origin in whole pixels.</param>
    /// <param name="cameraY">Room-space camera Y origin in whole pixels.</param>
    /// <param name="fxYPosition">Live liquid-surface Y in room pixels, used to anchor the diving splash on each update.</param>
    /// <param name="artwork">Installed Samus spritemaps required when visible diving splashes or bubbles use the spritemap draw path.</param>
    /// <param name="directArtwork">Installed small-sprite attributes required for visible direct effects other than type two's WRAM-backed attributes.</param>
    public void UpdateAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        ushort fxYPosition,
        SamusSpritemapArtworkCatalog? artwork = null,
        SamusAtmosphericArtworkCatalog? directArtwork = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        // Native Y begins at byte offset six and decrements by two. Slot three therefore
        // receives the earlier/lower OAM index whenever multiple effects overlap.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusAtmosphericEffectSlot slot = _slots[slotIndex];
            if (slot.FrameAndType == 0)
                continue;

            byte type = slot.Type;
            byte frameBeforeTimer = slot.AnimationFrame;
            slot.AnimationTimer = unchecked((ushort)(slot.AnimationTimer - 1));

            if (slot.AnimationTimer == 0)
            {
                // `$90:8A86` reloads using the OLD frame and only then increments the
                // packed word. That ordering is why a newly created timer-three frame is
                // visible for two calls before frame one begins.
                slot.AnimationTimer =
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frameBeforeTimer);
                slot.FrameAndType = unchecked((ushort)(slot.FrameAndType + 1));
                if (slot.AnimationFrame >=
                    SamusAtmosphericAnimationDefinitions.FrameCount(type))
                {
                    slot.FrameAndType = 0;
                    continue;
                }
            }
            else if (unchecked((short)slot.AnimationTimer) < 0)
            {
                // Negative timers suppress both drawing and motion. Exactly `$8000` is a
                // delayed-start marker: reload the current frame's ROM duration and draw it.
                if (slot.AnimationTimer != 0x8000)
                    continue;
                slot.AnimationTimer =
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frameBeforeTimer);
            }

            switch (type)
            {
                case 1:
                case 2:
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY, directArtwork);
                    break;

                case 3:
                    // Diving splash graphics remain pinned to the live water surface even
                    // if room FX moves it while the nine-frame animation is running.
                    slot.YPosition = fxYPosition;
                    DrawSamusTableSpritemap(bus, oam, slot, 0x018f, cameraX, cameraY, artwork);
                    break;

                case 4:
                    // Slots 0/1 drift apart to the right and slots 2/3 to the left. Native
                    // tests bit two of the byte offset (0,2,4,6), equivalent to index >= 2.
                    slot.XPosition = slotIndex >= 2
                        ? unchecked((ushort)(slot.XPosition - 1))
                        : unchecked((ushort)(slot.XPosition + 1));
                    slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY, directArtwork);
                    break;

                case 5:
                    DrawSamusTableSpritemap(bus, oam, slot, 0x0186, cameraX, cameraY, artwork);
                    break;

                case 6:
                case 7:
                    slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY, directArtwork);
                    break;

                default:
                    // Type zero is the inactive word and was filtered above. The producer
                    // validates 0..7, so reaching this branch means externally corrupted state.
                    throw new InvalidDataException($"Unsupported atmospheric graphics type {type}.");
            }
        }
    }

    /// <summary>Clips and emits one direct 8-by-8 atmospheric sprite using live type-two attributes or installed artwork.</summary>
    /// <param name="bus">Address space used for type-two WRAM-backed sprite attributes.</param>
    /// <param name="oam">OAM buffer receiving the sprite when its screen position is visible.</param>
    /// <param name="slot">Effect state providing the room-space anchor and animation frame.</param>
    /// <param name="type">Native effect type selecting the attribute source.</param>
    /// <param name="cameraX">Room-space camera origin subtracted from the horizontal anchor.</param>
    /// <param name="cameraY">Room-space camera origin subtracted from the vertical anchor.</param>
    /// <param name="directArtwork">Installed attributes for direct sprite types other than type two.</param>
    private static void DrawDirectSmallSprite(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusAtmosphericEffectSlot slot,
        byte type,
        ushort cameraX,
        ushort cameraY,
        SamusAtmosphericArtworkCatalog? directArtwork)
    {
        short screenX = unchecked((short)(slot.XPosition - cameraX - 4));
        short screenY = unchecked((short)(slot.YPosition - cameraY - 4));
        if (screenX < 0 || screenX >= 0x0100 || screenY < 0 || screenY >= 0x0100)
            return;

        // Type two's native list pointer is zero. In bank $90 that address mirrors
        // live WRAM, so it is not an art asset and must retain its mutable read.
        ushort attributes = type == 2
            ? ReadTypeTwoWorkRamAttributes(bus, slot.AnimationFrame)
            : ResolveInstalledAttributes(directArtwork, type, slot.AnimationFrame);
        oam.AddRawSmallSprite(unchecked((ushort)screenX), unchecked((ushort)screenY), attributes);
    }

    /// <summary>Reads the two-byte, frame-indexed type-two sprite attributes from mirrored movement WRAM.</summary>
    /// <param name="bus">Address space that must expose mutable WRAM reads.</param>
    /// <param name="frame">Animation frame selecting the attribute word.</param>
    /// <returns>The packed OAM attribute word read from WRAM.</returns>
    /// <exception cref="InvalidOperationException">The address space does not provide mutable WRAM access.</exception>
    private static ushort ReadTypeTwoWorkRamAttributes(ISnesAddressSpace bus, byte frame)
    {
        ISnesMutableMemory memory = bus as ISnesMutableMemory ?? throw new InvalidOperationException(
            "Atmospheric type two requires mirrored WRAM.");
        int source = SamusMovementRomData.Banks.Movement | (frame * sizeof(ushort));
        return (ushort)(memory.ReadWorkRamByte(source) |
            memory.ReadWorkRamByte(source + 1) << 8);
    }

    /// <summary>Resolves a direct effect's frame attributes from the installed artwork catalog.</summary>
    /// <param name="artwork">Catalog containing the direct sprite definitions.</param>
    /// <param name="type">Native atmospheric graphics type.</param>
    /// <param name="frame">Frame within that type's animation.</param>
    /// <returns>The packed OAM attribute word for the requested sprite.</returns>
    /// <exception cref="InvalidOperationException">No artwork catalog is installed.</exception>
    /// <exception cref="InvalidDataException">The catalog has no entry for the requested type and frame.</exception>
    private static ushort ResolveInstalledAttributes(
        SamusAtmosphericArtworkCatalog? artwork, byte type, byte frame)
    {
        if (artwork is null)
            throw new InvalidOperationException(
                $"Atmospheric type {type} requires installed small-sprite artwork.");
        if (!artwork.TryResolve(type, frame, out ushort attributes))
            throw new InvalidDataException(
                $"Atmospheric type {type} frame {frame} is outside the installed sprite artwork.");
        return attributes;
    }

    /// <summary>Draws one frame from a Samus spritemap table at the effect's camera-relative anchor.</summary>
    /// <param name="bus">Address space providing the mutable WRAM required by spritemap decoding.</param>
    /// <param name="oam">OAM buffer receiving the decoded spritemap pieces.</param>
    /// <param name="slot">Effect state providing the world anchor and frame index.</param>
    /// <param name="firstSpritemap">First table entry; the current frame is added to select the entry.</param>
    /// <param name="cameraX">Room-space camera origin subtracted from the horizontal anchor.</param>
    /// <param name="cameraY">Room-space camera origin subtracted from the vertical anchor.</param>
    /// <param name="artwork">Installed sprite definitions used to decode the selected table entry.</param>
    private static void DrawSamusTableSpritemap(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusAtmosphericEffectSlot slot,
        ushort firstSpritemap,
        ushort cameraX,
        ushort cameraY,
        SamusSpritemapArtworkCatalog? artwork)
    {
        ushort screenX = unchecked((ushort)(slot.XPosition - cameraX));
        ushort screenY = unchecked((ushort)(slot.YPosition - cameraY));

        // `$90:8B83` rejects any nonzero high byte of Y. X is deliberately not clipped;
        // AddSamusSpritemap preserves its ninth hardware coordinate bit and wrap behavior.
        if ((screenY & 0xff00) != 0)
            return;

        oam.AddSamusSpritemap(
            bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Samus atmospheric spritemap requires WRAM."),
            unchecked((ushort)(firstSpritemap + slot.AnimationFrame)),
            screenX,
            screenY,
            artwork ?? throw new InvalidOperationException(
                "Samus atmospheric spritemap requires installed artwork."));
    }

    /// <summary>Ensures an index addresses one of the four native atmospheric-effect slots.</summary>
    /// <param name="slotIndex">Zero-based slot index to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the slot array.</exception>
    private static void ValidateSlotIndex(int slotIndex)
    {
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }
}

/// <summary>One debugger-visible packed atmospheric slot.</summary>
public sealed class SamusAtmosphericEffectSlot
{
    /// <summary>Low byte animation frame, high byte graphics type.</summary>
    public ushort FrameAndType { get; internal set; }

    /// <summary>Low byte of the $0AEC array's packed word: the zero-based frame within the current graphics type.</summary>
    public byte AnimationFrame => unchecked((byte)FrameAndType);

    /// <summary>High byte of the packed word: 1/2 footstep splashes, 3 diving splash, 4 lava spray, 5 bubbles, or 6/7 dust; zero is reserved for inactive state.</summary>
    public byte Type => unchecked((byte)(FrameAndType >> 8));

    /// <summary>$0AD4 array countdown in update calls; negative words suppress motion/drawing until decrement reaches $8000 and reloads the current frame duration.</summary>
    public ushort AnimationTimer { get; internal set; }

    /// <summary>$0ADC array horizontal anchor in whole room pixels; camera subtraction occurs only when drawing, and motion wraps as a native word.</summary>
    public ushort XPosition { get; internal set; }

    /// <summary>$0AE4 array vertical anchor in whole room pixels; diving splashes follow the live FX surface and lava spray/dust rise one pixel per active update.</summary>
    public ushort YPosition { get; internal set; }

    /// <summary>Resets the packed activity/type word, timer, and both room-space coordinates.</summary>
    internal void Clear()
    {
        FrameAndType = 0;
        AnimationTimer = 0;
        XPosition = 0;
        YPosition = 0;
    }
}

/// <summary>One exact request to a native sound-library queue.</summary>
/// <param name="SoundEffect">Cartridge library and sound identifier.</param>
/// <param name="MaximumQueued">Native queue occupancy threshold.</param>
/// <param name="SoundSuppressed">Queue guard captured by the producer, before any later state changes.</param>
public readonly record struct SamusSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued,
    bool SoundSuppressed = false);
