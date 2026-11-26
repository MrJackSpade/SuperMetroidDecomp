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
/// recording. It ends when the recording does or its time is up, and the next demo of its set follows, or the title
/// when it was the last; it goes back to the title at once when Samus dies or the player presses confirm or pause.
/// </summary>
/// <remarks>
/// A demo's file is a recording, with the room it plays in; it gives where in the room Samus starts ("start": "x",
/// "y", the way she is "facing", and what she starts "as": "standing", "facing_forward" as her ship leaves her,
/// "rolling" in the morph ball, "falling", or already flying a "shinespark_across" or a "shinespark_diagonal"; the
/// place is her middle in that body), where the view's top-left corner is as it begins ("view": "x", "y"), how long
/// it lasts ("lasts_updates"), and may list the "equipment" she has,
/// which of it is switched "off", how much of each thing she can carry ("capacities") and how much she has
/// ("amounts"), and the "facts" that are so before its room is entered.
/// </remarks>
public partial class AttractDemoMode : PlayingMode
{
    private readonly DataNode _data;
    private readonly RecordedInputSource _recording;
    private bool _equipped;
    private int _played;

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
        if (!_equipped)
        {
            _equipped = true;
            Equip(Session.Player.Inventory);
            Stage((Samus)Session.Player);
        }

        if (input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause))
            return BackToTitle();
        if (_recording.IsOver || _played >= _data.Int("lasts_updates"))
            return PlayedOut();
        _played++;
        GameMode next = base.Step(_recording.Next());

        // Nothing a demo leads to is followed but the change from one room to the next.
        if (ReferenceEquals(next, this) || next is DoorTransitionMode)
            return next;
        return BackToTitle();
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
        string? next = Session.Service<DemoSchedule>().AfterDemo();
        CartridgeSounds.Of(Session).DemoPlaying = false;
        Session.BeginAgain();
        if (next is not null)
            return Of(Session.Content, next);
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
