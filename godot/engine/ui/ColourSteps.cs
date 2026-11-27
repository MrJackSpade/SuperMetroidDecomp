using Engine.Content;
using Godot;

namespace Engine.Ui;

/// <summary>
/// Steps of colours played over time on whatever they are shown on: each step shows some colours of the pictures as
/// other colours for a number of updates (<see cref="ColourChange"/>). Whoever owns one gives it its turn each
/// update; a room's <c>ColourEffect</c> plays one on the room's pictures, and a screen that is no room plays its own
/// on what it shows. The colours are changed by a shader where the pictures are drawn; no picture is kept twice.
/// </summary>
/// <remarks>
/// Its steps are data:
/// <code>
/// { "steps": [ { "updates": 8, "colours": { "tiles/norfair": { "f8a800": "f8d038", "c05800/1": "e08010" } }, "sound": "room_thunder" },
///              { "updates": 8, "colours": { } } ],
///   "loops_to": 0 }
/// </code>
/// Each step lasts its "updates". A step may name a "sound", which is handed back as the step begins for its owner
/// to ask for. After the last step it goes back to the step "loops_to" names; with none it stays on its last step.
/// Its first update shows its first step.
/// </remarks>
public sealed class ColourSteps
{
    private readonly List<(int Updates, ColourChange Colours, string Sound)> _steps = [];
    private readonly List<IRecoloured> _shownOn = [];
    private readonly int? _loopsTo;
    private readonly string _source;
    private int _at = -1;
    private int _left;

    /// <summary>Reads the steps from <paramref name="data"/>; its changes are kept under <paramref name="source"/> on what it is shown on.</summary>
    public ColourSteps(DataNode data, string source)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(source);
        _source = source;
        foreach (DataNode step in data.List("steps"))
        {
            int updates = step.Int("updates");
            if (updates < 1)
                throw step.Get("updates").Problem("must be at least 1");
            _steps.Add((updates, ColourChange.Read(step.Get("colours")), step.Has("sound") ? step.Text("sound") : ""));
        }

        if (_steps.Count == 0)
            throw data.Get("steps").Problem("lists no step");
        _loopsTo = data.Has("loops_to") ? data.Int("loops_to") : null;
        if (_loopsTo is { } loop && (loop < 0 || loop >= _steps.Count))
            throw data.Get("loops_to").Problem("names no step of the effect");
    }

    /// <summary>Whether its first step has been shown since it was made or last stopped.</summary>
    public bool HasBegun => _at >= 0;

    /// <summary>
    /// Shows the steps on <paramref name="target"/> as well: the step that shows now at once, where one does, and
    /// each step begun from then on. A target it is already shown on is left as it is. One that is gone, freed with
    /// whatever it was part of, is forgotten as the next step begins.
    /// </summary>
    public void ShowOn(IRecoloured target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_shownOn.Contains(target))
            return;
        _shownOn.Add(target);
        if (_at >= 0)
            _steps[_at].Colours.ShowOn(target, _source);
    }

    /// <summary>Forgets what it was shown on that is gone.</summary>
    private void ForgetGone() => _shownOn.RemoveAll(static target => target is GodotObject node && !GodotObject.IsInstanceValid(node));

    /// <summary>
    /// Plays one update: the first shows the first step, and each later one counts the step shown down and begins
    /// the next when it is over. Returns the sound of a step begun on this update; empty when none began or it names none.
    /// </summary>
    public string Step()
    {
        if (_at < 0)
            return Begin(0);
        if (--_left > 0)
            return "";
        int next = _at + 1;
        if (next == _steps.Count)
        {
            // With nowhere to go back to, it stays on its last step.
            if (_loopsTo is not { } loop)
            {
                _left = 1;
                return "";
            }

            next = loop;
        }

        return Begin(next);
    }

    /// <summary>Begins again at the first step, and returns its sound.</summary>
    public string Restart() => Begin(0);

    /// <summary>Takes its colours back from everything it is shown on; played again, it begins at its first step.</summary>
    public void Stop()
    {
        ForgetGone();
        foreach (IRecoloured target in _shownOn)
            target.Recolour(_source, []);
        _at = -1;
    }

    /// <summary>Begins a step: its colours are shown, and its sound returned.</summary>
    private string Begin(int step)
    {
        _at = step;
        _left = _steps[step].Updates;
        ForgetGone();
        foreach (IRecoloured target in _shownOn)
            _steps[step].Colours.ShowOn(target, _source);
        return _steps[step].Sound;
    }
}
