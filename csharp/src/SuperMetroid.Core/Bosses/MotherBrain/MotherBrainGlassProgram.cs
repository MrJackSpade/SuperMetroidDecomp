namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass projectile programs: eight spinning shard loops by flight angle, and the sparkle.</summary>
public enum MotherBrainGlassProgram
{
    /// <summary>Shard angle group 0 ($86:CC93).</summary>
    ShardGroup0,
    /// <summary>Shard angle group 1 ($86:CCB7).</summary>
    ShardGroup1,
    /// <summary>Shard angle group 2 ($86:CCDB).</summary>
    ShardGroup2,
    /// <summary>Shard angle group 3 ($86:CCFF).</summary>
    ShardGroup3,
    /// <summary>Shard angle group 4 ($86:CD23).</summary>
    ShardGroup4,
    /// <summary>Shard angle group 5 ($86:CD47).</summary>
    ShardGroup5,
    /// <summary>Shard angle group 6 ($86:CD6B).</summary>
    ShardGroup6,
    /// <summary>Shard angle group 7 ($86:CD8F).</summary>
    ShardGroup7,
    /// <summary>The finite sparkle a falling shard sheds ($86:CDB3).</summary>
    Sparkle,
}
