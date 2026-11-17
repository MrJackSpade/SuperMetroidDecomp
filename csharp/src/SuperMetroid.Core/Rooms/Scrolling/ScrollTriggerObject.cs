using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// A room's scroll trigger (#1554): a block of special air that, once Samus touches it, applies its room's authored scroll
/// change on the next room-object update and then rests again, ready for the next touch.
/// </summary>
/// <param name="change">The room's authored scroll change.</param>
internal sealed class ScrollTriggerObject(RoomScrollChange change) : RoomObjectBehaviour
{
    /// <summary>Whether Samus has touched the trigger since its writes last ran; repeated touches before then change nothing.</summary>
    internal bool Triggered { get; private set; }

    /// <summary>The authored scroll change the trigger applies, independent of later room-argument edits.</summary>
    internal RoomScrollChange Change { get; } = change;

    /// <summary>Samus touches the trigger: its writes run on the next room-object update.</summary>
    internal void Touch() => Triggered = true;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!Triggered)
            return;
        RoomObjectScrollChanges.Apply(Change, context.Frame.Scrolls.SetScreen);
        Rest(context.Level, place.Block);
    }

    /// <summary>
    /// Makes the trigger's block special air that reacts as a scroll trigger, as its setup does and as it rests again
    /// after its writes.
    /// </summary>
    /// <param name="level">The room's level data.</param>
    /// <param name="block">The trigger's block.</param>
    internal void Rest(RoomLevelData level, Position16<RoomBlocks> block)
    {
        Ensure.NotNull(level);
        Triggered = false;
        level.SetTerrain(block, level.GetCollisionBlock(block).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(block, RoomBlockBehavior.Reacting(SpecialAirReaction.ScrollTrigger));
    }
}
