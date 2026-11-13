using SuperMetroid.Core;
using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where the bank-$90 trail programs are stored.</summary>
[EncodingOwner("bank-$90 addresses of the projectile trail programs")]
internal static class ProjectileTrailSourceDefinitions
{
    /// <summary>The bank-$90 address of each program's first step.</summary>
    internal static readonly FrozenDictionary<ProjectileTrailSegment, ushort> Starts = new Dictionary<ProjectileTrailSegment, ushort>
    {
        [ProjectileTrailSegment.Empty] = 0xb4c9,
        [ProjectileTrailSegment.LeftIce] = 0xb4cb,
        [ProjectileTrailSegment.RightIce] = 0xb52d,
        [ProjectileTrailSegment.Wave] = 0xb58f,
        [ProjectileTrailSegment.Missile] = 0xb5a1,
    }.ToFrozenDictionary();
}
