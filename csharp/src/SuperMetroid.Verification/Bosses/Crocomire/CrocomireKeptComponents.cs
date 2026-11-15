namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Which of a Crocomire frame's native collision components his compiled frames keep (#1937): every component whose
/// hitbox list holds a hitbox, the first time the frame names it at its place. An empty list, and a list the frame
/// already named at the same place, can never be the first hitbox a probe meets, so the compiled frames leave them out.
/// </summary>
internal static class CrocomireKeptComponents
{
    /// <summary>
    /// For each native component of the frame whose record lies at <paramref name="root"/>, in order, the index of the
    /// compiled component it became, or null when the compiled frame leaves it out.
    /// </summary>
    /// <param name="readWord">Reads the native word at a long cartridge address.</param>
    /// <param name="root">The frame record's long address.</param>
    internal static int?[] Of(Func<int, ushort> readWord, int root)
    {
        int count = readWord(root) & byte.MaxValue;
        int bank = root & ~ushort.MaxValue;
        var kept = new int?[count];
        var seen = new HashSet<(ushort X, ushort Y, ushort List)>();
        int next = 0;
        for (int component = 0; component < count; component++)
        {
            int record = root + 2 + component * 8;
            (ushort X, ushort Y, ushort List) placed = (readWord(record), readWord(record + 2), readWord(record + 6));
            bool empty = readWord(bank | placed.List) == 0;
            kept[component] = empty || !seen.Add(placed) ? null : next++;
        }
        return kept;
    }
}
