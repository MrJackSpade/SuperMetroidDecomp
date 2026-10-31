using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Samus's atmospheric effect slots and their animation and drawing.
/// </summary>
/// <remarks>
/// These are not free particles. Water entry, bubbles, lava spray, footsteps and boost dust share
/// <see cref="SlotCount"/> fixed slots, and a new effect replaces whatever its slot held.
/// Slot order: each producer writes its own slots by ordinal. Running footsteps and the shallow water splash take slots
/// zero and one, a diving splash slot zero, the landing pair slots two and three, bubbles slot two, wall-jump dust slot
/// three, and lava spray all four. The update runs from the highest ordinal down, so the higher slot draws in front where
/// effects overlap. Delayed-start timers (negative countdowns) stagger an effect's first frame by an update.
/// </remarks>
public sealed class SamusAtmosphericEffectsState
{
    /// <summary>Exactly $8000: a delayed-start timer, which reloads the current frame's duration and draws it.</summary>
    internal const ushort DelayedStartMarker = 1 << 15;

    /// <summary>How many atmospheric effects Samus can show at once.</summary>
    public const int SlotCount = 4;

    /// <summary>The slots by ordinal, shared by splashes, bubbles, lava spray and boost dust.</summary>
    private readonly SamusAtmosphericEffectSlot[] _slots =
    [
        new(),
        new(),
        new(),
        new(),
    ];

    /// <summary>The slots by ordinal.</summary>
    public IReadOnlyList<SamusAtmosphericEffectSlot> Slots => _slots;

    /// <summary>Clears every slot, as room and Samus initialization do.</summary>
    public void Clear()
    {
        foreach (SamusAtmosphericEffectSlot slot in _slots)
            slot.Clear();
    }

    /// <summary>
    /// Starts an effect in a slot. Public visibility is intentional: deterministic tests and debugger experiments can
    /// seed exact state without a fake water room.
    /// </summary>
    /// <param name="slotIndex">The slot's ordinal, 0 through <see cref="SlotCount"/> - 1.</param>
    /// <param name="type">The effect; an inactive effect with frame zero leaves the slot empty.</param>
    /// <param name="animationFrame">Animation frame, interpreted against the selected type when updated.</param>
    /// <param name="animationTimer">The countdown, including negative delayed-start countdowns.</param>
    /// <param name="anchor">Room-space anchor in whole pixels, wrapping at sixteen bits.</param>
    public void SetSlot(
        int slotIndex,
        SamusAtmosphericEffectType type,
        int animationFrame,
        int animationTimer,
        Position16<RoomPixels> anchor)
    {
        ValidateSlotIndex(slotIndex);
        SamusAtmosphericEffectSlot slot = _slots[slotIndex];
        slot.SetEffect(type, animationFrame);
        slot.AnimationTimer = WrappingCountdown.FromRaw(animationTimer);
        slot.Position = anchor;
    }

    /// <summary>
    /// Empties a slot's effect, exactly like the landing-graphics delete paths at <c>$91:F0BE/$91:F1CC</c>. The
    /// cartridge deliberately leaves the timer and coordinates behind; retaining that stale state matters when proving
    /// that a later producer really initialized every field it owns.
    /// </summary>
    /// <param name="slotIndex">The slot's ordinal; its timer and coordinates are not cleared.</param>
    public void ClearEffect(int slotIndex)
    {
        ValidateSlotIndex(slotIndex);
        _slots[slotIndex].ClearEffect();
    }

    /// <summary>
    /// Ports <c>Handle_AtmosphericEffects</c> at <c>$90:8A4C-$8C1E</c>, including timer
    /// underflow, reverse slot order, per-type motion, clipping, and the two OAM paths.
    /// </summary>
    /// <param name="sprites">Frame receiving visible effects as draws, the highest slot first.</param>
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

        // The update runs from the highest ordinal down, so the higher slot draws in front whenever effects overlap.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusAtmosphericEffectSlot slot = _slots[slotIndex];
            if (slot.IsEmpty)
                continue;

            SamusAtmosphericEffectType type = slot.Type;
            int frameBeforeTimer = slot.AnimationFrame;
            slot.AnimationTimer = slot.AnimationTimer.Decrement().Remaining;

            if (slot.AnimationTimer.IsZero)
            {
                // `$90:8A86` reloads using the OLD frame and only then advances the frame.
                // That ordering is why a newly created timer-three frame is visible for
                // two calls before frame one begins.
                slot.AnimationTimer =
                    WrappingCountdown.FromRaw(SamusAtmosphericAnimations.FrameTimer(type, frameBeforeTimer));
                slot.AdvanceFrame();
                if (slot.AnimationFrame >=
                    SamusAtmosphericAnimations.FrameCount(type))
                {
                    slot.ClearEffect();
                    continue;
                }
            }
            else if (slot.AnimationTimer.IsNegative)
            {
                // Negative timers suppress both drawing and motion. Exactly `$8000` is a
                // delayed-start marker: reload the current frame's ROM duration and draw it.
                if (!slot.AnimationTimer.IsAt(DelayedStartMarker))
                    continue;
                slot.AnimationTimer =
                    WrappingCountdown.FromRaw(SamusAtmosphericAnimations.FrameTimer(type, frameBeforeTimer));
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
                    // The first two slots' spray drifts right and the last two slots' left.
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
        if (ScreenConversions.IsOutsideSpriteRange(screenY))
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

    /// <summary>Restricts slot operations to the slots that exist.</summary>
    private static void ValidateSlotIndex(int slotIndex)
    {
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }
}
