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

/// <summary>Import provenance and bounded dimensions; runtime AI owns cadence and frame selection.</summary>
internal static class EnemyAuxiliaryColorDefinitions
{
    /// <summary>Blue Brinstar face-block four-color cycle at $A8:E7CC, eight frames.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition FaceBlock = new(
        EnemyAuxiliaryPalette.FaceBlock, 0xa8e7cc, 8, 4, 4);
    /// <summary>Dead sidehopper's seven stages at $A9:EBCC; each native sixteen-color row supplies fifteen colors.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition DeadSidehopper = new(
        EnemyAuxiliaryPalette.DeadSidehopper, 0xa9ebcc, 7, 15, 16);
    /// <summary>Golden Torizo body health bands at $84:8032, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBody = new(
        EnemyAuxiliaryPalette.GoldenTorizoBody, 0x848032, 8, 16, 16);
    /// <summary>Golden Torizo belly health bands at $84:8132, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBelly = new(
        EnemyAuxiliaryPalette.GoldenTorizoBelly, 0x848132, 8, 16, 16);

    /// <summary>Provides the four auxiliary palette families in stable catalog order.</summary>
    internal static DefinitionSet All { get; } = new();

    /// <summary>Read-only indexed view over the fixed auxiliary palette catalog.</summary>
    internal sealed class DefinitionSet : IReadOnlyList<EnemyAuxiliaryPaletteDefinition>
    {
        /// <summary>Number of palette definitions exposed by this catalog.</summary>
        public int Count => 4;

        /// <summary>Number of palette definitions, also available through <see cref="Count"/>.</summary>
        public int Length => Count;

        /// <summary>Gets the palette definition at its catalog index.</summary>
        /// <param name="index">Zero-based position in the order FaceBlock, DeadSidehopper, GoldenTorizoBody, GoldenTorizoBelly.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the four catalog entries.</exception>
        public EnemyAuxiliaryPaletteDefinition this[int index] => index switch
        {
            0 => FaceBlock,
            1 => DeadSidehopper,
            2 => GoldenTorizoBody,
            3 => GoldenTorizoBelly,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Enumerates all four palette definitions in the same order used by the indexer.</summary>
        /// <returns>An enumerator over the fixed catalog entries.</returns>
        public IEnumerator<EnemyAuxiliaryPaletteDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Describes the cartridge source and row dimensions needed to import one auxiliary palette family.</summary>
/// <param name="Id">Palette family identity used to select the corresponding native consumer.</param>
/// <param name="SourceAddress">SNES address of the first native palette row.</param>
/// <param name="FrameCount">Number of source rows or animation stages in the family.</param>
/// <param name="ColorCount">Number of supplied RGB5 colors in each row.</param>
/// <param name="NativeFrameStrideColors">Distance in colors between successive native rows, including any omitted color zero.</param>
internal readonly record struct EnemyAuxiliaryPaletteDefinition(
    EnemyAuxiliaryPalette Id, int SourceAddress, int FrameCount, int ColorCount, int NativeFrameStrideColors);

/// <summary>Installed auxiliary-palette document identity, separate from its consumer.</summary>
public static class EnemyAuxiliaryColorFormat
{
    /// <summary>Asset filename for the four editable face-block, sidehopper, and Golden Torizo auxiliary palette families.</summary>
    public const string FileName = "enemy-auxiliary-colors.json";
    /// <summary>Supported revision of the complete four-family RGB5 row document; dimensions remain fixed by the native palette definitions.</summary>
    public const int Version = 1;
}
