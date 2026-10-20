using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// The bank-$B4 room sprite-object pool: 32 instruction-driven decorative actors (dust, splashes, shells)
/// that enemies and projectiles spawn, step and draw independently of enemy and projectile slots.
/// </summary>
internal sealed class RoomSpriteObjectPool
{
    /// <summary>Supplies installed spritemap artwork when visible room objects are drawn.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Connects decorative sprite objects to the room's presentation bindings.</summary>
    internal RoomSpriteObjectPool(EnemyRoomBinding room)
    {
        _room = room;
    }
    /// <summary>The cartridge allocates 32 physical sprite-object slots, searched from highest to lowest.</summary>
    internal const int RoomSpriteObjectSlotCount = 32;
    /// <summary>Fixed bank-$B4 decorative actor slots, including inactive entries.</summary>
    private readonly RoomSpriteObjectSlot[] _roomSpriteObjects =
        Enumerable.Range(0, RoomSpriteObjectSlotCount)
            .Select(index => new RoomSpriteObjectSlot(index))
            .ToArray();

    /// <summary>All 32 physical slots, including inactive ones.</summary>
    internal IReadOnlyList<RoomSpriteObjectSlot> Slots => _roomSpriteObjects;

    /// <summary>Ports <c>CreateSpriteAtPos</c> at <c>$B4:BC26</c>.</summary>
    internal RoomSpriteObjectSlot? SpawnRoomSpriteObject(
        Position16<RoomPixels> position,
        RoomSpriteObjectKind kind,
        ushort graphicsIndex)
    {
        // Native allocation searches indexes $3E,$3C,...,$00. A saturated pool drops the
        // request and returns $FFFF; callers must not grow an unbounded host collection.
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (slot.IsActive)
                continue;

            slot.Clear();
            slot.Kind = kind;
            slot.Position = slot.Position.WithWhole(position);
            slot.Attributes = new SnesObjAttributeWord(graphicsIndex);
            slot.Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectDefinitions.Program(kind));
            AdvanceProgram(slot);
            return slot;
        }

        return null;
    }

    /// <summary>Ports the non-frozen body of <c>HandleSpriteObjects</c> at $B4:BC82.</summary>
    internal void StepRoomSpriteObjects()
    {
        // The hardware walks descending native indexes. No current program spawns another
        // object, but retaining the order keeps pool behavior stable for later families.
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive || slot.UpdatesDisabled)
                continue;
            NativeWordCounterStep step = slot.Timer.Decrement();
            slot.Timer = step.Remaining;
            if (step.IsZero)
                AdvanceProgram(slot);
        }
    }

    /// <summary>
    /// Runs the slot's program to its next frame: a timed frame installs its frame and duration, the repeat-last hold
    /// keeps the frame and reloads $7FFF as <c>$B4:BCF0</c> does, and delete clears the slot.
    /// </summary>
    private static void AdvanceProgram(RoomSpriteObjectSlot slot)
    {
        AnimationProgramCursor<RoomSpriteObjectProgram> cursor = slot.Cursor ?? throw new InvalidOperationException(
            $"Sprite object slot {slot.SlotIndex} has no program to advance.");
        NativeCountdown counter = default;
        AnimationProgramAdvance<RoomSpriteObjectFrame> advance =
            RoomSpriteObjectAnimations.Programs.Advance(ref cursor, ref counter, static (_, _) => null);
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                slot.Cursor = cursor;
                slot.Frame = advance.Frame;
                slot.Timer = NativeCountdown.Start(advance.Duration);
                return;
            case AnimationProgramAdvanceKind.Hold:
                slot.Cursor = cursor;
                slot.Timer = NativeCountdown.FromRaw(short.MaxValue);
                return;
            case AnimationProgramAdvanceKind.Stop:
                slot.Clear();
                return;
            case AnimationProgramAdvanceKind.Wait:
            default:
                throw new InvalidOperationException($"Sprite object program {cursor.Program} advanced with {advance.Kind}.");
        }
    }

    /// <summary>
    /// Points the slot at <paramref name="program"/> past its first frame and shows that frame, keeping the current
    /// countdown: the native frozen Metroid shell stores the list's address and draws its first record without
    /// reloading the timer, so the next expiry continues at the list's second record.
    /// </summary>
    internal static void ShowFirstFrameKeepingTimer(RoomSpriteObjectSlot slot, RoomSpriteObjectProgram program)
    {
        Ensure.NotNull(slot);
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> first =
            RoomSpriteObjectAnimations.Programs.Steps(program)[0];
        if (first.Kind != AnimationProgramStepKind.Show)
            throw new InvalidOperationException($"Sprite object program {program} does not begin with a timed frame.");
        slot.Cursor = new(program, 1);
        slot.Frame = first.Frame;
    }

    /// <summary>Ports <c>DrawSpriteObjects</c> at <c>$B4:BD32</c>.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="camera">Layer-one camera position in whole room pixels.</param>
    internal void DrawRoomSpriteObjects(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera)
    {
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive)
                continue;

            Position16<ScreenPixels> screen = slot.Position.Whole.ToScreen(camera);
            // The 65816 uses signed branch tests on `(x + 16)`, `(x - 272)`, `y`, and
            // `(y - 272)`. Unsigned host comparisons incorrectly discarded partially visible
            // objects whose wrapped screen coordinate represented a small negative offset.
            if (unchecked((short)(screen.X + 16)) < 0 ||
                unchecked((short)(screen.X - 272)) >= 0 ||
                unchecked((short)screen.Y) < 0 ||
                unchecked((short)(screen.Y - 272)) >= 0)
            {
                continue;
            }

            ushort paletteBits = slot.Attributes.PaletteBits;
            ushort baseTileIndex = checked((ushort)slot.Attributes.TileNumber);
            var spritemaps = _room.Artwork?.Spritemaps ?? throw new InvalidOperationException(
                "Room sprite objects require installed sprite artwork.");
            ushort spritemap = RoomSpriteObjectVisualDefinitions.Spritemap(slot.Frame);
            if (!spritemaps.TryGetDisplay(
                    RoomSpriteObjectVisualDefinitions.Bank, spritemap,
                    out EnemySpritemapParts installed))
            {
                throw new InvalidDataException(
                    $"Installed room sprite-object artwork lacks frame {slot.Frame} " +
                    $"($B4:{spritemap:X4}) for {slot.Kind}.");
            }
            EnemyPresentation.PublishParts(sprites, objects, new SpriteSource(SpriteSourceKind.RoomSpriteObject, index),
                EnemySpriteFrames.Frame(installed, baseTileIndex, paletteBits), screen, paletteBits);
        }
    }
}
