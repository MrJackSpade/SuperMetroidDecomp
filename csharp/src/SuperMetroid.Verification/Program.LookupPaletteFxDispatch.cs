using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Enumerates the 63 native bank-$8D palette-FX object pointers in their three declared physical regions.</summary>
    private static IEnumerable<ushort> OriginalPaletteFxObjects()
    {
        // Independent physical regions from pinned bank8D object declarations.
        foreach ((int first, int last) in new[] { (0xe194, 0xe200), (0xf745, 0xf7a5), (0xffc9, 0xffed) })
            for (int pointer = first; pointer <= last; pointer += 4)
                yield return (ushort)pointer;
    }

    /// <summary>Checks that palette-FX lookup accepts every native object pointer and rejects all other 16-bit values.</summary>
    private static void VerifyPaletteFxDispatchDomain()
    {
        var original = OriginalPaletteFxObjects().ToHashSet();
        AssertEqual(63, original.Count, "Native palette-FX identity count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (original.Contains(pointer))
                _ = RoomPaletteFxDefinitions.Get(pointer);
            else
                AssertThrows<InvalidDataException>(() => RoomPaletteFxDefinitions.Get(pointer),
                    "Unknown/interior palette-FX identity rejects");
        }
    }

    /// <summary>Compares each compiled setup callback with the word stored in its native bank-$8D object header.</summary>
    /// <param name="rom">ROM address space containing the palette-FX object headers.</param>
    private static void VerifyPaletteFxSetupSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer),
                RoomPaletteFxDefinitions.Get(pointer).SetupCallback, "Native palette-FX setup callback");
    }

    /// <summary>Compares each compiled initial instruction-list pointer with the second word of its native object header.</summary>
    /// <param name="rom">ROM address space containing the palette-FX object headers.</param>
    private static void VerifyPaletteFxInitialListSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, (0x8d0000 | pointer) + 2),
                RoomPaletteFxDefinitions.Get(pointer).InitialInstructionList, "Native palette-FX initial program");
    }

    /// <summary>Runs domain and header checks, then verifies dispatch-created effects without allowing reads of compiled definitions.</summary>
    /// <param name="rom">ROM address space used for native header comparisons and guarded effect dispatch.</param>
    private static void VerifyPaletteFxDispatch(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        var forbidden = OriginalPaletteFxObjects().SelectMany(pointer =>
            Enumerable.Range(0x8d0000 | pointer, 4)).ToHashSet();
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(new RoomPaletteFxDefinitionReadGuard(rom, forbidden),
            OriginalPaletteFxObjects()));
    }
}
