using Engine.Actors;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The scenes the opening shows between its pages. The two flashbacks are rooms played by Samus herself from
/// recorded input, with the thing she met there in them; the two in the laboratory are a picture that slides into
/// place with the larva pulsing in its jar.
/// </summary>
/// <remarks>
/// The data names each flashback's recording ("mother_brain_demo"; "discovery_run_demo" and "discovery_look_demo"),
/// which says which room it is played in and may name what Samus carries there ("equipment", "amounts", "selected");
/// and each laboratory scene's picture ("delivery_picture", "examination_picture") and larva ("delivered_baby",
/// "examined_baby", timelines that give the cue "next_page").
/// </remarks>
public partial class Intro
{
    /// <summary>How near the egg Samus runs before she stops to look.</summary>
    private const int DiscoveryStopX = 178;

    private Room? _room;
    private RecordedInputSource? _recording;
    private bool _looking;

    /// <summary>Whether Samus has stopped for the rest of the scene, and takes no part in its room's updates.</summary>
    private bool _samusStopped;
    private Sprite2D? _laboratory;
    private CinematicSprite? _larva;
    private bool _larvaWantsNextPage;

    /// <summary>Sets a scene up, unseen, for the crossfade that brings it in.</summary>
    private void BeginScene(string scene)
    {
        _scene = scene;
        _samusStopped = false;
        switch (scene)
        {
            case "mother_brain":
                BeginFlashback(Data().Text("mother_brain_demo"), keepsFraction: false);
                break;

            case "discovery":
                // She comes to the discovery with the parts of a pixel the fight left her.
                BeginFlashback(Data().Text("discovery_run_demo"), keepsFraction: true);
                _looking = false;
                break;

            case "delivery":
                // The picture starts a little to one side and slides across as it comes in; the larva slides with it.
                BeginLaboratory("delivery_picture", "delivered_baby", new Vector2(-32, -8));
                break;

            case "examination":
                BeginLaboratory("examination_picture", "examined_baby", new Vector2(0, 24));
                break;

            default:
                throw new InvalidOperationException($"The opening has no scene named \"{scene}\".");
        }
    }

    /// <summary>Plays one update of the scene that is up.</summary>
    private void StepScene(bool comingIn)
    {
        switch (_scene)
        {
            case "mother_brain":
                StepFlashback();
                break;

            case "discovery":
                // She runs in until she is near the egg, then stops and looks.
                if (!_looking && Session.Player.Position.X.Whole < DiscoveryStopX)
                {
                    _looking = true;
                    Record(DemoRecording.Read(Session.Content.ReadData(Data().Text("discovery_look_demo"))));
                }

                StepFlashback();
                break;

            case "delivery":
            case "examination":
                // Once the larva has gone, as the crossfade back nears its end, nothing of the scene moves.
                if (_larva is not { } larva)
                    break;
                Sprite2D picture = _laboratory ?? throw NotEntered();

                // As the scene comes in, every fourth update, the third of each four the crossfade takes, the picture
                // slides a pixel toward its place.
                bool across = _scene == "delivery";
                if (comingIn && _crossfade % CrossfadeStepUpdates == CrossfadeStepUpdates - 1 && (across ? picture.Position.X < 0 : picture.Position.Y > -8))
                {
                    picture.Position += across ? new Vector2(1, 0) : new Vector2(0, -1);
                    larva.Place += across ? PixelOffset.FromWhole(1, 0) : PixelOffset.FromWhole(0, -1);
                }

                foreach (string cue in larva.Step())
                {
                    if (cue != "next_page")
                        throw new InvalidOperationException($"The larva's timeline gives the cue \"{cue}\", which the opening does not know.");
                    _larvaWantsNextPage = true;
                }

                break;

            default:
                throw new InvalidOperationException($"The opening has no scene named \"{_scene}\".");
        }
    }

    /// <summary>Whether the scene that is up has run its course.</summary>
    private bool SceneWantsNextPage() => _scene switch
    {
        "mother_brain" => RoomNow().Enemies.OfType<IntroMotherBrain>().Any(brain => brain.WantsNextPage),
        "discovery" => RoomNow().Objects.OfType<IntroEgg>().Any(egg => egg.WantsNextPage),
        "delivery" or "examination" => _larvaWantsNextPage,
        _ => throw new InvalidOperationException($"The opening has no scene named \"{_scene}\"."),
    };

    /// <summary>
    /// Takes the scene's characters away as the crossfade back to the page reaches its update before its last: Samus
    /// and what she met in a flashback, or the larva in the laboratory. The scene's picture is left until the end.
    /// </summary>
    private void SceneCharactersLeave()
    {
        switch (_scene)
        {
            case "mother_brain":
                foreach (IntroMotherBrain brain in RoomNow().Enemies.OfType<IntroMotherBrain>())
                    brain.Leave();
                Session.Player.Visible = false;
                break;

            case "discovery":
                foreach (IntroEgg egg in RoomNow().Objects.OfType<IntroEgg>())
                    egg.Leave();
                Session.Player.Visible = false;

                // The discovery's end stops her moving and animating from this update, where the fight lets her play it.
                _samusStopped = true;
                break;

            case "delivery":
            case "examination":
                _larva?.QueueFree();
                _larva = null;
                break;

            default:
                throw new InvalidOperationException($"The opening has no scene named \"{_scene}\".");
        }
    }

    /// <summary>Puts the scene away, once the page that follows it has come.</summary>
    private void EndScene()
    {
        if (_room is { } room)
        {
            // Samus, hidden as the crossfade back neared its end, is shown again wherever she goes next.
            Session.Player.Visible = true;
            ((Samus)Session.Player).IsShownByScene = false;
            (_sceneRoot ?? throw NotEntered()).Grade([]);
            room.Teardown();
            room.QueueFree();
        }

        _laboratory?.QueueFree();
        _larva?.QueueFree();
        (_room, _recording, _laboratory, _larva, _scene) = (null, null, null, null, "");
    }

    /// <summary>
    /// Sets up the room a recording is played in, with Samus carrying what the recording says. Where she keeps the
    /// parts of a pixel she had, only the whole pixels of her place are those of the recording's way in, a marker.
    /// </summary>
    private void BeginFlashback(string file, bool keepsFraction)
    {
        DataNode data = Session.Content.ReadData(file);
        DemoRecording recording = DemoRecording.Read(data);
        Room room = Session.Definition.CreateRoom(recording.Room);
        (_sceneRoot ?? throw NotEntered()).Stage.AddChild(room);
        Player samus = Session.Player;
        var entry = new RoomEntry(recording.EntryPoint, PixelVelocity.Zero, Facing.Left);
        if (keepsFraction)
        {
            PixelPosition start = EntityNode.PlaceOf(room.GetNode<Marker2D>(recording.EntryPoint));
            entry = entry with { At = new PixelPosition(samus.Position.X.WithWhole(start.X.Whole), samus.Position.Y.WithWhole(start.Y.Whole)) };
        }

        room.Setup(Session, samus, entry);
        _room = room;
        if (data.Has("equipment"))
        {
            foreach (DataNode equipment in data.List("equipment"))
                samus.Inventory.Grant(equipment.AsText());
        }

        if (data.Has("amounts"))
        {
            foreach ((string amount, DataNode quantity) in data.Get("amounts").Entries())
            {
                samus.Inventory.SetCapacity(amount, Math.Max(samus.Inventory.Capacity(amount), quantity.AsInt()));
                samus.Inventory.Restore(amount, quantity.AsInt());
            }
        }

        ((Samus)samus).Select(data.Has("selected") ? data.Text("selected") : "");

        // A flashback is seen through the opening's own colours, all of it alike: the room, Samus, her shots and
        // what she meets there are each shown as they are anywhere, and the scene's view is graded as one.
        ((Samus)samus).IsShownByScene = true;
        (_sceneRoot ?? throw NotEntered()).Grade([.. Data().List("flashback_ramp").Select(colour => new Color(colour.AsText()))]);
        Record(recording);
    }

    private void Record(DemoRecording recording)
    {
        _recording = new RecordedInputSource(recording);
    }

    /// <summary>Plays one update of the flashback's room; once the recording is over Samus is left to stand.</summary>
    private void StepFlashback()
    {
        RecordedInputSource recording = _recording ?? throw NotEntered();
        GameInput input = recording.IsOver ? GameInput.None : recording.Next();
        RoomNow().Step(input, _samusStopped ? Stopped.Player : Stopped.Nothing);
        _roomOpenedUpdate = true;
    }

    private void BeginLaboratory(string picture, string larva, Vector2 startsAt)
    {
        Node2D root = (_sceneRoot ?? throw NotEntered()).Stage;
        _laboratory = new Sprite2D
        {
            Name = "Laboratory", Centered = false, Position = startsAt, Texture = Session.Content.LoadTexture(Data().Text(picture)),
        };
        root.AddChild(_laboratory);
        _larva = Sprite(larva, larva);
        root.AddChild(_larva);
        _larvaWantsNextPage = false;
    }

    private Room RoomNow() => _room ?? throw NotEntered();
}
