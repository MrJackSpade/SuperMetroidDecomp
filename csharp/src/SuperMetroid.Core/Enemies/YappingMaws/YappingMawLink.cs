namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// One of the Yapping Maw's four tongue links (#2162), counted from the root: the body projectile showing it at the link's
/// near end (the Maw's root for the first link, the previous link's far end for the others), and the solved offset of its
/// far end from the root. The last link's far end is the mouth.
/// </summary>
public sealed partial class YappingMawLink
{
    /// <summary>Links the tongue to <paramref name="body"/>, its far end still at the root.</summary>
    /// <param name="body">The body projectile the Maw spawned for the link at its room load.</param>
    internal YappingMawLink(EnemyProjectile body) => Body = Ensure.NotNull(body);

    /// <summary>The body projectile showing the link, which the Maw spawned at its room load and places every update.</summary>
    public EnemyProjectile Body { get; }

    /// <summary>The signed offset of the link's far end from the Maw's root (Y positive downward), as the curl last solved it.</summary>
    public Offset16<RoomPixels> Offset { get; internal set; }
}
