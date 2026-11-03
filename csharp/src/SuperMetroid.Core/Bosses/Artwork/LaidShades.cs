using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The shades a boss has laid over one group of pictures, in the order it laid them (#1512): each colour load of a boss
/// encounter is a shade the import pre-colours, a later load covers the colours an earlier one shares with it, and a
/// shade of the same effect replaces the one laid before it, as loading a fade's next step replaced its last.
/// </summary>
internal sealed class LaidShades
{
    /// <summary>The shades laid, oldest first.</summary>
    private readonly List<PictureShade> laid = [];

    /// <summary>The shades laid, oldest first.</summary>
    internal IReadOnlyList<PictureShade> Laid => laid;

    /// <summary>Lays <paramref name="shade"/> over the others, replacing an earlier shade of its effect.</summary>
    internal void Lay(PictureShade shade)
    {
        laid.RemoveAll(earlier => earlier.Effect == shade.Effect);
        laid.Add(shade);
    }

    /// <summary>A separate layer holding the shades laid here so far, as a copy of these colours into another row holds them.</summary>
    internal LaidShades Copy()
    {
        var copy = new LaidShades();
        copy.laid.AddRange(laid);
        return copy;
    }

    /// <summary><paramref name="drawn"/>, a draw's own shading, with the shades laid over it in order.</summary>
    internal PictureShading Over(PictureShading drawn)
    {
        Ensure.NotNull(drawn);
        PictureShading shading = drawn;
        foreach (PictureShade shade in laid)
            shading = shading.With(shade);
        return shading;
    }
}
