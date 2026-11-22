namespace SuperMetroid.Core.Title;

/// <summary>The floor transform one title scene starts with.</summary>
/// <param name="Phase">The phase the scene runs in.</param>
/// <param name="startingZoom">The starting zoom, which the scene's zoom doubles.</param>
/// <param name="Position">The starting offset.</param>
public readonly record struct SceneDefinition(
    int Phase,
    int startingZoom,
    int Position)
{
    /// <summary>The starting zoom, doubled.</summary>
    public int Zoom { get; } = startingZoom * 2;
}

/// <summary>A fade, whose level its start gives.</summary>
/// <param name="start">The start, one below the fade's level.</param>
public sealed record Fade(int start)
{
    /// <summary>How deep the fade is.</summary>
    public int Level { get; } = start + 1;

    /// <summary>How long the fade lasts.</summary>
    public int Length { get; } = 4;
}

/// <summary>The scenes the title runs.</summary>
public static class Scenes
{
    /// <summary>The first pan.</summary>
    public static readonly SceneDefinition FirstPan = new(1, 3, 5);

    /// <summary>The first fade.</summary>
    public static readonly Fade FirstFade = new(2);
}
