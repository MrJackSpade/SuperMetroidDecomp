namespace SuperMetroid.Core.Assets;

/// <summary>Bank-$93 identities for the charge/Hyper flare and its directional sparks.</summary>
public static class ChargeFlareSpriteDefinitions
{
    /// <summary>Installed JSON resource filename for charge/Hyper/grapple flare and spark OAM compositions, separate from ordinary projectile compositions.</summary>
    public const string FileName = "charge-flare-compositions.json";
    /// <summary>$93:A225/A22B, FlareSpritemapTable_IndexOffsets: both facing rows select the main flare at offset zero.</summary>
    public const ushort MainFlareSelectorOffset = 0;
    /// <summary>$93:AB6C, ProjectileFlareSpritemaps_Flare_Charge_Hyper_Grapple_0.</summary>
    public const ushort ProjectileFlareSpritemaps_Flare_Charge_Hyper_Grapple_0 = 0xAB6C;
    /// <summary>$93:A6FD, ProjectileFlareSpritemaps_FlareSlowSparks_FacingRight_0.</summary>
    public const ushort ProjectileFlareSpritemaps_FlareSlowSparks_FacingRight_0 = 0xA6FD;
    /// <summary>$93:A8DE, ProjectileFlareSpritemaps_FlareSlowSparks_FacingLeft_0.</summary>
    public const ushort ProjectileFlareSpritemaps_FlareSlowSparks_FacingLeft_0 = 0xA8DE;

    /// <summary>$93:A1A1: thirty main-flare phases followed by slow/fast right/left sparks.</summary>
    public static SelectorSequence Selectors => default;
    /// <summary>Three one-object main flares followed by the four-object crest, plus four runs of six three-object sparks.</summary>
    public static NativePointerSequence NativePointers => default;

    /// <summary>Allocation-free view of the $93:A1A1 selector table: repeated main-flare phases followed by slow/fast right-facing and left-facing spark phases.</summary>
    public readonly struct SelectorSequence : IReadOnlyList<ushort>
    {
        /// <summary>Fifty-four selector positions, comprising thirty main-flare entries and twenty-four spark entries; repeated pointers remain separate phase selections.</summary>
        public int Count => 54;
        /// <summary>Selector-position count, equivalent to <see cref="Count"/>, used to bound drawing requests.</summary>
        public int Length => Count;
        /// <summary>Returns the bank-$93 composition selected by one native flare-table position without consulting cartridge memory.</summary>
        /// <param name="index">Zero-based selector ordinal 0..53, not a byte offset: 0..29 main flare, then six phases each of right slow, right fast, left slow, and left fast sparks.</param>
        /// <returns>Bank-relative spritemap pointer, with main-flare phase repetitions preserved.</returns>
        /// <exception cref="IndexOutOfRangeException">The selector ordinal is outside 0..53.</exception>
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index >= 30) return NativePointers[index - 26];
                int phase = index < 8 ? index % 2 : index < 16 ? 2 - index % 2 : 2 + index % 2;
                return NativePointers[phase];
            }
        }
        /// <summary>Enumerates all selector positions in native table order, including repeated main-flare pointers.</summary>
        /// <returns>An enumerator yielding fifty-four bank-relative spritemap pointers.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Allocation-free view of the twenty-eight distinct flare/spark compositions required by the editable artwork catalog, independent of selector repetition.</summary>
    public readonly struct NativePointerSequence : IReadOnlyList<ushort>
    {
        /// <summary>Twenty-eight distinct compositions: four main-flare maps and four six-phase spark groups.</summary>
        public int Count => 28;
        /// <summary>Returns one required composition identity in main-flare, right-spark, then left-spark group order.</summary>
        /// <param name="index">Zero-based identity index 0..27: four main flares, then right slow/fast and left slow/fast groups of six.</param>
        /// <returns>Bank-relative spritemap pointer in bank $93, calculated from its native two-byte header and five-byte OAM-part layout.</returns>
        /// <exception cref="IndexOutOfRangeException">The identity index is outside 0..27.</exception>
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index < 4) return (ushort)(ProjectileFlareSpritemaps_Flare_Charge_Hyper_Grapple_0 + index * 7);
                int spark = index - 4;
                ushort first = spark < 12 ? ProjectileFlareSpritemaps_FlareSlowSparks_FacingRight_0 :
                    ProjectileFlareSpritemaps_FlareSlowSparks_FacingLeft_0;
                return (ushort)(first + spark % 12 * 17);
            }
        }
        /// <summary>Enumerates each distinct required flare/spark composition once in catalog group order.</summary>
        /// <returns>An enumerator yielding twenty-eight bank-relative spritemap pointers.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
