using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive blue-spore room palette owner.</summary>
public enum BrinstarBlueSporePaletteOwner
{
    /// <summary>The ordinary blue-spore rooms' spores: the loop animates the <see cref="ColourEffectColours.BrinstarBlueSpores"/> in ordinary blue-spore rooms, without a boss-death frame behaviour.</summary>
    StandardRooms,
    /// <summary>Spore Spawn's room's spores: the variant uses the same spore colors but deletes the object when the current area's mini-boss is defeated.</summary>
    SporeSpawnRoom,
}
