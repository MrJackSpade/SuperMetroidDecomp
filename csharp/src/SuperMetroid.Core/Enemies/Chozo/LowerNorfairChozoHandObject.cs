using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair Chozo statue's hand (#1554). On its first update it checks whether the statue has already lowered
/// the acid: if so it lowers the acid to its resting height, draws the cleared hand and leaves the room on its second
/// update; otherwise, from its second update on, it waits until the Power Bomb block at the hand's trigger block has
/// broken open, makes that block the hand's special block and leaves the room on that update.
/// </summary>
/// <param name="statue">The room's Chozo statue objects, which hold the acid and the lowered-acid event.</param>
[RoomLifetime]
internal sealed class LowerNorfairChozoHandObject(ChozoStatueObjects statue) : RoomObjectBehaviour
{
    /// <summary>The room's Chozo statue objects.</summary>
    private readonly ChozoStatueObjects _statue = Ensure.NotNull(statue);

    /// <summary>Where the hand is in its course.</summary>
    internal LowerNorfairChozoHandPhase Phase { get; private set; } = LowerNorfairChozoHandPhase.Starting;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        switch (Phase)
        {
            case LowerNorfairChozoHandPhase.Starting when _statue.HasLoweredLowerNorfairAcid:
                _statue.SetLoweredAcidHeight();
                context.Draw(place, ChozoStatueObjectDefinitions.LoweredHand);
                Phase = LowerNorfairChozoHandPhase.Lowered;
                return;
            case LowerNorfairChozoHandPhase.Starting:
                Phase = LowerNorfairChozoHandPhase.Waiting;
                return;
            case LowerNorfairChozoHandPhase.Waiting:
                RoomLevelData level = context.Level;
                Position16<RoomBlocks> trigger = level.GridBlock(new(ChozoStatueRoomDefinitions.LowerNorfairTriggerX, ChozoStatueRoomDefinitions.LowerNorfairTriggerY));
                if (!context.RoomObjects.BlockReactions.IsBrokenOpen(trigger))
                    return;
                ChozoStatueObjects.WriteChozoBlock(level, trigger, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.LowerNorfairHandBehavior);
                context.RoomObjects.DeleteObject(place);
                return;
            case LowerNorfairChozoHandPhase.Lowered:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(LowerNorfairChozoHandPhase)} {Phase}.");
        }
    }
}
