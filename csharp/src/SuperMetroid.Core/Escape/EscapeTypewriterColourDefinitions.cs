using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Escape;

/// <summary>The escape typewriter's text colours and the background colours it copies them from (#1405, #1464).</summary>
public static class EscapeTypewriterColourDefinitions
{
    /// <summary>The two room colours the text takes: the sixth tile set's colours thirteen and fourteen.</summary>
    public static readonly BackgroundColourRun Source = new(BackgroundColourSet.TilesSixth, 13, 2);

    /// <summary>The two sprite colours the text shows, which take <see cref="Source"/>'s colours.</summary>
    public static readonly SpriteColourRun Text = new(SpriteColourStore.EnemiesFirst, 13, 2);
}
