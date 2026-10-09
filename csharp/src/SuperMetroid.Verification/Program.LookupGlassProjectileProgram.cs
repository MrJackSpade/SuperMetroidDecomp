using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled mechanics-word addresses against the native glass-projectile programs.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassMechanicsAddresses(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 0);
    /// <summary>Checks calculated presentation-word addresses against artwork operands in the native programs.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassPresentationAddresses(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 1);
    /// <summary>Compares replacement shard-animation durations with values parsed from the cartridge programs.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassShardCadence(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 2);
    /// <summary>Checks the cartridge cadence for the sparkle program's presentation frames.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassSparkleCadence(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 3);
    /// <summary>Checks the native projectile control operands against the compiled glass definitions.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassProjectileControls(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 4);
    /// <summary>Checks that each parsed native projectile loop points to the expected instruction target.</summary>
    /// <param name="rom">Cartridge address space containing the original projectile programs.</param>
    private static void VerifyGlassProjectileLoopTargets(SuperMetroidAddressSpace rom) => VerifyGlassProjectileField(rom, 5);

    /// <summary>Parses bounded native glass-projectile programs and compares the selected data category with compiled definitions.</summary>
    /// <param name="rom">Cartridge address space used to read the original instruction words.</param>
    /// <param name="field">Category to verify: mechanics addresses, presentation addresses, shard cadence, sparkle cadence, control operands, or loop targets.</param>
    private static void VerifyGlassProjectileField(SuperMetroidAddressSpace rom, int field)
    {
        // Parse the original bounded programs; never obtain expected addresses or values
        // from the replacement's enumeration or cadence calculations.
        int[] starts = [0xcc93,0xccb7,0xccdb,0xccff,0xcd23,0xcd47,0xcd6b,0xcd8f,0xcdb3];
        var words = new List<(ushort Address, ushort Value, int Field)>();
        var art = new List<ushort>();
        foreach (int start in starts)
        {
            int cursor = start;
            for (int guard = 0; guard < 10; guard++)
            {
                ushort word = ReadMotherBrainGlassShardWord(rom, 0x860000 | cursor);
                words.Add(((ushort)cursor, word, word >= 0x8000 ? 4 : start == 0xcdb3 ? 3 : 2));
                if (word == 0x81ab) // Native goto opcode, followed by its self target.
                {
                    words.Add(((ushort)(cursor + 2), ReadMotherBrainGlassShardWord(rom, 0x860000 | (cursor + 2)), 5));
                    break;
                }
                if (word == 0x8154) break; // Native deletion opcode.
                AssertTrue(word < 0x8000, "Glass native parser recognizes only duration/goto/delete");
                art.Add((ushort)(cursor + 2));
                cursor += 4;
                AssertTrue(guard < 9, "Glass native program terminates");
            }
        }
        AssertEqual(85, words.Count, "Glass native mechanics count");
        AssertEqual(68, art.Count, "Glass native presentation count");
        if (field == 0)
        {
            AssertEqual(words.Count, MotherBrainGlassInstructionProgramDefinitionsTooling.MechanicsWordCount, "Glass calculated mechanics count");
            for (int index = 0; index < words.Count; index++)
                AssertEqual(words[index].Address, MotherBrainGlassInstructionProgramDefinitionsTooling.MechanicsWord(index).Address,
                    "Glass original ordered mechanics address");
            foreach (int bad in new[] {int.MinValue,-1,85,int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => MotherBrainGlassInstructionProgramDefinitionsTooling.MechanicsWord(bad), "Glass mechanics index bounds");
            var addresses = words.Select(word => word.Address).ToHashSet();
            var bytes = words.SelectMany(word => new[] {(int)word.Address,word.Address+1}).ToHashSet();
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort address = (ushort)raw;
                if (!addresses.Contains(address))
                    AssertThrows<InvalidDataException>(() => MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord(address), "Glass rejects nonmechanics word starts");
                foreach (int bank in new[] {0x860000,0x01860000,0x850000,0,unchecked((int)0xff860000)})
                    AssertEqual((bank & 0xff0000) == 0x860000 && bytes.Contains(raw),
                        MotherBrainGlassInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(bank | raw), "Glass byte ownership and bank-mask aliases");
            }
        }
        else if (field == 1)
        {
            AssertEqual(art.Count, MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount, "Glass calculated art count");
            for (int index = 0; index < art.Count; index++)
                AssertEqual(art[index], MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress(index), "Glass original ordered art address");
            foreach (int bad in new[] {int.MinValue,-1,68,int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress(bad), "Glass art index bounds");
        }
        else
        {
            int count = 0;
            for (int index = 0; index < words.Count; index++)
            {
                var expected = words[index];
                if (expected.Field != field) continue;
                count++;
                AssertEqual(expected.Value, MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord(expected.Address), "Glass native mechanics value");
                AssertEqual(expected.Value, MotherBrainGlassInstructionProgramDefinitionsTooling.MechanicsWord(index).Value, "Glass native enumerated value");
            }
            AssertEqual(field switch {2 => 64,3 => 4,4 => 9,_ => 8}, count, "Glass independent field count");
        }
    }
}
