using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

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
    /// <summary>Exactly $8000: a delayed-start timer, which reloads the current frame's duration and draws it.</summary>
    internal const ushort DelayedStartMarker = 1 << 15;

    /// <summary>Four shared slots, represented by four words in each native timer, X, Y, and packed frame/type array.</summary>
    public const int SlotCount = 4;

    /// <summary>Fixed WRAM-order storage shared by splashes, bubbles, lava spray, and boost dust.</summary>
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
    /// <param name="type">The effect; an inactive effect with frame zero leaves the slot empty.</param>
    /// <param name="animationFrame">Low-byte animation index; interpreted against the selected type when updated.</param>
    /// <param name="animationTimer">Literal native countdown word, including signed delayed-start sentinels such as $8002.</param>
    /// <param name="anchor">Room-space anchor in whole pixels, stored with native 16-bit wrapping.</param>
    public void SetSlot(
        int slotIndex,
        SamusAtmosphericEffectType type,
        int animationFrame,
        int animationTimer,
        Position16<RoomPixels> anchor)
    {
        ValidateSlotIndex(slotIndex);
        SamusAtmosphericEffectSlot slot = _slots[slotIndex];
        slot.SetFrameAndType(type, animationFrame);
        slot.AnimationTimer = WrappingCountdown.FromRaw(animationTimer);
        slot.Position = anchor;
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
    /// <param name="sprites">Frame receiving visible effects as draws, with slot three processed first.</param>
    /// <param name="camera">Room-space camera X and Y origin in whole pixels.</param>
    /// <param name="fxYPosition">Live liquid-surface Y in room pixels, used to anchor the diving splash on each update.</param>
    /// <param name="sheets">The installed sheets every visible effect draws from; required when one is visible.</param>
    public void UpdateAndDraw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        int fxYPosition,
        InstalledSpriteSheets? sheets = null)
    {
        Ensure.NotNull(sprites);
        var drawing = new AtmosphereDrawing(sprites, camera, sheets);

        // Native Y begins at byte offset six and decrements by two. Slot three therefore
        // receives the earlier/lower OAM index whenever multiple effects overlap.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusAtmosphericEffectSlot slot = _slots[slotIndex];
            if (slot.FrameAndType == 0)
                continue;

            SamusAtmosphericEffectType type = slot.Type;
            int frameBeforeTimer = slot.AnimationFrame;
            slot.AnimationTimer = slot.AnimationTimer.Decrement().Remaining;

            if (slot.AnimationTimer.IsZero)
            {
                // `$90:8A86` reloads using the OLD frame and only then increments the
                // packed word. That ordering is why a newly created timer-three frame is
                // visible for two calls before frame one begins.
                slot.AnimationTimer =
                    WrappingCountdown.FromRaw(SamusAtmosphericAnimationDefinitions.FrameTimer(type, frameBeforeTimer));
                slot.FrameAndType = unchecked((ushort)(slot.FrameAndType + 1));
                if (slot.AnimationFrame >=
                    SamusAtmosphericAnimationDefinitions.FrameCount(type))
                {
                    slot.FrameAndType = 0;
                    continue;
                }
            }
            else if (slot.AnimationTimer.IsNegative)
            {
                // Negative timers suppress both drawing and motion. Exactly `$8000` is a
                // delayed-start marker: reload the current frame's ROM duration and draw it.
                if (slot.AnimationTimer.RawValue != DelayedStartMarker)
                    continue;
                slot.AnimationTimer =
                    WrappingCountdown.FromRaw(SamusAtmosphericAnimationDefinitions.FrameTimer(type, frameBeforeTimer));
            }

            switch (type)
            {
                case SamusAtmosphericEffectType.FootstepSplash:
                case SamusAtmosphericEffectType.AlternateFootstepSplash:
                    DrawDirectSmallSprite(drawing, slot, slotIndex, type, SamusAtmosphereSheetDefinitions.FootstepFrame(slot.AnimationFrame));
                    break;

                case SamusAtmosphericEffectType.DivingSplash:
                    // Diving splash graphics remain pinned to the live water surface even
                    // if room FX moves it while the nine-frame animation is running.
                    slot.Position = slot.Position.WithY(fxYPosition);
                    DrawSamusTableSpritemap(drawing, slot, slotIndex, SamusAtmosphereSheetDefinitions.DivingSplashFrame(slot.AnimationFrame));
                    break;

                case SamusAtmosphericEffectType.LavaSurfaceDamage:
                    // Slots 0/1 drift apart to the right and slots 2/3 to the left. Native
                    // tests bit two of the byte offset (0,2,4,6), equivalent to index >= 2.
                    slot.Position += new Offset16<RoomPixels>(slotIndex >= 2 ? (short)-1 : (short)1, -1);
                    DrawDirectSmallSprite(drawing, slot, slotIndex, type, SamusAtmosphereSheetDefinitions.LavaAndDustFrame(slot.AnimationFrame));
                    break;

                case SamusAtmosphericEffectType.Bubbles:
                    DrawSamusTableSpritemap(drawing, slot, slotIndex, SamusAtmosphereSheetDefinitions.BubblesFrame(slot.AnimationFrame));
                    break;

                case SamusAtmosphericEffectType.Dust:
                case SamusAtmosphericEffectType.AlternateDust:
                    slot.Position -= new Offset16<RoomPixels>(0, 1);
                    DrawDirectSmallSprite(drawing, slot, slotIndex, type, SamusAtmosphereSheetDefinitions.LavaAndDustFrame(slot.AnimationFrame));
                    break;

                case SamusAtmosphericEffectType.Inactive:
                default:
                    // An inactive slot has a zero packed word and was filtered above. The producer
                    // validates 0..7, so reaching this branch means externally corrupted state.
                    throw new InvalidDataException($"Unsupported atmospheric graphics type {type}.");
            }
        }
    }

    /// <summary>Clips an eight-pixel effect to the 256-pixel OAM viewport, then draws the sheet frame of its installed attributes.</summary>
    private static void DrawDirectSmallSprite(
        AtmosphereDrawing drawing,
        SamusAtmosphericEffectSlot slot,
        int slotIndex,
        SamusAtmosphericEffectType type,
        string frame)
    {
        int screenX = unchecked((short)(slot.Position.X - drawing.Camera.X - 4));
        int screenY = unchecked((short)(slot.Position.Y - drawing.Camera.Y - 4));
        if (screenX < 0 || screenX >= 256 || screenY < 0 || screenY >= 256)
            return;

        // Type two's native list pointer is zero, so bank $90 reads direct-page scratch as its sprite attributes. That
        // needs direct-page emulation the port does not model; type two is exempt and draws nothing (owner decision
        // on #1318).
        if (type == SamusAtmosphericEffectDefinitions.ScratchListType)
            return;
        drawing.Sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusAtmosphere, slotIndex),
            drawing.Frame(frame), new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)screenY)), SpriteMirror.None));
    }

    /// <summary>Draws splash/bubble frames of Samus's spritemap table, clipping Y's high byte while preserving native X wrap.</summary>
    private static void DrawSamusTableSpritemap(
        AtmosphereDrawing drawing,
        SamusAtmosphericEffectSlot slot,
        int slotIndex,
        string frame)
    {
        int screenX = unchecked((ushort)(slot.Position.X - drawing.Camera.X));
        int screenY = unchecked((ushort)(slot.Position.Y - drawing.Camera.Y));

        // `$90:8B83` rejects any nonzero high byte of Y. X is deliberately not clipped;
        if ((SnesLittleEndian.HighByteInPlace(screenY)) != 0)
            return;

        drawing.Sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusAtmosphere, slotIndex),
            drawing.Frame(frame), Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
    }

    /// <summary>One pass's drawing inputs: the frame, camera and installed sheets.</summary>
    private readonly record struct AtmosphereDrawing(
        SpriteFrameBuffer Sprites,
        Position16<RoomPixels> Camera,
        InstalledSpriteSheets? Sheets)
    {
        /// <summary>A frame of the atmosphere sheet, which a visible effect requires.</summary>
        public SpriteSheetFrame Frame(string name) => (Sheets ?? throw new InvalidOperationException(
            "Samus's atmospheric effects require the installed sprite sheets.")).Get(SpriteSheetId.SamusAtmosphere).Frame(name);
    }

    /// <summary>Restricts literal slot operations to the four native atmospheric records.</summary>
    private static void ValidateSlotIndex(int slotIndex)
    {
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }
}
