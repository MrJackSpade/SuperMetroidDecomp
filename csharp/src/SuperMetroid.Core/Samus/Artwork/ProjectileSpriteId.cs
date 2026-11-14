namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One Samus projectile or charge-flare composition: its family and the pose of the family it shows (#1762), such as
/// <c>"Power.UpRight"</c> or <c>"Wave.Right.Up1"</c>.
/// </summary>
/// <param name="Group">The composition's family.</param>
/// <param name="Pose">What the composition shows within its family: the direction it points or travels, and the stage of
/// its animation it shows.</param>
public readonly record struct ProjectileSpriteId(ProjectileSpriteGroup Group, string Pose)
{
    /// <summary>What the composition shows within its family.</summary>
    public string Pose { get; } = string.IsNullOrEmpty(Pose)
        ? throw new ArgumentException("A projectile composition's pose has a name.", nameof(Pose))
        : Pose;

    /// <summary>The composition's name in installed composition and binding files, such as <c>"Power.UpRight"</c>.</summary>
    public string Name => $"{Group}.{Pose}";

    /// <summary>Whether this is the empty composition an invisible projectile shows, which draws nothing (#1513).</summary>
    public bool DrawsNothing => Group == ProjectileSpriteGroup.Nothing;
}
