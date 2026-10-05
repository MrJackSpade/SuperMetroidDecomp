using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native atmospheric OBJ frame identity progression; selected palette/priority and tile pixels remain required.</summary>
internal static class SamusAtmosphericArtworkDefinitions
{
    /// <summary>$90:8C0F, AtmosphericGraphics_SpriteTileNumberAttributes_1_Footstep: first of four consecutive footstep OBJ tiles.</summary>
    internal const int FootstepFirstTile = 0x02C;
    /// <summary>$90:8C17, AtmosphericGraphics_SpriteTileNumberAttribute_4_6_7_LavaDust: first of four consecutive shared lava/dust OBJ tiles.</summary>
    internal const int LavaDustFirstTile = 0x048;
    /// <summary>$90:8C0F-$8C1E: chosen OBJ palette five, still-required presentation input.</summary>
    internal const int Palette = 5;
    /// <summary>$90:8C0F-$8C1E: chosen OBJ priority two, still-required presentation input.</summary>
    internal const int Priority = 2;

    internal static ushort Attributes(bool footstep, int frame)
    {
        if ((uint)frame >= SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return SnesObjAttributeWord.Create((footstep ? FootstepFirstTile : LavaDustFirstTile) + frame,
            Palette, Priority).Raw;
    }
}
