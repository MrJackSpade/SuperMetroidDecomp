using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream4(ISnesAddressSpace rom)
    {
        VerifyLookupStream4Programs(rom);
        VerifyLookupStream4ProgramConsumers(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            var actual = RidleyPogoDefinitions.Read(pattern, stage);
            int x = 0xa60000 | Word(0xa6b965 + 2 * pattern);
            int y = 0xa60000 | Word(0xa6b96d + 2 * pattern);
            AssertEqual(Word(x + 2 * stage), actual.X, "stream4 native pogo horizontal magnitude");
            AssertEqual(Word(y + 2 * stage), actual.Y, "stream4 native pogo signed vertical speed");
            AssertEqual(Word(0xa6b94d + 2 * stage), actual.UpwardAcceleration, "stream4 native pogo upward acceleration");
            AssertEqual(Word(0xa6b959 + 2 * stage), actual.DownwardAcceleration, "stream4 native pogo downward acceleration");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(invalid, 0), "stream4 invalid pogo pattern");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(0, invalid), "stream4 invalid pogo stage");
        for (ushort parameter = 0; parameter <= 22; parameter += 2)
        {
            var actual = RidleyExplosionDefinitions.GetPart(parameter);
            AssertEqual(parameter, actual.Parameter, "stream4 breakup parameter identity");
            AssertEqual(Word(0xa6c6ce + parameter), actual.Lifetime, "stream4 native breakup lifetime");
            AssertEqual(Word(0xa6c6e6 + parameter), actual.InitializationRoutine, "stream4 native breakup initialization routine");
        }
        for (int orientation = 0; orientation < 16; orientation++)
            AssertEqual(Word(0xa6c7ba + 2 * orientation), RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, orientation), "stream4 native tail-tip orientation program");
        foreach (ushort invalid in new ushort[] { 1, 23, 24, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.GetPart(invalid), "stream4 invalid breakup parameter");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, invalid), "stream4 invalid tail-tip orientation");
    }
    private static void VerifyLookupStream4Programs(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort phase = 0; phase < 4; phase++)
        {
            AssertEqual(Word(0xa3894e + 2 * phase), SkreeMetareeAnimationDefinitions.MetareeInstructionList((SkreeMetareeAnimationPhase)phase), "stream4 native Metaree phase program");
            AssertEqual(Word(0xa3c69c + 2 * phase), SkreeMetareeAnimationDefinitions.SkreeInstructionList((SkreeMetareeAnimationPhase)phase), "stream4 native Skree phase program");
        }
        foreach (ushort invalid in new ushort[] { 4, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationDefinitions.MetareeInstructionList((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Metaree phase");
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationDefinitions.SkreeInstructionList((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Skree phase");
        }
        int particleMechanics = 0, particlePresentation = 0;
        for (int address = 0x8abd; address < 0x8acd; address += 2)
        {
            bool presentation = address is 0x8abf or 0x8ac7;
            if (presentation)
            {
                AssertEqual((ushort)address, SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(particlePresentation++), "stream4 debris presentation ordering");
                AssertThrows<InvalidDataException>(() => SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 debris presentation not mechanics");
            }
            else
            {
                var actual = SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(particleMechanics++);
                AssertEqual((ushort)address, actual.Address, "stream4 debris mechanics ordering");
                AssertEqual(Word(0x860000 | address), actual.Value, "stream4 native debris mechanics");
                AssertEqual(actual.Value, SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 debris direct mechanics");
            }
            for (int half = 0; half < 2; half++)
                AssertEqual(!presentation, SkreeMetareeParticleInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | (address + half)), "stream4 debris byte ownership");
        }
        AssertEqual(particleMechanics, SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWordCount, "stream4 debris mechanics count");
        AssertEqual(particlePresentation, SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordCount, "stream4 debris presentation count");
        int electricMechanics = 0, electricPresentation = 0;
        for (int address = 0xe683; address < 0xe6ad; address += 2)
        {
            bool presentation = address >= 0xe689 && address <= 0xe6a5 && (address - 0xe689) % 4 == 0;
            if (presentation)
            {
                AssertEqual((ushort)address, SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(electricPresentation++), "stream4 electricity presentation ordering");
                AssertThrows<InvalidDataException>(() => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 electricity presentation not mechanics");
            }
            else
            {
                var actual = SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(electricMechanics++);
                AssertEqual((ushort)address, actual.Address, "stream4 electricity mechanics ordering");
                AssertEqual(Word(0x860000 | address), actual.Value, "stream4 native electricity mechanics");
                AssertEqual(actual.Value, SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 electricity direct mechanics");
            }
            for (int half = 0; half < 2; half++)
                AssertEqual(!presentation, SaveStationElectricityInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | (address + half)), "stream4 electricity byte ownership");
        }
        AssertEqual(electricMechanics, SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount, "stream4 electricity mechanics count");
        AssertEqual(electricPresentation, SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount, "stream4 electricity presentation count");
        foreach (int invalid in new[] { -1, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(invalid), "stream4 debris mechanics bounds");
            AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream4 debris presentation bounds");
            AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(invalid), "stream4 electricity mechanics bounds");
            AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream4 electricity presentation bounds");
        }
        AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(particleMechanics), "stream4 debris mechanics end");
        AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(particlePresentation), "stream4 debris presentation end");
        AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(electricMechanics), "stream4 electricity mechanics end");
        AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(electricPresentation), "stream4 electricity presentation end");
    }
    private static void VerifyLookupStream4ProgramConsumers(ISnesAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var process = typeof(RoomEnemySystem).GetMethod("ProcessEnemyProjectileInstructions", flags)!;

        var electricityGuard = new SaveStationElectricityInstructionReadGuard(rom);
        var electricitySystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(electricitySystem, electricityGuard);
        electricitySystem.SpawnSaveStationElectricity(42, 16);
        var electricity = electricitySystem.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        for (int frame = 0; frame < 160; frame++)
        {
            electricity.InstructionTimer = 1;
            process.Invoke(electricitySystem, [electricity, null, (ushort)0, (ushort)0]);
            AssertTrue(electricity.IsActive, "stream4 electricity active throughout twenty cycles");
            AssertEqual((ushort)(0xe68b + 4 * (frame % 8)), electricity.InstructionPointer, "stream4 electricity exact frame order");
            AssertEqual((ushort)1, electricity.InstructionTimer, "stream4 electricity one-frame timing");
            AssertEqual((ushort)(0xe689 + 4 * (frame % 8)), electricity.PresentationOperandAddress, "stream4 electricity installed presentation operand identity");
        }
        electricity.InstructionTimer = 1;
        process.Invoke(electricitySystem, [electricity, null, (ushort)0, (ushort)0]);
        AssertTrue(!electricity.IsActive, "stream4 electricity deletes after twenty complete cycles");
        AssertEqual(0, electricityGuard.ForbiddenReadAttempts, "stream4 electricity no mechanics ROM reads");
        foreach (bool metaree in new[] { false, true })
        {
            var guard = new SkreeMetareeParticleInstructionReadGuard(rom);
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            var spawn = typeof(RoomEnemySystem).GetMethod(metaree ? "SpawnMetareeParticleBurst" : "SpawnSkreeParticleBurst", flags)!.CreateDelegate<Action<RoomEnemySlot>>(system);
            spawn(new RoomEnemySlot(0) { XPosition = 128, YPosition = 96 });
            ushort start = metaree ? (ushort)0x8ac5 : (ushort)0x8abd;
            int active = 0;
            foreach (var particle in system.EnemyProjectiles)
            {
                if (!particle.IsActive) continue;
                active++;
                for (int loop = 0; loop < 2; loop++)
                {
                    particle.InstructionTimer = 1;
                    process.Invoke(system, [particle, null, (ushort)0, (ushort)0]);
                    AssertEqual((ushort)(start + 4), particle.InstructionPointer, "stream4 debris goto-self loop pointer");
                    AssertEqual((ushort)16, particle.InstructionTimer, "stream4 debris sixteen-frame duration");
                    ushort nativeSprite = (ushort)(rom.ReadByte(0x860000 | (start + 2)) | rom.ReadByte(0x860000 | (start + 3)) << 8);
                    AssertEqual(nativeSprite, particle.SpritemapPointer, "stream4 debris compiled presentation identity");
                }
            }
            AssertEqual(4, active, "stream4 debris native four-direction burst");
            AssertEqual(0, guard.ForbiddenReadAttempts, "stream4 debris no mechanics ROM reads");
        }
    }
}
