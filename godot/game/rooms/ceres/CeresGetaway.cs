using Engine.Content;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// Ridley's getaway from Ceres and what follows it. Told to start as he leaves with the capsule, it shows him flying
/// off over the turning floor toward the screen, throws Samus aside as he passes, and then begins the station's
/// self-destruct: the warning comes up, the escape music starts, the words are typed out, and the countdown begins.
/// The warning stays up, flashing, for as long as the room is there.
/// </summary>
/// <remarks>
/// The floor it turns is hidden until the getaway and after it; its parts "Capsule" and "Wings" are the visuals that
/// change as he flies.
/// </remarks>
public partial class CeresGetaway : RoomSequence
{
    /// <summary>The floor he flies off over.</summary>
    [Export]
    public NodePath Floor { get; set; } = new();

    /// <summary>The capsule's pictures as it tumbles, in order.</summary>
    [Export]
    public Godot.Collections.Array<string> CapsulePictures { get; set; } = [];

    /// <summary>His wings' pictures as they beat, in order.</summary>
    [Export]
    public Godot.Collections.Array<string> WingPictures { get; set; } = [];

    /// <summary>
    /// The data file of what each of his wings' pictures leaves behind as it is drawn, in the order of
    /// <see cref="WingPictures"/>: under "stride_updates", the lengths of the pictures of Samus's stride at the pace
    /// the cartridge never wrote (<see cref="Samus.TakeStridePaceLeftByWings"/>); empty for none.
    /// </summary>
    [Export(PropertyHint.File)]
    public string WingLeftoversFile { get; set; } = "";

    /// <summary>The floor's tint at each whole size it is shown at, from nearest to farthest; empty for none.</summary>
    [Export]
    public Godot.Collections.Array<Color> SizeTints { get; set; } = [];

    /// <summary>The sound of him leaving, by name.</summary>
    [Export]
    public string GetawaySound { get; set; } = "";

    /// <summary>The picture of the warning's heading.</summary>
    [Export(PropertyHint.File)]
    public string HeadingPicture { get; set; } = "";

    /// <summary>Where the heading goes on the screen.</summary>
    [Export]
    public Vector2I HeadingAt { get; set; } = new(88, 48);

    /// <summary>The warning's words, by their name in the game's words.</summary>
    [Export]
    public string Message { get; set; } = "";

    /// <summary>
    /// The folder, within the assets tree, of the files that say how each warning is laid out: the "font" its words
    /// are typed in, where their first letter's cell lies ("x", "y"), how many updates each letter takes
    /// ("updates_per_letter"), how many letters at the start are followed by the next at once ("undelayed_letters"),
    /// how many letters appear for each sound ("letters_per_sound"), the sound of a letter appearing ("letter_sound"),
    /// and its "second_wordings", each under a name with the "languages" it is shown in and its "pictures", each with
    /// where it goes.
    /// </summary>
    [Export]
    public string LayoutFolder { get; set; } = "text/typed";

    /// <summary>The warning's second wording, shown beneath a moment into the typing; by its name in the warning's layout, empty for none.</summary>
    [Export]
    public string Subtitle { get; set; } = "";

    /// <summary>The colours the warning flashes through, in order; empty for none.</summary>
    [Export]
    public Godot.Collections.Array<Color> AlarmColours { get; set; } = [];

    /// <summary>The things told "start" as the countdown begins: the room's shaking, say.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenCountdownBegins { get; set; } = [];

    /// <summary>The music of the escape, by name.</summary>
    [Export]
    public string EscapeMusic { get; set; } = Music.Escape;

    /// <summary>How many updates the flight lasts.</summary>
    private const int FlightUpdates = 112;

    /// <summary>The update of the flight on which he throws Samus aside.</summary>
    private const int ShoveUpdate = 104;

    /// <summary>How far the floor turns each update, in 65536ths of a whole turn.</summary>
    private const int TurnPerUpdate = 48;

    /// <summary>
    /// The size the floor is shown at through the flight, in 256ths of a floor pixel to each screen pixel: where each
    /// stretch starts, how much it loses each update, and how long it lasts.
    /// </summary>
    private static readonly (int From, int LostEachUpdate, int Updates)[] SizeCurve =
    [
        (8 * 256, 0, 32), (127 * 16, 16, 16), (55 * 32, 32, 22), (67 * 16, 0, 1), (4 * 256, 32, 15),
        (35 * 16, 0, 1), (2 * 256, 32, 9), (15 * 16, 16, 14), (32, 0, 2),
    ];

    /// <summary>How far the floor drifts each update through the flight, and for how many updates.</summary>
    private static readonly (int Across, int Down, int Updates)[] DriftCurve =
    [
        (-1, -6, 12), (-1, -4, 10), (-1, -2, 7), (-1, -1, 16), (-1, 0, 35), (0, 1, 8), (1, 2, 8), (2, 3, 1), (2, 4, 1),
        (3, 6, 1), (3, 8, 1), (4, 10, 1), (4, 12, 1), (5, 14, 1), (5, 16, 1), (8, 20, 1), (12, 24, 1), (16, 44, 1),
        (20, 48, 1), (32, 128, 1), (32, 256, 3),
    ];

    private enum Stage
    {
        Flying,
        BeforeWarning,
        BeforeHeading,
        HoldingHeading,
        Typing,
        Finishing,
        Alarm,
    }

    private Stage _stage;
    private int _wait;
    private int _flown;
    private int _angle;
    private Vector2I _scroll;
    private int _capsule;
    private int _wings;
    private int _alarm = -1;
    private int _typedFor;
    private CanvasLayer? _warning;
    private TypedText? _words;
    private Node2D? _subtitle;
    private IReadOnlyList<int[]> _wingStrideUpdates = [];

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            Begin(room);
        switch (_stage)
        {
            case Stage.Flying:
                Fly(room);
                break;

            case Stage.BeforeWarning:
                if (--_wait > 0)
                    break;
                // The warning's layer comes up, with nothing on it yet.
                _warning = new CanvasLayer { Name = "Warning", Layer = 5 };
                AddChild(_warning);
                (_stage, _wait) = (Stage.BeforeHeading, 8);
                goto case Stage.BeforeHeading;

            case Stage.BeforeHeading:
                if (--_wait > 0)
                    break;
                ShowHeading(room);
                room.Session.Sounds.RequestMusic(EscapeMusic);
                (_stage, _wait) = (Stage.HoldingHeading, 128);
                break;

            case Stage.HoldingHeading:
                StepAlarm(elapsed);
                if (--_wait > 0)
                    break;
                BeginTyping(room);
                _stage = Stage.Typing;
                break;

            case Stage.Typing:
                StepAlarm(elapsed);
                TypedText words = _words ?? throw new InvalidOperationException("The warning is not being typed.");
                if (_subtitle is not null && ++_typedFor == 32)
                    _subtitle.Visible = true;
                // It is done on the update that would have typed another letter, had there been one.
                if (!words.IsOver)
                    words.Step(room.Session.Sounds);
                else
                    _stage = Stage.Finishing;
                break;

            case Stage.Finishing:
                StepAlarm(elapsed);

                // The station is now going to blow up, and the way out is open.
                room.Session.Progress.Record(StoryEvents.CeresSelfDestructStarted);
                room.Session.Progress.Record(Bosses.Defeated(Bosses.CeresRidley));
                room.Session.Service<Escape>().Begin(Escape.CeresTime);
                foreach (NodePath path in ToldWhenCountdownBegins)
                {
                    (GetNode(path) as ITriggerTarget
                        ?? throw new InvalidOperationException($"Ridley's getaway is to tell \"{path}\" that the countdown has begun, but that is nothing that can be told."))
                        .Triggered("start");
                }

                _stage = Stage.Alarm;
                break;

            case Stage.Alarm:
                StepAlarm(elapsed);
                break;

            default:
                throw new InvalidOperationException($"Ridley's getaway has no stage {_stage}.");
        }

        return false;
    }

    private void Begin(Room room)
    {
        room.Session.Progress.Record(StoryEvents.CeresRidleyEscaped);
        (_stage, _flown, _angle, _scroll) = (Stage.Flying, 0, 1, new Vector2I(-128, 32));
        (_capsule, _wings, _alarm, _typedFor) = (0, 0, -1, 0);
        _wingStrideUpdates = WingLeftoversFile.Length == 0
            ? []
            : [.. room.Content.ReadData(WingLeftoversFile).List("stride_updates").Select(wing => wing.Items().Select(updates => updates.AsInt()).ToArray())];
        if (_wingStrideUpdates.Count != 0 && _wingStrideUpdates.Count != WingPictures.Count)
            throw new InvalidOperationException($"Ridley's getaway \"{Name}\" has {WingPictures.Count} wing pictures but what {_wingStrideUpdates.Count} of them leave behind.");
        TurningFloor floor = FloorOf();
        floor.Visible = true;

        // The cartridge shows the floor in place of the room's own blocks while he flies, with the room's walls and
        // ground still round it: it lies in front of the room's tiles, behind the part of the room's picture that
        // lies in front of what stands in the room.
        floor.ZIndex = RoomDepth.BehindThings;
        if (CapsulePictures.Count > 0)
            floor.ShowPart("Capsule", CapsulePictures[0]);
        if (WingPictures.Count > 0)
            floor.ShowPart("Wings", WingPictures[0]);
        ShowFloor(floor, SizeCurve[0].From);
    }

    /// <summary>Plays one update of the flight, or ends it.</summary>
    private void Fly(Room room)
    {
        TurningFloor floor = FloorOf();
        if (_flown == FlightUpdates)
        {
            // He is gone: the floor is put away and the station's last moments begin.
            floor.Visible = false;
            (_stage, _wait) = (Stage.BeforeWarning, 7);
            return;
        }

        if (_flown == 0 && GetawaySound.Length > 0)
            room.Session.Sounds.Request(GetawaySound, channel: "room");
        if (_flown == ShoveUpdate)
            room.Player.ChangeState(new ShovedState());
        (int size, Vector2I drift) = FlightStep(_flown);
        _flown++;
        _scroll = new Vector2I(_scroll.X - drift.X, _scroll.Y + drift.Y);
        _angle += TurnPerUpdate;
        ShowFloor(floor, size);

        long updates = room.Session.Clock.Updates;
        if (updates % 4 == 0 && CapsulePictures.Count > 0)
        {
            _capsule = (_capsule + 1) % CapsulePictures.Count;
            floor.ShowPart("Capsule", CapsulePictures[_capsule]);
        }

        if (updates % 8 == 0 && WingPictures.Count > 0)
        {
            _wings = (_wings + 1) % WingPictures.Count;
            floor.ShowPart("Wings", WingPictures[_wings]);

            // The cartridge lays the wings' picture out where Samus's stride reads its unwritten pace.
            if (_wingStrideUpdates.Count != 0)
                ((Samus)room.Player).TakeStridePaceLeftByWings(_wingStrideUpdates[_wings]);
        }
    }

    /// <summary>The floor's size and drift on one update of the flight.</summary>
    private static (int Size, Vector2I Drift) FlightStep(int update)
    {
        int size = -1, into = update;
        foreach ((int from, int lost, int updates) in SizeCurve)
        {
            if (into < updates)
            {
                size = from - lost * into;
                break;
            }

            into -= updates;
        }

        into = update;
        foreach ((int across, int down, int updates) in DriftCurve)
        {
            if (into < updates)
                return (size, new Vector2I(across, down));
            into -= updates;
        }

        throw new ArgumentOutOfRangeException(nameof(update), update, "The flight has no such update.");
    }

    private void ShowFloor(TurningFloor floor, int size)
    {
        floor.Turn(_angle * Math.Tau / 65536, size / 256.0, new Vector2(64, 64), _scroll);
        int whole = size / 256;
        if (SizeTints.Count > 0)
            floor.Modulate = SizeTints[Math.Min(whole, SizeTints.Count - 1)];
    }

    private void ShowHeading(Room room)
    {
        if (HeadingPicture.Length == 0)
            return;
        var heading = new Sprite2D
        {
            Name = "Heading", Texture = room.Content.LoadTexture(HeadingPicture), Centered = false, Position = HeadingAt,
        };
        Warning().AddChild(heading);
    }

    private void BeginTyping(Room room)
    {
        DataNode layout = room.Session.Content.ReadData($"{LayoutFolder}/{Message}.json");
        _words = new TypedText
        {
            Name = "Words", Font = PixelFont.Read(room.Session.Content, layout.Text("font")), UpdatesPerCharacter = layout.Int("updates_per_letter"),
            UndelayedCharacters = layout.Int("undelayed_letters"), Sound = layout.Text("letter_sound"), LettersPerSound = layout.Int("letters_per_sound"),
            Position = new Vector2(layout.Int("x"), layout.Int("y")),
        };
        Warning().AddChild(_words);
        _words.Type(room.Session.Text.Get(Message));
        _typedFor = 0;
        if (Subtitle.Length == 0)
            return;

        // The second wording is pictures, shown only in the languages that have one.
        DataNode second = layout.Get("second_wordings").Get(Subtitle);
        if (!second.List("languages").Any(language => language.AsText() == room.Session.Options.Language))
            return;
        _subtitle = new Node2D { Name = "Subtitle", Visible = false };
        foreach (DataNode line in second.List("pictures"))
        {
            _subtitle.AddChild(new Sprite2D
            {
                Texture = room.Content.LoadTexture(line.Text("picture")), Centered = false, Position = new Vector2(line.Int("x"), line.Int("y")),
            });
        }

        Warning().AddChild(_subtitle);
    }

    /// <summary>Moves the warning's flashing on, every fourth update.</summary>
    private void StepAlarm(int elapsed)
    {
        if (elapsed % 4 != 0 || AlarmColours.Count == 0)
            return;
        _alarm = (_alarm + 1) % AlarmColours.Count;
        foreach (CanvasItem written in Warning().GetChildren().OfType<CanvasItem>())
            written.Modulate = AlarmColours[_alarm];
    }

    private CanvasLayer Warning() => _warning ?? throw new InvalidOperationException("The warning has not come up.");

    private TurningFloor FloorOf() => GetNode(Floor) as TurningFloor
        ?? throw new InvalidOperationException($"Ridley's getaway \"{Name}\" is to turn \"{Floor}\", but that is no turning floor.");
}
