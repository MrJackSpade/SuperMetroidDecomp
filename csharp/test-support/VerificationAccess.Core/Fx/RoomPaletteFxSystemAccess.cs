namespace SuperMetroid.Core.Fx;

using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="ColourEffectSystem"/> members production does not use.</summary>
internal static class RoomPaletteFxSystemAccess
{
    /// <summary>Builds room palette FX from an explicit FX record.</summary>
    extension(ColourEffectSystem)
    {
        /// <summary>
        /// Builds room palette FX from an explicit FX record through the normal room palette-object path, its sounds joining a
        /// check's own audio requests, as a runtime binds its update's (#2057).
        /// </summary>
        internal static ColourEffectSystem ForDefinition(ColourEffectSessionState session, RoomEffectDefinition definition,
            ushort equippedItems, bool areaMiniBossDefeated)
        {
            Ensure.NotNull(definition);
            ColourEffectSystem system = ColourEffectSystem.ForEffect(session, null, null, SamusEquipmentWordCodec.FromEquipmentWord(equippedItems), areaMiniBossDefeated, (RoomEffectDefinition?)(definition), SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone());
            return system;
        }
    }
}
