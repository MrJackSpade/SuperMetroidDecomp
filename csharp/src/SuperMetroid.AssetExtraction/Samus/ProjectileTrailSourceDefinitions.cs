using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where the bank-$90 trail programs are stored.</summary>
internal static class ProjectileTrailSourceDefinitions
{
    /// <summary>The bank-$90 address of each program's first step.</summary>
    internal static readonly FrozenDictionary<ProjectileTrailProgram, ushort> Starts = new Dictionary<ProjectileTrailProgram, ushort>
    {
        [ProjectileTrailProgram.Empty] = 0xb4c9,
        [ProjectileTrailProgram.LeftIce] = 0xb4cb,
        [ProjectileTrailProgram.RightIce] = 0xb52d,
        [ProjectileTrailProgram.Wave] = 0xb58f,
        [ProjectileTrailProgram.Missile] = 0xb5a1,
    }.ToFrozenDictionary();
}
