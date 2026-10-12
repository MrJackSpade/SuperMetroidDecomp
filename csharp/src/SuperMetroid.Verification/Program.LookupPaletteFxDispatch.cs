using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static RoomPaletteFxDefinitionsVerification;
using static SuiteTimingVerification;

internal static class LookupPaletteFxDispatchVerification
{
    private static IEnumerable<ushort> OriginalPaletteFxObjects()
    {
        // Independent physical regions from pinned bank8D object declarations.
        foreach ((int first, int last) in new[] { (0xe194, 0xe200), (0xf745, 0xf7a5), (0xffc9, 0xffed) })
            for (int pointer = first; pointer <= last; pointer += 4)
                yield return (ushort)pointer;
    }

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

    internal static void VerifyPaletteFxSetupSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer),
                (ushort)RoomPaletteFxDefinitions.Get(pointer).SetupCallback, "Native palette-FX setup callback");
    }

    internal static void VerifyPaletteFxInitialListSelection(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, (0x8d0000 | pointer) + 2),
                RoomPaletteFxDefinitions.Get(pointer).InitialInstructionList, "Native palette-FX initial program");
    }

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