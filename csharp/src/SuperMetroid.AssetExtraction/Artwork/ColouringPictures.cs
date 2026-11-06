using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where a sheet pictures the frames each colouring draws (#1513): every frame is pictured once, its palette-inheriting
/// parts in the sprite set of its standard colouring, so each colouring's drawn shade shows the colouring's colours over
/// the inks of every set its frames are pictured in. A set some colouring's shade shows in another set's colours is
/// remapped; no frame drawn in that colouring may keep its own colours in that set.
/// </summary>
/// <typeparam name="TColouring">The screen's colouring domain.</typeparam>
internal sealed class ColouringPictures<TColouring>
    where TColouring : struct, Enum
{
    /// <summary>The sets each colouring's frames are pictured in.</summary>
    private readonly Dictionary<TColouring, SortedSet<int>> pictured;

    /// <summary>Keeps the sets each colouring's frames are pictured in, and the sets some colouring remaps.</summary>
    private ColouringPictures(Dictionary<TColouring, SortedSet<int>> pictured, SortedSet<int> remapped)
    {
        this.pictured = pictured;
        Remapped = remapped;
    }

    /// <summary>The sets some colouring's drawn shade shows in another set's colours.</summary>
    internal SortedSet<int> Remapped { get; }

    /// <summary>The sets the frames drawn in <paramref name="colouring"/> are pictured in, in rising order; none when no frame is.</summary>
    internal SortedSet<int> SetsOf(TColouring colouring) => pictured.TryGetValue(colouring, out SortedSet<int>? sets) ? sets : [];

    /// <summary>
    /// Where <paramref name="frames"/> picture each colouring, each frame's palette row its standard colouring's set as
    /// <paramref name="setOf"/> gives it.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A frame is not pictured in its standard colouring's set, or a part of a frame keeps its own colours in a set the
    /// drawn shade of one of the frame's colourings shows in that colouring's colours, which one picture cannot show.
    /// </exception>
    internal static ColouringPictures<TColouring> Of(IReadOnlyList<ColouredFrame<TColouring>> frames, Func<TColouring, int> setOf)
    {
        Ensure.NotNull(frames);
        Ensure.NotNull(setOf);
        var pictured = new Dictionary<TColouring, SortedSet<int>>();
        var remapped = new SortedSet<int>();
        foreach (ColouredFrame<TColouring> frame in frames)
        {
            int set = setOf(frame.Colourings[0]);
            if (frame.Frame.Palette != set)
                throw new InvalidDataException($"Sheet frame {frame.Frame.Name} is not pictured in its standard colouring {frame.Colourings[0]}'s set {set}.");
            foreach (TColouring colouring in frame.Colourings)
            {
                if (!pictured.TryGetValue(colouring, out SortedSet<int>? sets))
                {
                    sets = [];
                    pictured.Add(colouring, sets);
                }
                sets.Add(set);
                if (setOf(colouring) != set)
                    remapped.Add(set);
            }
        }
        foreach (ColouredFrame<TColouring> frame in frames)
        {
            foreach (TColouring colouring in frame.Colourings)
            {
                foreach (SpriteFramePart part in frame.Frame.Frame.Parts)
                {
                    if (part.Palette is int own && pictured[colouring].Contains(own) && setOf(colouring) != own)
                    {
                        throw new InvalidDataException($"Sheet frame {frame.Frame.Name} keeps its own colours in set {own}, which its colouring "
                            + $"{colouring}'s drawn shade shows in that colouring's colours.");
                    }
                }
            }
        }
        return new ColouringPictures<TColouring>(pictured, remapped);
    }
}
