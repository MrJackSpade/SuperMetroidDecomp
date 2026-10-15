
namespace SuperMetroid.Core.Saves;

/// <summary>Verification access to <see cref="SuperMetroidSaveSlot"/> members production does not use.</summary>
internal static class SuperMetroidSaveSlotAccess
{
    /// <summary>Copies the complete save-slot state into the snapshot accepted by the SRAM encoder.</summary>
    extension(SuperMetroidSaveSlot self)
    {
        /// <summary>Creates the complete translated snapshot accepted by the SRAM encoder.</summary>
        internal SuperMetroidSaveSnapshot ToSnapshot() => new()
        {
            ControllerBindings = self.ControllerBindings,
            MoonwalkEnabled = self.MoonwalkEnabled,
            DebugFlag = self.DebugFlag,
            NewFileMarker = self.NewFileMarker,
            IconCancelEnabled = self.IconCancelEnabled,
            ReserveMissiles = self.ReserveMissiles,
            JapaneseText = self.JapaneseText,
            LoadedItemCount = self.LoadedItemCount,
            EquippedItems = self.EquippedItems,
            CollectedItems = self.CollectedItems,
            EquippedBeams = self.EquippedBeams,
            CollectedBeams = self.CollectedBeams,
            ReserveMode = self.ReserveMode,
            Health = self.Health,
            MaxHealth = self.MaxHealth,
            Missiles = self.Missiles,
            MaxMissiles = self.MaxMissiles,
            SuperMissiles = self.SuperMissiles,
            MaxSuperMissiles = self.MaxSuperMissiles,
            PowerBombs = self.PowerBombs,
            MaxPowerBombs = self.MaxPowerBombs,
            HudItem = self.HudItem,
            MaxReserveEnergy = self.MaxReserveEnergy,
            ReserveEnergy = self.ReserveEnergy,
            GameTimeFrames = self.GameTimeFrames,
            GameTimeSeconds = self.GameTimeSeconds,
            GameTimeMinutes = self.GameTimeMinutes,
            GameTimeHours = self.GameTimeHours,
            SaveStation = self.SaveStation,
            Area = self.Area,
            EventBytes = self.EventBytes.ToArray(),
            BossBytes = self.BossBytes.ToArray(),
            RoomChozoBytes = self.RoomChozoBytes.ToArray(),
            CollectedItemBytes = self.CollectedItemBytes.ToArray(),
            OpenedDoorBytes = self.OpenedDoorBytes.ToArray(),
            UsedSaveStationBytes = self.UsedSaveStationBytes.ToArray(),
            MapStationBytes = self.MapStationBytes.ToArray(),
            ExploredMapBytes = self.ExploredMapBytes.ToArray(),
            LoadingGameState = self.LoadingGameState,
        };
    }
}
