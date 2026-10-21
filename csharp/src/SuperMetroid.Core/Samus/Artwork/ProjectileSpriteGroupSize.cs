namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>How many compositions one projectile or charge-flare family holds.</summary>
/// <param name="Group">The family.</param>
/// <param name="Count">Its compositions.</param>
internal readonly record struct ProjectileSpriteGroupSize(ProjectileSpriteGroup Group, int Count)
{
    /// <summary>The family's compositions, in order.</summary>
    internal IEnumerable<ProjectileSpriteId> Members
    {
        get
        {
            ProjectileSpriteGroup group = Group;
            return Enumerable.Range(0, Count).Select(index => new ProjectileSpriteId(group, index));
        }
    }
}
