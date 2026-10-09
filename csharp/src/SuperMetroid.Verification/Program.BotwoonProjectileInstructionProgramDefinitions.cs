using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and runs the Botwoon projectile instruction checks.</summary>
    private static void VerifyBotwoonProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyBotwoonProjectileInstructionProgramDefinitions), () => VerifyBotwoonProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks compiled controls, visual operands, and production projectile execution while rejecting runtime mechanics reads.</summary>
    /// <param name="rom">Retail cartridge address space supplying the authored instruction and spritemap words.</param>
    private static void VerifyBotwoonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyBotwoonProjectileControlMapping), () => VerifyBotwoonProjectileControlMapping(rom));
        Suite(nameof(VerifyBotwoonProjectileOperandMapping), () => VerifyBotwoonProjectileOperandMapping(rom));
        Suite(nameof(VerifyBotwoonProjectileProgramEnumeration), () => VerifyBotwoonProjectileProgramEnumeration());
        for (int index = 0;
             index < BotwoonProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                BotwoonProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Botwoon projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new BotwoonProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawnBody = typeof(RoomEnemySystem).GetMethod(
            "SpawnBotwoonBodySegment", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState, ushort>>(enemies);
        var spawnSpit = typeof(RoomEnemySystem).GetMethod(
            "SpawnBotwoonSpit", flags)!
            .CreateDelegate<Action<RoomEnemySlot, byte, ushort>>(enemies);
        var runBodyPreInstruction = typeof(RoomEnemySystem).GetMethod(
            "RunBotwoonBodyPreInstruction", flags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, byte>>(enemies);

        RoomEnemySlot head = enemies.Slots[0];
        head.XPosition = 128;
        head.YPosition = 96;
        var state = new BotwoonEnemyState(head);
        typeof(RoomEnemySystem).GetField("_botwoonState", flags)!.SetValue(enemies, state);
        spawnBody(head, state, 2);
        RoomEnemyProjectileSlot body = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonBody);
        AssertEqual(BotwoonProjectileInstructionProgramDefinitions.Hidden,
            body.InstructionPointer,
            "real Botwoon body producer starts a non-tail segment hidden");
        body.YPosition = 200;
        body.XVelocity = BotwoonProjectileCodePointers.BodyFallingFunction;
        runBodyPreInstruction(body, 0);
        AssertEqual(BotwoonProjectileCodePointers.BodyLandedFunction, body.XVelocity,
            "landed Botwoon body installs the cartridge RTS state");
        body.XVelocity = BotwoonProjectileCodePointers.LegacyBodyLandedFunction;
        runBodyPreInstruction(body, 0);
        AssertEqual(BotwoonProjectileCodePointers.LegacyBodyLandedFunction, body.XVelocity,
            "older debugger-state Botwoon landed sentinel remains restorable");

        for (int index = 0;
             index < BotwoonProjectileInstructionProgramDefinitions.BodyProgramCount;
             index++)
        {
            ushort program = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(index);
            body.InstructionPointer = program;
            body.InstructionTimer = 1;
            Run(body, 5, program, index < 8 ? 4 : 1, index < 8 ? (ushort)8 : (ushort)1);
        }

        spawnSpit(head, 0x40, 0x0180);
        RoomEnemyProjectileSlot spit = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonSpit);
        AssertEqual(BotwoonProjectileInstructionProgramDefinitions.Spit,
            spit.InstructionPointer,
            "real Botwoon spit producer selects the compiled animation loop");
        Run(spit, 6, 0xebae, 5, 3);
        AssertEqual((ushort)0x0003, spit.InstructionTimer,
            "Botwoon spit loop retains its three-frame cadence");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Botwoon body, tail, hidden, and spit selectors use installed definitions");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Botwoon projectile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe811),
            "Botwoon body spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe84b),
            "unused adjacent Botwoon body program is rejected as mechanics");

        _ = ProbeBotwoonProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBotwoonProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Botwoon projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Botwoon projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Botwoon projectile instruction mechanics: seventy-three compiled words, " +
            "all seventeen body/tail programs and the real spit producer pass with " +
            "forty-six installed sprite selectors and mechanics bytes forbidden.");

        void Run(RoomEnemyProjectileSlot projectile, int steps, ushort program, int frames, ushort duration)
        {
            for (int step = 0; step < steps; step++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
                AssertEqual((ushort)(program + 4 * (step % frames) + 2), projectile.PresentationOperandAddress,
                    "Botwoon projectile exact frame order including loop and sleep");
                AssertEqual(frames == 1 && step > 0 ? (ushort)0 : duration, projectile.InstructionTimer,
                    "Botwoon projectile native duration or sleep timer");
            }
        }
    }

    /// <summary>Checks the ordered list of seventeen used body/tail programs and rejects out-of-range ordinals.</summary>
    private static void VerifyBotwoonProjectileProgramEnumeration()
    {
        ushort[] programs = [0xe80f,0xe823,0xe837,0xe85f,0xe873,0xe887,0xe89b,0xe8af,
            0xe8c3,0xe8c9,0xe8cf,0xe8d5,0xe8db,0xe8e1,0xe8e7,0xe8ed,0xe8f3];
        AssertEqual(programs.Length, BotwoonProjectileInstructionProgramDefinitions.BodyProgramCount, "Botwoon projectile program count");
        for (int i = 0; i < programs.Length; i++)
            AssertEqual(programs[i], BotwoonProjectileInstructionProgramDefinitions.BodyProgram(i), "Botwoon native program order excludes unused slot");
        foreach (int index in new[] {int.MinValue,-1,17,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonProjectileInstructionProgramDefinitions.BodyProgram(index), "Botwoon program ordinal domain");
    }

    /// <summary>Compares compiled control words with ROM, verifies byte ownership, and rejects visual words and unused gaps as mechanics.</summary>
    /// <param name="rom">Retail cartridge address space containing the native control words.</param>
    private static void VerifyBotwoonProjectileControlMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] looping = [0xe80f,0xe823,0xe837,0xe85f,0xe873,0xe887,0xe89b,0xe8af];
        ushort[] sleeping = [0xe8c3,0xe8c9,0xe8cf,0xe8d5,0xe8db,0xe8e1,0xe8e7,0xe8ed,0xe8f3];
        var words = new List<ushort>();
        foreach (ushort program in looping)
        foreach (int offset in new[] {0,4,8,12,16,18}) words.Add((ushort)(program + offset));
        foreach (ushort program in sleeping)
        foreach (int offset in new[] {0,4}) words.Add((ushort)(program + offset));
        words.AddRange(new ushort[] {0xebae,0xebb2,0xebb6,0xebba,0xebbe,0xebc2,0xebc4});
        AssertEqual(words.Count, BotwoonProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount, "Botwoon projectile native control count");
        var owned = new HashSet<int>();
        for (int i = 0; i < words.Count; i++)
        {
            ushort address = words[i];
            var actual = BotwoonProjectileInstructionProgramDefinitionsTooling.MechanicsWord(i);
            ushort native = ReadBotwoonInstructionWord(rom, 0x860000 | address);
            AssertEqual(address, actual.Address, "Botwoon projectile control enumeration");
            AssertEqual(native, actual.Value, "Botwoon projectile enumerated native control");
            AssertEqual(native, BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(address), "Botwoon projectile direct native control");
            owned.Add(address); owned.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(owned.Contains(address), BotwoonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address), "Botwoon projectile byte ownership");
            AssertEqual(owned.Contains(address), BotwoonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1860000 | address), "Botwoon projectile ownership high-bit aliases");
            AssertTrue(!BotwoonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x870000 | address), "Botwoon projectile other bank rejected");
        }
        for (int address = 0xe80d; address <= 0xebc7; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Botwoon projectile rejects visual words, odd starts, unused slot and adjacent programs");
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(address), "Botwoon projectile distant invalid control");
        foreach (int index in new[] {int.MinValue,-1,73,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index), "Botwoon control ordinal domain");
    }

    /// <summary>Checks the presentation-operand address sequence and its complete membership and bounds behavior.</summary>
    /// <param name="rom">Retail cartridge address space used by the follow-up visual-selector comparison.</param>
    private static void VerifyBotwoonProjectileOperandMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xe811,0xe815,0xe819,0xe81d,0xe825,0xe829,0xe82d,0xe831,
            0xe839,0xe83d,0xe841,0xe845,0xe861,0xe865,0xe869,0xe86d,
            0xe875,0xe879,0xe87d,0xe881,0xe889,0xe88d,0xe891,0xe895,
            0xe89d,0xe8a1,0xe8a5,0xe8a9,0xe8b1,0xe8b5,0xe8b9,0xe8bd,
            0xe8c5,0xe8cb,0xe8d1,0xe8d7,0xe8dd,0xe8e3,0xe8e9,0xe8ef,0xe8f5,
            0xebb0,0xebb4,0xebb8,0xebbc,0xebc0];
        AssertEqual(operands.Length, BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount, "Botwoon projectile operand count");
        for (int i = 0; i < operands.Length; i++)
            AssertEqual(operands[i], BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress(i), "Botwoon projectile native operand enumeration");
        var known = operands.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(known.Contains((ushort)address), BotwoonProjectileInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Botwoon projectile full operand membership");
        foreach (int index in new[] {int.MinValue,-1,46,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index), "Botwoon operand ordinal domain");
        Suite(nameof(VerifyBotwoonProjectileVisualMapping), () => VerifyBotwoonProjectileVisualMapping(rom, operands));
    }

    /// <summary>Compares each compiled projectile selector with its ROM sprite pointer and verifies visible, hidden, and gap records.</summary>
    /// <param name="rom">Retail cartridge address space containing operand and spritemap data.</param>
    /// <param name="nativeOperands">Presentation addresses enumerated by the operand verification.</param>
    private static void VerifyBotwoonProjectileVisualMapping(SuperMetroidAddressSpace rom, ushort[] nativeOperands)
    {
        foreach (ushort operand in nativeOperands)
        {
            ushort native = ReadBotwoonInstructionWord(rom, 0x860000 | operand);
            AssertEqual(native, EnemyProjectileSpritemapDefinitions.BotwoonProjectileFrameAt(operand), "Botwoon projectile native sprite pointer");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, operand, out ushort shared), "Botwoon projectile shared selector exists");
            AssertEqual(native, shared, "Botwoon projectile shared native pointer");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0x860000 | operand), "Botwoon projectile excluded from literal regeneration");
            AssertEqual(operand == 0xe8f5 ? (ushort)0 : (ushort)1,
                ReadBotwoonInstructionWord(rom, 0x8d0000 | native), "Botwoon visible single-piece and hidden empty record sizes");
        }
        // Unused fourth body direction still occupies four single-entry OAM records.
        foreach (ushort pointer in new ushort[] {0xb682,0xb689,0xb690,0xb697})
            AssertEqual((ushort)1, ReadBotwoonInstructionWord(rom, 0x8d0000 | pointer), "Botwoon unused physical map contributes seven bytes");
        var known = nativeOperands.ToHashSet();
        for (int address = 0xe80d; address <= 0xebc7; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.BotwoonProjectileFrameAt((ushort)address), "Botwoon projectile visual rejects controls and gaps");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x86, (ushort)address, out ushort missing), "Botwoon projectile shared gap rejected");
                AssertEqual((ushort)0, missing, "Botwoon projectile missing output cleared");
            }
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.BotwoonProjectileFrameAt(address), "Botwoon projectile distant invalid visual");
    }

    /// <summary>Exercises alternating body and spit mechanics lookups and accumulates their values to keep the work observable.</summary>
    /// <returns>Checksum of the selected compiled mechanics words.</returns>
    private static int ProbeBotwoonProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft
                    : BotwoonProjectileInstructionProgramDefinitions.Spit);
        }
        return checksum;
    }

    /// <summary>Wraps the cartridge bus to reject compiled mechanics reads and record accesses to compiled presentation words.</summary>
    /// <param name="source">Underlying address space for allowed reads and delegated writes.</param>
    private sealed class BotwoonProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses whose bytes were read through the guarded cartridge path.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads of bytes represented by compiled mechanics definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the mechanics-read guard and presentation-access tracker.</summary>
        /// <param name="address">Bus address requested by production code.</param>
        /// <returns>The source byte when the request is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics bytes and records reads of compiled presentation operands.</summary>
        /// <param name="address">Bus address requested by production code.</param>
        /// <returns>The underlying source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The requested byte belongs to a compiled mechanics word.</exception>
        public byte ReadByte(int address)
        {
            if (BotwoonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Botwoon projectile mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = BotwoonProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the wrapped address space.</summary>
        /// <param name="address">Bus address to write.</param>
        /// <param name="value">Byte stored at the address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
