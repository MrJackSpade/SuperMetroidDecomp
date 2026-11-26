using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Ui;

/// <summary>
/// A change of colours as data lists it, place by place: for the pictures of each place, a picture's path within the
/// assets tree or a folder of pictures, every colour changed, written as the pictures have it, and the colour it is
/// shown as. Two sets of pictures may have one colour and change it differently, which is why it is listed by place:
/// each thing it is shown on is shown the colours of the places it shows pictures from.
/// </summary>
/// <remarks>
/// <code>
/// { "tiles/norfair": { "f8a800": "f8d038", "c05800/1": "e08010" }, "backdrops/norfair_cave": { "301808": "000000" } }
/// </code>
/// A colour is six hexadecimal digits, red, green and blue. One written with a number after a stroke is changed only
/// in that region of the pictures, as their maps of regions number it. The colour a colour is shown as may have two
/// digits more, how opaque it is shown: "00" not at all, "ff" wholly, which is what six digits alone mean.
/// </remarks>
public sealed class ColourChange
{
    private readonly (string Place, ColourSwap[] Swaps)[] _places;

    /// <summary>For each thing it has been shown on, the colours it changes there.</summary>
    private readonly Dictionary<IRecoloured, (string Places, ColourSwap[] Swaps)> _on = [];

    private ColourChange((string, ColourSwap[])[] places) => _places = places;

    /// <summary>The change that changes nothing: shown under a name, it takes back what that name asked for.</summary>
    public static ColourChange None { get; } = new([]);

    /// <summary>Reads a change from a group of places, each a group of colours.</summary>
    public static ColourChange Read(DataNode places)
    {
        ArgumentNullException.ThrowIfNull(places);
        return new ColourChange([.. places.Entries().Select(place => (place.Key, ReadSwaps(place.Value)))]);
    }

    /// <summary>Reads the colours of one place: a group naming each colour as the pictures have it and as it is shown.</summary>
    public static ColourSwap[] ReadSwaps(DataNode colours)
    {
        ArgumentNullException.ThrowIfNull(colours);
        var swaps = new List<ColourSwap>();
        foreach ((string from, DataNode to) in colours.Entries())
        {
            // A colour changed only in one region of the pictures has the region's number after a stroke.
            int stroke = from.IndexOf('/');
            int region = ColourSwap.Everywhere;
            if (stroke >= 0 && (!int.TryParse(from[(stroke + 1)..], out region) || region < 0))
                throw colours.Problem($"names the region \"{from[(stroke + 1)..]}\", which is not a region's number");
            swaps.Add(new ColourSwap(ReadColour(colours, stroke < 0 ? from : from[..stroke]), ReadShownColour(to, to.AsText()), region));
        }

        return [.. swaps];
    }

    /// <summary>
    /// The colours it changes on <paramref name="target"/>: those it lists for the places the target shows pictures
    /// from now. What was worked out for a target holds while those places stay the same, and is worked out again
    /// when they change, as when an enemy that keeps a set of its pictures for each of its shades takes up another.
    /// </summary>
    public IReadOnlyList<ColourSwap> On(IRecoloured target)
    {
        ArgumentNullException.ThrowIfNull(target);
        (string Place, ColourSwap[] Swaps)[] shown = [.. _places.Where(place => target.ShowsPicturesIn(place.Place))];
        string places = string.Join('|', shown.Select(place => place.Place));
        if (_on.TryGetValue(target, out (string Places, ColourSwap[] Swaps) known) && known.Places == places)
            return known.Swaps;
        ColourSwap[] swaps = [.. shown.SelectMany(place => place.Swaps)];
        _on[target] = (places, swaps);
        return swaps;
    }

    /// <summary>Shows it on <paramref name="target"/> at once, in place of whatever <paramref name="source"/> last asked for there.</summary>
    public void ShowOn(IRecoloured target, string source) => target.Recolour(source, On(target));

    /// <summary>A colour a colour is shown as: six hexadecimal digits, or eight, the last two how opaque it is shown.</summary>
    private static Color ReadShownColour(DataNode where, string text)
    {
        if (text.Length != 8)
            return ReadColour(where, text);
        if (!text.All(Uri.IsHexDigit))
            throw where.Problem($"names the colour \"{text}\", which is not hexadecimal digits");
        Color colour = ReadColour(where, text[..6]);
        return new Color(colour.R, colour.G, colour.B, Convert.ToInt32(text[6..], 16) / 255f);
    }

    /// <summary>A colour written as six hexadecimal digits, red, green and blue.</summary>
    private static Color ReadColour(DataNode where, string text)
    {
        if (text.Length != 6 || !text.All(Uri.IsHexDigit))
            throw where.Problem($"names the colour \"{text}\", which is not six hexadecimal digits");
        return new Color(
            Convert.ToInt32(text[..2], 16) / 255f, Convert.ToInt32(text[2..4], 16) / 255f, Convert.ToInt32(text[4..], 16) / 255f);
    }
}
