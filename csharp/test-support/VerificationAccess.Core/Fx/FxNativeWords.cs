using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's native view of the room-effect identities (#1415): the cartridge routine and header words naming
/// each member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FxNativeWords
{
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
