using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.Content;
using Godot;

namespace Engine.Tests.Core;

/// <summary>Checks of what is kept outside a game being played: save slots, options, the game's words and recorded inputs.</summary>
public sealed class SavingChecks
{
    [Check]
    public void ASlotGivesBackWhatWasWrittenToIt()
    {
        using var assets = new TempAssets();
        var saves = new SaveFile(Path.Combine(assets.Folder, "saves"), 3);
        Expect.False(saves.Has(2));
        Expect.Throws<InvalidOperationException>(() => saves.Read(2));

        saves.Write(2, new SaveData
        {
            Facts = ["one", "two"], Explored = ["caves|3|4"], PlayUpdates = 1234, Owned = ["boots"], SwitchedOn = ["boots"],
            Amounts = { ["energy"] = 55 }, Capacities = { ["energy"] = 99 }, Room = "room.tscn", EntryPoint = "Save",
        });
        SaveData read = saves.Read(2);

        Expect.True(read.Facts.SequenceEqual(["one", "two"]));
        Expect.True(read.Explored.SequenceEqual(["caves|3|4"]));
        Expect.Equal(1234L, read.PlayUpdates);
        Expect.Equal(55, read.Amounts["energy"]);
        Expect.Equal(99, read.Capacities["energy"]);
        Expect.Equal("room.tscn", read.Room);
        Expect.Equal("Save", read.EntryPoint);
    }

    [Check]
    public void SlotsAreCopiedAndClearedAndNumberedFromOne()
    {
        using var assets = new TempAssets();
        var saves = new SaveFile(Path.Combine(assets.Folder, "saves"), 3);
        saves.Write(1, new SaveData { Room = "first" });

        saves.Copy(1, 3);
        Expect.Equal("first", saves.Read(3).Room);
        saves.Clear(1);
        Expect.False(saves.Has(1));
        Expect.True(saves.Has(3));
        Expect.Throws<ArgumentOutOfRangeException>(() => saves.Has(0));
        Expect.Throws<ArgumentOutOfRangeException>(() => saves.Has(4));
    }

    [Check]
    public void ASlotThatCannotBeReadSaysSo()
    {
        using var assets = new TempAssets();
        assets.With("saves/slot1.json", "this is not a save");
        var saves = new SaveFile(Path.Combine(assets.Folder, "saves"), 3);
        Expect.Throws<InvalidDataException>(() => saves.Read(1));
    }

    [Check]
    public void ASavedGameIsPutBackIntoASession()
    {
        using var game = new StandInGame();
        GameSession session = game.Session;
        session.Progress.Record("a fact");
        session.Progress.MarkExplored("caves", 3, 4);
        session.Player.Inventory.Grant("boots");
        session.Player.Inventory.Grant("visor");
        session.Player.Inventory.Switch("visor", false);
        session.Player.Inventory.SetCapacity("energy", 99);
        session.Player.Inventory.Restore("energy", 42);
        game.Step(7);
        SaveData save = SaveData.Capture(session, "b", "West");

        session.BeginAgain();
        Expect.False(session.Progress.Has("a fact"));
        save.ApplyTo(session);

        Expect.True(session.Progress.Has("a fact"));
        Expect.True(session.Progress.IsExplored("caves", 3, 4));
        Expect.Equal(7L, session.Progress.PlayUpdates);
        Expect.True(session.Player.Inventory.Owns("boots"));
        Expect.True(session.Player.Inventory.IsOn("boots"));
        Expect.True(session.Player.Inventory.Owns("visor"));
        Expect.False(session.Player.Inventory.IsOn("visor"));
        Expect.Equal(99, session.Player.Inventory.Capacity("energy"));
        Expect.Equal(42, session.Player.Inventory.Amount("energy"));
        session.Player.Free();
    }

    [Check]
    public void OptionsAreKeptInTheirFile()
    {
        using var assets = new TempAssets();
        string file = Path.Combine(assets.Folder, "options.json");
        // A game's standard buttons come before the engine's for a control with no binding yet.
        var gameStandard = new Dictionary<string, List<string>> { [GameAction.Cancel.Name] = ["key:Q"] };
        Options fresh = Options.Load(file, gameStandard);
        Expect.Equal("english", fresh.Language);
        Expect.True(fresh.Bindings[GameAction.Jump.Name].SequenceEqual(["key:X", "pad:0"]));
        Expect.True(fresh.Bindings[GameAction.Cancel.Name].SequenceEqual(["key:Q"]));
        Expect.False(fresh.IsOn("anything"));

        fresh.Language = "japanese";
        fresh.Toggles["icon cancel"] = true;
        fresh.Bindings[GameAction.Jump.Name] = ["key:Space"];
        fresh.Bindings.Remove(GameAction.Attack.Name);
        fresh.Save(file);
        Options read = Options.Load(file, gameStandard);

        Expect.Equal("japanese", read.Language);
        Expect.True(read.IsOn("icon cancel"));
        Expect.True(read.Bindings[GameAction.Jump.Name].SequenceEqual(["key:Space"]));
        Expect.True(read.Bindings[GameAction.Attack.Name].SequenceEqual(["key:Z", "pad:2"]));
    }

    [Check]
    public void BindingsArePutIntoTheInputMap()
    {
        var options = new Options { Bindings = { ["check jump"] = ["key:Space", "pad:3"] } };
        options.ApplyBindings([new GameAction("check jump"), new GameAction("check unbound")]);

        Expect.True(InputMap.HasAction("check unbound"));
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents("check jump");
        Expect.Equal(2, events.Count);
        Expect.Equal(Key.Space, ((InputEventKey)events[0]).Keycode);
        Expect.Equal(JoyButton.Y, ((InputEventJoypadButton)events[1]).ButtonIndex);

        options.Bindings["check jump"] = ["key:A"];
        options.ApplyBindings([new GameAction("check jump")]);
        Expect.Equal(1, InputMap.ActionGetEvents("check jump").Count);
        InputMap.EraseAction("check jump");
        InputMap.EraseAction("check unbound");
    }

    [Check]
    public void ABindingThatCannotBeReadSaysSo()
    {
        Expect.Throws<InvalidDataException>(() => new Options { Bindings = { ["check bad"] = ["key:NoSuchKeyAtAll"] } }.ApplyBindings([new GameAction("check bad")]));
        Expect.Throws<InvalidDataException>(() => new Options { Bindings = { ["check bad"] = ["mouse:1"] } }.ApplyBindings([new GameAction("check bad")]));
        InputMap.EraseAction("check bad");
    }

    [Check]
    public void TheGamesWordsAreAskedForByName()
    {
        using var game = new StandInGame();
        Expect.Equal("HELLO", game.Session.Text.Get("hello"));
        Expect.Throws<ContentException>(() => game.Session.Text.Get("no such words"));
    }

    [Check]
    public void TheWordsComeFromTheLanguageTheOptionsName()
    {
        using var game = new StandInGame();
        game.Assets.With("text/french.json", """{ "hello": "BONJOUR" }""");
        game.Session.Options.Language = "french";
        Expect.Equal("BONJOUR", game.Session.Text.Get("hello"));
    }

    [Check]
    public void ARecordingGivesItsInputsUpdateByUpdateThenNothing()
    {
        using var assets = new TempAssets();
        assets.With("demo.json", """
            { "room": "demo.tscn", "entry_point": "Start",
              "inputs": [ { "held": ["right"], "updates": 2 }, { "held": ["right", "jump"], "updates": 1 }, { "held": [], "updates": 1 } ] }
            """);
        DemoRecording recording = DemoRecording.Read(assets.Library.ReadData("demo.json"));
        Expect.Equal("demo.tscn", recording.Room);
        Expect.Equal("Start", recording.EntryPoint);
        Expect.Equal(4, recording.Updates.Count);

        var source = new RecordedInputSource(recording);
        var sampler = new InputSampler(source);
        GameInput first = sampler.Sample();
        Expect.True(first.WasPressed(GameAction.Right));
        GameInput second = sampler.Sample();
        Expect.True(second.IsHeld(GameAction.Right));
        Expect.False(second.WasPressed(GameAction.Right));
        GameInput third = sampler.Sample();
        Expect.True(third.WasPressed(GameAction.Jump));
        Expect.False(source.IsOver);
        Expect.False(sampler.Sample().IsHeld(GameAction.Right));
        Expect.True(source.IsOver);
        Expect.False(sampler.Sample().IsHeld(GameAction.Right));
    }

    [Check]
    public void ARecordingWithAStretchOfNoUpdatesIsRefused()
    {
        using var assets = new TempAssets();
        assets.With("demo.json", """{ "room": "r", "entry_point": "e", "inputs": [ { "held": [], "updates": 0 } ] }""");
        Expect.Throws<ContentException>(() => DemoRecording.Read(assets.Library.ReadData("demo.json")));
    }
}
