using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One statue fragment the breaking Bomb Torizo hand throws from its block.</summary>
/// <param name="Fragment">The piece that falls, which selects its animation, place and fall.</param>
/// <param name="RoomObjectBlock">The block the hand occupies, which the fragment's placement is relative to.</param>
public sealed record BombTorizoStatueProjectileRequest(
    BombTorizoStatueFragment Fragment,
    Position16<RoomBlocks> RoomObjectBlock) : RoomObjectProjectileRequest(RoomObjectBlock)
{
    /// <inheritdoc/>
    internal override void React<TContext>(RoomObjectProjectileRequestReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).BombTorizoStatueProjectileRequest(this, context);
}
