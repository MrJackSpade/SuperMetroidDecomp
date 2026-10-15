using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Zebetite health pose, an index into <see cref="ZebetiteVisualDefinitions"/> in program order.</summary>
/// <param name="Index">The pose's position: large tiers 0 to 4, then split tiers 5 to 9.</param>
public readonly record struct ZebetiteFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => ZebetiteVisualDefinitions.Spritemap(Index);
}
