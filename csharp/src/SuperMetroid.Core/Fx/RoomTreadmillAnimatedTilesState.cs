using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Treadmill objects selected by LoadFxHeader's area animation bitset, independently
/// of the additional entrance object spawned by door ASM.
/// </summary>
public sealed class RoomTreadmillAnimatedTilesState
{
    /// <summary>Treadmill animations selected by the current room FX bitset in native bit order.</summary>
    internal readonly List<WreckedShipTreadmillAnimatedTilesState> objects = [];

    /// <summary>Clears the previous population and follows the selected FX record's native bit order.</summary>
    public void LoadRoom(ushort fxPointer, CartridgeDoorHeader? door, AreaId area)
    {
        objects.Clear();
        if (fxPointer == 0) return;
        ushort record = RoomFxRecordDefinitions.Select(fxPointer, door);
        if (record == 0) return;
        byte bits = RoomFxRecordDefinitions.Get(record).AnimatedTileBitset;
        for (int bit = 0; bit < 8; bit++)
        {
            if ((bits & (1 << bit)) == 0) continue;
            AnimatedTileObject definition = AreaAnimatedTileObjectDefinitions.Read(area, bit);
            WreckedShipTreadmillDirection? direction = definition switch
            {
                AnimatedTileObject.WreckedShipTreadmillRightwards => WreckedShipTreadmillDirection.Rightwards,
                AnimatedTileObject.WreckedShipTreadmillLeftwards => WreckedShipTreadmillDirection.Leftwards,
                AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or
                    AnimatedTileObject.TourianStatueRidley or AnimatedTileObject.TourianStatueKraid or
                    AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty or
                    AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.VerticalSpikes or
                    AnimatedTileObject.CrateriaLake or AnimatedTileObject.UnusedCrateriaLava or
                    AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen or
                    AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling or
                    AnimatedTileObject.Lava or AnimatedTileObject.Acid or AnimatedTileObject.Rain or
                    AnimatedTileObject.Spores => null,
                _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {definition}."),
            };
            // Other animation families retain their own owners. Selection here is by
            // native object identity, not room number or an assumed area-specific bit.
            if (direction is null) continue;
            var animation = new WreckedShipTreadmillAnimatedTilesState();
            animation.Start(direction.Value);
            objects.Add(animation);
        }
    }

    /// <summary>Runs each selected native list; its boss wait gates graphics independently of floor collision.</summary>
    public void Step(bool areaBossDefeated, VramWriteQueue writes)
    {
        foreach (var animation in objects) animation.Step(areaBossDefeated, writes);
    }
}
