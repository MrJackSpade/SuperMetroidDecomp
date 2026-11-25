using Engine.Shared;

namespace Engine.Ui;

/// <summary>
/// Something whose pictures can be shown with some of their colours changed: an actor's visual, a room's terrain, a
/// backdrop. Whoever changes colours does so under its own name, so several may change different colours of the same
/// pictures at once and each takes back only its own.
/// </summary>
public interface IRecoloured
{
    /// <summary>
    /// Shows the colours <paramref name="swaps"/> lists as the colours they are swapped for, in place of whatever
    /// <paramref name="source"/> last asked for; an empty list takes back all that source asked for.
    /// </summary>
    /// <param name="source">Who asks: the name the change is kept under.</param>
    /// <param name="swaps">The colours changed, and what each is shown as.</param>
    void Recolour(string source, IReadOnlyList<ColourSwap> swaps);

    /// <summary>
    /// Whether any picture it shows is the picture at <paramref name="place"/>, or lies in the folder that names: a
    /// change of colours written for the pictures of one place is shown only on what shows pictures from it.
    /// </summary>
    /// <param name="place">A picture's path within the assets tree, or a folder of pictures.</param>
    bool ShowsPicturesIn(string place);
}
