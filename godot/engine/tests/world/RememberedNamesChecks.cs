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

        Expect.Equal(0, names.Mistakes.Count);
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

        Expect.Equal(1, names.Mistakes.Count);
        Expect.True(names.Mistakes[0].Contains("\"Gate\" in the room \"second\""));
        Expect.True(names.Mistakes[0].Contains("\"Tank\" in the room \"first\""));
        Expect.Throws<InvalidOperationException>(names.Require);
        first.Free();
        second.Free();
    }

    [Check]
    public void AnItemOrBossEncounterWithNoNameIsAMistakeAndEveryMistakeIsReported()
    {
        var room = RoomWith(new Item { Name = "Tank" }, new BossEncounter { Name = "Fight" }, new Barrier { Name = "Gate" });
        var names = new RememberedNames();
        names.Add("room", room);

        Expect.Equal(2, names.Mistakes.Count);
        Expect.True(names.Mistakes[0].Contains("\"Tank\""));
        Expect.True(names.Mistakes[1].Contains("\"Fight\""));
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

        Expect.Equal(1, names.Mistakes.Count);
        room.Free();
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
