using Engine.Core;
using Engine.Tests.Core;
using Engine.Tests.Input;
using Engine.World;
using Engine.World.Objects;
using Godot;

namespace Engine.Tests.World;

/// <summary>Checks that a game whose remembered names are wrong is refused as it loads.</summary>
public sealed class RememberedNamesChecks
{
    [Check]
    public void NamesThatAreAllDifferentAreAccepted()
    {
        var first = RoomWith(new Item { Name = "Tank", RememberedAs = "tank one" }, new Barrier { Name = "Gate" });
        var second = RoomWith(new Item { Name = "Tank", RememberedAs = "tank two" }, new Barrier { Name = "Gate", RememberedAs = "gate" });
        var names = new RememberedNames();
        names.Add("first", first);
        names.Add("second", second);

        names.Require();
        first.Free();
        second.Free();
    }

    [Check]
    public void ANameSharedAcrossRoomsIsAMistakeNamingBoth()
    {
        var first = RoomWith(new Item { Name = "Tank", RememberedAs = "tank" });
        var second = RoomWith(new Barrier { Name = "Gate", RememberedAs = "tank" });
        var names = new RememberedNames();
        names.Add("first", first);
        names.Add("second", second);

        string refusal = Refusal(names);
        Expect.True(refusal.Contains("\"Gate\" in the room \"second\""));
        Expect.True(refusal.Contains("\"Tank\" in the room \"first\""));
        first.Free();
        second.Free();
    }

    [Check]
    public void AnItemOrBossEncounterWithNoNameIsAMistakeAndEveryMistakeIsReported()
    {
        var room = RoomWith(new Item { Name = "Tank" }, new BossEncounter { Name = "Fight" }, new Barrier { Name = "Gate" });
        var names = new RememberedNames();
        names.Add("room", room);

        string refusal = Refusal(names);
        Expect.True(refusal.Contains("\"Tank\""));
        Expect.True(refusal.Contains("\"Fight\""));
        room.Free();
    }

    [Check]
    public void ThingsPlacedBeneathOtherNodesAreFoundToo()
    {
        var room = new Room { Name = "Room" };
        var group = new Node2D { Name = "Group" };
        group.AddChild(new Item { Name = "Tank" });
        room.AddChild(group);
        var names = new RememberedNames();
        names.Add("room", room);

        Expect.Throws<InvalidOperationException>(names.Require);
        room.Free();
    }

    /// <summary>What the audit says as it refuses the names it was given; empty when it accepts them.</summary>
    private static string Refusal(RememberedNames names)
    {
        try
        {
            names.Require();
        }
        catch (InvalidOperationException refused)
        {
            return refused.Message;
        }

        return "";
    }

    [Check]
    public void AGameWhoseRoomsShareANameDoesNotStart()
    {
        var loop = new GameLoop();
        var definition = new SharedNameDefinition { Rooms = ["first", "second"] };
        Expect.Throws<InvalidOperationException>(() => loop.StartStandIn(definition, new ScriptedInputSource()));
        Expect.Throws<InvalidOperationException>(loop.Step);
        loop.Free();
    }

    /// <summary>A room holding the given things.</summary>
    private static Room RoomWith(params Node[] things)
    {
        var room = new Room { Name = "Room" };
        foreach (Node thing in things)
            room.AddChild(thing);
        return room;
    }
}
