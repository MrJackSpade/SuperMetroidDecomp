namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The selection missile file select, options and game over share: the pictures of its roll, each frame's hold being
/// installed (#1615).
/// </summary>
public static class MenuMissileAnimationDefinitions
{
    /// <summary>The pictures the missile's roll shows, in order, before it starts its roll again.</summary>
    internal static IReadOnlyList<MenuMissileFrame> Roll { get; } =
        [MenuMissileFrame.FinUp, MenuMissileFrame.FinsUpAndDown, MenuMissileFrame.FinDown, MenuMissileFrame.FinsFlat];

    /// <summary>The name each of the missile's pictures is installed under (#2202).</summary>
    internal static IReadOnlyDictionary<MenuMissileFrame, string> FrameNames { get; } = new Dictionary<MenuMissileFrame, string>
    {
        [MenuMissileFrame.FinUp] = "Cursor.FinUp",
        [MenuMissileFrame.FinsUpAndDown] = "Cursor.FinsUpAndDown",
        [MenuMissileFrame.FinDown] = "Cursor.FinDown",
        [MenuMissileFrame.FinsFlat] = "Cursor.FinsFlat",
    };
}
