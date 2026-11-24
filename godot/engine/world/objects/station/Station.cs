using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A station: it holds the player while it works, then grants or records its result. What it does is a setting: save
/// the game, reveal the area's map, or refill listed inventory amounts. It may ask first, show itself working, and
/// say when it is done.
/// </summary>
public partial class Station : RoomObject
{
    /// <summary>What a station does when it has finished working.</summary>
    public enum Does
    {
        /// <summary>It asks for the game to be saved, resuming at this station.</summary>
        Saves,

        /// <summary>It reveals the map of the room's area.</summary>
        RevealsMap,

        /// <summary>It fills the listed inventory amounts.</summary>
        Refills,
    }

    /// <summary>Which way the player must face to use a station.</summary>
    public enum Faces
    {
        /// <summary>Either way.</summary>
        EitherWay,

        /// <summary>Left.</summary>
        Left,

        /// <summary>Right.</summary>
        Right,
    }

    /// <summary>What the station does.</summary>
    [Export]
    public Does Kind { get; set; }

    /// <summary>The inventory amounts a refilling station fills, by name.</summary>
    [Export]
    public string[] Refills { get; set; } = [];

    /// <summary>The station's width and height, in pixels; its top-left is where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(32, 48);

    /// <summary>How many updates the station works for.</summary>
    [Export]
    public int WorkUpdates { get; set; } = 60;

    /// <summary>Which way the player must face to use it.</summary>
    [Export]
    public Faces MustFace { get; set; }

    /// <summary>
    /// For a station that saves: the entry point of its room a game saved here resumes at. Empty for the station
    /// itself.
    /// </summary>
    [Export]
    public string ResumesAt { get; set; } = "";

    /// <summary>The message that asks whether to use it, by name, answered yes or no; empty when it does not ask.</summary>
    [Export]
    public string AsksFirst { get; set; } = "";

    /// <summary>The message shown once it has finished, by name; empty for none.</summary>
    [Export]
    public string DoneMessage { get; set; } = "";

    /// <summary>The data file of the timeline its visuals show at rest; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string RestTimeline { get; set; } = "";

    /// <summary>The data file of the timeline its visuals show while it works; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string WorkingTimeline { get; set; } = "";

    /// <summary>The data file of the timeline of an effect it leaves as it starts to work; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string WorkingEffect { get; set; } = "";

    /// <summary>Where that effect is left, in pixels from where the station stands.</summary>
    [Export]
    public Vector2I WorkingEffectAt { get; set; }

    /// <summary>The sound of it starting to work, by name; empty for none.</summary>
    [Export]
    public string StartSound { get; set; } = "";

    /// <summary>The sound of it finishing, by name; empty for none.</summary>
    [Export]
    public string DoneSound { get; set; } = "";

    private Countdown _working = Countdown.Over;
    private bool _busy;
    private bool _asking;
    private bool? _answer;
    private bool _mustLeave;
    private TimelinePlayer? _timeline;

    /// <summary>The fact recorded when an area's map is revealed, for <paramref name="area"/>.</summary>
    public static string MapRevealedFact(string area) => $"map revealed: {area}";

    /// <summary>Whether the station is holding the player.</summary>
    public bool IsWorking => _busy || _asking;

    /// <summary>The region of the room the station covers.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_busy, _asking, _answer, _mustLeave) = (false, false, null, false);
        Show(RestTimeline);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        StepTimeline();
        if (_asking)
        {
            // The room is not stepped while the question is up; by now it has been answered.
            if (_answer is not { } yes)
                return;
            (_asking, _answer) = (false, null);
            if (!yes)
            {
                _mustLeave = true;
                player.Release();
                return;
            }

            BeginWork();
            return;
        }

        if (_busy)
        {
            _working = _working.Stepped();
            if (_working.IsRunning)
                return;
            _busy = false;
            _mustLeave = true;
            Finish(player);
            Show(RestTimeline);
            Sound(DoneSound);
            if (DoneMessage.Length > 0)
                Room.Session.RequestMessage(DoneMessage);
            player.Release();
            return;
        }

        if (!Area.Overlaps(player.BodyRect))
        {
            _mustLeave = false;
            return;
        }

        if (_mustLeave || player.State is not StandingState || !FacesRightWay(player) || !CanBeUsed(player))
            return;
        player.UseStation();
        if (AsksFirst.Length == 0)
        {
            BeginWork();
            return;
        }

        _asking = true;
        Room.Session.RequestQuestion(AsksFirst, answer => _answer = answer);
    }

    private void BeginWork()
    {
        _busy = true;
        _working = Countdown.Of(WorkUpdates);
        Show(WorkingTimeline.Length > 0 ? WorkingTimeline : RestTimeline);
        Sound(StartSound);
        if (WorkingEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(WorkingEffect)), Position + PixelOffset.FromWhole(WorkingEffectAt.X, WorkingEffectAt.Y)));
    }

    private bool FacesRightWay(Player player) => MustFace switch
    {
        Faces.EitherWay => true,
        Faces.Left => player.Facing == Facing.Left,
        Faces.Right => player.Facing == Facing.Right,
        _ => throw new InvalidOperationException($"Undefined facing {MustFace}."),
    };

    /// <summary>Whether the station has anything to do for the player; a refill station does nothing for one who is full.</summary>
    private bool CanBeUsed(Player player) =>
        Kind != Does.Refills || Refills.Any(amount => !player.Inventory.IsFull(amount));

    /// <summary>Grants or records the station's result.</summary>
    private void Finish(Player player)
    {
        switch (Kind)
        {
            case Does.Saves:
                Room.Session.RequestSave(Room.SceneFilePath, ResumesAt.Length > 0 ? ResumesAt : Name);
                break;
            case Does.RevealsMap:
                if (Room.Area.Length == 0)
                    throw new InvalidOperationException($"The map station \"{Name}\" is in a room that is in no area.");
                Room.Session.Progress.Record(MapRevealedFact(Room.Area));
                break;
            case Does.Refills:
                foreach (string amount in Refills)
                    player.Inventory.Fill(amount);
                break;
            default:
                throw new InvalidOperationException($"Undefined station kind {Kind}.");
        }
    }

    /// <summary>Starts its visuals on a timeline; empty leaves them as they are.</summary>
    private void Show(string timeline)
    {
        if (timeline.Length == 0)
            return;
        _timeline = new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(timeline)));
        ShowStep();
    }

    private void StepTimeline()
    {
        if (_timeline is null)
            return;
        _timeline.Step();
        ShowStep();
    }

    private void ShowStep()
    {
        if (_timeline is null)
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_timeline.Current);
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
