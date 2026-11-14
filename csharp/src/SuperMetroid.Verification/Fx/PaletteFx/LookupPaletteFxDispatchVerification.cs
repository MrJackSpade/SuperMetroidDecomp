using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.RoomPaletteFxDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>Checks palette-FX object identity and dispatch fields against bank $8D.</summary>
internal static class LookupPaletteFxDispatchVerification
{
    /// <summary>Enumerates the three declared bank $8D object regions in four-byte records.</summary>
    private static IEnumerable<ushort> OriginalPaletteFxObjects()
    {
        // Independent physical regions from pinned bank8D object declarations.
        foreach ((int first, int last) in new[] { new OriginalPaletteFxObjectsCase(0xe194, 0xe200), new OriginalPaletteFxObjectsCase(0xf745, 0xf7a5), new OriginalPaletteFxObjectsCase(0xffc9, 0xffed) })
            for (int pointer = first; pointer <= last; pointer += 4)
                if (!UnreachableObjects.Contains(pointer))
                    yield return (ushort)pointer;
    }

    /// <summary>
    /// The declared objects that are no effect of the game: the Nintendo boot logo's fade (#1686), the cinematic fade, the
    /// dark lightning, the Wrecked Ship lights' copy and the Tourian glow's copy, which no scene spawns and no room's record
    /// sets, and the empty object the unused area bits select, which shows nothing (#1827).
    /// </summary>
    private static readonly HashSet<int> UnreachableObjects = [0xe198, 0xe1ec, 0xf745, 0xf769, 0xf771, 0xf7a5];

    /// <summary>Accepts exactly the 57 declared object pointers that are effects and rejects every other 16-bit value.</summary>
    internal static void VerifyPaletteFxDispatchDomain()
    {
        var original = OriginalPaletteFxObjects().ToHashSet();
        AssertEqual(57, original.Count, "Native palette-FX identity count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (original.Contains(pointer))
                _ = ColourEffectSelection.Get(pointer);
            else
                AssertThrows<InvalidDataException>(() => ColourEffectSelection.Get(pointer),
                    "Unknown/interior palette-FX identity rejects");
        }
    }

    /// <summary>Compares how each compiled effect starts with the setup routine the first word of its retail object record names.</summary>
    internal static void VerifyPaletteFxSetupSelection(CartridgeImportAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(FxNativeWords.StartOf(FxNativeWords.PaletteFxSetups.Decode(ReadVerificationWord(rom, 0x8d0000 | pointer), "palette-FX setup")),
                ColourEffectSelection.Get(pointer).Start, "Native palette-FX setup");
    }

    /// <summary>Compares each compiled initial instruction pointer with the record's second word.</summary>
    internal static void VerifyPaletteFxInitialListSelection(CartridgeImportAddressSpace rom)
    {
        foreach (ushort pointer in OriginalPaletteFxObjects())
            AssertEqual(ReadVerificationWord(rom, (0x8d0000 | pointer) + 2),
                PaletteFxNativeListAddresses.Entry(ColourEffectSelection.Get(pointer).InitialAnimation), "Native palette-FX initial program");
    }

    /// <summary>Runs identity, setup, list, and spawn checks while guarding compiled definition bytes.</summary>
    internal static void VerifyPaletteFxDispatch(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        var forbidden = OriginalPaletteFxObjects().SelectMany(pointer =>
            Enumerable.Range(0x8d0000 | pointer, 4)).ToHashSet();
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(OriginalPaletteFxObjects()));
    }
}
