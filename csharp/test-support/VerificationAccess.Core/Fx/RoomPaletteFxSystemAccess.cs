namespace SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

/// <summary>Verification access to <see cref="RoomPaletteFxSystem"/> members production does not use.</summary>
internal static class RoomPaletteFxSystemAccess
{
    /// <summary>Builds room palette FX from an explicit FX record.</summary>
    extension(RoomPaletteFxSystem)
    {
        /// <summary>Builds room palette FX from an explicit FX record through the normal room palette-object path.</summary>
        internal static RoomPaletteFxSystem ForDefinition(PaletteFxSessionState session, RoomFxRecordDefinition definition,
            AreaId area, ushort equippedItems, bool areaMiniBossDefeated)
        {
            Ensure.NotNull(definition);
            return RoomPaletteFxSystem.ForRecord(session, null, definition.Door, area, (SamusEquipmentFlags)equippedItems, areaMiniBossDefeated, (RoomFxRecordDefinition?)(definition));
        }
    }
}
