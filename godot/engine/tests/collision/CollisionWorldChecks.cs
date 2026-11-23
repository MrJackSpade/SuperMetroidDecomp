using Engine.Collision;
using Engine.Shared;

namespace Engine.Tests.Collision;

/// <summary>Checks that the collision world finds what a rectangle overlaps, by role, in a stable order.</summary>
public sealed class CollisionWorldChecks
{
    [Check]
    public void OverlapsAreReportedInTheOrderCollidersWereRegistered()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (_, third) = AddHurtbox(room, 30);
        var (_, first) = AddHurtbox(room, 10);
        var (_, second) = AddHurtbox(room, 20);
        room.World.Unregister(third);
        room.World.Register(third);

        var found = room.World.Overlapping(new PixelRect(0, 0, 128, 48), ColliderRole.Hurtbox);

        Expect.True(found.SequenceEqual([first, second, third]));
    }

    [Check]
    public void OnlyTheRoleAskedForIsFound()
    {
        using var room = new StandInRoom("........", "........", "########");
        room.AddBody(20, 32);
        var (_, hurtbox) = AddHurtbox(room, 20);
        var found = room.World.Overlapping(new PixelRect(0, 0, 128, 48), ColliderRole.Hurtbox);
        Expect.True(found.SequenceEqual([hurtbox]));
    }

    [Check]
    public void TouchingCountsOnlyWhenAskedFor()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (_, hurtbox) = AddHurtbox(room, 20);
        var beside = new PixelRect(hurtbox.RoomRect.Right, hurtbox.RoomRect.Top, 4, 4);
        Expect.Equal(0, room.World.Overlapping(beside, ColliderRole.Hurtbox).Count);
        Expect.Equal(1, room.World.Overlapping(beside, ColliderRole.Hurtbox, touchingCounts: true).Count);
    }

    [Check]
    public void AnOwnersOwnCollidersCanBeLeftOut()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, _) = AddHurtbox(room, 20);
        var (_, other) = AddHurtbox(room, 22);
        var found = room.World.Overlapping(new PixelRect(0, 0, 128, 48), ColliderRole.Hurtbox, except: owner);
        Expect.True(found.SequenceEqual([other]));
    }

    [Check]
    public void AColliderFollowsItsOwner()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, hurtbox) = AddHurtbox(room, 20);
        owner.Position = PixelPosition.FromWhole(100, 40);
        Expect.Equal(new PixelRect(96, 24, 8, 16), hurtbox.RoomRect);
    }

    /// <summary>Adds something with one hurtbox, standing at the given column.</summary>
    private static (StandInBody Owner, Collider Hurtbox) AddHurtbox(StandInRoom room, int x)
    {
        var (owner, _) = room.AddBody(x, 32);
        Collider hurtbox = owner.AddCollider(ColliderRole.Hurtbox, new PixelRect(-4, -16, 8, 16));
        room.World.Register(hurtbox);
        return (owner, hurtbox);
    }
}
