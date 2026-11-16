namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Names the selection missile's pictures (<see cref="MenuMissileAnimationDefinitions"/>) for every menu that shows it.</summary>
public static class MenuMissileFrames
{
    /// <summary>The name the missile's picture at step <paramref name="frame"/> of its roll is installed under.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a step of the roll.</exception>
    public static string Name(int frame) => Ensure.Element(MenuMissileAnimationDefinitions.FrameNames, frame);

    /// <summary>The names of the missile's pictures in the order its roll shows them.</summary>
    public static IReadOnlyList<string> Names => MenuMissileAnimationDefinitions.FrameNames;
}
