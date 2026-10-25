using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// What a saved slot keeps of a snapshot (#1421): <c>SaveToSRAM</c> stores and <c>LoadFromSRAM</c> reads back the
/// snapshot's fields as their saved words, so every value is checked as the save stores it and the explored map keeps
/// only the bytes a stock map tile occupies.
/// </summary>
[SnesEncodingOwner]
internal static class SaveSlotRetention
{
    /// <summary>The slot <paramref name="snapshot"/> saves as in <paramref name="slot"/>.</summary>
    internal static SuperMetroidSaveSlot Retain(SuperMetroidSaveSnapshot snapshot, int slot, IAreaMapSource? maps)
    {
        Ensure.NotNull(snapshot);
        ControllerBindings bindings = snapshot.ControllerBindings.RequireRetailPermutation();
        ReserveTankMode reserveMode = ReserveTankModes.Decode(snapshot.ReserveMode.ToNativeWord());
        int health = Kept(snapshot.Health), maxHealth = Kept(snapshot.MaxHealth);
        int missiles = Kept(snapshot.Missiles), maxMissiles = Kept(snapshot.MaxMissiles);
        int superMissiles = Kept(snapshot.SuperMissiles), maxSuperMissiles = Kept(snapshot.MaxSuperMissiles);
        int powerBombs = Kept(snapshot.PowerBombs), maxPowerBombs = Kept(snapshot.MaxPowerBombs);
        HudItem hudItem = HudItems.Decode(snapshot.HudItem.ToNativeWord());
        int maxReserveEnergy = Kept(snapshot.MaxReserveEnergy), reserveEnergy = Kept(snapshot.ReserveEnergy);
        int reserveMissiles = Kept(snapshot.ReserveMissiles);
        ushort loadedItemCount = SaveFieldValue.Encode(snapshot.LoadedItemCount, nameof(snapshot.LoadedItemCount));
        ushort gameTimeFrames = SaveFieldValue.Encode(snapshot.GameTimeFrames, nameof(snapshot.GameTimeFrames));
        ushort gameTimeSeconds = SaveFieldValue.Encode(snapshot.GameTimeSeconds, nameof(snapshot.GameTimeSeconds));
        ushort gameTimeMinutes = SaveFieldValue.Encode(snapshot.GameTimeMinutes, nameof(snapshot.GameTimeMinutes));
        ushort gameTimeHours = SaveFieldValue.Encode(snapshot.GameTimeHours, nameof(snapshot.GameTimeHours));
        ushort debugFlag = SaveFieldValue.Encode(snapshot.DebugFlag, nameof(snapshot.DebugFlag));
        ushort newFileMarker = SaveFieldValue.Encode(snapshot.NewFileMarker, nameof(snapshot.NewFileMarker));

        RequireLength(snapshot.EventBytes, SharedGameState.EventByteCount, "A save snapshot requires exactly eight event bytes.");
        RequireLength(snapshot.BossFlags, SharedGameState.AreaCount, "A save snapshot requires exactly eight area-boss flag sets.");
        RequireLength(snapshot.RoomChozoBytes, SharedGameState.RoomChozoBitByteCount, "A save snapshot requires exactly 64 room-Chozo bytes.");
        RequireLength(snapshot.CollectedItemBytes, SharedGameState.ItemBitByteCount, "A save snapshot requires exactly 64 collected-item bytes.");
        RequireLength(snapshot.OpenedDoorBytes, SharedGameState.DoorBitByteCount, "A save snapshot requires exactly 64 opened-door bytes.");
        RequireLength(snapshot.UsedSaveStationBytes, SharedGameState.UsedSaveStationByteCount, "A save snapshot requires exactly 16 used save/elevator bytes.");
        RequireLength(snapshot.MapStationBytes, SharedGameState.MapStationByteCount, "A save snapshot requires exactly 12 map-station bytes.");
        ushort loadingGameState = SaveFieldValue.Encode(snapshot.LoadingGameState, nameof(snapshot.LoadingGameState));
        ushort saveStation = SaveFieldValue.Encode(snapshot.SaveStation, nameof(snapshot.SaveStation));
        ushort area = SaveFieldValue.Encode(snapshot.Area, nameof(snapshot.Area));
        byte[] exploredMap = ExploredMapStorage.Retained(snapshot.ExploredMapBytes, maps);

        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: snapshot.EquippedItems,
            CollectedItems: snapshot.CollectedItems,
            EquippedBeams: snapshot.EquippedBeams,
            CollectedBeams: snapshot.CollectedBeams,
            ReserveMode: reserveMode,
            Health: health,
            MaxHealth: maxHealth,
            Missiles: missiles,
            MaxMissiles: maxMissiles,
            SuperMissiles: superMissiles,
            MaxSuperMissiles: maxSuperMissiles,
            PowerBombs: powerBombs,
            MaxPowerBombs: maxPowerBombs,
            HudItem: hudItem,
            MaxReserveEnergy: maxReserveEnergy,
            ReserveEnergy: reserveEnergy,
            GameTimeFrames: gameTimeFrames,
            GameTimeSeconds: gameTimeSeconds,
            GameTimeMinutes: gameTimeMinutes,
            GameTimeHours: gameTimeHours,
            EventBytes: snapshot.EventBytes.ToArray(),
            BossBytes: [.. snapshot.BossFlags.Select(flags => (byte)flags)],
            RoomChozoBytes: snapshot.RoomChozoBytes.ToArray(),
            CollectedItemBytes: snapshot.CollectedItemBytes.ToArray(),
            OpenedDoorBytes: snapshot.OpenedDoorBytes.ToArray(),
            UsedSaveStationBytes: snapshot.UsedSaveStationBytes.ToArray(),
            MapStationBytes: snapshot.MapStationBytes.ToArray(),
            ExploredMapBytes: exploredMap,
            LoadingGameState: loadingGameState,
            SaveStation: saveStation,
            Area: area)
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = snapshot.MoonwalkEnabled,
            DebugFlag = debugFlag,
            NewFileMarker = newFileMarker,
            IconCancelEnabled = snapshot.IconCancelEnabled,
            ReserveMissiles = reserveMissiles,
            JapaneseText = snapshot.JapaneseText,
            LoadedItemCount = loadedItemCount,
        };
    }

    /// <summary>A quantity as the save keeps it: checked against the saved word's range.</summary>
    private static int Kept(int quantity) => SamusQuantity.Decode(SamusQuantity.Encode(quantity));

    /// <summary>Rejects a snapshot array of the wrong length.</summary>
    private static void RequireLength<T>(T[] values, int length, string message)
    {
        if (values.Length != length)
            throw new InvalidDataException(message);
    }
}
