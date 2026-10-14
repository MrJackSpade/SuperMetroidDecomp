namespace SuperMetroid.Core.Assets;

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

    /// <summary>Four imported palette families in stable face-block, corpse, body, and belly order.</summary>
    internal static DefinitionSet All { get; } = new();

    /// <summary>Read-only ordered view of the four auxiliary palette source definitions.</summary>
    internal sealed class DefinitionSet : IReadOnlyList<EnemyAuxiliaryPaletteDefinition>
    {
        /// <summary>Face block, dead Sidehopper, and two Golden Torizo palette families.</summary>
        public int Count => 4;
        /// <summary>Alias for the fixed family count used by definition consumers.</summary>
        public int Length => Count;
        /// <summary>Maps a bounded family ordinal to its native source and dimensions.</summary>
        public EnemyAuxiliaryPaletteDefinition this[int index] => index switch
        {
            0 => FaceBlock,
            1 => DeadSidehopper,
            2 => GoldenTorizoBody,
            3 => GoldenTorizoBelly,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Yields the four native palette source definitions in stable order.</summary>
        public IEnumerator<EnemyAuxiliaryPaletteDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same family order through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
