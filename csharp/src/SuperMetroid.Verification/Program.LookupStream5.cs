using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream5Initialization(ISnesAddressSpace rom)
    {
        for (ushort variant = 0; variant < 7; variant++)
        {
            var actual = CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), actual.InstructionList, "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), actual.MainFunction, "Ceres door original function dispatch");
        }
        for (ushort variant = 0; variant < 6; variant++)
        {
            var actual = CeresSteamDefinitions.Initialization((CeresSteamVariant)variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), actual.InstructionList, "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), (ushort)actual.Function, "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdollitePhaseDefinitions.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), actual.BodyInstructionList, "Magdollite original body program");
            AssertEqual(ReadVerificationWord(rom, 0xa8af79 + 2 * index), actual.OverlayYOffset, "Magdollite original overlay offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(7), "Ceres door upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(ushort.MaxValue), "Ceres door full-word rejection");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)6), "Ceres steam upper bound");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)ushort.MaxValue), "Ceres steam full-word rejection");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(-1), "Magdollite lower bound");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(9), "Magdollite upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[-1]; }, "Ceres DMA list lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[19]; }, "Ceres DMA list upper bound");
        Console.WriteLine("Stream 5 initialization: all 53 native selector/phase words and rejected domains pass.");
    }
    private static void VerifyLookupStream5PaletteEntries(ISnesAddressSpace rom)
    {
        CheckEntries(CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 3);
        CheckEntries(CinematicGlowPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 4);
        int lightWords = 0, glowWords = 0, redWords = 0, greyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort light))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), light, "Ceres light original mechanics");
                lightWords++;
            }
            if (CinematicGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort glow))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), glow, "Cinematic glow original mechanics");
                glowWords++;
            }
            if (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort red))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), red, "Tourian red-flash original mechanics");
                redWords++;
            }
            if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort grey))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), grey, "Tourian statue original mechanics");
                greyWords++;
            }
        }
        AssertEqual(44, lightWords, "Ceres light mechanics coverage");
        AssertEqual(64, glowWords, "Cinematic glow mechanics coverage");
        AssertEqual(50, redWords, "Tourian shared red mechanics coverage");
        AssertEqual(31, greyWords, "Tourian statue mechanics coverage");
        Console.WriteLine("Stream 5 palette entries: all eleven entry identities, original controls and collection bounds pass.");

        void CheckEntries<T>(IReadOnlyList<T> entries, Func<T, (ushort Definition, ushort Program)> project, int expectedCount)
        {
            AssertEqual(expectedCount, entries.Count, "Palette entry count");
            int index = 0;
            foreach (T entry in entries)
            {
                var identity = project(entry);
                AssertEqual(identity, project(entries[index]), "Palette entry enumeration order");
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | identity.Definition + 2), identity.Program, "Original palette definition list pointer");
                index++;
            }
            AssertEqual(expectedCount, index, "All palette entries enumerated");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[-1]; }, "Palette entries lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[expectedCount]; }, "Palette entries upper bound");
        }
    }}