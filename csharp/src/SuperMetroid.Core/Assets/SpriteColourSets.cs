namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's sprite colour sets (#1405). A room load fills the common sprite colours and the enemy projectiles', the
/// room's graphics set loads each enemy's colours into the set it gives that enemy, and enemy effects change them. A
/// sprite shows the set its graphics set gives it, or one its AI or a hit flash selects. The colours Samus holds are
/// hers, not the sets'.
/// </summary>
public sealed class SpriteColourSets : ColourSets, ISpriteColourSets
{
    /// <summary>Empty sets.</summary>
    public SpriteColourSets()
        : base(128)
    {
    }
}
