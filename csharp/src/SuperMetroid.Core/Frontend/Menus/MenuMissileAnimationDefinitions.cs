namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The selection missile file select, options and game over share: the pictures of its roll, each frame's hold being
/// installed (#1615).
/// </summary>
public static class MenuMissileAnimationDefinitions
{
    /// <summary>The pictures the missile rolls through before it starts its roll again.</summary>
    public const int FrameCount = 4;

    /// <summary>
    /// The name each of the missile's pictures is installed under, in the order its roll shows them (#2202): a fin raised
    /// behind the red flame, fins above and below as the flame dims, a fin lowered behind the red flame, and the fins
    /// turned flat to the screen behind a yellow flame.
    /// </summary>
    internal static readonly string[] FrameNames = ["Cursor.FinUp", "Cursor.FinsUpAndDown", "Cursor.FinDown", "Cursor.FinsFlat"];
}
