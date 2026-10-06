using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresDestructionActorDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        Suite(nameof(VerifyCeresInitialActorMetadata), () => VerifyCeresInitialActorMetadata(retail));

        Suite(nameof(VerifyCeresZebesActorMetadata), () => VerifyCeresZebesActorMetadata(retail));

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDestructionActorDefinitions.InitialActor(
                CeresDestructionActorDefinitions.InitialActorCount),
            "destruction initial actor definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDestructionActorDefinitions.ZebesActor(
                CeresDestructionActorDefinitions.ZebesActorCount),
            "Zebes reveal actor definition boundary");

        Suite(nameof(VerifyCeresBackdropPrograms), () => VerifyCeresBackdropPrograms(retail));
        Suite(nameof(VerifyCeresInitialExplosionProgram), () => VerifyCeresInitialExplosionProgram(retail));
        Suite(nameof(VerifyCeresRepeatingExplosionProgram), () => VerifyCeresRepeatingExplosionProgram(retail));
        Suite(nameof(VerifyCeresFinalWaveProgram), () => VerifyCeresFinalWaveProgram(retail));
        Suite(nameof(VerifyCeresStationBlastProgram), () => VerifyCeresStationBlastProgram(retail));
        AssertThrows<InvalidDataException>(() =>
            CeresDestructionSpriteInstructionDefinitions.ReadWord(
                CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd - 1),
            "Ceres explosion reader cannot cross between adjacent lists");
        AssertThrows<InvalidDataException>(() =>
            CeresDestructionSpriteInstructionDefinitions.ReadWord(
                CeresDestructionSpriteInstructionDefinitions.StarSheetsStart + 7),
            "Ceres star-sheet reader cannot cross into the next quadrant");
        Suite(nameof(VerifyCeresRearPrograms), () => VerifyCeresRearPrograms(retail));

        var guard = new CeresDestructionActorDefinitionReadGuard(retail);
        var state = CreateRetailDestructionFixture(guard);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished,
            "Ceres destruction and Zebes reveal complete through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres destruction never rereads compiled actor metadata or instruction lists");

        Console.WriteLine(
            "  Ceres destruction actors: 65 metadata words and 214 list bytes match ROM; all thirteen consumed lists and the full station/Zebes production sequence pass with source reads forbidden.");

    }

    private sealed class CeresDestructionActorDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ceres destruction reread compiled scene actor byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address) =>
            address is >= 0x8bbf23 and < 0x8bbf25 or
                >= 0x8bbf29 and < 0x8bbf2b or
                >= 0x8bbf2f and < 0x8bbf31 or
                >= 0x8bbf3a and < 0x8bbf3c or
                >= 0x8bbf46 and < 0x8bbf48 or
                >= 0x8bbf77 and < 0x8bbf79 or
                >= 0x8bbf7d and < 0x8bbf7f or
                >= 0x8bbf83 and < 0x8bbf85 or
                >= 0x8bbf8e and < 0x8bbf90 or
                >= 0x8bbf9a and < 0x8bbf9c or
                >= 0x8bbfa6 and < 0x8bbfa8 or
                >= 0x8bbfac and < 0x8bbfae or
                >= 0x8bbfba and < 0x8bbfbc or
                >= 0x8bbfc0 and < 0x8bbfc2 or
                >= 0x8bc83c and < 0x8bc83e or
                >= 0x8bc842 and < 0x8bc844 or
                >= 0x8bc848 and < 0x8bc84a or
                >= 0x8bc857 and < 0x8bc859 or
                >= 0x8bc862 and < 0x8bc864 or
                >= 0x8bc8b3 and < 0x8bc8b5 or
                >= 0x8bc8be and < 0x8bc8c0 or
                >= 0x8bc902 and < 0x8bc904 or
                >= 0x8bc90d and < 0x8bc90f or
                >= 0x8bc944 and < 0x8bc946 or
                >= 0x8bc94a and < 0x8bc94c or
                >= 0x8bc950 and < 0x8bc952 or
                >= 0x8bc958 and < 0x8bc95a or
                >= 0x8bc95e and < 0x8bc960 or
                >= 0x8bc964 and < 0x8bc966 or
                >= 0x8bc96c and < 0x8bc96e or
                >= 0x8bc972 and < 0x8bc974 or
                >= 0x8bc978 and < 0x8bc97a or
                >= 0x8bc980 and < 0x8bc982 or
                >= 0x8bc986 and < 0x8bc988 or
                >= 0x8bc98c and < 0x8bc98e or
                >= 0x8bc993 and < 0x8bc995 or
                >= 0x8bc999 and < 0x8bc99b or
                >= 0x8bc99f and < 0x8bc9a1 or
                >= 0x8bcc3f and < 0x8bcc47 or
                >= 0x8bcc4f and < 0x8bcc63 or
                >= 0x8bccab and < 0x8bccb3 or
                >= 0x8bccbb and < 0x8bccd5 or
                >= 0x8bccdb and < 0x8bcd39 or
                >= 0x8bcd83 and < 0x8bcda3 or
                >= 0x8bce1b and < 0x8bce35 or
                >= 0x8bce7f and < 0x8bce85 or
                >= 0x8bce8b and < 0x8bce97 or
                >= 0x8bcea3 and < 0x8bcea9 or
                >= 0x8bceaf and < 0x8bceb5 or
                >= 0x8bcef7 and < 0x8bcf0f;
    }
}
