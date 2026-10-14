namespace SuperMetroid.Core.Game;

/// <summary>One translated species-owned bank-$86 instruction; it advances <paramref name="cursor"/> past its operands.</summary>
internal delegate EnemyProjectileInstructionOutcome EnemyProjectileInstructionHandler(
    RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor);
