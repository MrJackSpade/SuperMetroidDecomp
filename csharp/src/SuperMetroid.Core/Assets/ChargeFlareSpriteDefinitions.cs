namespace SuperMetroid.Core.Assets;

/// <summary>Bank-$93 identities for the charge/Hyper flare and its directional sparks.</summary>
public static class ChargeFlareSpriteDefinitions
{
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

    public readonly struct SelectorSequence : IReadOnlyList<ushort>
    {
        public int Count => 54;
        public int Length => Count;
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
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public readonly struct NativePointerSequence : IReadOnlyList<ushort>
    {
        public int Count => 28;
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
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
