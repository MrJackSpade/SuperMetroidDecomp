using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The six ending cloud compositions, in the order of the bank-$8B cloud instruction lists.</summary>
public static class EndingCloudSpriteDefinitions
{
    /// <summary>Cloud roles in the order used by the ending actors.</summary>
    internal enum Role
    {
        /// <summary>Upper cloud pattern requested first by the ending actor lists.</summary>
        UpperPattern,
        /// <summary>Upper cloud edge paired with the first pattern.</summary>
        UpperEdge,
        /// <summary>Lower cloud edge drawn before its lower pattern.</summary>
        LowerEdge,
        /// <summary>Lower cloud pattern selected after the lower edge.</summary>
        LowerPattern,
        /// <summary>Right-hand side cloud.</summary>
        Right,
        /// <summary>Left-hand side cloud.</summary>
        Left,
    }

    /// <summary>The composition each cloud role draws.</summary>
    private static readonly Dictionary<Role, CinematicSpriteFrameDefinition> ByRole = new()
    {
        [Role.UpperPattern] = new("scene-b-upper-a", new(159), 16),
        [Role.UpperEdge] = new("scene-b-upper-b", new(160), 16),
        [Role.LowerEdge] = new("scene-b-lower-a", new(161), 16),
        [Role.LowerPattern] = new("scene-b-lower-b", new(162), 16),
        [Role.Right] = new("scene-a-right", new(163), 32),
        [Role.Left] = new("scene-a-left", new(164), 32),
    };

    /// <summary>The cloud compositions in <see cref="Role"/> order: four sixteen-part horizontal clouds, then two 32-part side clouds.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
        [.. Enum.GetValues<Role>().Select(role => ByRole[role])];
}
