namespace SuperMetroid.Core.Rendering;

/// <summary>The range of a VRAM word address that a render layer names as a tilemap or character base.</summary>
public static class VramWordRange
{

    /// <summary>Returns <paramref name="word"/> when it addresses VRAM, or throws.</summary>
    /// <param name="word">A VRAM word address, 0 through <c>MaximumWord</c>.</param>
    public static int Require(int word) => SnesPpuLayout.RequireVramWord(word);
}
