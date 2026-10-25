using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// EnemyMain's draw phase (<c>WriteEnemyOams</c>): the per-layer queues the enemy pass fills, the common
/// draw-palette selection, and the ordinary, extended and supplemental sprite output with its BG2 frame side effects.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDrawList
{
    /// <summary>Runs the Dead Torizo draw hook after the layer-five ordinary queue.</summary>
    private readonly DeadTorizoBehavior _deadTorizoBehavior;
    /// <summary>Supplies the shared enemy frame counter used for hit-flash palette phases.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Applies installed extended-frame BG2 changes before drawing visual parts.</summary>
    private readonly EnemyInstructionInterpreter _instructions;
    /// <summary>Runs Mother Brain's draw hook on its authored layer-five pass.</summary>
    private readonly MotherBrainBehavior _motherBrainBehavior;
    /// <summary>Emits ordinary enemy spritemaps to OAM.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies installed extended frames and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves native queue indices to current enemy slots at draw time.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Retains the behavior and presentation owners used by EnemyMain's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _deadTorizoBehavior = dependencies.DeadTorizoBehavior;
        _frameContext = dependencies.FrameContext;
        _instructions = dependencies.Instructions;
        _motherBrainBehavior = dependencies.MotherBrainBehavior;
        _presentation = dependencies.Presentation;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }
    /// <summary>Native enemy indices grouped into the eight ordered drawing layers.</summary>
    internal readonly List<int>[] DrawQueues =
        Enumerable.Range(0, 8).Select(_ => new List<int>()).ToArray();

    /// <summary>Empties every layer queue before the enemy pass rebuilds them.</summary>
    internal void Clear()
    {
        foreach (List<int> queue in DrawQueues)
            queue.Clear();
    }

    /// <summary>Queues an enemy, by native index, for its draw layer.</summary>
    internal void Enqueue(int layer, int enemyIndex) => DrawQueues[layer].Add(enemyIndex);

    /// <summary>The draws <see cref="AdvanceLayers"/> prepared for the layer range <see cref="DrawLayers"/> publishes next.</summary>
    [NonSerialized] private List<PreparedEnemyDraw>? _prepared;
    /// <summary>The inclusive layer range <see cref="_prepared"/> covers.</summary>
    [NonSerialized] private EnemyDrawLayerRange _preparedLayers;

    /// <summary>
    /// The simulation half of <c>WriteEnemyOams</c> for an inclusive layer range, in native slot order: each queued
    /// enemy's shake tick is consumed while its origin is formed, and an extended enemy's installed BG2 frame is
    /// applied. These run whenever the cartridge's draw pass runs, whether or not the frame is shown.
    /// </summary>
    /// <param name="camera">Layer-one camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="firstLayer">First native draw layer, inclusively, in the range 0..7.</param>
    /// <param name="lastLayer">Last native draw layer, inclusively, in the range 0..7 and not below firstLayer.</param>
    /// <exception cref="ArgumentOutOfRangeException">The inclusive layer range is invalid.</exception>
    public void AdvanceLayers(Position16<RoomPixels> camera, int firstLayer, int lastLayer)
    {
        _room.EnsureLoaded();
        if ((uint)firstLayer > 7 || (uint)lastLayer > 7 || firstLayer > lastLayer)
            throw new ArgumentOutOfRangeException(nameof(firstLayer));

        var prepared = new List<PreparedEnemyDraw>();
        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            foreach (int enemyIndex in DrawQueues[layer])
            {
                RoomEnemySlot slot = _slotTable.SlotAtEnemyIndex(enemyIndex);
                // Power-bomb damage runs after queue construction and can clear a
                // queued actor or replace it with the inert respawn reservation.
                // Neither lifecycle state owns a display composition.
                if (slot.DefinitionId is 0 or EnemyDefinitionId.Respawn)
                    continue;

                // Enemy spawn-point offsets are zero for ordinary room-population entries.
                // WriteEnemyOams nevertheless performs the additions before subtracting
                // layer-1 position, so preserve modular 16-bit arithmetic here.
                Position16<ScreenPixels> origin = (slot.Position.Whole + slot.SpawnOffset).ToScreen(camera);

                // `WriteEnemyOAM` `$A0:947B-$9494` consumes one shake tick while forming
                // the shared origin for both ordinary and extended spritemaps. Bit one of
                // the enemy's frame counter chooses -1 or +1; this is deliberately not the
                // room's BG displacement. Ceres quake types >=$12 reload ShakeTimer for
                // every active enemy, which is why its ordinary door sprites visibly move
                // with the station instead of remaining pinned to host screen space.
                if (!slot.ShakeTimer.IsZero)
                {
                    origin = origin.WithX(unchecked((ushort)(origin.X +
                        ((slot.FrameCounter & 2) != 0 ? -1 : 1))));
                    slot.ShakeTimer = slot.ShakeTimer.Decrement().Remaining;
                }
                ushort drawPaletteIndex = SelectCommonEnemyDrawPalette(slot);
                if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
                {
                    EnemyFrameId frame = slot.Frame ?? throw new InvalidDataException(
                        $"Enemy {slot.DefinitionId} draws before showing a frame.");
                    prepared.Add(new PreparedEnemyDraw(layer, slot, enemyIndex, origin, drawPaletteIndex, frame, null, null));
                    continue;
                }

                if (slot.Frame is { } shown &&
                    _room.Artwork?.ExtendedFrames?.TryGetDisplay(shown, out ReadOnlyMemory<EnemyExtendedDrawComponent> installed) == true)
                {
                    // Resolve both halves through one presentation binding. Validate
                    // the composition before any BG2 side effects; physical selectors
                    // continue to belong to compiled animation and collision programs.
                    EnemyFrameId selected = _room.Artwork.ExtendedFrames.GetDisplayFrame(shown);
                    _instructions.ApplyInstalledEnemyBg2Frame(slot, selected);
                    prepared.Add(new PreparedEnemyDraw(layer, slot, enemyIndex, origin, drawPaletteIndex, null, shown, installed));
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
    /// Publishes the draws <see cref="AdvanceLayers"/> prepared for the same layer range, in native slot order, as
    /// whole-frame entries over the gameplay OBJ page. An occupant's supplemental sprites and the layer-five boss hooks
    /// still write OAM between them, in the cartridge's order.
    /// </summary>
    /// <param name="sprites">The frame's sprite description; the occupant and boss hooks write its OAM.</param>
    /// <param name="camera">Layer-one camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="firstLayer">First native draw layer, inclusively; the range <see cref="AdvanceLayers"/> prepared.</param>
    /// <param name="lastLayer">Last native draw layer, inclusively; the range <see cref="AdvanceLayers"/> prepared.</param>
    /// <exception cref="InvalidOperationException">The range was not prepared by <see cref="AdvanceLayers"/>.</exception>
    public void DrawLayers(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        int firstLayer,
        int lastLayer)
    {
        Ensure.NotNull(sprites);
        if (_prepared is not { } prepared || _preparedLayers != new EnemyDrawLayerRange(firstLayer, lastLayer))
            throw new InvalidOperationException($"Enemy layers {firstLayer}..{lastLayer} were not advanced before drawing.");
        _prepared = null;

        int next = 0;
        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            for (; next < prepared.Count && prepared[next].Layer == layer; next++)
            {
                PreparedEnemyDraw draw = prepared[next];
                RoomEnemySlot slot = draw.Slot;
                var source = new SpriteSource(SpriteSourceKind.Enemy, draw.EnemyIndex / EnemySlotTable.EnemyIndexStride);
                // An occupant may draw supplemental sprites before the common writer emits its body (Ridley's tail
                // and wings), preserving both that drawing order and the enemy's normal layer queue.
                if (slot.Entity is IEnemyDrawHooks drawHooks)
                    drawHooks.DrawBeforeBody(sprites, camera);
                if (draw.Frame is { } frame)
                {
                    // The common empty frame is compiled definition data and draws nothing.
                    if (frame == EnemyFrameId.Empty)
                        continue;
                    _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(slot.DefinitionId, EnemySheetDefinitions.FrameName(frame),
                        new Position16<ScreenPixels>(draw.Origin.X, draw.Origin.Y), draw.PaletteBits, slot.TileIndexBase,
                        (slot.Entity as IShowsCorpseRot)?.ShownRot));
                    continue;
                }

                // This installed frame owns only the composite's visual offsets and parts.
                // Collision still uses the original engine-owned extended hitbox records,
                // never the editable JSON geometry.
                ReadOnlySpan<EnemyExtendedDrawComponent> components = (draw.Components ?? throw new InvalidOperationException(
                    "An extended enemy draw was prepared without its components.")).Span;
                for (int index = 0; index < components.Length; index++)
                {
                    EnemyExtendedDrawComponent component = components[index];
                    Position16<OamPixels> componentOrigin = draw.Origin.ToOam() + component.Offset;
                    if (!EnemyDrawRange.Contains(componentOrigin))
                        continue;
                    _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(slot.DefinitionId,
                        EnemySheetDefinitions.ComponentFrameName(draw.ExtendedFrame ?? throw new InvalidOperationException(
                            "An extended enemy draw was prepared without its frame."), index),
                        new Position16<ScreenPixels>(componentOrigin.X, componentOrigin.Y), draw.PaletteBits, slot.TileIndexBase,
                        (slot.Entity as IShowsCorpseRot)?.ShownRot));
                }
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
    /// <param name="Layer">The native draw layer it was queued on.</param>
    /// <param name="Slot">The enemy.</param>
    /// <param name="EnemyIndex">The enemy's index.</param>
    /// <param name="Origin">Its shaken screen origin.</param>
    /// <param name="PaletteBits">The common draw palette selected for this frame.</param>
    /// <param name="Frame">An ordinary enemy's frame; null for an extended enemy.</param>
    /// <param name="ExtendedFrame">The extended frame an extended enemy shows, which names its components' sheet frames; null for an ordinary enemy.</param>
    /// <param name="Components">An extended enemy's installed components; null for an ordinary enemy.</param>
    private readonly record struct PreparedEnemyDraw(int Layer, RoomEnemySlot Slot, int EnemyIndex,
        Position16<ScreenPixels> Origin, int PaletteBits, EnemyFrameId? Frame, EnemyFrameId? ExtendedFrame,
        ReadOnlyMemory<EnemyExtendedDrawComponent>? Components);

    /// <summary>
    /// Ports the palette selection at <c>$A0:9473-$A0:949A</c>, immediately before
    /// <c>WriteEnemyOams</c> emits either an ordinary or extended enemy spritemap.
    /// </summary>
    /// <remarks>
    /// A hit does not replace an enemy's stored graphics-set palette. On alternating
    /// phases of the shared enemy frame counter, the writer temporarily supplies OBJ
    /// palette zero; the following draw restores the actor's assigned palette. Frozen
    /// flashing similarly selects OBJ palette six. Keeping this at the common writer is
    /// important: Space Pirates are merely the first ordinary enemies whose missing flash
    /// was obvious, and a family-specific palette mutation would corrupt every later draw.
    /// Ridley's translated AI already publishes the exact common-writer palette because
    /// its body, wings, and tail must share one latched phase.
    /// </remarks>
    private ushort SelectCommonEnemyDrawPalette(RoomEnemySlot slot)
    {
        if (slot.Entity is IEnemyDrawHooks drawHooks && drawHooks.CommonDrawPaletteIndex() is { } owned)
            return owned;

        // A hit flash shows the common sprite colours on alternate phases.
        if (!slot.FlashTimer.IsZero && (_frameContext.RandomEnemyCounter & 2) != 0)
            return SnesObjPalettes.Palette(SpriteColourSetDefinitions.Common).PaletteBits;

        if (!slot.FrozenTimer.IsZero &&
            (slot.FrozenTimer.RawValue >= 90 || (slot.FrozenTimer.RawValue & 2) != 0))
        {
            return EnemyFrozenPaletteDefinitions.PaletteBits;
        }

        return checked((ushort)slot.PaletteIndex);
    }
}
