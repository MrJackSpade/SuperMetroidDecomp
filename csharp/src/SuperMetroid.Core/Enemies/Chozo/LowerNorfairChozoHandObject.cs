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
internal sealed partial class LowerNorfairChozoHandObject(ChozoStatueObjects statue) : RoomObjectBehaviour
{
    /// <summary>The room's Chozo statue objects.</summary>
    private readonly ChozoStatueObjects _statue = Ensure.NotNull(statue);

    /// <summary>Where the hand is in its course.</summary>
    internal LowerNorfairChozoHandPhase Phase { get; set; } = LowerNorfairChozoHandPhase.Starting;

    /// <summary>What <c>Update</c> does for each <see cref="LowerNorfairChozoHandPhase"/> member, each member's own arm.</summary>
    private IdentityTable<LowerNorfairChozoHandPhase, Action<RoomObjectPlace, RoomObjectStepContext>> UpdateForLowerNorfairChozoHandPhase => field ??= new(new Dictionary<LowerNorfairChozoHandPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [LowerNorfairChozoHandPhase.Starting] = (place, context) =>
        {
            if (_statue.HasLoweredLowerNorfairAcid)
            {
                _statue.SetLoweredAcidHeight();
                context.Draw(place, ChozoStatueObjectDefinitions.LoweredHand);
                Phase = LowerNorfairChozoHandPhase.Lowered;
            }
            else
            {
                Phase = LowerNorfairChozoHandPhase.Waiting;
            }
        },
        [LowerNorfairChozoHandPhase.Waiting] = (place, context) =>
        {
            RoomLevelData level = context.Level;
            Position16<RoomBlocks> trigger = level.GridBlock(ChozoStatueRoomDefinitions.LowerNorfairTriggerBlock);
            if (!context.RoomObjects.BlockReactions.IsBrokenOpen(trigger))
                return;
            ChozoStatueObjects.WriteChozoBlock(level, trigger, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.LowerNorfairHandBehavior);
            context.RoomObjects.DeleteObject(place);
        },
        [LowerNorfairChozoHandPhase.Lowered] = (place, context) => context.RoomObjects.DeleteObject(place),
    });

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        UpdateForLowerNorfairChozoHandPhase[Phase](place, context);
    }
}
