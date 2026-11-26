using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;

namespace Game.Menus;

/// <summary>
/// One of the demos the title screen shows when it is left alone: play in a room, with the buttons pressed by a
/// recording. It takes the cartridge's updates: the screen is black for eight while its room is made ready; play is
/// shown for one update and then for as many as the demo lasts, whatever the recording's length; its last picture is
/// held for ninety, nothing moving; and the screen is black for two more, on the second of which the next demo of
/// its set follows, or the title when it was the last. Anything pressed after its first update of play, or while its
/// last picture is held, ends it there: the two black updates, and the title at rest. It goes back to the title at
/// once when play leads anywhere but through a door, as when Samus dies.
/// </summary>
/// <remarks>
/// A demo's file is a recording, with the room it plays in; it gives where in the room Samus starts ("start": "x",
/// "y", the way she is "facing", and what she starts "as": "standing", "facing_forward" as her ship leaves her,
/// "rolling" in the morph ball, "falling", or already flying a "shinespark_across" or a "shinespark_diagonal"; the
/// place is her middle in that body), where the view's top-left corner is as it begins ("view": "x", "y"), how long
/// it lasts ("lasts_updates"), and may list the "equipment" she has,
/// which of it is switched "off", how much of each thing she can carry ("capacities") and how much she has
/// ("amounts"), and the "facts" that are so before its room is entered. What it does to its room once the room is
/// built it says too: how screens of the room let the view in ("screens", each "x" and "y" in screens with its
/// "scroll"), as the door the cartridge's demo comes in by sets them and as the demo itself does; and how long Kraid
/// waits before rising in place of his own wait ("kraid_waits_updates").
/// </remarks>
public partial class AttractDemoMode : PlayingMode
{
    private readonly DataNode _data;
    private readonly RecordedInputSource _recording;

    /// <summary>The updates the screen is black before play: the one its room is made ready on, and seven after.</summary>
    private const int LoadingUpdates = 8;

    /// <summary>The updates its last picture is held.</summary>
    private const int HoldUpdates = 90;

    /// <summary>The updates the screen is black after it: one on which what follows is chosen, and the one it follows on.</summary>
    private const int LeavingUpdates = 2;

    private enum Part
    {
        Loading,
        Playing,
        Holding,
        Leaving,
    }

    private Part _part;

    /// <summary>The updates left of the part it is in, but of its play.</summary>
    private int _left = LoadingUpdates;

    /// <summary>The updates of play it has shown.</summary>
    private int _played;

    /// <summary>Whether a press ended it, and the demo that follows it when none did and one does.</summary>
    private bool _cancelled;
    private string? _next;

    private AttractDemoMode(DataNode data, DemoRecording recording) : base(recording.Room, StartOf(data))
    {
        _data = data;
        _recording = new RecordedInputSource(recording);
    }

    /// <inheritdoc/>
    protected override void Enter()
    {
        // What the demo says has already happened is so before its room is entered.
        if (_data.Has("facts"))
        {
            foreach (DataNode fact in _data.List("facts"))
                Session.Progress.Record(fact.AsText());
        }

        base.Enter();

        // While a demonstration plays every sound asked for is turned away.
        CartridgeSounds.Of(Session).DemoPlaying = true;

        // The screen is black until its first update of play.
        Session.Darkness.Show(Session.Darkness.Levels);
    }

    /// <summary>The demo in the file at <paramref name="file"/>, a path within the assets tree.</summary>
    public static AttractDemoMode Of(ContentLibrary content, string file)
    {
        ArgumentNullException.ThrowIfNull(content);
        DataNode data = content.ReadData(file);
        return new AttractDemoMode(data, DemoRecording.Read(data));
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        switch (_part)
        {
            case Part.Loading:
                // On the first of its black updates Samus is given what the demo gives her and its room is set up.
                if (_left == LoadingUpdates)
                {
                    Equip(Session.Player.Inventory);
                    Stage((Samus)Session.Player);
                    SetUpRoom();
                }

                if (--_left == 0)
                    _part = Part.Playing;
                return this;

            case Part.Playing:
                if (_played == 0)
                    Session.Darkness.Show(0);
                _played++;
                GameMode next = base.Step(_recording.Next());

                // Nothing a demo leads to is followed but the change from one room to the next.
                if (!ReferenceEquals(next, this) && next is not DoorTransitionMode)
                    return BackToTitle();

                // Its first update of play listens for nothing; on the others anything pressed ends it.
                if (_played > 1 && input.AnyPressed)
                {
                    Leave(cancelled: true);
                    return this;
                }

                // It plays one update and then for as long as it lasts; a recording that ends sooner presses nothing more.
                if (_played > _data.Int("lasts_updates"))
                    (_part, _left) = (Part.Holding, HoldUpdates);
                return next;

            case Part.Holding:
                // Its last picture is held, nothing of the room taking a turn.
                if (input.AnyPressed)
                    Leave(cancelled: true);
                else if (--_left == 0)
                    Leave(cancelled: false);
                return this;

            default:
                // On the first black update what follows is chosen; on the second it follows.
                if (--_left > 0)
                {
                    if (!_cancelled)
                        _next = Session.Service<DemoSchedule>().AfterDemo();
                    return this;
                }

                return _cancelled ? BackToTitle() : PlayedOut();
        }
    }

    /// <summary>Ends what the demo shows: the screen is black from this update, for the two the demo takes to leave.</summary>
    private void Leave(bool cancelled)
    {
        (_part, _left, _cancelled) = (Part.Leaving, LeavingUpdates, cancelled);
        Session.Darkness.Show(Session.Darkness.Levels);
    }

    /// <summary>Where Samus starts, as the demo's file says.</summary>
    private static RoomEntry StartOf(DataNode data)
    {
        DataNode start = data.Get("start");
        Facing facing = start.Text("facing") switch
        {
            "left" => Facing.Left,
            "right" => Facing.Right,
            _ => throw start.Get("facing").Problem("must be \"left\" or \"right\""),
        };
        DataNode view = data.Get("view");
        return new RoomEntry("", PixelVelocity.Zero, facing)
        {
            At = PixelPosition.FromWhole(start.Int("x"), start.Int("y")),
            ViewAt = (view.Int("x"), view.Int("y")),
        };
    }

    /// <summary>
    /// Puts Samus as the demo says she starts, once she has what the demo gives her: the room set her down standing
    /// at the demo's place, and she takes the body of what she starts as about that same middle.
    /// </summary>
    private void Stage(Samus samus)
    {
        DataNode start = _data.Get("start");
        int forward = samus.Facing == Facing.Right ? 1 : -1;
        switch (start.Text("as"))
        {
            case "standing":
                break;
            case "facing_forward":
                samus.ChangeState(new StandingForwardState());
                break;
            case "rolling":
                Begin(samus, new CrawlingState());
                break;
            case "falling":
                Begin(samus, new FallingState());
                break;
            case "shinespark_across":
                samus.ChangeState(ShinesparkState.Launched(forward, 0));
                break;
            case "shinespark_diagonal":
                samus.ChangeState(ShinesparkState.Launched(forward, -1));
                break;
            default:
                throw start.Get("as").Problem("must be \"standing\", \"facing_forward\", \"rolling\", \"falling\", \"shinespark_across\" or \"shinespark_diagonal\"");
        }
    }

    /// <summary>Does to the demo's room, now built, what the demo's file says is done to it before play begins.</summary>
    private void SetUpRoom()
    {
        if (_data.Has("screens"))
        {
            foreach (DataNode screen in _data.List("screens"))
                Session.ScrollCells.Write(screen.Int("x"), screen.Int("y"), screen.Get("scroll").AsEnum<ScreenScroll>());
        }

        if (!_data.Has("kraid_waits_updates"))
            return;
        Enemies.Kraid kraid = Room.Enemies.OfType<Enemies.Kraid>().FirstOrDefault()
            ?? throw _data.Get("kraid_waits_updates").Problem("is given for a room that has no Kraid");
        kraid.WaitBeforeRising(_data.Int("kraid_waits_updates"));
    }

    /// <summary>Puts Samus in <paramref name="state"/>, in the body that state is entered in, about the middle she has.</summary>
    private static void Begin(Samus samus, PlayerState state)
    {
        if (state.EntersIn(samus) is { } wants && !samus.TrySetForm(wants.Form, aroundMiddle: true))
            throw new InvalidOperationException($"A demo starts Samus in the body \"{wants.Form}\" where it does not fit.");
        samus.ChangeState(state);
    }

    /// <summary>
    /// The demo has played to its end: the next of its set follows, or, after the last, the music stops and the title
    /// begins again from its opening.
    /// </summary>
    private GameMode PlayedOut()
    {
        CartridgeSounds.Of(Session).DemoPlaying = false;
        Session.BeginAgain();
        if (_next is not null)
            return Of(Session.Content, _next);
        Session.Sounds.RequestMusic("");
        return new TitleMode(TitleMode.Beginning.AfterDemos);
    }

    private GameMode BackToTitle()
    {
        CartridgeSounds.Of(Session).DemoPlaying = false;
        Session.BeginAgain();
        return new TitleMode(TitleMode.Beginning.AtRest);
    }

    /// <summary>Gives Samus what the demo says she has.</summary>
    private void Equip(Inventory inventory)
    {
        if (_data.Has("equipment"))
        {
            foreach (DataNode equipment in _data.List("equipment"))
                inventory.Grant(equipment.AsText());
        }

        if (_data.Has("off"))
        {
            foreach (DataNode equipment in _data.List("off"))
                inventory.Switch(equipment.AsText(), false);
        }

        if (_data.Has("capacities"))
        {
            foreach ((string amount, DataNode quantity) in _data.Get("capacities").Entries())
                inventory.SetCapacity(amount, quantity.AsInt());
        }

        if (!_data.Has("amounts"))
            return;
        foreach ((string amount, DataNode quantity) in _data.Get("amounts").Entries())
        {
            inventory.SetCapacity(amount, Math.Max(inventory.Capacity(amount), quantity.AsInt()));
            inventory.Restore(amount, quantity.AsInt());
        }
    }
}
