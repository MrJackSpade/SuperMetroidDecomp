using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Places Mother Brain in a check's enemy system as her room's population places her (#2554).</summary>
internal static class MotherBrainPlacement
{
    /// <summary>
    /// Places Mother Brain as her room places her: its objects set her glass up before its enemies spawn, and her body, in
    /// the first place, and then her brain, in the second, are set up as the population sets them up with
    /// <paramref name="samus"/>, her body's setup confirming the glass and beginning the encounter her brain joins.
    /// </summary>
    /// <returns>Her encounter.</returns>
    internal static MotherBrainEnemyState PlaceMotherBrain(RoomEnemySystem enemies, SamusState samus)
    {
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectPlace glassPlace = roomObjects.Places.Spawn() ?? throw new InvalidOperationException("A new room has a free room-object place.");
        glassPlace.Block = new Position16<RoomBlocks>(8, 8);
        roomObjects.MotherBrainGlass.SetupMotherBrainGlass(RoomEnemyFrameFixtures.OpenRoom(new(16, 16)), glassPlace);
        enemies.VerificationServices().RoomGlass = roomObjects.MotherBrainGlass;
        MotherBrainEnemyState state = enemies.Places.InUpdateOrder[0]
            .PlaceAs<MotherBrainBodyEntity>(EnemyDefinitionId.MotherBrainBody, EnemySpawnSettings.None, samus).Encounter;
        _ = enemies.Places.InUpdateOrder[1].PlaceAs<MotherBrainHeadEntity>(EnemyDefinitionId.MotherBrainHead, EnemySpawnSettings.None, samus);
        return state;
    }
}
