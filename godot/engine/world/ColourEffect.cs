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
/// Its steps are a data file, played by the engine's <see cref="ColourSteps"/>, which says how they are written: each
/// thing the effect is shown on is shown the colours of the places it shows pictures from, and a step's sound is asked for as the step begins.
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

    /// <summary>Its steps, read as the room is set up; they keep their changes under the effect's name.</summary>
    private ColourSteps? _colours;

    private ColourSteps Colours => _colours ?? throw new InvalidOperationException($"The colour effect \"{Name}\" has not been set up.");

    /// <summary>Shows the effect on <paramref name="target"/> as well, from its next step on.</summary>
    public void ShowOn(IRecoloured target) => Colours.ShowOn(target);

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        if (StepsFile.Length == 0)
            throw new InvalidOperationException($"The colour effect \"{Name}\" names no file of steps.");
        _colours = new ColourSteps(room.Content.ReadData(StepsFile), Name);
        if (OnTerrain)
            _colours.ShowOn(room.Terrain);
        if (OnBackdrops)
        {
            foreach (Backdrop backdrop in room.GetChildren().OfType<Backdrop>())
                _colours.ShowOn(backdrop);
        }

        foreach (string thing in OnThings)
        {
            Node node = room.GetNodeOrNull(thing)
                ?? throw new InvalidOperationException($"The colour effect \"{Name}\" is shown on \"{thing}\", which is not in the room \"{room.Name}\".");
            IRecoloured[] visuals = [.. node.FindChildren("*", owned: false).Prepend(node).OfType<IRecoloured>()];
            if (visuals.Length == 0)
                throw new InvalidOperationException($"The colour effect \"{Name}\" is shown on \"{thing}\", which has nothing that can be recoloured.");
            foreach (IRecoloured visual in visuals)
                _colours.ShowOn(visual);
        }
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        ColourSteps colours = Colours;

        // Its first update shows its first step.
        if (!colours.HasBegun)
        {
            Ask(room, colours.Step());
            return;
        }

        if (EndsWhen is { } ends && ends.IsTrue(room))
        {
            Stop();
            return;
        }

        if (RestartsWhile is { } restarts && restarts.IsTrue(room))
        {
            Ask(room, colours.Restart());
            return;
        }

        Ask(room, colours.Step());
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        if (what == "stop")
            Stop();
        else
            base.Triggered(what);
    }

    /// <summary>Asks for the sound of a step that has just begun, where it names one.</summary>
    private static void Ask(Room room, string sound)
    {
        if (sound.Length > 0)
            room.Session.Sounds.Request(sound, channel: "room");
    }

    /// <summary>Ends the effect: its colours are taken back, and it begins at its first step if started again.</summary>
    private void Stop()
    {
        Colours.Stop();
        SetActive(false);
    }
}
