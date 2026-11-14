namespace SuperMetroid.Core.Fx;

using SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="ColourEffectSystem"/> members production does not use.</summary>
internal static class RoomPaletteFxSystemAccess
{
    /// <summary>Builds room palette FX from an explicit FX record.</summary>
    extension(ColourEffectSystem)
    {
        /// <summary>Builds room palette FX from an explicit FX record through the normal room palette-object path.</summary>
        internal static ColourEffectSystem ForDefinition(ColourEffectSessionState session, RoomFxRecordDefinition definition,
            ushort equippedItems, bool areaMiniBossDefeated)
        {
            Ensure.NotNull(definition);
            return ColourEffectSystem.ForRecord(session, null, null, SamusEquipmentWordCodec.FromEquipmentWord(equippedItems), areaMiniBossDefeated, (RoomFxRecordDefinition?)(definition));
        }
    }
}
