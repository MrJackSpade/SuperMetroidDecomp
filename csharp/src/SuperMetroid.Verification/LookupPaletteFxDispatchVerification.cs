using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static RoomPaletteFxDefinitionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Fx;

/// <summary>Checks palette-FX object identity and dispatch fields against bank $8D.</summary>
internal static class LookupPaletteFxDispatchVerification
{
    /// <summary>Enumerates the three declared bank $8D object regions in four-byte records.</summary>
    private static IEnumerable<ushort> OriginalPaletteFxObjects()
    {
        // Independent physical regions from pinned bank8D object declarations.
        foreach ((int first, int last) in new[] { (0xe194, 0xe200), (0xf745, 0xf7a5), (0xffc9, 0xffed) })
            for (int pointer = first; pointer <= last; pointer += 4)
                yield return (ushort)pointer;
    }

    /// <summary>Accepts exactly 63 declared object pointers and rejects every other 16-bit value.</summary>
    internal static void VerifyPaletteFxDispatchDomain()
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

    /// <summary>Compares each compiled setup callback with the first word of its retail object record.</summary>
    internal static void VerifyPaletteFxSetupSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer),
                (ushort)RoomPaletteFxDefinitions.Get(pointer).SetupCallback, "Native palette-FX setup callback");
    }

    /// <summary>Compares each compiled initial instruction pointer with the record's second word.</summary>
    internal static void VerifyPaletteFxInitialListSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, (0x8d0000 | pointer) + 2),
                RoomPaletteFxDefinitions.Get(pointer).InitialInstructionList, "Native palette-FX initial program");
    }

    /// <summary>Runs identity, setup, list, and spawn checks while guarding compiled definition bytes.</summary>
    internal static void VerifyPaletteFxDispatch(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        var forbidden = OriginalPaletteFxObjects().SelectMany(pointer =>
            Enumerable.Range(0x8d0000 | pointer, 4)).ToHashSet();
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(NewRoomPaletteFxDefinitionGuard(rom, forbidden),
            OriginalPaletteFxObjects()));
    }
}
