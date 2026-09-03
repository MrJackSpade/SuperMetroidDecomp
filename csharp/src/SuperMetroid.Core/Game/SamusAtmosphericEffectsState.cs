using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The four packed atmospheric-graphics slots at WRAM <c>$0A8D-$0AB4</c> and their
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
    public const int SlotCount = 4;

    private const int AnimationTimerPointerTable = 0x908b93;
    private const int AnimationFrameCountTable = 0x908bef;
    private const int DirectSpriteAttributePointerTable = 0x908bff;

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
    public void ClearFrameAndType(int slotIndex)
    {
        ValidateSlotIndex(slotIndex);
        _slots[slotIndex].FrameAndType = 0;
    }

    /// <summary>
    /// Ports <c>Handle_AtmosphericEffects</c> at <c>$90:8A4C-$8C1E</c>, including timer
    /// underflow, reverse slot order, per-type motion, clipping, and the two OAM paths.
    /// </summary>
    public void UpdateAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        ushort fxYPosition)
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
                slot.AnimationTimer = ReadFrameTimer(bus, type, frameBeforeTimer);
                slot.FrameAndType = unchecked((ushort)(slot.FrameAndType + 1));
                if (slot.AnimationFrame >= ReadFrameCount(bus, type))
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
                slot.AnimationTimer = ReadFrameTimer(bus, type, frameBeforeTimer);
            }

            switch (type)
            {
                case 1:
                case 2:
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY);
                    break;

                case 3:
                    // Diving splash graphics remain pinned to the live water surface even
                    // if room FX moves it while the nine-frame animation is running.
                    slot.YPosition = fxYPosition;
                    DrawSamusTableSpritemap(bus, oam, slot, 0x018f, cameraX, cameraY);
                    break;

                case 4:
                    // Slots 0/1 drift apart to the right and slots 2/3 to the left. Native
                    // tests bit two of the byte offset (0,2,4,6), equivalent to index >= 2.
                    slot.XPosition = slotIndex >= 2
                        ? unchecked((ushort)(slot.XPosition - 1))
                        : unchecked((ushort)(slot.XPosition + 1));
                    slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY);
                    break;

                case 5:
                    DrawSamusTableSpritemap(bus, oam, slot, 0x0186, cameraX, cameraY);
                    break;

                case 6:
                case 7:
                    slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                    DrawDirectSmallSprite(bus, oam, slot, type, cameraX, cameraY);
                    break;

                default:
                    // Type zero is the inactive word and was filtered above. The producer
                    // validates 0..7, so reaching this branch means externally corrupted state.
                    throw new InvalidDataException($"Unsupported atmospheric graphics type {type}.");
            }
        }
    }

    private static void DrawDirectSmallSprite(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusAtmosphericEffectSlot slot,
        byte type,
        ushort cameraX,
        ushort cameraY)
    {
        short screenX = unchecked((short)(slot.XPosition - cameraX - 4));
        short screenY = unchecked((short)(slot.YPosition - cameraY - 4));
        if (screenX < 0 || screenX >= 0x0100 || screenY < 0 || screenY >= 0x0100)
            return;

        // `$90:8BFF` is a pointer table. Type two's retail pointer is literally zero; do
        // not silently alias it to type one if a debugger deliberately creates that slot.
        ushort attributeList = ReadWord(bus, DirectSpriteAttributePointerTable + type * 2);
        ushort attributes = ReadWord(
            bus,
            0x900000 | unchecked((ushort)(attributeList + slot.AnimationFrame * 2)));
        oam.AddRawSmallSprite(unchecked((ushort)screenX), unchecked((ushort)screenY), attributes);
    }

    private static void DrawSamusTableSpritemap(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusAtmosphericEffectSlot slot,
        ushort firstSpritemap,
        ushort cameraX,
        ushort cameraY)
    {
        ushort screenX = unchecked((ushort)(slot.XPosition - cameraX));
        ushort screenY = unchecked((ushort)(slot.YPosition - cameraY));

        // `$90:8B83` rejects any nonzero high byte of Y. X is deliberately not clipped;
        // AddSamusSpritemap preserves its ninth hardware coordinate bit and wrap behavior.
        if ((screenY & 0xff00) != 0)
            return;

        oam.AddSamusSpritemap(
            bus,
            unchecked((ushort)(firstSpritemap + slot.AnimationFrame)),
            screenX,
            screenY);
    }

    private static ushort ReadFrameTimer(ISnesAddressSpace bus, byte type, byte frame)
    {
        ushort timerList = ReadWord(bus, AnimationTimerPointerTable + type * 2);
        return ReadWord(bus, 0x900000 | unchecked((ushort)(timerList + frame * 2)));
    }

    private static ushort ReadFrameCount(ISnesAddressSpace bus, byte type) =>
        ReadWord(bus, AnimationFrameCountTable + type * 2);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

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

    public byte AnimationFrame => unchecked((byte)FrameAndType);

    public byte Type => unchecked((byte)(FrameAndType >> 8));

    public ushort AnimationTimer { get; internal set; }

    public ushort XPosition { get; internal set; }

    public ushort YPosition { get; internal set; }

    internal void Clear()
    {
        FrameAndType = 0;
        AnimationTimer = 0;
        XPosition = 0;
        YPosition = 0;
    }
}

/// <summary>One exact request to a native sound-library queue.</summary>
public readonly record struct SamusSoundRequest(
    SoundEffectLibrary Library,
    byte SoundId,
    byte MaximumQueued);
