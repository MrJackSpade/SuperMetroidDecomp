using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Engine.World;

/// <summary>
/// A room-wide change of colours over time: rock that glows, lights that flicker, lightning. It plays the steps of
/// its data file, each showing some colours of the pictures it is shown on as other colours for a number of updates,
/// on the room's terrain, its backdrops and whatever placed things it names. The colours are changed by a shader
/// where the pictures are drawn; no picture is kept twice. Like every room effect it is stepped before the player
/// and the enemies act, shows its first step on its first update, and affects play in no way of its own.
/// </summary>
/// <remarks>
/// Its steps are a data file:
/// <code>
/// { "steps": [ { "updates": 8, "colours": { "tiles/norfair": { "f8a800": "f8d038", "c05800/1": "e08010" } }, "sound": "room_thunder" },
///              { "updates": 8, "colours": { } } ],
///   "loops_to": 0 }
/// </code>
/// Each step lasts its "updates". Its "colours" are listed place by place, a place being a picture's path within
/// the assets tree or a folder of pictures: for the pictures of that place the step shows every colour it lists,
/// written as the pictures have it, as the colour given for it, and a colour it does not list shows as the pictures
/// have it. A colour written with a number after a stroke is changed only in that region of the pictures, as their
/// maps of regions number it. Each thing the effect is shown on is shown the colours of the places it shows
/// pictures from. A step may name a "sound", asked for as the step
/// begins. After the last step the effect goes back to the step "loops_to" names; with none it stays on its last step.
/// It can be told to "start" and to "stop", which takes its colours back; it ends by itself, taking its colours
/// back, once its <see cref="EndsWhen"/> holds; and on every update its <see cref="RestartsWhile"/> holds it begins
/// again at its first step.
/// </remarks>
public partial class ColourEffect : RoomEffect
{
    /// <summary>The data file of its steps.</summary>
    [Export(PropertyHint.File)]
    public string StepsFile { get; set; } = "";

    /// <summary>Whether it is shown on the room's terrain.</summary>
    [Export]
    public bool OnTerrain { get; set; } = true;

    /// <summary>Whether it is shown on the room's backdrops.</summary>
    [Export]
    public bool OnBackdrops { get; set; } = true;

    /// <summary>The things placed in the room, by the names of their nodes, whose visuals it is shown on as well.</summary>
    [Export]
    public string[] OnThings { get; set; } = [];

    /// <summary>
    /// When it ends by itself: asked as each of its updates begins, from the update after it shows its first step.
    /// Never, when null.
    /// </summary>
    [Export]
    public Condition? EndsWhen { get; set; }

    /// <summary>
    /// While this holds the effect shows its first step anew on every update: asked as each of its updates begins,
    /// from the update after it shows its first step, once it is known not to have ended. Never, when null.
    /// </summary>
    [Export]
    public Condition? RestartsWhile { get; set; }

    private readonly List<(int Updates, (string Place, ColourSwap[] Swaps)[] Places, string Sound)> _steps = [];
    private readonly List<IRecoloured> _shownOn = [];

    /// <summary>For each thing it is shown on, the colours each step changes on it: those of the places it shows pictures from.</summary>
    private readonly Dictionary<IRecoloured, ColourSwap[][]> _swapsOn = [];
    private int? _loopsTo;
    private int _at = -1;
    private int _left;

    /// <summary>The name its changes are kept under on what it is shown on.</summary>
    private string Source => Name;

    /// <summary>Shows the effect on <paramref name="target"/> as well, from its next step on.</summary>
    public void ShowOn(IRecoloured target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_shownOn.Contains(target))
            _shownOn.Add(target);
    }

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        if (StepsFile.Length == 0)
            throw new InvalidOperationException($"The colour effect \"{Name}\" names no file of steps.");
        DataNode data = room.Content.ReadData(StepsFile);
        _steps.Clear();
        foreach (DataNode step in data.List("steps"))
        {
            int updates = step.Int("updates");
            if (updates < 1)
                throw step.Get("updates").Problem("must be at least 1");
            var places = new List<(string, ColourSwap[])>();
            foreach ((string place, DataNode colours) in step.Get("colours").Entries())
            {
                var swaps = new List<ColourSwap>();
                foreach ((string from, DataNode to) in colours.Entries())
                {
                    // A colour changed only in one region of the pictures has the region's number after a stroke.
                    int stroke = from.IndexOf('/');
                    int region = ColourSwap.Everywhere;
                    if (stroke >= 0 && (!int.TryParse(from[(stroke + 1)..], out region) || region < 0))
                        throw colours.Problem($"names the region \"{from[(stroke + 1)..]}\", which is not a region's number");
                    swaps.Add(new ColourSwap(ReadColour(colours, stroke < 0 ? from : from[..stroke]), ReadColour(to, to.AsText()), region));
                }

                places.Add((place, [.. swaps]));
            }

            _steps.Add((updates, [.. places], step.Has("sound") ? step.Text("sound") : ""));
        }

        if (_steps.Count == 0)
            throw data.Get("steps").Problem("lists no step");
        _loopsTo = data.Has("loops_to") ? data.Int("loops_to") : null;
        if (_loopsTo is { } loop && (loop < 0 || loop >= _steps.Count))
            throw data.Get("loops_to").Problem("names no step of the effect");

        _shownOn.Clear();
        _swapsOn.Clear();
        if (OnTerrain)
            _shownOn.Add(room.Terrain);
        if (OnBackdrops)
            _shownOn.AddRange(room.GetChildren().OfType<Backdrop>());
        foreach (string thing in OnThings)
        {
            Node node = room.GetNodeOrNull(thing)
                ?? throw new InvalidOperationException($"The colour effect \"{Name}\" is shown on \"{thing}\", which is not in the room \"{room.Name}\".");
            IRecoloured[] visuals = [.. node.FindChildren("*", owned: false).Prepend(node).OfType<IRecoloured>()];
            if (visuals.Length == 0)
                throw new InvalidOperationException($"The colour effect \"{Name}\" is shown on \"{thing}\", which has nothing that can be recoloured.");
            _shownOn.AddRange(visuals);
        }

        _at = -1;
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        // Its first update shows its first step.
        if (_at < 0)
        {
            Begin(room, 0);
            return;
        }

        if (EndsWhen is { } ends && ends.IsTrue(room))
        {
            Stop();
            return;
        }

        if (RestartsWhile is { } restarts && restarts.IsTrue(room))
        {
            Begin(room, 0);
            return;
        }

        if (--_left > 0)
            return;
        int next = _at + 1;
        if (next == _steps.Count)
        {
            // With nowhere to go back to, it stays on its last step.
            if (_loopsTo is not { } loop)
            {
                _left = 1;
                return;
            }

            next = loop;
        }

        Begin(room, next);
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        if (what == "stop")
            Stop();
        else
            base.Triggered(what);
    }

    /// <summary>Begins a step: its colours are shown and its sound asked for.</summary>
    private void Begin(Room room, int step)
    {
        _at = step;
        _left = _steps[step].Updates;
        foreach (IRecoloured target in _shownOn)
            target.Recolour(Source, SwapsOn(target)[step]);
        if (_steps[step].Sound.Length > 0)
            room.Session.Sounds.Request(_steps[step].Sound, channel: "room");
    }

    /// <summary>Ends the effect: its colours are taken back, and it begins at its first step if started again.</summary>
    private void Stop()
    {
        foreach (IRecoloured target in _shownOn)
            target.Recolour(Source, []);
        _at = -1;
        SetActive(false);
    }

    /// <summary>
    /// The colours each step changes on <paramref name="target"/>: those the step lists for the places the target
    /// shows pictures from, worked out the first time the target is shown a step.
    /// </summary>
    private ColourSwap[][] SwapsOn(IRecoloured target)
    {
        if (_swapsOn.TryGetValue(target, out ColourSwap[][]? known))
            return known;
        var shows = new Dictionary<string, bool>(StringComparer.Ordinal);
        var swaps = new ColourSwap[_steps.Count][];
        for (int step = 0; step < swaps.Length; step++)
        {
            var changed = new List<ColourSwap>();
            foreach ((string place, ColourSwap[] listed) in _steps[step].Places)
            {
                if (!shows.TryGetValue(place, out bool shown))
                    shows[place] = shown = target.ShowsPicturesIn(place);
                if (shown)
                    changed.AddRange(listed);
            }

            swaps[step] = [.. changed];
        }

        return _swapsOn[target] = swaps;
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
