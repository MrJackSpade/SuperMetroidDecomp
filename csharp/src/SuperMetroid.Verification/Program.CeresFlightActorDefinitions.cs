using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresFlightActorDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCeresFlightActorMetadata), () => VerifyCeresFlightActorMetadata(retail));

        AssertThrows<InvalidDataException>(() =>
            CeresFlightSpriteInstructionDefinitions.ReadWord(
                CeresFlightSpriteInstructionDefinitions.RearClusterEnd - 1),
            "Ceres flight instruction reader cannot cross into the next actor list");
        AssertThrows<InvalidDataException>(() =>
            CeresFlightSpriteInstructionDefinitions.ReadWord(
                CeresFlightSpriteInstructionDefinitions.StarsEnd),
            "Ceres flight instruction reader cannot enter the adjacent explosion list");

        Suite(nameof(VerifyCeresFlightPrograms), () => VerifyCeresFlightPrograms(retail));

        var guard = new CeresFlightActorDefinitionReadGuard(retail);
        var state = new IntroCeresFlightState(guard, RepositoryInstallation.Installation.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished, "Ceres approach completes through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres approach never rereads compiled actor definitions or animation lists");

        Console.WriteLine(
            "  Ceres flight actors: 43 metadata words and 44 instruction bytes match ROM; all six lists and the complete production approach pass with source reads forbidden.");

    }

    private sealed class CeresFlightActorDefinitionReadGuard(ISnesAddressSpace source) :
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
                    $"Ceres flight reread compiled actor byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address) =>
            address is >= 0x8bbe84 and < 0x8bbe86 or
                >= 0x8bbe8a and < 0x8bbe8c or
                >= 0x8bbe90 and < 0x8bbe92 or
                >= 0x8bbe96 and < 0x8bbe98 or
                >= 0x8bbe9d and < 0x8bbe9f or
                >= 0x8bbea3 and < 0x8bbea5 or
                >= 0x8bbea9 and < 0x8bbeab or
                >= 0x8bbeaf and < 0x8bbeb1 or
                >= 0x8bbec3 and < 0x8bbec5 or
                >= 0x8bbf23 and < 0x8bbf25 or
                >= 0x8bbf29 and < 0x8bbf2b or
                >= 0x8bbf2f and < 0x8bbf31 or
                >= 0x8bbf3a and < 0x8bbf3c or
                >= 0x8bbf46 and < 0x8bbf48 or
                >= 0x8bbf4d and < 0x8bbf4f or
                >= 0x8bbf53 and < 0x8bbf55 or
                >= 0x8bbf59 and < 0x8bbf5b or
                >= 0x8bbf64 and < 0x8bbf66 or
                >= 0x8bbf70 and < 0x8bbf72 or
                >= 0x8bbf77 and < 0x8bbf79 or
                >= 0x8bbf7d and < 0x8bbf7f or
                >= 0x8bbf83 and < 0x8bbf85 or
                >= 0x8bbf8e and < 0x8bbf90 or
                >= 0x8bbf9a and < 0x8bbf9c or
                >= 0x8bbfb4 and < 0x8bbfb6 or
                >= 0x8bbfba and < 0x8bbfbc or
                >= 0x8bbfc0 and < 0x8bbfc2 or
                >= 0x8bbfcb and < 0x8bbfcd or
                >= 0x8bcc47 and < 0x8bcc63 or
                >= 0x8bcda3 and < 0x8bcdab or
                >= 0x8bce4b and < 0x8bce53 or
                >= 0x8bce85 and < 0x8bce97 or
                >= 0x8bcf0f and < 0x8bcf15 or
                >= 0x8bcf39 and < 0x8bcf3f;
    }
}
