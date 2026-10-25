using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's native view of the room-effect identities (#1415): the cartridge routine and header words naming
/// each member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FxNativeWords
{
    /// <summary>A <see cref="RoomFxType"/> value naming no effect, standing for a dispatcher byte that names none.</summary>
    internal const RoomFxType UndefinedFxType = (RoomFxType)ushort.MaxValue;

    /// <summary>A <see cref="LayerBlendingConfiguration"/> value naming no configuration, standing for a byte that names none.</summary>
    internal const LayerBlendingConfiguration UndefinedLayerBlend = (LayerBlendingConfiguration)ushort.MaxValue;

    /// <summary>A <see cref="RoomFxPaletteBlend"/> value naming no blend, standing for a selector byte that names none.</summary>
    internal const RoomFxPaletteBlend UndefinedPaletteBlend = (RoomFxPaletteBlend)byte.MaxValue;

    /// <summary>
    /// The effect a native record's dispatcher byte <paramref name="word"/> names, or <see cref="UndefinedFxType"/> when it
    /// names none: a domain sweep over native bytes keeps testing rejection now that the identity's values are arbitrary.
    /// </summary>
    internal static RoomFxType FxType(int word) =>
        word is >= 0 and <= ushort.MaxValue && SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.TryDecode((ushort)word, out RoomFxType type)
            ? type : UndefinedFxType;

    /// <summary>The layer-blending configuration native byte <paramref name="word"/> names, or <see cref="UndefinedLayerBlend"/>.</summary>
    internal static LayerBlendingConfiguration LayerBlend(int word) =>
        word is >= 0 and <= ushort.MaxValue && RoomFxRecordCodec.LayerBlends.TryDecode((ushort)word, out LayerBlendingConfiguration configuration)
            ? configuration : UndefinedLayerBlend;

    /// <summary>The palette blend native selector byte <paramref name="word"/> names, or <see cref="UndefinedPaletteBlend"/>.</summary>
    internal static RoomFxPaletteBlend PaletteBlend(int word) =>
        word is >= 0 and <= ushort.MaxValue && SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.TryDecode((ushort)word, out RoomFxPaletteBlend blend)
            ? blend : UndefinedPaletteBlend;

    /// <summary>The bank-$87 object header naming each <see cref="AnimatedTileObject"/>; zero names no object.</summary>
    internal static readonly NativeWordIdentities<AnimatedTileObject> AnimatedTileObjects = new(new Dictionary<AnimatedTileObject, ushort>
    {
        [AnimatedTileObject.None] = 0x0000,
        [AnimatedTileObject.TourianStatuePhantoon] = 0x854c,
        [AnimatedTileObject.TourianStatueRidley] = 0x8552,
        [AnimatedTileObject.TourianStatueKraid] = 0x8558,
        [AnimatedTileObject.TourianStatueDraygon] = 0x855e,
        [AnimatedTileObject.Empty] = 0x824b,
        [AnimatedTileObject.HorizontalSpikes] = 0x8257,
        [AnimatedTileObject.VerticalSpikes] = 0x8251,
        [AnimatedTileObject.CrateriaLake] = 0x825d,
        [AnimatedTileObject.UnusedCrateriaLava] = 0x8263,
        [AnimatedTileObject.BrinstarPlant] = 0x8281,
        [AnimatedTileObject.WreckedShipScreen] = 0x826f,
        [AnimatedTileObject.MaridiaSandCeiling] = 0x8287,
        [AnimatedTileObject.MaridiaSandFalling] = 0x828d,
        [AnimatedTileObject.WreckedShipTreadmillRightwards] = 0x8275,
        [AnimatedTileObject.WreckedShipTreadmillLeftwards] = 0x827b,
        [AnimatedTileObject.Lava] = 0x82ab,
        [AnimatedTileObject.Acid] = 0x82c9,
        [AnimatedTileObject.Rain] = 0x82e7,
        [AnimatedTileObject.Spores] = 0x82fd,
    });

    /// <summary>The bank-$84 setup routine naming each <see cref="SpecialAirReactionSetup"/>.</summary>
    internal static readonly NativeWordIdentities<SpecialAirReactionSetup> SpecialAirReactionSetups = new(new Dictionary<SpecialAirReactionSetup, ushort>
    {
        [SpecialAirReactionSetup.Nothing] = 0xb3cf,
        [SpecialAirReactionSetup.ClearCarry] = 0xb3d0,
        [SpecialAirReactionSetup.BrinstarFloorPlant] = 0xb0dc,
        [SpecialAirReactionSetup.BrinstarCeilingPlant] = 0xb113,
        [SpecialAirReactionSetup.IcePhysics] = 0xb3eb,
        [SpecialAirReactionSetup.QuicksandSurface] = 0xb408,
        [SpecialAirReactionSetup.SubmergingQuicksand] = 0xb497,
        [SpecialAirReactionSetup.SandFallsSlow] = 0xb4a8,
        [SpecialAirReactionSetup.SandFallsFast] = 0xb4b6,
        [SpecialAirReactionSetup.QuicksandSurfaceCollision] = 0xb4c4,
        [SpecialAirReactionSetup.SubmergingQuicksandCollision] = 0xb541,
        [SpecialAirReactionSetup.SandFallsCollision] = 0xb54f,
        [SpecialAirReactionSetup.SpeedBlock] = 0xcdea,
        [SpecialAirReactionSetup.LowerNorfairChozoHand] = 0xd18f,
        [SpecialAirReactionSetup.WreckedShipChozoHand] = 0xd620,
    });

    /// <summary>The bank-$8D pre-instruction routine naming each <see cref="PaletteFxPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<PaletteFxPreInstruction> PaletteFxPreInstructions = new(new Dictionary<PaletteFxPreInstruction, ushort>
    {
        [PaletteFxPreInstruction.Null] = 0xc526,
        [PaletteFxPreInstruction.Cleared] = 0xc5e3,
        [PaletteFxPreInstruction.Intro] = 0xe20b,
        [PaletteFxPreInstruction.DeleteWhenEnemyZeroDies] = 0xe2e0,
        [PaletteFxPreInstruction.Heat] = 0xe379,
        [PaletteFxPreInstruction.SwitchAboveY380] = 0xec59,
        [PaletteFxPreInstruction.SwitchAboveY380Second] = 0xed84,
        [PaletteFxPreInstruction.DeleteWhenAreaMiniBossDies] = 0xeec5,
        [PaletteFxPreInstruction.InspectAdjacentSlot] = 0xf621,
    });

    /// <summary>The bank-$8D setup routine naming each <see cref="PaletteFxSetup"/>.</summary>
    internal static readonly NativeWordIdentities<PaletteFxSetup> PaletteFxSetups = new(new Dictionary<PaletteFxSetup, ushort>
    {
        [PaletteFxSetup.Null] = 0xc685,
        [PaletteFxSetup.Intro] = 0xe204,
        [PaletteFxSetup.Norfair] = 0xe440,
        [PaletteFxSetup.Brinstar] = 0xf730,
    });
}
