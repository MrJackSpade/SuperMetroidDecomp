using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One of the Baby Metroid's three pulse poses, an index into <see cref="BabyMetroidVisualDefinitions"/>.</summary>
/// <param name="Index">The pose's position in the catalog: closed, half open, wide open.</param>
public readonly record struct BabyMetroidFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => BabyMetroidVisualDefinitions.Spritemap(Index);
}
