using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// A room's scroll trigger (#1554): a block of special air that, once Samus touches it, applies its ordered scroll-cell
/// writes on the next room-object update and then rests again, ready for the next touch. It replaces the trigger's
/// family state and sleeping program.
/// </summary>
internal sealed class ScrollTriggerObject : RoomObjectBehaviour
{
    /// <summary>Creates a trigger that applies a room's authored program or a constructed room's validated writes.</summary>
    /// <param name="writes">A constructed room's writes, in order; null when the trigger applies <paramref name="program"/>.</param>
    /// <param name="program">The room's authored program; null when the trigger applies <paramref name="writes"/>.</param>
    /// <exception cref="InvalidOperationException">Neither is given.</exception>
    internal ScrollTriggerObject(IReadOnlyList<RoomScrollWrite>? writes, RoomScrollChange? program)
    {
        Change = program;
        Writes = program.HasValue ? null : writes?.ToArray() ?? throw new InvalidOperationException(
            "A scroll trigger has neither an authored program nor constructed writes.");
    }

    /// <summary>Whether Samus has touched the trigger since its writes last ran; repeated touches before then change nothing.</summary>
    internal bool Triggered { get; private set; }

    /// <summary>The authored program the trigger applies, independent of later room-argument edits; null for constructed writes.</summary>
    internal RoomScrollChange? Change { get; }

    /// <summary>A constructed room's validated writes, in order; null when the trigger applies <see cref="Change"/>.</summary>
    internal IReadOnlyList<RoomScrollWrite>? Writes { get; }

    /// <summary>Samus touches the trigger: its writes run on the next room-object update.</summary>
    internal void Touch() => Triggered = true;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!Triggered)
            return;
        RoomScrollGrid scrolls = context.Frame.Scrolls ?? throw new InvalidOperationException(
            "A touched scroll trigger requires the active scroll grid.");
        if (Change is { } program)
            RoomObjectScrollChanges.Apply(program, scrolls.SetCell);
        else
            foreach (RoomScrollWrite write in Writes ?? throw new InvalidOperationException("A scroll trigger lost its writes."))
                scrolls.SetCell(write.Index, write.State);
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
        level.SetBehavior(block, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger));
    }
}
