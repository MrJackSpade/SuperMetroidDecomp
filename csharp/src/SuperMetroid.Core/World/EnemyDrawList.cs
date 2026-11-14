using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' draw phase: the queue of live enemies the enemy pass fills, the common draw-shading selection, and each
/// enemy's frame and supplemental sprites with the boss body frames the draw pass shows. Each draw shows at its frame's
/// depth; the display culls what lies off screen.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDrawList
{
    /// <summary>Draws the dead Torizo's sand heap after the queued enemies.</summary>
    private readonly DeadTorizoBehavior _deadTorizoBehavior;
    /// <summary>Supplies the shared enemy frame counter that phases the hit flash.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Shows a reached enemy's boss body frame before its picture is drawn.</summary>
    private readonly EnemyAnimationPlayer _animationPlayer;
    /// <summary>Draws Mother Brain's late parts after the queued enemies.</summary>
    private readonly MotherBrainBehavior _motherBrainBehavior;
    /// <summary>Publishes each enemy's frame picture to the frame's sprites.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies every installed enemy frame's display and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Told of each queued enemy the draw pass reaches, which a Puyo's later ceiling strike reads.</summary>
    private readonly PuyoCeilingTurnFromLastDraw _puyoCeilingTurn;

    /// <summary>Retains the behavior and presentation owners used by the enemy phase's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _deadTorizoBehavior = dependencies.DeadTorizoBehavior;
        _frameContext = dependencies.FrameContext;
        _animationPlayer = dependencies.AnimationPlayer;
        _motherBrainBehavior = dependencies.MotherBrainBehavior;
        _presentation = dependencies.Presentation;
        _room = dependencies.Room;
        _puyoCeilingTurn = dependencies.PuyoCeilingTurn;
    }
    /// <summary>The live enemies the enemy pass queued to show, in <see cref="EnemySlotOrder"/>, each with whether the draw pass reaches it.</summary>
    private readonly List<QueuedEnemy> _queued = [];

    /// <summary>Empties the queue before the enemy pass rebuilds it.</summary>
    internal void Clear() => _queued.Clear();

    /// <summary>
    /// Queues <paramref name="enemy"/> to show after the enemies already queued; <paramref name="reached"/> tells whether
    /// the draw pass reaches it, which only a reached enemy's draw-pass side effects depend on.
    /// </summary>
    internal void Enqueue(RoomEnemySlot enemy, bool reached) => _queued.Add(new QueuedEnemy(Ensure.NotNull(enemy), reached));

    /// <summary>The draws <see cref="Advance"/> prepared for <see cref="Draw"/> to publish next.</summary>
    private List<PreparedEnemyDraw>? _prepared;

    /// <summary>
    /// The simulation half of the enemies' draw, in queue order. For each enemy the draw pass reaches, the Puyo value its
    /// draw clears is cleared, its shake tick is consumed while its origin is formed, and its boss body frame is shown;
    /// these run whenever the game's draw pass runs, whether or not the frame is seen. Every queued enemy's picture is
    /// prepared for <see cref="Draw"/>.
    /// </summary>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <exception cref="InvalidOperationException">No installed enemy frame displays are bound.</exception>
    /// <exception cref="InvalidDataException">A reached enemy shows no frame, or an enemy shows a frame that is not installed.</exception>
    public void Advance(Position16<RoomPixels> camera)
    {
        _room.EnsureLoaded();
        var prepared = new List<PreparedEnemyDraw>();
        foreach ((RoomEnemySlot slot, bool reached) in _queued)
        {
            // The draw of every reached enemy, a cleared or reserved one too, leaves the value a Puyo's ceiling
            // strike reads clear.
            if (reached)
                _puyoCeilingTurn.EnemyDrawn();
            // Power-bomb damage runs after queue construction and can clear a
            // queued actor or replace it with the inert respawn reservation.
            // Neither lifecycle state owns a display composition.
            if (slot.DefinitionId == EnemyDefinitionId.None)
                continue;

            // The draw offset is zero for ordinary room-population entries; the addition
            // precedes the camera subtraction and wraps at sixteen bits.
            Position16<ScreenPixels> origin = (slot.Position.Whole + slot.DrawOffset).ToScreen(camera);

            // The draw pass consumes one shake tick of an enemy it reaches while forming
            // its origin. The enemy's own update count chooses one pixel left or right; this is deliberately not the
            // room's background displacement. Ceres quake types 18 and above reload ShakeTimer for
            // every active enemy, which is why its door sprites visibly move
            // with the station instead of remaining pinned to host screen space.
            if (reached && slot.ShakeTimer != 0)
            {
                origin = origin.WithX(CoordinateAxis.Wrap(origin.X +
                    (EnemyDrawPhaseDefinitions.ShakeShiftsLeft.IsSetIn(slot.FrameCounter) ? -1 : 1)));
                slot.ShakeTimer--;
            }
            PictureShading drawShading = SelectCommonEnemyDrawShading(slot);
            // An enemy the draw pass does not reach and that has shown no frame yet shows nothing; one it reaches has
            // always shown a frame.
            if (slot.Frame is not { } shown)
            {
                if (reached)
                    throw new InvalidDataException($"Enemy {slot.DefinitionId} draws before showing a frame.");
                continue;
            }
            // Resolve the sprite picture and the body frame through one presentation binding. Validate the
            // frame before the body is shown; physical selectors continue to belong to the compiled animations
            // and collision definitions. The empty frame draws nothing and is on no sheet (#1513).
            EnemyFrameDisplays displays = _room.FrameDisplays;
            if (!displays.TryGetDisplay(shown, out bool pictured))
                throw new InvalidDataException($"Enemy {slot.DefinitionId} frame {shown} has no installed presentation.");
            // Only the draw pass shows a boss body frame; a frame without a body shows none.
            if (reached)
                _animationPlayer.DrawBossBodyFrame(slot, displays.GetDisplayFrame(shown));
            prepared.Add(new PreparedEnemyDraw(slot, origin, drawShading, shown, pictured));
        }
        _prepared = prepared;
    }

    /// <summary>
    /// Publishes the draws <see cref="Advance"/> prepared, in queue order, each at the depth its frame shows at. An
    /// occupant's supplemental sprites are written just before its body; Mother Brain's late parts and the dead Torizo's
    /// sand heap follow every queued enemy.
    /// </summary>
    /// <param name="sprites">The frame's sprite description; the occupants and the late draws write its sprites.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <exception cref="InvalidOperationException">The draws were not prepared by <see cref="Advance"/>.</exception>
    public void Draw(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        if (_prepared is not { } prepared)
            throw new InvalidOperationException("The enemies were not advanced before drawing.");
        _prepared = null;

        foreach (PreparedEnemyDraw draw in prepared)
        {
            RoomEnemySlot slot = draw.Slot;
            var source = new SpriteSource(SpriteSourceKind.Enemy, slot.SlotIndex);
            // An occupant may draw supplemental sprites before the common writer emits its body (Ridley's tail
            // and wings).
            if (slot.Entity is IEnemyDrawHooks drawHooks)
                drawHooks.DrawBeforeBody(sprites, camera);
            // Every frame is one whole sheet picture placed at the enemy's origin. Collision still
            // uses the game's own frame hitboxes, never the picture.
            if (draw.Pictured)
                _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(slot.DefinitionId, EnemySheetDefinitions.FrameName(draw.Frame),
                    new Position16<ScreenPixels>(draw.Origin.X, draw.Origin.Y))
                {
                    Shading = draw.Shading,
                    RotRows = (slot.Entity as IShowsCorpseRotRows)?.ShownRotRows,
                });
        }

        // Mother Brain's late parts advance her brain, which reads the room's random numbers, after every queued
        // enemy's draw.
        _motherBrainBehavior.Head.DrawMotherBrainBrainAndNeck(sprites, camera);
        _deadTorizoBehavior.DrawDeadTorizoSandHeap(sprites, camera);
    }

    /// <summary>One live enemy queued to show.</summary>
    /// <param name="Slot">The enemy.</param>
    /// <param name="Reached">Whether the draw pass reaches it, running its draw's side effects.</param>
    private readonly record struct QueuedEnemy(RoomEnemySlot Slot, bool Reached);

    /// <summary>One queued enemy's draw, formed by <see cref="Advance"/>.</summary>
    /// <param name="Slot">The enemy.</param>
    /// <param name="Origin">Its screen origin, shaken when the draw pass reaches it.</param>
    /// <param name="Shading">The shading the common draw selected for this frame (#1512).</param>
    /// <param name="Frame">The frame the enemy shows, which names its sheet picture.</param>
    /// <param name="Pictured">Whether the frame has a sheet picture: the empty frame, a frame shown only on a boss body and
    /// a frame whose pieces show nothing have none.</param>
    private readonly record struct PreparedEnemyDraw(RoomEnemySlot Slot,
        Position16<ScreenPixels> Origin, PictureShading Shading, EnemyFrameId Frame, bool Pictured);

    /// <summary>
    /// Selects the shading an enemy is drawn in, immediately before
    /// the draw publishes its frame's picture.
    /// </summary>
    /// <remarks>
    /// A hit does not replace an enemy's stored colouring. On alternating phases of the
    /// shared enemy frame counter, the writer temporarily supplies the common sprite
    /// colours; the following draw restores the actor's own. Frozen flashing similarly
    /// selects the frozen colours. Keeping this at the common writer is
    /// important: Space Pirates are merely the first enemies whose missing flash
    /// was obvious, and a family-specific colouring change would corrupt every later draw.
    /// Ridley's AI already publishes the exact common-writer colouring because
    /// its body, wings, and tail must share one latched phase.
    /// </remarks>
    private PictureShading SelectCommonEnemyDrawShading(RoomEnemySlot slot)
    {
        if (slot.Entity is IEnemyDrawHooks drawHooks && drawHooks.CommonDrawShading() is { } owned)
            return owned;

        // A hit flash shows the common sprite colours on alternate phases. The flash selects another row for the draw, so
        // it lies over the enemy's own shading: pixels it moves show the selected row, and pixels whose row the selection
        // leaves as it is keep showing what the enemy's own effects laid there.
        if (slot.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(_frameContext.RandomEnemyCounter))
            return slot.Shading.With(EnemyShades.HurtFlash);

        if (slot.FrozenTimer != 0 &&
            (slot.FrozenTimer >= 90 || EnemyDrawPhaseDefinitions.FrozenColoursShown.IsSetIn(slot.FrozenTimer)))
        {
            return slot.Shading.With(EnemyShades.Frozen);
        }

        return slot.Shading;
    }
}
