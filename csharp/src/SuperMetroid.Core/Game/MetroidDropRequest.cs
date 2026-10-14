namespace SuperMetroid.Core.Game;

/// <summary>
/// One requested drop from <c>Enemy_ItemDrop_Metroid</c> at <c>$A0:B968</c>. Actual pickup
/// actors are spawned into the shared enemy-projectile pool. This empty marker counts
/// each of the five requests; it does not retain the pickup's RNG-scattered coordinates.
/// </summary>
public readonly record struct MetroidDropRequest();
