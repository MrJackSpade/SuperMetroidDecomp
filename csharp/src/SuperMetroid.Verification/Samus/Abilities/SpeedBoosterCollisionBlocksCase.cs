using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The BTS of one Speed Booster collision block case and the crumble the block it selects plays.</summary>
/// <param name="Bts">The block's BTS.</param>
/// <param name="Crumble">The crumble the selected block plays.</param>
internal readonly record struct SpeedBoosterCollisionBlocksCase(byte Bts, BlockReactionChange Crumble);
