using System.Text.RegularExpressions;
using Engine.Core;
using Engine.Input;
using Engine.Tests;
using Engine.Tests.Content;
using Game.Data;
using Godot;

namespace Game.Tests;

/// <summary>Checks that the engine is pointed at this game, and that the names this game's code refers to are sound.</summary>
public sealed class GameDefinitionChecks
{
    [Check]
    public void TheProjectIsPointedAtThisGame()
    {
        string path = ProjectSettings.GetSetting(GameLoop.GameDefinitionSetting, "").AsString();
        Expect.Equal("res://game/game_definition.tres", path);
        Expect.True(ResourceLoader.Load<GameDefinition>(path) is SuperMetroid);
    }

    [Check]
    public void TheGamesPlayerIsSamus()
    {
        var definition = ResourceLoader.Load<GameDefinition>("res://game/game_definition.tres");
        Engine.Actors.Player player = definition.CreatePlayer();
        Expect.True(player is Samus);
        player.Free();
    }

    [Check]
    public void TheGameAddsItsActionsAndBindsEachOne()
    {
        var definition = new SuperMetroid();
        Expect.True(definition.Actions.SequenceEqual(["run", "item_select", "item_cancel", "angle_up", "angle_down"]));
        Expect.False(definition.Actions.Intersect(GameAction.Standard.Select(action => action.Name)).Any());

        using var assets = new TempAssets();
        Options options = Options.Load(Path.Combine(assets.Folder, "options.json"), definition.StandardBindings);
        options.AddMissingControls(definition.StandardControls);

        // The menus confirm with the cartridge's A and cancel with its B, which lie where play's A and B lie.
        Expect.True(options.ButtonsOf(GameAction.Confirm.Name).Contains(options.ButtonsOf(GameAction.Jump.Name).Single(button => button.StartsWith("pad:", StringComparison.Ordinal))));
        Expect.True(options.ButtonsOf(GameAction.Cancel.Name).Contains(options.ButtonsOf("run").Single(button => button.StartsWith("pad:", StringComparison.Ordinal))));
        foreach (string action in definition.Actions)
            Expect.True(options.ButtonsOf(action).Count > 0);
        options.ApplyBindings(Actions.All);
        Expect.Equal(2, InputMap.ActionGetEvents(Actions.Run.Name).Count);
        foreach (GameAction action in Actions.All)
            InputMap.EraseAction(action.Name);
    }

    [Check]
    public void EveryListOfNamesIsWrittenTheSameWayWithNoNameTwice()
    {
        IReadOnlyList<string>[] lists =
        [
            Equipment.All, Amounts.All, HitKinds.All, Areas.All, Bosses.All, StoryEvents.All, Music.All, Toggles.All,
            [.. Actions.All.Select(action => action.Name)],
        ];
        foreach (IReadOnlyList<string> list in lists)
        {
            Expect.True(list.Count > 0);
            Expect.Equal(list.Count, list.Distinct(StringComparer.Ordinal).Count());
            foreach (string name in list)
                Expect.True(Regex.IsMatch(name, "^[a-z0-9]+(_[a-z0-9]+)*$"), name);
        }
    }

    [Check]
    public void NoTwoFactsInProgressShareAName()
    {
        // Equipment is not a fact, but story events and boss defeats are recorded side by side in progress.
        var facts = StoryEvents.All.Concat(Bosses.All.Select(Bosses.Defeated)).ToList();
        Expect.Equal(facts.Count, facts.Distinct(StringComparer.Ordinal).Count());
        Expect.Equal("kraid_defeated", Bosses.Defeated(Bosses.Kraid));
    }

    [Check]
    public void TheEnginesEnergyIsThisGamesEnergy() =>
        Expect.Equal(Engine.Actors.Inventory.Energy, Amounts.Energy);
}
