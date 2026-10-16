using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

/// <summary>Verification access to <see cref="RoomPaletteFxSystem"/> members production does not use.</summary>
internal static class RoomPaletteFxSystemAccess
{
    /// <summary>Reads the current and prior Samus-in-heat palette indexes used by room palette FX.</summary>
    extension(RoomPaletteFxSystem self)
    {
        /// <summary>
        /// Heat-animation phase at WRAM $1EED, published by Norfair palette streams and
        /// consumed one descending-slot pass later by the shared Samus-in-heat object.
        /// </summary>
        internal ushort SamusInHeatPaletteIndex => self.samusInHeatPaletteIndex;

        /// <summary>
        /// Last heat-animation phase consumed by the shared Samus-in-heat object, corresponding
        /// to WRAM $1EEF. Exposing both words makes native one-frame handoff timing inspectable.
        /// </summary>
        internal ushort PreviousSamusInHeatPaletteIndex => self.previousSamusInHeatPaletteIndex;

        /// <summary>Loads an explicit FX record through the normal room palette-object path.</summary>
        internal void LoadDefinition(RoomFxRecordDefinition definition,
            AreaId area, ushort equippedItems, bool areaMiniBossDefeated)
        {
            Ensure.NotNull(definition);
            self.LoadRoomCore((ushort)(definition.Pointer), (ushort)(definition.DoorPointer), (AreaId)(area), (SamusEquipmentFlags)((ushort)(equippedItems)), (bool)(areaMiniBossDefeated), (RoomFxRecordDefinition?)(definition));
        }
    }
}
