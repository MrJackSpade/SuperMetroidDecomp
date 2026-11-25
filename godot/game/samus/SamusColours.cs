using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;

namespace Game;

/// <summary>
/// Samus's colours: the colouring her suit shows now, and every colouring and glow her data gives her. Her pictures
/// are one set, in their sheet's own colours; each colouring, her suits' own among them, is a change of those colours
/// shown where she is drawn. Which colouring she shows is hers to keep, as the cartridge keeps her palette: it is set
/// by what colours her (a suit put on, a hurt, a glow) and stays until something sets another, whatever she wears.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "starting": "suit_starting_0",
///   "suits": { "power": "suit_power_0", "varia": "suit_varia_0", "gravity": "suit_gravity_0" },
///   "hurt": "hurt_hurt_0",
///   "drain_flash": { "power": "speed_booster_power_3" },
///   "glows": { "stored_shine_power": { "steps": [ { "shade": "stored_shine_power_0", "updates": 1 } ], "loops_from": 0 } },
///   "shades": { "suit_power_0": { "samus/body": { "5a5a00": "734200" } } }
/// }
/// </code>
/// "shades" names every colouring and lists, for the pictures of each place, the colours it changes, as any change of
/// colours is written (<see cref="ColourChange"/>). "glows" names every glow (<see cref="SamusGlow"/>). "starting" is
/// the colouring she is in before any is set, "suits" the colouring of each suit, "hurt" that of a hurt's flash, and
/// "drain_flash" the colouring she flashes in each suit while something drains her.
/// </remarks>
public sealed class SamusColours
{
    /// <summary>The name her colours are asked under on what shows her.</summary>
    private const string Source = "samus";

    private readonly string _file;
    private readonly Dictionary<string, (string Place, ColourSwap[] Swaps)[]> _shades = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SamusGlow> _glows = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _suits = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _drainFlash = new(StringComparer.Ordinal);

    /// <summary>What each visual was last shown: which change of hers, and the places it showed pictures from.</summary>
    private readonly Dictionary<ActorVisual, (int Change, string Places)> _shownOn = [];

    /// <summary>How many times the colouring she shows has changed.</summary>
    private int _change;

    private SamusColours(DataNode data)
    {
        _file = data.File;
        foreach ((string name, DataNode places) in data.Get("shades").Entries())
            _shades[name] = [.. places.Entries().Select(place => (place.Key, ColourChange.ReadSwaps(place.Value)))];
        foreach ((string name, DataNode glow) in data.Get("glows").Entries())
        {
            SamusGlow read = SamusGlow.Read(glow);
            foreach (SamusGlowStep step in read.Steps)
                Known(step.Shade);
            _glows[name] = read;
        }

        foreach ((string suit, DataNode shade) in data.Get("suits").Entries())
            _suits[suit] = Known(shade.AsText());
        foreach ((string suit, DataNode shade) in data.Get("drain_flash").Entries())
            _drainFlash[suit] = Known(shade.AsText());
        Hurt = Known(data.Text("hurt"));
        Suit = Known(data.Text("starting"));
    }

    /// <summary>Reads her colours from their data file.</summary>
    public static SamusColours Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return new SamusColours(data);
    }

    /// <summary>The colouring her suit shows now, by name.</summary>
    public string Suit { get; private set; }

    /// <summary>The colouring of a hurt's flash.</summary>
    public string Hurt { get; }

    /// <summary>The colouring of the suit named: "power", "varia" or "gravity".</summary>
    public string SuitOf(string suit) => _suits.TryGetValue(suit, out string? shade)
        ? shade
        : throw new ContentException(_file, $"gives no colouring for the suit \"{suit}\"");

    /// <summary>The colouring she flashes while something drains her, in the suit named.</summary>
    public string DrainFlashOf(string suit) => _drainFlash.TryGetValue(suit, out string? shade)
        ? shade
        : throw new ContentException(_file, $"gives no colouring of a drain's flash for the suit \"{suit}\"");

    /// <summary>The glow of the name given.</summary>
    public SamusGlow Glow(string name) => _glows.TryGetValue(name, out SamusGlow? glow)
        ? glow
        : throw new ContentException(_file, $"has no glow named \"{name}\"");

    /// <summary>Shows <paramref name="shade"/> as her suit's colours, in place of the colouring it showed.</summary>
    public void ShowSuit(string shade)
    {
        Known(shade);
        if (shade == Suit)
            return;
        Suit = shade;
        _change++;
    }

    /// <summary>
    /// Shows her colours on <paramref name="visual"/>: the colours her colouring changes in the pictures of the places
    /// the visual is showing a picture from. It asks anew only when her colouring or those places have changed.
    /// </summary>
    public void ShowOn(ActorVisual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        (string Place, ColourSwap[] Swaps)[] shown = [.. _shades[Suit].Where(place => visual.ShowsPicturesIn(place.Place))];
        string places = string.Join('|', shown.Select(place => place.Place));
        if (_shownOn.TryGetValue(visual, out (int Change, string Places) last) && last == (_change, places))
            return;
        _shownOn[visual] = (_change, places);
        visual.Recolour(Source, [.. shown.SelectMany(place => place.Swaps)]);
    }

    /// <summary>The name of a colouring her data has, or an error naming one it has not.</summary>
    private string Known(string shade) => _shades.ContainsKey(shade)
        ? shade
        : throw new ContentException(_file, $"names the colouring \"{shade}\", which it does not list");
}
