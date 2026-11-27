using Engine.Content;

namespace Engine.World;

/// <summary>A shake of the room that something starts: how strong it is, how long it lasts and which way it throws the view.</summary>
/// <remarks><code>{ "strength": 1, "updates": 30, "way": "both" }</code></remarks>
/// <param name="Strength">How far the view is thrown, in pixels.</param>
/// <param name="Updates">How many updates the shake lasts.</param>
/// <param name="Way">The way the view is thrown about.</param>
public sealed record RoomShake(int Strength, int Updates, ShakeWay Way, ShakeReach Reach)
{
    /// <summary>Reads a shake from a data file's group.</summary>
    public static RoomShake Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var shake = new RoomShake(data.Int("strength"), data.Int("updates"), data.Has("way") ? data.Get("way").AsEnum<ShakeWay>() : ShakeWay.Across,
            data.Get("reach").AsEnum<ShakeReach>());
        if (shake.Strength <= 0 || shake.Updates <= 0)
            throw data.Problem("gives a shake of no strength or no length");
        return shake;
    }

    /// <summary>Starts the shake in <paramref name="room"/>.</summary>
    public void Start(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        room.Shake(Strength, Updates, Way, Reach);
    }
}
