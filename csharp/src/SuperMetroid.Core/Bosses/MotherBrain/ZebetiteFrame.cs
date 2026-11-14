using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One Zebetite health pose (#1747), named for the health tier that shows it; <see cref="ZebetiteVisualDefinitions"/> holds
/// its picture. Its values are arbitrary.
/// </summary>
public enum ZebetiteFrame
{
    /// <summary>Large barrier, health 800 or more.</summary>
    LargeAtLeast800,
    /// <summary>Large barrier, health 600 to 799.</summary>
    LargeAtLeast600,
    /// <summary>Large barrier, health 400 to 599.</summary>
    LargeAtLeast400,
    /// <summary>Large barrier, health 200 to 399.</summary>
    LargeAtLeast200,
    /// <summary>Large barrier, health below 200.</summary>
    LargeBelow200,
    /// <summary>Split pair, health 800 or more.</summary>
    SplitAtLeast800,
    /// <summary>Split pair, health 600 to 799.</summary>
    SplitAtLeast600,
    /// <summary>Split pair, health 400 to 599.</summary>
    SplitAtLeast400,
    /// <summary>Split pair, health 200 to 399.</summary>
    SplitAtLeast200,
    /// <summary>Split pair, health below 200.</summary>
    SplitBelow200,
}
