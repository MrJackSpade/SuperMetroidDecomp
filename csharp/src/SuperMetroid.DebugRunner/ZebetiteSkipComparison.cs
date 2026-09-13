/// <summary>Strict numeric CSV comparison for #442; known mismatches still fail loudly.</summary>
internal static class ZebetiteSkipComparison
{
    public static int Run(string managedPath, string nativePath)
    {
        string[] managed = File.ReadAllLines(managedPath), native = File.ReadAllLines(nativePath);
        if (managed.Length != 41 || native.Length != 41 || managed[0] != native[0])
            throw new InvalidDataException("Expected matching #442 CSV headers and forty complete frames.");
        string[] fields = managed[0].Split(',');
        int differences = 0;
        for (int row = 1; row < managed.Length; row++)
        {
            string[] actual = managed[row].Split(','), expected = native[row].Split(',');
            if (actual.Length != fields.Length || expected.Length != fields.Length ||
                int.Parse(actual[0]) != 119 + row || int.Parse(expected[0]) != 119 + row)
                throw new InvalidDataException($"Malformed or noncontiguous #442 trace row {row}.");
            for (int field = 0; field < fields.Length; field++)
                if (uint.Parse(actual[field]) != uint.Parse(expected[field]))
                {
                    differences++;
                    Console.Error.WriteLine($"Frame {actual[0]} {fields[field]}: managed={actual[field]}, native={expected[field]}");
                }
        }
        if (differences != 0)
            throw new InvalidDataException($"#442 collision comparison has {differences} differing fields; parity is not established.");
        Console.WriteLine("All forty #442 collision frames match the native trace.");
        return 0;
    }
}
