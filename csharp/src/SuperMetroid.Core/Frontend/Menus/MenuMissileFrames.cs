namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Names the selection missile's pictures (<see cref="MenuMissileAnimationDefinitions"/>) for every menu that shows it.</summary>
public static partial class MenuMissileFrames
{
    /// <summary>The name the missile's picture <paramref name="frame"/> is installed under.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of the missile's pictures.</exception>
    internal static string Name(MenuMissileFrame frame) => MenuMissileAnimationDefinitions.FrameNames[frame];

    /// <summary>The names of the missile's pictures in the order its roll shows them.</summary>
    public static IReadOnlyList<string> Names { get; } = [.. MenuMissileAnimationDefinitions.Roll.Select(Name)];
}
