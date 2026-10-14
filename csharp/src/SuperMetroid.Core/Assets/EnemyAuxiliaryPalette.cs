namespace SuperMetroid.Core.Assets;

/// <summary>Four distinct RGB5 animation sources; values are identities, not composable bits.</summary>
public enum EnemyAuxiliaryPalette
{
    /// <summary><c>$A8:E7CC</c>, native <c>FaceBlockGlowColors</c>: eight four-color glow rows for the Blue Brinstar face block, selected by its shared timed palette hook.</summary>
    FaceBlock,
    /// <summary><c>$A9:EBCC-ECAB</c>, native <c>Palette_SidehopperCorpseBeingDrained_0..6</c>: six draining rows followed by corpse paint; each supplied row omits color zero and updates CGRAM entries 145..159.</summary>
    DeadSidehopper,
    /// <summary><c>$84:8032-8131</c>, native <c>GoldenTorizo_HealthBasedPalette_Handling.palette1</c>: eight sixteen-color body health bands from low to high health, copied to OBJ palette 1; the runtime clamps the highest range to band seven.</summary>
    GoldenTorizoBody,
    /// <summary><c>$84:8132-8231</c>, native <c>GoldenTorizo_HealthBasedPalette_Handling.palette2</c>: eight sixteen-color belly health bands paired with the body bands, copied to OBJ palette 2 using the same runtime health selector.</summary>
    GoldenTorizoBelly,
}
