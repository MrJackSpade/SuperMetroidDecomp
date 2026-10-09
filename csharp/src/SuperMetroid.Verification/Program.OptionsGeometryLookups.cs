using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares all primary-menu cursor Y positions with their native words and checks lookup bounds.</summary>
    private static void VerifyOptionsPrimaryCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f309, 4, 5,
            GameOptionsRomData.Cursors.PrimaryY, "primary cursor Y"));

    /// <summary>Compares all controller-menu cursor Y positions with their native words and checks lookup bounds.</summary>
    private static void VerifyOptionsControllerCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f31d, 4, 9,
            GameOptionsRomData.Cursors.ControllerY, "controller cursor Y"));

    /// <summary>Compares all special-menu cursor Y positions with their native words and checks lookup bounds.</summary>
    private static void VerifyOptionsSpecialCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f341, 4, 3,
            GameOptionsRomData.Cursors.SpecialY, "special cursor Y"));

    /// <summary>Compares controller-label destination offsets with their native words and checks lookup bounds.</summary>
    private static void VerifyOptionsLabelDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f639, 2, 7,
            GameOptionsRomData.ControllerLabels.Destination, "controller label destination"));

    /// <summary>Compares controller-label source offsets with their native words and checks lookup bounds.</summary>
    private static void VerifyOptionsLabelSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f647, 2, 7,
            GameOptionsRomData.ControllerLabels.Source, "controller label source"));

    /// <summary>Checks each selected geometry lookup against native words and verifies representative invalid indices.</summary>
    /// <param name="rom">ROM address space containing the native geometry table.</param>
    /// <param name="address">Address of the first native word in the selected table.</param>
    /// <param name="stride">Byte distance between successive words.</param>
    /// <param name="count">Number of supported entries in the table.</param>
    /// <param name="actual">Lookup returning the compiled value for an index.</param>
    /// <param name="name">Table label used to identify assertion failures.</param>
    private static void VerifyOptionsGeometryWords(ISnesAddressSpace rom, int address,
        int stride, int count, Func<int, ushort> actual, string name)
    {
        for (int index = 0; index < count; index++)
            AssertEqual(ReadVerificationWord(rom, address + stride * index), actual(index),
                $"{name} native word {index}");
        foreach (int index in new[] { int.MinValue, -1, count, count + 1, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => actual(index),
                $"{name} rejects {index}");
    }
}
