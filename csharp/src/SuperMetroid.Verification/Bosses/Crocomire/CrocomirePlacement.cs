using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Places Crocomire in a check's enemy system as his room's population places him (#2554).</summary>
internal static class CrocomirePlacement
{
    /// <summary>
    /// Places a living Crocomire as his room's population places him: his body in the first place, at
    /// <paramref name="at"/>, and his tongue in the second, listed after his body, each set up as the population sets it
    /// up, in a room whose Crocomire has not been beaten. Returns his encounter, which his body's setup builds and his
    /// tongue's setup joins.
    /// </summary>
    internal static CrocomireEnemyState PlaceCrocomire(RoomEnemySystem enemies, Position16<RoomPixels> at)
    {
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        // His room's screens' scrolls are the room's own; the check keeps no room around him.
        enemies.VerificationServices().SetRoomScrollState = (_, _) => { };
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace tongue = enemies.Places.InUpdateOrder[1];
        body.Bind(EnemyDefinitionId.Crocomire, body.KeptDefinition);
        tongue.Bind(EnemyDefinitionId.CrocomireTongue, tongue.KeptDefinition);
        body.Position = body.Position.WithWhole(at);
        body.SetUpAsPlaced();
        tongue.SetUpAsPlaced();
        // His setup moves him where his arena has him; the check stands him where it asks.
        body.Position = body.Position.WithWhole(at);
        return body.EntityOf<CrocomireBodyEntity>().Encounter;
    }
}
