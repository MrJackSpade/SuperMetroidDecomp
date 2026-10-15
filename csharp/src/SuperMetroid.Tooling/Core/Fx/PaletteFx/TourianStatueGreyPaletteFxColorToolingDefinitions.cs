using SuperMetroid.Core.Areas.Tourian;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool members of <see cref="TourianStatueGreyPaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class TourianStatueGreyPaletteFxColorToolingDefinitions
{
    /// <summary>The Draygon, Kraid, Ridley, and Phantoon entries in cartridge order.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<TourianStatueGreyPaletteFxColorDefinition> All =>
        TourianStatueGreyPaletteFxColorToolingDefinitions.Definitions;
    /// <summary>$8D:F749: first of four consecutive statue palette-FX definitions.</summary>
    internal const ushort FirstDefinition = 0xf749;
    /// <summary>$8D:E222: first color-index/goto entry; the final entry falls through.</summary>
    internal const ushort FirstProgram = 0xe222;
    /// <summary>CGRAM byte $C0: first statue palette, with palette row eight skipped.</summary>
    internal const ushort FirstColorByte = 0xc0;
    /// <summary>Four boss-specific entries sharing the eight-frame grey-out program.</summary>
    internal static readonly IReadOnlyList<TourianStatueGreyPaletteFxColorDefinition> Definitions = new ProgramEntries();
    /// <summary>Computes consecutive definition and program addresses without storing duplicate records.</summary>
    internal sealed class ProgramEntries : IReadOnlyList<TourianStatueGreyPaletteFxColorDefinition>
    {
        /// <summary>Draygon, Kraid, Ridley, and Phantoon each have one setup entry.</summary>
        public int Count => 4;
        /// <summary>Maps a boss index to its definition, instruction start, and CGRAM row.</summary>
        public TourianStatueGreyPaletteFxColorDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return new((TourianStatueBoss)index,
                    (ushort)(FirstDefinition + 4 * index),
                    (ushort)(FirstProgram + 8 * index),
                    (ushort)(FirstColorByte + 32 * (index + (index >= 2 ? 1 : 0))),
                    usesGoto: index < Count - 1);
            }
        }
        /// <summary>Yields the four boss entries in native table order.</summary>
        public IEnumerator<TourianStatueGreyPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same ordered entries to non-generic collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
