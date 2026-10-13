using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The Zebetites' health-tier programs, in bank $A6 order: five for the large barrier, then five for the halves of
/// a split pair. Each shows its tier's pose once and sleeps.
/// </summary>
public enum ZebetiteProgram
{
    /// <summary><c>InstList_Big_HealthGreaterThanEqualTo800</c> (<c>$A6:FDCC</c>): large barrier, health 800 or more.</summary>
    LargeAtLeast800,
    /// <summary>Large barrier, health 600 to 799 (<c>$A6:FDD2</c>).</summary>
    LargeAtLeast600,
    /// <summary>Large barrier, health 400 to 599 (<c>$A6:FDD8</c>).</summary>
    LargeAtLeast400,
    /// <summary>Large barrier, health 200 to 399 (<c>$A6:FDDE</c>).</summary>
    LargeAtLeast200,
    /// <summary>Large barrier, health below 200 (<c>$A6:FDE4</c>).</summary>
    LargeBelow200,
    /// <summary>Split pair, health 800 or more (<c>$A6:FDEA</c>).</summary>
    SplitAtLeast800,
    /// <summary>Split pair, health 600 to 799 (<c>$A6:FDF0</c>).</summary>
    SplitAtLeast600,
    /// <summary>Split pair, health 400 to 599 (<c>$A6:FDF6</c>).</summary>
    SplitAtLeast400,
    /// <summary>Split pair, health 200 to 399 (<c>$A6:FDFC</c>).</summary>
    SplitAtLeast200,
    /// <summary>Split pair, health below 200 (<c>$A6:FE02</c>).</summary>
    SplitBelow200,
}

/// <summary>One Zebetite health pose, an index into <see cref="ZebetiteVisualDefinitions"/> in program order.</summary>
/// <param name="Index">The pose's position: large tiers 0 to 4, then split tiers 5 to 9.</param>
public readonly record struct ZebetiteFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => ZebetiteVisualDefinitions.Spritemap(Index);
}

/// <summary>The Zebetites' typed health-tier programs (#641).</summary>
internal static class ZebetiteAnimations
{
    /// <summary>Every health-tier program: its pose for one update, then the native sleep.</summary>
    internal static AnimationProgramSet<ZebetiteProgram, ZebetiteFrame, NoAnimationCue> Programs { get; } = new(
        Enum.GetValues<ZebetiteProgram>().ToDictionary(program => program, program =>
            new[]
            {
                AnimationProgramStep<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>.Show(new ZebetiteFrame((int)program), 1),
                AnimationProgramStep<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>.Hold,
            }));

    /// <summary>
    /// The program for a barrier's form and health: large or split, then one tier for each 200 health below 1000,
    /// the last covering everything under 200.
    /// </summary>
    internal static ZebetiteProgram HealthProgram(bool linkedPair, ushort health)
    {
        int tier = health < 200 ? 4 :
            health < 400 ? 3 :
            health < 600 ? 2 :
            health < 800 ? 1 : 0;
        return (linkedPair ? ZebetiteProgram.SplitAtLeast800 : ZebetiteProgram.LargeAtLeast800) + tier;
    }
}
