using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The name, kind, count and address at of one torizo family.</summary>
/// <param name="Name">The name.</param>
/// <param name="Kind">The kind.</param>
/// <param name="Count">The count.</param>
/// <param name="AddressAt">The address at.</param>
internal readonly record struct TorizoFamily(string Name, RoomEnemyProjectileKind Kind, int Count, Func<int, ushort> AddressAt);
