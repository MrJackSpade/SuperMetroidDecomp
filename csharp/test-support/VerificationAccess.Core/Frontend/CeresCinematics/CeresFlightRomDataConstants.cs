namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Cartridge values and helpers of <see cref="CeresFlightRomData"/> that only verification reads.</summary>
internal static class CeresFlightRomDataConstants
{
    /// <summary>Word-addressed BG1 layout used after the Mode 7 approach when the SPACE COLONY caption switches the scene to Mode 1.</summary>
    public static class Layers
    {
        /// <summary>Mode-1 SPACE COLONY BG1 tilemap base after the approach.</summary>
        public const ushort SpaceColonyTilemapWord = 0x5c00;
        /// <summary>Mode-1 SPACE COLONY BG1 character base within the OBJ transfer.</summary>
        public const ushort SpaceColonyCharacterWord = 0x6000;
    }
}
