using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresExplosionDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        VerifyActor(retail, CeresExplosionDefinitions.InitialActor, "initial explosion");
        VerifyActor(retail, CeresExplosionDefinitions.RepeatingActor, "repeating explosion");
        VerifyActor(retail, CeresExplosionDefinitions.FinalWaveActor, "final-wave explosion");
        VerifyActor(retail, CeresExplosionDefinitions.StationBlastActor, "station blast");
        VerifyActor(retail, CeresExplosionDefinitions.SpawnerActor, "explosion spawner");

        for (int index = 0; index < CeresExplosionDefinitions.InitialExplosionCount; index++)
        {
            CeresExplosionPlacement placement = CeresExplosionDefinitions.InitialExplosion(index);
            AssertEqual(ReadWord(retail, 0x8bc475 + index * 2), unchecked((ushort)placement.X),
                $"initial explosion {index} X offset");
            AssertEqual(ReadWord(retail, 0x8bc47f + index * 2), unchecked((ushort)placement.Y),
                $"initial explosion {index} Y offset");
            AssertEqual(ReadWord(retail, 0x8bc46b + index * 2), placement.DelayFrames,
                $"initial explosion {index} delay");
        }

        for (int index = 0; index < CeresExplosionDefinitions.RepeatingExplosionCount; index++)
        {
            CeresExplosionPlacement placement = CeresExplosionDefinitions.RepeatingExplosion(index);
            AssertEqual(ReadWord(retail, 0x8bc4eb + index * 4), unchecked((ushort)placement.X),
                $"repeating explosion {index} X offset");
            AssertEqual(ReadWord(retail, 0x8bc4ed + index * 4), unchecked((ushort)placement.Y),
                $"repeating explosion {index} Y offset");
            AssertEqual((ushort)1, placement.DelayFrames,
                $"repeating explosion {index} initial instruction delay");
        }

        for (int index = 0; index < CeresExplosionDefinitions.FinalExplosionCount; index++)
        {
            CeresExplosionPlacement placement = CeresExplosionDefinitions.FinalExplosion(index);
            AssertEqual(ReadWord(retail, 0x8bc572 + index * 2), unchecked((ushort)placement.X),
                $"final explosion {index} X offset");
            AssertEqual(ReadWord(retail, 0x8bc57a + index * 2), unchecked((ushort)placement.Y),
                $"final explosion {index} Y offset");
            AssertEqual(ReadWord(retail, 0x8bc56a + index * 2), placement.DelayFrames,
                $"final explosion {index} delay");
        }

        ushort[] spawnerProgram =
        [
            CeresExplosionDefinitions.InitialWaitFrames,
            0,
            CeresExplosionDefinitions.SpawnInitialWaveInstruction,
            CeresExplosionDefinitions.RepeatingStartWaitFrames,
            0,
            CinematicCodePointers.CinematicSpriteObject_Instruction_SetPreInstruction,
            CeresExplosionDefinitions.SpawnRepeatingWavePreInstruction,
            CeresExplosionDefinitions.RepeatingLifetimeFrames,
            0,
            CeresExplosionDefinitions.SpawnFinalWaveInstruction,
            CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
        ];
        for (int index = 0; index < spawnerProgram.Length; index++)
            AssertEqual(ReadWord(retail, 0x8bce35 + index * 2), spawnerProgram[index],
                $"Ceres explosion spawner word {index}");
        AssertEqual(CeresExplosionDefinitions.RepeatingPeriodFrames,
            ReadWord(retail, 0x8bc4a9),
            "Ceres repeating explosion cadence");

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.InitialExplosion(
                CeresExplosionDefinitions.InitialExplosionCount),
            "initial explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.RepeatingExplosion(
                CeresExplosionDefinitions.RepeatingExplosionCount),
            "repeating explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.FinalExplosion(
                CeresExplosionDefinitions.FinalExplosionCount),
            "final explosion definition boundary");

        var guard = new CeresExplosionDefinitionReadGuard(retail);
        var state = new CeresDestructionCinematicState(guard);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished, "Ceres destruction completes through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres destruction never rereads compiled explosion definitions");

        Console.WriteLine(
            "  Ceres explosion definitions: 70 native words and the complete production cinematic pass with actor, placement, timing and callback metadata reads forbidden.");

        static void VerifyActor(
            SuperMetroidAddressSpace source,
            CeresExplosionActorDefinition definition,
            string name)
        {
            int address = CeresExplosionDefinitions.NativeBank | definition.Pointer;
            AssertEqual(ReadWord(source, address), definition.Initialization,
                $"{name} initialization callback");
            AssertEqual(ReadWord(source, address + 2), definition.PreInstruction,
                $"{name} pre-instruction callback");
            AssertEqual(ReadWord(source, address + 4), definition.InstructionList,
                $"{name} instruction list");
        }

        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
    }

    private sealed class CeresExplosionDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ceres explosion reread compiled definition byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address) =>
            address is >= 0x8bc46b and < 0x8bc489 or
                >= 0x8bc4a9 and < 0x8bc4ab or
                >= 0x8bc4eb and < 0x8bc50b or
                >= 0x8bc56a and < 0x8bc582 or
                >= 0x8bce35 and < 0x8bce4b or
                >= 0x8bcebb and < 0x8bcecd or
                >= 0x8bcf2d and < 0x8bcf39;
    }
}
