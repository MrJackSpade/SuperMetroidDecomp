namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Four distinct RGB5 animation sources; values are identities, not composable bits.</summary>
public enum EnemyAuxiliaryPalette
{
    /// <summary>Eight four-color glow rows for the Blue Brinstar face block, selected by its shared timed palette hook.</summary>
    FaceBlock,
    /// <summary>Six draining rows followed by corpse paint; each supplied row omits color zero and updates palette colours 145..159.</summary>
    DeadSidehopper,
    /// <summary>Eight sixteen-color body health bands from low to high health, copied to OBJ palette 1; the runtime clamps the highest range to band seven.</summary>
    GoldenTorizoBody,
    /// <summary>Eight sixteen-color belly health bands paired with the body bands, copied to OBJ palette 2 using the same runtime health selector.</summary>
    GoldenTorizoBelly,
}
