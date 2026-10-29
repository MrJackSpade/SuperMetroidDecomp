using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Escape;

/// <summary>The background colours the escape typewriter copies its text colours from (#1405).</summary>
public static class EscapeTypewriterColourDefinitions
{
    /// <summary>The two room colours the text takes: the sixth tile set's colours thirteen and fourteen.</summary>
    public static readonly BackgroundColourRun Source = new(BackgroundColourSet.TilesSixth, 13, 2);
}
