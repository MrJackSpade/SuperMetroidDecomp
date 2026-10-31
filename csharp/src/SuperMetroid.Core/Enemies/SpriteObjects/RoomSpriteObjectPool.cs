using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// The room's sprite-object pool: program-driven decorative actors (dust, splashes, shells) that enemies and projectiles
/// spawn, step and draw independently of enemy and projectile slots.
/// </summary>
/// <remarks>
/// Slot order: the pool holds at most <see cref="Capacity"/> objects, each slot with a fixed ordinal. Spawning, stepping
/// and drawing all run in one processing order, from the highest ordinal down. A spawn takes the first free slot in that
/// order, so in a pool where nothing has been freed the objects step and draw in the order they were spawned; a full
/// pool drops the spawn.
/// </remarks>
[RoomLifetime]
internal sealed class RoomSpriteObjectPool
{
    /// <summary>Publishes the objects from the loaded graphics set's sheets.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Connects decorative sprite objects to the room's presentation bindings.</summary>
    internal RoomSpriteObjectPool(EnemyPresentation presentation) => _presentation = presentation;
    /// <summary>How many sprite objects the pool holds at once; a spawn into a full pool is dropped.</summary>
    internal const int Capacity = 32;
    /// <summary>The slots by ordinal, including free ones.</summary>
    private readonly RoomSpriteObjectSlot[] _roomSpriteObjects =
        Enumerable.Range(0, Capacity)
            .Select(index => new RoomSpriteObjectSlot(index))
            .ToArray();

    /// <summary>The slots by ordinal, including free ones.</summary>
    internal IReadOnlyList<RoomSpriteObjectSlot> Slots => _roomSpriteObjects;

    /// <summary>The slots in processing order, highest ordinal first, including free ones.</summary>
    internal IEnumerable<RoomSpriteObjectSlot> InProcessingOrder
    {
        get
        {
            for (int ordinal = _roomSpriteObjects.Length - 1; ordinal >= 0; ordinal--)
                yield return _roomSpriteObjects[ordinal];
        }
    }

    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/> in the first free slot in processing order.</summary>
    /// <returns>The slot, or null when the pool is full and the spawn is dropped.</returns>
    internal RoomSpriteObjectSlot? SpawnRoomSpriteObject(
        Position16<RoomPixels> position,
        RoomSpriteObjectKind kind,
        SpriteGraphics graphics)
    {
        // A full pool drops the request; callers must not grow an unbounded collection instead.
        foreach (RoomSpriteObjectSlot slot in InProcessingOrder)
        {
            if (slot.IsActive)
                continue;

            slot.Clear();
            slot.Kind = kind;
            slot.Position = slot.Position.WithWhole(position);
            slot.Graphics = graphics;
            slot.Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectDefinitions.Programs[kind]);
            AdvanceProgram(slot);
            return slot;
        }

        return null;
    }

    /// <summary>Steps every object whose updates are enabled, in processing order.</summary>
    internal void StepRoomSpriteObjects()
    {
        // No current program spawns another object, but keeping the processing order keeps pool behaviour stable for
        // later families.
        foreach (RoomSpriteObjectSlot slot in InProcessingOrder)
        {
            if (!slot.IsActive || slot.UpdatesDisabled)
                continue;
            CountdownStep step = slot.Timer.Decrement();
            slot.Timer = step.Remaining;
            if (step.IsZero)
                AdvanceProgram(slot);
        }
    }

    /// <summary>
    /// Runs the slot's program to its next frame: a timed frame installs its frame and duration, the repeat-last hold
    /// keeps the frame and reloads the longest countdown, and delete clears the slot.
    /// </summary>
    private static void AdvanceProgram(RoomSpriteObjectSlot slot)
    {
        AnimationProgramCursor<RoomSpriteObjectProgram> cursor = slot.Cursor ?? throw new InvalidOperationException(
            $"Sprite object slot {slot.SlotIndex} has no program to advance.");
        WrappingCountdown counter = default;
        AnimationProgramAdvance<RoomSpriteObjectFrame> advance =
            RoomSpriteObjectAnimations.Programs.Advance(ref cursor, ref counter, static (_, _) => null);
        switch (advance)
        {
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Show shown:
                slot.Cursor = cursor;
                slot.Frame = shown.Frame;
                slot.Timer = WrappingCountdown.Start(shown.Duration);
                return;
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Hold:
                slot.Cursor = cursor;
                slot.Timer = WrappingCountdown.FromRaw(short.MaxValue);
                return;
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Stop:
                slot.Clear();
                return;
            default:
                throw new InvalidOperationException($"Sprite object program {cursor.Program} advanced with {advance}.");
        }
    }

    /// <summary>
    /// Points the slot at <paramref name="program"/> past its first frame and shows that frame, keeping the current
    /// countdown: the frozen Metroid shell shows the program's first frame without reloading the timer, so the next
    /// expiry continues at the program's second step.
    /// </summary>
    internal static void ShowFirstFrameKeepingTimer(RoomSpriteObjectSlot slot, RoomSpriteObjectProgram program)
    {
        Ensure.NotNull(slot);
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> first =
            RoomSpriteObjectAnimations.Programs.Steps(program)[0];
        if (first is not AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.ShowStep show)
            throw new InvalidOperationException($"Sprite object program {program} does not begin with a timed frame.");
        slot.Cursor = new(program, 1);
        slot.Frame = show.Frame;
    }

    /// <summary>Draws every visible object, in processing order.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Layer-one camera position in whole room pixels.</param>
    internal void DrawRoomSpriteObjects(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        foreach (RoomSpriteObjectSlot slot in InProcessingOrder)
        {
            if (!slot.IsActive)
                continue;

            Position16<ScreenPixels> screen = slot.Position.Whole.ToScreen(camera);
            // The 65816 uses signed branch tests on `(x + 16)`, `(x - 272)`, `y`, and
            // `(y - 272)`. Unsigned host comparisons incorrectly discarded partially visible
            // objects whose wrapped screen coordinate represented a small negative offset.
            if (CoordinateAxis.AsSigned(screen.X + 16) < 0 ||
                CoordinateAxis.Difference(screen.X, 272) >= 0 ||
                CoordinateAxis.AsSigned(screen.Y) < 0 ||
                CoordinateAxis.Difference(screen.Y, 272) >= 0)
            {
                continue;
            }

            EnemyFrameId shown = RoomSpriteObjectVisualDefinitions.ShownFrames.Of(slot.Frame);
            _presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.RoomSpriteObject, slot.SlotIndex),
                EnemySheetDefinitions.SpriteObjectFrameName(shown), screen, slot.Graphics.TileBase, slot.Graphics.Colouring);
        }
    }
}
