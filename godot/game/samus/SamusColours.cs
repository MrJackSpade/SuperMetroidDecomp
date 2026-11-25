using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;

namespace Game;

/// <summary>
/// Samus's colours: the colouring her suit shows now, the colouring her beams show with what is laid over it, and
/// every colouring and glow her data gives her. Her pictures
/// are one set, in their sheet's own colours; each colouring, her suits' own among them, is a change of those colours
/// shown where she is drawn. Which colouring her suit shows is chosen by her, once an update, from what she is doing.
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
/// "drain_flash" the colouring she flashes in each suit while something drains her. "starting_beams" is the
/// colouring her beams' colours are in before any is set, and "beams" that of each set of beams she can have
/// switched on, named for them: "power", "wave", "spazer_ice_wave". Her pictures draw some of their colours from
/// her beams', so a colouring of the beams shows on her too, over her suit's; and a colouring laid over the beams'
/// shows over that, each effect's last colouring in the order they were laid. "death" lists the colourings of her
/// death, step by step: her suit's in each suit ("suits"), those of her body without the suit ("suitless"), which
/// show over everything else once one is shown, and those her beams' colours whiten through ("whiteout").
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
    private readonly Dictionary<string, string> _beams = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string[]> _deathSuits = new(StringComparer.Ordinal);
    private readonly string[] _deathSuitless;
    private readonly string[] _deathWhiteout;

    /// <summary>The colouring her body without the suit shows, by name, or null for its own colours.</summary>
    private string? _suitless;

    /// <summary>The colourings laid over her beams', in the order they were laid; the last covers the others.</summary>
    private readonly List<string> _overBeams = [];

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
        foreach ((string beams, DataNode shade) in data.Get("beams").Entries())
            _beams[beams] = Known(shade.AsText());
        DataNode death = data.Get("death");
        foreach ((string suit, DataNode steps) in death.Get("suits").Entries())
            _deathSuits[suit] = [.. steps.Items().Select(shade => Known(shade.AsText()))];
        _deathSuitless = [.. death.List("suitless").Select(shade => Known(shade.AsText()))];
        _deathWhiteout = [.. death.List("whiteout").Select(shade => Known(shade.AsText()))];
        Hurt = Known(data.Text("hurt"));
        Suit = Known(data.Text("starting"));
        Beams = Known(data.Text("starting_beams"));
    }

    /// <summary>Reads her colours from their data file.</summary>
    public static SamusColours Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return new SamusColours(data);
    }

    /// <summary>The colouring her suit shows now, by name.</summary>
    public string Suit { get; private set; }

    /// <summary>The colouring her beams show now, by name.</summary>
    public string Beams { get; private set; }

    /// <summary>The colouring of a hurt's flash.</summary>
    public string Hurt { get; }

    /// <summary>The colouring of the beams named for those switched on: "power" for none, else "spazer", "plasma", "ice" and "wave" in that order with strokes between.</summary>
    public string BeamsOf(string beams) => _beams.TryGetValue(beams, out string? shade)
        ? shade
        : throw new ContentException(_file, $"gives no colouring for the beams \"{beams}\"");

    /// <summary>Shows <paramref name="shade"/> as her beams' colours, in place of the colouring they showed and of whatever was laid over it.</summary>
    public void ShowBeams(string shade)
    {
        Beams = Known(shade);
        _overBeams.Clear();
        _change++;
    }

    /// <summary>Lays <paramref name="shade"/> over her beams' colours, in place of an earlier colouring of the same effect.</summary>
    public void LayOverBeams(string shade)
    {
        Known(shade);
        string effect = EffectOf(shade);
        _overBeams.RemoveAll(laid => EffectOf(laid) == effect);
        _overBeams.Add(shade);
        _change++;
    }

    /// <summary>The effect a colouring is a step of: its name without the step's number at its end.</summary>
    private static string EffectOf(string shade) => shade[..shade.LastIndexOf('_')];

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

    /// <summary>The colouring her suit shows at a step of her death, in the suit named.</summary>
    public string DeathSuitOf(string suit, int step) => _deathSuits.TryGetValue(suit, out string[]? steps)
        ? StepOf(steps, step, $"her death's colours in the {suit} suit")
        : throw new ContentException(_file, $"gives no colourings of her death for the suit \"{suit}\"");

    /// <summary>The colouring her body without the suit shows at a step of her death.</summary>
    public string DeathSuitlessAt(int step) => StepOf(_deathSuitless, step, "her death's colours without the suit");

    /// <summary>The colouring her beams' colours show at a step of her death's whiteout.</summary>
    public string DeathWhiteoutAt(int step) => StepOf(_deathWhiteout, step, "her death's whiteout");

    private string StepOf(string[] steps, int step, string what) => step >= 0 && step < steps.Length
        ? steps[step]
        : throw new ContentException(_file, $"has no step {step} of {what}");

    /// <summary>Shows <paramref name="shade"/> as the colours of her body without the suit, over everything else she shows.</summary>
    public void ShowSuitless(string shade)
    {
        _suitless = Known(shade);
        _change++;
    }

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
        (string Place, ColourSwap[] Swaps)[] shown = ShownOn(visual);
        string places = string.Join('|', shown.Select(place => place.Place).Distinct());
        if (_shownOn.TryGetValue(visual, out (int Change, string Places) last) && last == (_change, places))
            return;
        _shownOn[visual] = (_change, places);
        visual.Recolour(Source, [.. shown.SelectMany(place => place.Swaps)]);
    }

    /// <summary>
    /// Shows her colours, as they stand, on something of hers that is made and gone again, as a shot is as it is
    /// fired: the colours her colouring changes in the pictures of the places it shows a picture from. It is not
    /// kept track of, and is not shown her colours again when they change.
    /// </summary>
    public void ShowOnNew(Engine.Ui.IRecoloured thing)
    {
        ArgumentNullException.ThrowIfNull(thing);
        (string Place, ColourSwap[] Swaps)[] shown = ShownOn(thing);
        if (shown.Length > 0)
            thing.Recolour(Source, [.. shown.SelectMany(place => place.Swaps)]);
    }

    /// <summary>
    /// The colours her colouring changes on <paramref name="thing"/>, place by place. What was laid last comes
    /// first, so it is the one shown where two change one colour: the colours of her body without the suit first,
    /// then over her beams' colours what was laid over them, and her beams' over her suit's.
    /// </summary>
    private (string Place, ColourSwap[] Swaps)[] ShownOn(Engine.Ui.IRecoloured thing)
    {
        IEnumerable<string> suitless = _suitless is { } bare ? [bare] : [];
        return
        [
            .. suitless.Concat(((IEnumerable<string>)_overBeams).Reverse()).Append(Beams).Append(Suit)
                .SelectMany(shade => _shades[shade]).Where(place => thing.ShowsPicturesIn(place.Place)),
        ];
    }

    /// <summary>The name of a colouring her data has, or an error naming one it has not.</summary>
    private string Known(string shade) => _shades.ContainsKey(shade)
        ? shade
        : throw new ContentException(_file, $"names the colouring \"{shade}\", which it does not list");
}
