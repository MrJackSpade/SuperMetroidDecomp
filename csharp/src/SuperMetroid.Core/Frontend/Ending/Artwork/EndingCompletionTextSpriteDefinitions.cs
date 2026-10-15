namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The OAM record identities paired with the fourteen fixed bank-$8B ending
/// completion-text lists. Counts preserve the cartridge's progressively revealed
/// two-part-per-letter compositions without treating those visuals as timing data.
/// </summary>
public static class EndingCompletionTextSpriteDefinitions
{
    /// <summary>$8C:A69D, first THE OPERATION WAS prefix.</summary>
    internal const ushort OperationMap = 0xa69d;
    /// <summary>$8C:AB6B, first COMPLETED SUCCESSFULLY prefix.</summary>
    internal const ushort CompletedMap = 0xab6b;
    /// <summary>$8C:B49B, first CLEAR TIME prefix.</summary>
    internal const ushort ClearMap = 0xb49b;
    /// <summary>$8C:B67B, first of ten two-part digit frames.</summary>
    internal const ushort ZeroMap = 0xb67b;
    /// <summary>$8C:B66F, the two-part colon frame.</summary>
    internal const ushort ColonMap = 0xb66f;
    /// <summary>Progressive THE OPERATION WAS prefix stages in the native typewriter list.</summary>
    private const int OperationLetters = 15;
    /// <summary>Progressive COMPLETED SUCCESSFULLY prefix stages.</summary>
    private const int CompletedLetters = 21;
    /// <summary>Progressive CLEAR TIME prefix stages.</summary>
    private const int ClearLetters = 9;
    /// <summary>Total phrase-prefix compositions before digit and colon frames.</summary>
    private const int PrefixCount = OperationLetters + CompletedLetters + ClearLetters;
    /// <summary>Phrase prefixes plus ten decimal digits and one colon.</summary>
    private const int FrameCount = PrefixCount + 11;

    /// <summary>Stable 56-frame catalog: 15 THE OPERATION WAS prefixes at $8C:A69D, 21 COMPLETED SUCCESSFULLY prefixes at $AB6B, nine CLEAR TIME prefixes at $B49B, then ten digits at $B67B and the colon at $B66F.</summary>
    public static IReadOnlyList<EndingCompletionTextSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Sum preceding records: two header bytes and two five-byte OAM parts
    /// per revealed letter. Shared with the generated typewriter instructions.</summary>
    internal static ushort PrefixMap(ushort first, int prefix) => (ushort)(first + 2 * prefix + 5 * prefix * (prefix + 1));
    /// <summary>Advances from the zero digit by one two-part OAM record per decimal digit.</summary>
    internal static ushort DigitMap(int digit) => (ushort)(ZeroMap + digit * (2 + 2 * 5));

    /// <summary>Projects a catalog ordinal into a phrase stage, decimal digit, or colon composition.</summary>
    private static EndingCompletionTextSpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < OperationLetters) return Prefix("operation", OperationMap, index);
        if (index < OperationLetters + CompletedLetters) return Prefix("completed", CompletedMap, index - OperationLetters);
        if (index < PrefixCount) return Prefix("clear-time", ClearMap, index - OperationLetters - CompletedLetters);
        int glyph = index - PrefixCount;
        return glyph == 10 ? new("colon-00", ColonMap, 2) : new(Key("digit", glyph), DigitMap(glyph), 2);
    }

    /// <summary>Names one progressively revealed phrase stage and calculates its native OAM part count.</summary>
    private static EndingCompletionTextSpriteFrameDefinition Prefix(string family, ushort first, int stage) =>
        new(Key(family, stage), PrefixMap(first, stage), 2 * (stage + 1));
    /// <summary>Formats stable two-digit JSON keys for phrase stages and decimal digits.</summary>
    private static string Key(string family, int stage) => family + "-" + stage.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Calculates frame definitions on demand instead of storing fifty-six duplicate records.</summary>
    private sealed class FrameView : IReadOnlyList<EndingCompletionTextSpriteFrameDefinition>
    {
        /// <summary>Phrase prefixes, digits, and colon included in the installed catalog.</summary>
        public int Count => FrameCount;
        /// <summary>Resolves a native frame pointer and stock part count for one catalog ordinal.</summary>
        public EndingCompletionTextSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Visits all completion-text frames in native presentation order.</summary>
        public IEnumerator<EndingCompletionTextSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Exposes the same frame order through the non-generic interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
