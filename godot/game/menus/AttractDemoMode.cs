using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;

namespace Game.Menus;

/// <summary>
/// One of the demos the title screen shows when it is left alone: play in a room, with the buttons pressed by a
/// recording. It ends when the recording does, when Samus dies, or as soon as the player presses confirm or pause,
/// and goes back to the title.
/// </summary>
/// <remarks>
/// A demo's file is a recording, with the room and entry point it plays in; it may also list the "equipment" Samus
/// has switched on in it and the "amounts" she carries, each amount's name with how much.
/// </remarks>
public partial class AttractDemoMode : PlayingMode
{
    private readonly DataNode _data;
    private readonly RecordedInputSource _recording;
    private readonly InputSampler _sampler;
    private bool _equipped;

    private AttractDemoMode(DataNode data, DemoRecording recording) : base(recording.Room, recording.EntryPoint)
    {
        _data = data;
        _recording = new RecordedInputSource(recording);
        _sampler = new InputSampler(_recording);
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
        }

        if (input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause) || _recording.IsOver)
            return BackToTitle();
        GameMode next = base.Step(_sampler.Sample());

        // Nothing a demo leads to is followed but the change from one room to the next.
        if (ReferenceEquals(next, this) || next is RoomTransitionMode)
            return next;
        return BackToTitle();
    }

    private GameMode BackToTitle()
    {
        Session.BeginAgain();
        return new TitleMode();
    }

    /// <summary>Gives Samus what the demo says she has.</summary>
    private void Equip(Inventory inventory)
    {
        if (_data.Has("equipment"))
        {
            foreach (DataNode equipment in _data.List("equipment"))
                inventory.Grant(equipment.AsText());
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
