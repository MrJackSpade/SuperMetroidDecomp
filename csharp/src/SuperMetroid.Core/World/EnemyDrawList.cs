using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' draw phase: the queue the enemy pass fills, the common draw-shading selection, and the ordinary,
/// extended and supplemental sprite output with the boss body frames it shows. Each draw shows at its frame's depth.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDrawList
{
    /// <summary>Draws the dead Torizo's sand heap after the queued enemies.</summary>
    private readonly DeadTorizoBehavior _deadTorizoBehavior;
    /// <summary>Supplies the shared enemy frame counter that phases the hit flash.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draws an extended enemy's boss body frame before drawing its visual parts.</summary>
    private readonly EnemyInstructionInterpreter _instructions;
    /// <summary>Draws Mother Brain's late parts after the queued enemies.</summary>
    private readonly MotherBrainBehavior _motherBrainBehavior;
    /// <summary>Emits ordinary enemy frames to the frame's sprites.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies installed extended frames and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Told of each queued enemy the draw pass reaches, which a Puyo's later ceiling strike reads.</summary>
    private readonly PuyoCeilingTurnFromLastDraw _puyoCeilingTurn;

    /// <summary>Retains the behavior and presentation owners used by the enemy phase's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _deadTorizoBehavior = dependencies.DeadTorizoBehavior;
        _frameContext = dependencies.FrameContext;
        _instructions = dependencies.Instructions;
        _motherBrainBehavior = dependencies.MotherBrainBehavior;
        _presentation = dependencies.Presentation;
        _room = dependencies.Room;
        _puyoCeilingTurn = dependencies.PuyoCeilingTurn;
    }
    /// <summary>The enemies the enemy pass queued to draw, in <see cref="EnemySlotOrder"/>.</summary>
    internal readonly List<RoomEnemySlot> Queued = [];

    /// <summary>Empties the queue before the enemy pass rebuilds it.</summary>
    internal void Clear() => Queued.Clear();

    /// <summary>Queues <paramref name="enemy"/> to draw, after the enemies already queued.</summary>
    internal void Enqueue(RoomEnemySlot enemy) => Queued.Add(Ensure.NotNull(enemy));

    /// <summary>The draws <see cref="Advance"/> prepared for <see cref="Draw"/> to publish next.</summary>
    private List<PreparedEnemyDraw>? _prepared;

    /// <summary>
    /// The simulation half of the enemies' draw, in queue order: each queued enemy's shake tick is consumed while its
    /// origin is formed, and an extended enemy's boss body frame is shown. These run whenever the game's draw pass runs,
    /// whether or not the frame is shown.
    /// </summary>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    public void Advance(Position16<RoomPixels> camera)
    {
        _room.EnsureLoaded();
        var prepared = new List<PreparedEnemyDraw>();
        foreach (RoomEnemySlot slot in Queued)
        {
            // The draw of every queued enemy, a cleared or reserved one too, leaves the value a Puyo's ceiling
            // strike reads clear.
            _puyoCeilingTurn.EnemyDrawn();
            // Power-bomb damage runs after queue construction and can clear a
            // queued actor or replace it with the inert respawn reservation.
            // Neither lifecycle state owns a display composition.
            if (slot.DefinitionId is 0 or EnemyDefinitionId.Respawn)
                continue;

            // The draw offset is zero for ordinary room-population entries; the addition
            // precedes the camera subtraction and wraps at sixteen bits.
            Position16<ScreenPixels> origin = (slot.Position.Whole + slot.DrawOffset).ToScreen(camera);

            // Drawing an enemy consumes one shake tick while forming
            // the shared origin for both ordinary and extended spritemaps. Bit one of
            // the enemy's frame counter chooses -1 or +1; this is deliberately not the
            // room's background displacement. Ceres quake types 18 and above reload ShakeTimer for
            // every active enemy, which is why its ordinary door sprites visibly move
            // with the station instead of remaining pinned to host screen space.
            if (slot.ShakeTimer != 0)
            {
                origin = origin.WithX(CoordinateAxis.Wrap(origin.X +
                    ((slot.FrameCounter & 2) != 0 ? -1 : 1)));
                slot.ShakeTimer--;
            }
            PictureShading drawShading = SelectCommonEnemyDrawShading(slot);
            if (!slot.DrawsExtendedFrames)
            {
                EnemyFrameId frame = slot.Frame ?? throw new InvalidDataException(
                    $"Enemy {slot.DefinitionId} draws before showing a frame.");
                // The shared empty spritemap, which the common empty frame and the frames catalogued at its record show,
                // draws nothing and is on no sheet (#1513).
                prepared.Add(new PreparedEnemyDraw(slot, origin, drawShading, frame, Pictured: !CommonEnemyEmptyFrames.DrawsNothing(frame)));
                continue;
            }

            if (slot.Frame is { } shown &&
                _room.FrameDisplays?.TryGetDisplay(shown, out bool pictured) == true)
            {
                // Resolve the sprite picture and the body frame through one presentation binding. Validate the
                // frame before the body is drawn; physical selectors continue to belong to compiled animation
                // and collision programs.
                EnemyFrameId selected = _room.FrameDisplays.GetDisplayFrame(shown);
                _instructions.DrawBossBodyFrame(slot, selected);
                prepared.Add(new PreparedEnemyDraw(slot, origin, drawShading, shown, pictured));
                continue;
            }

            throw new InvalidDataException(
                $"Enemy {slot.DefinitionId} extended frame " +
                $"{slot.Frame} has no installed presentation.");
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
            // An ordinary or extended frame is one whole sheet picture placed at the enemy's origin. Collision still
            // uses the original engine-owned extended hitbox records, never the picture.
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

    /// <summary>One queued enemy's draw, formed by <see cref="Advance"/>.</summary>
    /// <param name="Slot">The enemy.</param>
    /// <param name="Origin">Its shaken screen origin.</param>
    /// <param name="Shading">The shading the common draw selected for this frame (#1512).</param>
    /// <param name="Frame">The frame the enemy shows, which names its sheet picture.</param>
    /// <param name="Pictured">Whether the frame has a sheet picture: the common empty frame, the shared empty extended
    /// frame and an extended frame drawn only on the boss body layer have none.</param>
    private readonly record struct PreparedEnemyDraw(RoomEnemySlot Slot,
        Position16<ScreenPixels> Origin, PictureShading Shading, EnemyFrameId Frame, bool Pictured);

    /// <summary>
    /// Selects the shading an enemy is drawn in, immediately before
    /// the draw emits either an ordinary or extended enemy spritemap.
    /// </summary>
    /// <remarks>
    /// A hit does not replace an enemy's stored colouring. On alternating phases of the
    /// shared enemy frame counter, the writer temporarily supplies the common sprite
    /// colours; the following draw restores the actor's own. Frozen flashing similarly
    /// selects the frozen colours. Keeping this at the common writer is
    /// important: Space Pirates are merely the first ordinary enemies whose missing flash
    /// was obvious, and a family-specific colouring change would corrupt every later draw.
    /// Ridley's translated AI already publishes the exact common-writer colouring because
    /// its body, wings, and tail must share one latched phase.
    /// </remarks>
    private PictureShading SelectCommonEnemyDrawShading(RoomEnemySlot slot)
    {
        if (slot.Entity is IEnemyDrawHooks drawHooks && drawHooks.CommonDrawShading() is { } owned)
            return owned;

        // A hit flash shows the common sprite colours on alternate phases. The flash selects another row for the draw, so
        // it lies over the enemy's own shading: pixels it moves show the selected row, and pixels whose row the selection
        // leaves as it is keep showing what the enemy's own effects laid there.
        if (slot.FlashTimer != 0 && (_frameContext.RandomEnemyCounter & 2) != 0)
            return slot.Shading.With(EnemyShades.HurtFlash);

        if (slot.FrozenTimer != 0 &&
            (slot.FrozenTimer >= 90 || (slot.FrozenTimer / 2 % 2 != 0)))
        {
            return slot.Shading.With(EnemyShades.Frozen);
        }

        return slot.Shading;
    }
}
