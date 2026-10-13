using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// EnemyMain's draw phase (<c>WriteEnemyOams</c>): the per-layer queues the enemy pass fills, the common
/// draw-palette selection, and the ordinary, extended and supplemental sprite output with its BG2 frame side effects.
/// </summary>
internal sealed class EnemyDrawList
{
    /// <summary>Draws Ceres Ridley's tail and wings before its common body spritemap.</summary>
    private readonly CeresRidleyBehavior _ceresRidleyBehavior;
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
    /// <summary>Supplies Ridley's latched body, wing, and tail palette index.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Supplies installed extended frames and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves native queue indices to current enemy slots at draw time.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Retains the behavior and presentation owners used by EnemyMain's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _ceresRidleyBehavior = dependencies.CeresRidleyBehavior;
        _deadTorizoBehavior = dependencies.DeadTorizoBehavior;
        _frameContext = dependencies.FrameContext;
        _instructions = dependencies.Instructions;
        _motherBrainBehavior = dependencies.MotherBrainBehavior;
        _presentation = dependencies.Presentation;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }
    /// <summary>Native enemy indices grouped into the eight ordered drawing layers.</summary>
    internal readonly List<ushort>[] _drawQueues =
        Enumerable.Range(0, 8).Select(_ => new List<ushort>()).ToArray();

    /// <summary>Empties every layer queue before the enemy pass rebuilds them.</summary>
    internal void Clear()
    {
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();
    }

    /// <summary>Queues an enemy, by native index, for its draw layer.</summary>
    internal void Enqueue(int layer, ushort nativeIndex) => _drawQueues[layer].Add(nativeIndex);

    /// <summary>
    /// Writes queued enemies for an inclusive layer range using native slot order.
    /// </summary>
    /// <remarks>Drawing consumes each emitted actor's shake timer and may apply installed extended-frame BG2 presentation changes; it is a native draw phase, not a side-effect-free snapshot render.</remarks>
    /// <param name="oam">OAM buffer receiving the selected actors' ordinary, extended, and supplemental sprites.</param>
    /// <param name="camera">Layer-one camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="firstLayer">First native draw layer, inclusively, in the range 0..7.</param>
    /// <param name="lastLayer">Last native draw layer, inclusively, in the range 0..7 and not below firstLayer.</param>
    /// <exception cref="ArgumentNullException">The OAM buffer is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The inclusive layer range is invalid.</exception>
    public void DrawLayers(
        OamBuffer oam,
        Position16<RoomPixels> camera,
        int firstLayer,
        int lastLayer)
    {
        Ensure.NotNull(oam);
        _room.EnsureLoaded();
        if ((uint)firstLayer > 7 || (uint)lastLayer > 7 || firstLayer > lastLayer)
            throw new ArgumentOutOfRangeException(nameof(firstLayer));

        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            foreach (ushort nativeIndex in _drawQueues[layer])
            {
                RoomEnemySlot slot = _slotTable.SlotFromNativeIndex(nativeIndex);
                // Power-bomb damage runs after queue construction and can clear a
                // queued actor or replace it with the inert respawn reservation.
                // Neither lifecycle state owns a display composition.
                if (slot.EnemyDefinitionPointer is 0 or EnemyDefinitionId.Respawn)
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
                if (RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer))
                {
                    // Both Ridley mains call DrawRidleyTail/DrawRidleyWings before the
                    // common WriteEnemyOams pass emits the extended body. Appending these
                    // here preserves both that OAM order and the enemy's normal layer queue.
                    _ceresRidleyBehavior.DrawRidleySupplementalSprites(oam, slot, camera);
                }
                if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
                {
                    _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Bank = slot.Definition.Bank, Pointer = slot.SpritemapPointer, Origin = origin.ToOam(), PaletteBits = drawPaletteIndex, BaseTileIndex = slot.VramTilesIndex });
                    continue;
                }

                if (_room.Artwork?.ExtendedFrames?.TryGetDisplay(slot.Definition.Bank,
                        slot.SpritemapPointer,
                        out ReadOnlyMemory<EnemyExtendedDrawComponent> installed) == true)
                {
                    // Resolve both halves through one presentation binding. Validate
                    // the composition before any BG2 side effects; physical selectors
                    // continue to belong to compiled animation and collision programs.
                    ushort selected = _room.Artwork.ExtendedFrames.GetDisplayPointer(
                        slot.Definition.Bank, slot.SpritemapPointer);
                    _instructions.ApplyInstalledEnemyBg2Frame(slot, selected);
                    // This installed frame owns only the composite's visual offsets
                    // and OAM parts. Collision still uses the original engine-owned
                    // extended hitbox records, never the editable JSON geometry.
                    foreach (EnemyExtendedDrawComponent component in installed.Span)
                    {
                        Position16<OamPixels> componentOrigin = origin.ToOam() + component.Offset;
                        if (((componentOrigin.X + 128) & 0xfe00) != 0 ||
                            ((componentOrigin.Y + 128) & 0xfe00) != 0)
                            continue;
                        oam.AddEnemySpritemap(component.Parts, componentOrigin, drawPaletteIndex, slot.VramTilesIndex,
                            clipVerticalWrap: true,
                            originYIsOnScreen: (SnesLittleEndian.HighByte(componentOrigin.Y)) == 0);
                    }
                    continue;
                }

                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} extended frame " +
                    $"${slot.Definition.Bank:X2}:{slot.SpritemapPointer:X4} has no installed presentation.");
            }

            // EnemyGraphicsDrawnHook runs after the complete ordinary enemy queue. Mother
            // Brain installs it from its hidden head record, but the hook itself belongs to
            // the record's authored layer-five pass and therefore must not run in both of
            // the frontend's disjoint low/high-priority layer ranges.
            if (layer == 5)
            {
                _motherBrainBehavior.DrawMotherBrainHook(oam, camera);
                _deadTorizoBehavior.DrawDeadTorizoHook(oam, camera);
            }
        }
    }

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
        if (RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer) && _ridleyEncounter.Ridley is not null)
            return _ridleyEncounter.Ridley.CommonDrawPaletteIndex;

        if (!slot.FlashTimer.IsZero && (_frameContext._randomEnemyCounter & 2) != 0)
            return 0;

        if (!slot.FrozenTimer.IsZero &&
            (slot.FrozenTimer.RawValue >= 0x005a || (slot.FrozenTimer.RawValue & 2) != 0))
        {
            return 0x0c00;
        }

        return slot.PaletteIndex;
    }
}
