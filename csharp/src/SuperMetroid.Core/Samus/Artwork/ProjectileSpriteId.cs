namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One Samus projectile or charge-flare composition: its family and its place in that family.</summary>
/// <param name="Group">The composition's family.</param>
/// <param name="Index">Its place within the family, counted from zero.</param>
public readonly record struct ProjectileSpriteId(ProjectileSpriteGroup Group, int Index)
{
    /// <summary>The composition's name in installed composition and binding files, such as <c>Power.3</c>.</summary>
    public string Name => $"{Group}.{Index}";

    /// <summary>Whether this is the empty composition an invisible projectile shows, which draws nothing (#1513).</summary>
    public bool DrawsNothing => Group == ProjectileSpriteGroup.Nothing;
}
