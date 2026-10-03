using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyFileSelectMainNavigation()
    {
        // Independent original managed lists from b9e1e0ff, cross-checked against
        // native $81:A25E..A2AF. Preserve this former representation only as the oracle.
        foreach (bool hasSave in new[] { false, true })
        {
            int[] original = hasSave ? [0, 1, 2, 3, 4, 5] : [0, 1, 2, 5];
            for (int ordinal = 0; ordinal < original.Length; ordinal++)
            foreach (bool down in new[] { false, true })
            {
                int expected = original[(ordinal + (down ? 1 : -1) + original.Length) % original.Length];
                AssertEqual(expected, FileSelectMainNavigation.Move(original[ordinal], hasSave, down),
                    $"main navigation saves={hasSave} row={original[ordinal]} down={down}");
            }
            foreach (int selected in new[] { int.MinValue, -1, 3, 4, 6, int.MaxValue })
            {
                if (Array.IndexOf(original, selected) >= 0) continue;
                foreach (bool down in new[] { false, true })
                {
                    try
                    {
                        FileSelectMainNavigation.Move(selected, hasSave, down);
                        throw new InvalidOperationException("Invalid main selection was accepted.");
                    }
                    catch (InvalidDataException error)
                    {
                        AssertEqual($"File-select item {selected} is not currently visible.", error.Message,
                            "main navigation preserves rejection diagnostic");
                    }
                }
            }
        }
        Console.WriteLine("File-select navigation: all 20 original visible transitions and hidden/invalid selection rejection pass.");
    }
}
