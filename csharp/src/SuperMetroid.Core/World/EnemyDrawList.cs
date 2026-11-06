using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' draw phase: the per-layer queues the enemy pass fills, the common
/// draw-palette selection, and the ordinary, extended and supplemental sprite output with the boss body frames it draws.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDrawList
{
    /// <summary>Runs the Dead Torizo draw hook after the layer-five ordinary queue.</summary>
    private readonly DeadTorizoBehavior _deadTorizoBehavior;
    /// <summary>Supplies the shared enemy frame counter used for hit-flash palette phases.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draws an extended enemy's boss body frame before drawing its visual parts.</summary>
    private readonly EnemyInstructionInterpreter _instructions;
    /// <summary>Runs Mother Brain's draw hook on its authored layer-five pass.</summary>
    private readonly MotherBrainBehavior _motherBrainBehavior;
    /// <summary>Emits ordinary enemy frames to the frame's sprites.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies installed extended frames and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;


    /// <summary>Retains the behavior and presentation owners used by EnemyMain's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _deadTorizoBehavior = dependencies.DeadTorizoBehavior;
        _frameContext = dependencies.FrameContext;
        _instructions = dependencies.Instructions;
        _motherBrainBehavior = dependencies.MotherBrainBehavior;
        _presentation = dependencies.Presentation;
        _room = dependencies.Room;
    }
    /// <summary>The enemies queued on each of the eight drawing layers, each queue in <see cref="EnemySlotOrder"/>.</summary>
    internal readonly List<RoomEnemySlot>[] DrawQueues =
        Enumerable.Range(0, EnemyDrawLayer.Count).Select(_ => new List<RoomEnemySlot>()).ToArray();

    /// <summary>Empties every layer queue before the enemy pass rebuilds them.</summary>
    internal void Clear()
    {
        foreach (List<RoomEnemySlot> queue in DrawQueues)
            queue.Clear();
    }

    /// <summary>Queues <paramref name="enemy"/> on its draw layer, after the enemies already queued there.</summary>
    internal void Enqueue(EnemyDrawLayer layer, RoomEnemySlot enemy) => DrawQueues[layer.Order].Add(Ensure.NotNull(enemy));

    /// <summary>The draws <see cref="AdvanceLayers"/> prepared for the layer range <see cref="DrawLayers"/> publishes next.</summary>
    private List<PreparedEnemyDraw>? _prepared;
    /// <summary>The inclusive layer range <see cref="_prepared"/> covers.</summary>
    private InclusiveRange _preparedLayers;

    /// <summary>
    /// The simulation half of the enemies' draw for an inclusive layer range, in queue order: each queued
    /// enemy's shake tick is consumed while its origin is formed, and an extended enemy's boss body frame is
    /// drawn. These run whenever the game's draw pass runs, whether or not the frame is shown.
    /// </summary>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="firstLayer">First draw layer, inclusively, in the range 0..7.</param>
    /// <param name="lastLayer">Last draw layer, inclusively, in the range 0..7 and not below firstLayer.</param>
    /// <exception cref="ArgumentOutOfRangeException">The inclusive layer range is invalid.</exception>
    public void AdvanceLayers(Position16<RoomPixels> camera, int firstLayer, int lastLayer)
    {
        _room.EnsureLoaded();
        if ((uint)firstLayer > 7 || (uint)lastLayer > 7 || firstLayer > lastLayer)
            throw new ArgumentOutOfRangeException(nameof(firstLayer));

        var prepared = new List<PreparedEnemyDraw>();
        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            foreach (RoomEnemySlot slot in DrawQueues[layer])
            {
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
                if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
                {
                    EnemyFrameId frame = slot.Frame ?? throw new InvalidDataException(
                        $"Enemy {slot.DefinitionId} draws before showing a frame.");
                    // The shared empty spritemap, which the common empty frame and the frames catalogued at its record show,
                    // draws nothing and is on no sheet (#1513).
                    prepared.Add(new PreparedEnemyDraw(layer, slot, origin, drawShading, frame, Pictured: !CommonEnemyEmptyFrames.DrawsNothing(frame)));
                    continue;
                }

                if (slot.Frame is { } shown &&
                    _room.Artwork?.ExtendedFrameDisplays?.TryGetDisplay(shown, out bool pictured) == true)
                {
                    // Resolve the sprite picture and the body frame through one presentation binding. Validate the
                    // frame before the body is drawn; physical selectors continue to belong to compiled animation
                    // and collision programs.
                    EnemyFrameId selected = _room.Artwork.ExtendedFrameDisplays.GetDisplayFrame(shown);
                    _instructions.DrawBossBodyFrame(slot, selected);
                    prepared.Add(new PreparedEnemyDraw(layer, slot, origin, drawShading, shown, pictured));
                    continue;
                }

                throw new InvalidDataException(
                    $"Enemy {slot.DefinitionId} extended frame " +
                    $"{slot.Frame} has no installed presentation.");
            }
        }
        _prepared = prepared;
        _preparedLayers = new(firstLayer, lastLayer);
    }

    /// <summary>
    /// Publishes the draws <see cref="AdvanceLayers"/> prepared for the same layer range, in queue order, as
    /// whole-frame entries over the gameplay sprite page. An occupant's supplemental sprites and the layer-five boss hooks
    /// still write sprites between them, in the game's order.
    /// </summary>
    /// <param name="sprites">The frame's sprite description; the occupant and boss hooks write its sprites.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="firstLayer">First draw layer, inclusively; the range <see cref="AdvanceLayers"/> prepared.</param>
    /// <param name="lastLayer">Last draw layer, inclusively; the range <see cref="AdvanceLayers"/> prepared.</param>
    /// <exception cref="InvalidOperationException">The range was not prepared by <see cref="AdvanceLayers"/>.</exception>
    public void DrawLayers(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        int firstLayer,
        int lastLayer)
    {
        Ensure.NotNull(sprites);
        if (_prepared is not { } prepared || _preparedLayers != new InclusiveRange(firstLayer, lastLayer))
            throw new InvalidOperationException($"Enemy layers {firstLayer}..{lastLayer} were not advanced before drawing.");
        _prepared = null;

        int next = 0;
        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            for (; next < prepared.Count && prepared[next].Layer == layer; next++)
            {
                PreparedEnemyDraw draw = prepared[next];
                RoomEnemySlot slot = draw.Slot;
                var source = new SpriteSource(SpriteSourceKind.Enemy, slot.SlotIndex);
                // An occupant may draw supplemental sprites before the common writer emits its body (Ridley's tail
                // and wings), preserving both that drawing order and the enemy's normal layer queue.
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

            // EnemyGraphicsDrawnHook runs after the complete ordinary enemy queue. Mother
            // Brain installs it from its hidden head record, but the hook itself belongs to
            // the record's authored layer-five pass and therefore must not run in both of
            // the frontend's disjoint low/high-priority layer ranges.
            if (layer == 5)
            {
                _motherBrainBehavior.Head.DrawMotherBrainHook(sprites, camera);
                _deadTorizoBehavior.DrawDeadTorizoHook(sprites, camera);
            }
        }
    }

    /// <summary>One queued enemy's draw, formed by <see cref="AdvanceLayers"/>.</summary>
    /// <param name="Layer">The draw layer it was queued on.</param>
    /// <param name="Slot">The enemy.</param>

    /// <param name="Origin">Its shaken screen origin.</param>
    /// <param name="Shading">The shading the common draw selected for this frame (#1512).</param>
    /// <param name="Frame">The frame the enemy shows, which names its sheet picture.</param>
    /// <param name="Pictured">Whether the frame has a sheet picture: the common empty frame, the shared empty extended
    /// frame and an extended frame drawn only on the boss body layer have none.</param>
    private readonly record struct PreparedEnemyDraw(int Layer, RoomEnemySlot Slot,
        Position16<ScreenPixels> Origin, PictureShading Shading, EnemyFrameId Frame, bool Pictured);

    /// <summary>
    /// Ports the palette selection, immediately before
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
            return slot.Shading.With(EnemyShades.HitFlash);

        if (slot.FrozenTimer != 0 &&
            (slot.FrozenTimer >= 90 || (slot.FrozenTimer / 2 % 2 != 0)))
        {
            return slot.Shading.With(EnemyShades.Frozen);
        }

        return slot.Shading;
    }
}
