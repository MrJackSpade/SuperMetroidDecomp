using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeetomInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyBeetomInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        Suite(nameof(VerifyBeetomMechanicsMapping), () => VerifyBeetomMechanicsMapping(rom));
        Suite(nameof(VerifyBeetomPresentationMapping), () => VerifyBeetomPresentationMapping());
        Suite(nameof(VerifyBeetomVisualSelectors), () => VerifyBeetomVisualSelectors(rom));
        Suite(nameof(VerifyBeetomDistantActionMapping), () => VerifyBeetomDistantActionMapping(rom));
        Suite(nameof(VerifyBeetomDistantDirectionMapping), () => VerifyBeetomDistantDirectionMapping());

        var guard = new BeetomInstructionReadGuard(rom, forbidPresentation: artwork is not null);
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_setRandomNumber", flags)!.SetValue(
            enemies,
            (Action<ushort>)(_ => { }));
        var initialize = type.GetMethod("InitializeBeetom", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, ushort>>(enemies);
        var startCrawling = type.GetMethod("StartBeetomCrawling", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BeetomEnemyState, bool>>();
        var startDraining = type.GetMethod("StartBeetomDrain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BeetomEnemyState, bool>>();
        MethodInfo startHop = type.GetMethod("StartBeetomHop", flags)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Beetom;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.XPosition = 0x0200;
        var samus = new SamusState { XPosition = 0x0100 };
        initialize(slot, samus, 0);
        BeetomEnemyState state = enemies.BeetomStates[0]!;
        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0];

        AssertEqual(BeetomInstructionProgramDefinitions.CrawlingLeft,
            slot.CurrentInstruction,
            "real Beetom initializer installs left-crawl program");
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.CrawlingLeft,
            BeetomInstructionProgramDefinitions.CrawlingLeftLoop,
            callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "left-crawl program disables off-screen processing");

        startCrawling(slot, state, false);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.CrawlingRight,
            BeetomInstructionProgramDefinitions.CrawlingRightLoop,
            callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "right-crawl program disables off-screen processing");

        StartBeetomHop(startHop, slot, state, left: true);
        RunBeetomSleepProgram(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.HopLeft,
            BeetomInstructionProgramDefinitions.HopLeftSleep,
            timedFrames: 4);
        AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "left-hop program enables off-screen processing");

        StartBeetomHop(startHop, slot, state, left: false);
        RunBeetomSleepProgram(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.HopRight,
            BeetomInstructionProgramDefinitions.HopRightSleep,
            timedFrames: 4);
        AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "right-hop program enables off-screen processing");

        startDraining(slot, state, true);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.DrainingLeft,
            BeetomInstructionProgramDefinitions.DrainingLeftLoop,
            callsThroughGoto: 9);

        startDraining(slot, state, false);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.DrainingRight,
            BeetomInstructionProgramDefinitions.DrainingRightLoop,
            callsThroughGoto: 9);

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Beetom programs read no cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Beetom mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BeetomInstructionProgramDefinitions.ReadMechanicsWord(0xb69a),
            "Beetom spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BeetomInstructionProgramDefinitions.ReadMechanicsWord(0xb6c0),
            "unused Beetom small-hop program is rejected as production mechanics");

        _ = ProbeBeetomInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBeetomInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Beetom allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Beetom mechanics lookups allocate no per-frame storage");

        Console.WriteLine(artwork is null
            ? "Beetom instruction mechanics: 48 compiled words, all six production " +
              "programs, and 32 compiled visual selectors pass with mechanics bytes forbidden."
            : "Installed Beetom: all six crawl, hop, and drain programs retain " +
              "mechanics while visual-selector reads are forbidden.");
    }

    private static void VerifyBeetomMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xb696,0xb698,0xb69c,0xb6a0,0xb6a4,0xb6a8,0xb6aa,
            0xb6ac,0xb6ae,0xb6b2,0xb6b6,0xb6ba,0xb6be,
            0xb6cc,0xb6d0,0xb6d4,0xb6d8,0xb6dc,0xb6de,0xb6e2,0xb6e6,0xb6ea,0xb6ee,0xb6f0,
            0xb6f2,0xb6f4,0xb6f8,0xb6fc,0xb700,0xb704,0xb706,
            0xb708,0xb70a,0xb70e,0xb712,0xb716,0xb71a,
            0xb728,0xb72c,0xb730,0xb734,0xb738,0xb73a,0xb73e,0xb742,0xb746,0xb74a,0xb74c];
        AssertEqual(addresses.Length, BeetomInstructionProgramDefinitionsTooling.MechanicsWordCount, "Beetom native control count");
        var bytes = new HashSet<int>();
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort address = addresses[i];
            var actual = BeetomInstructionProgramDefinitionsTooling.MechanicsWord(i);
            AssertEqual(address, actual.Address, "Beetom native control address order");
            ushort expected = ReadBeetomInstructionWord(rom, address);
            AssertEqual(expected, actual.Value, "Beetom enumerated native control word");
            AssertEqual(expected, BeetomInstructionProgramDefinitions.ReadMechanicsWord(address), "Beetom direct native control word");
            bytes.Add(address); bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), BeetomInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address), "Beetom full native byte ownership");
            AssertEqual(bytes.Contains(address), BeetomInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a80000 | address), "Beetom existing high-bit alias");
            AssertTrue(!BeetomInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa90000 | address), "Beetom rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = 0xb694; address <= 0xb750; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BeetomInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Beetom rejects presentation, odd bytes, unused small-hop programs and adjacent data");
        foreach (ushort address in new ushort[] {0, 0x7fff, 0xffff})
            AssertThrows<InvalidDataException>(() => BeetomInstructionProgramDefinitions.ReadMechanicsWord(address), "Beetom rejects distant pointers");
        foreach (int index in new[] {int.MinValue, -1, 48, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BeetomInstructionProgramDefinitionsTooling.MechanicsWord(index), "Beetom mechanics ordinal bounds");
    }

    private static (RoomEnemySystem Enemies, BeetomEnemyState State, Action<BeetomEnemyState> Choose)
        CreateBeetomDistantSelectionFixture(Func<ushort> nextRandom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_setRandomNumber", flags)!.SetValue(enemies, (Action<ushort>)(_ => { }));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, nextRandom);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBeetom", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, ushort>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Beetom;
        initialize(slot, new SamusState(), 0);
        var choose = typeof(RoomEnemySystem).GetMethod("ChooseDistantBeetomAction", flags)!
            .CreateDelegate<Action<BeetomEnemyState>>(enemies);
        return (enemies, enemies.BeetomStates[0]!, choose);
    }

    private static void VerifyBeetomDistantActionMapping(SuperMetroidAddressSpace rom)
    {
        ushort random = 0;
        int randomCalls = 0;
        var fixture = CreateBeetomDistantSelectionFixture(() => { randomCalls++; return random; });
        // The original eight pointer words at A8:B74E; high RNG bits are masked by B839.
        // Native short-hop labels are swapped; compare pointer identities, not label spelling.
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            random = (ushort)value;
            fixture.State.Function = BeetomEnemyFunction.DecideActionSamusNotInProximity;
            fixture.State.FunctionTimer = 0x1234;
            ushort instruction = fixture.Enemies.Slots[0].CurrentInstruction;
            fixture.Choose(fixture.State);
            ushort expected = ReadBeetomInstructionWord(rom, (ushort)(0xb74e + 2 * (value & 7)));
            AssertEqual(expected, (ushort)fixture.State.Function, "Beetom distant action matches native pointer for every RNG word");
            AssertEqual((ushort)0x1234, fixture.State.FunctionTimer, "Beetom choice preserves timer until setup runs");
            AssertEqual(instruction, fixture.Enemies.Slots[0].CurrentInstruction, "Beetom choice preserves program until setup runs");
            AssertEqual(value + 1, randomCalls, "Beetom consumes exactly one random value per choice");
        }
    }

    private static void VerifyBeetomDistantDirectionMapping()
    {
        ushort random = 0;
        var fixture = CreateBeetomDistantSelectionFixture(() => random);
        // A8:B844-B84A reloads that same seed, masks bit zero and stores direction.
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            random = (ushort)value;
            fixture.State.Direction = 0xffff;
            fixture.Choose(fixture.State);
            AssertEqual((ushort)(value % 2), fixture.State.Direction, "Beetom direction preserves native low-bit mapping independently of selected action");
        }
    }
    private static void VerifyBeetomVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xb69a,0xb69e,0xb6a2,0xb6a6,0xb6b0,0xb6b4,0xb6b8,0xb6bc,
            0xb6ce,0xb6d2,0xb6d6,0xb6da,0xb6e0,0xb6e4,0xb6e8,0xb6ec,
            0xb6f6,0xb6fa,0xb6fe,0xb702,0xb70c,0xb710,0xb714,0xb718,
            0xb72a,0xb72e,0xb732,0xb736,0xb73c,0xb740,0xb744,0xb748];
        foreach (ushort operand in operands)
        {
            ushort expected = ReadBeetomInstructionWord(rom, operand);
            AssertEqual(expected, EnemySpritemapDefinitions.BeetomFrameAt(operand), "Beetom native visual pointer");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, operand, out ushort shared), "Beetom shared selector found");
            AssertEqual(expected, shared, "Beetom shared selector value");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa80000 | operand), "Beetom excluded from literal regeneration");
        }
        foreach (ushort pointer in new ushort[] {0xbed3,0xbeee,0xbf09,0xbf24,0xbf3f,0xbf5a,0xbf75,0xbf90,
            0xc00b,0xc026,0xc041,0xc05c,0xc077,0xc092,0xc0ad,0xc0c8})
            AssertEqual((ushort)5, ReadBeetomInstructionWord(rom, pointer), "Beetom five-entry native map");
        foreach (ushort pointer in new ushort[] {0xbfab,0xbfcb,0xbfeb,0xc0e3,0xc103,0xc123})
            AssertEqual((ushort)6, ReadBeetomInstructionWord(rom, pointer), "Beetom six-entry native map");
        var known = operands.ToHashSet();
        for (int address = 0xb694; address <= 0xb750; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BeetomFrameAt((ushort)address), "Beetom visual resolver rejects controls, small-hop gaps and adjacent data");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8, (ushort)address, out ushort missing), "Beetom shared holes rejected");
                AssertEqual((ushort)0, missing, "Beetom missing output cleared");
            }
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BeetomFrameAt(address), "Beetom distant invalid operand");
    }
    private static void VerifyBeetomPresentationMapping()
    {
        ushort[] expected = [0xb69a,0xb69e,0xb6a2,0xb6a6,0xb6b0,0xb6b4,0xb6b8,0xb6bc,
            0xb6ce,0xb6d2,0xb6d6,0xb6da,0xb6e0,0xb6e4,0xb6e8,0xb6ec,
            0xb6f6,0xb6fa,0xb6fe,0xb702,0xb70c,0xb710,0xb714,0xb718,
            0xb72a,0xb72e,0xb732,0xb736,0xb73c,0xb740,0xb744,0xb748];
        AssertEqual(expected.Length, BeetomInstructionProgramDefinitionsTooling.PresentationWordCount, "Beetom native operand count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BeetomInstructionProgramDefinitionsTooling.PresentationWordAddress(i), "Beetom native operand order");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), BeetomInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Beetom full operand membership");
        foreach (int index in new[] {int.MinValue, -1, 32, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BeetomInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Beetom operand ordinal bounds");
    }
    private static void StartBeetomHop(
        MethodInfo startHop,
        RoomEnemySlot slot,
        BeetomEnemyState state,
        bool left) =>
        startHop.Invoke(null, [slot, state, (ushort)0, left, false]);

    private static void RunBeetomLoop(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort loop,
        int callsThroughGoto)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Beetom compiled loop entry");
        for (int call = 0; call < callsThroughGoto; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual(unchecked((ushort)(loop + 4)), slot.CurrentInstruction,
            "Beetom program completes its native goto and first repeated frame");
    }

    private static void RunBeetomSleepProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort sleep,
        int timedFrames)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Beetom compiled sleep-program entry");
        for (int frame = 0; frame < timedFrames; frame++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        slot.InstructionTimer = 1;
        process.Invoke(enemies, arguments);
        AssertEqual(sleep, slot.CurrentInstruction,
            "Beetom hop program reaches terminal sleep");
    }

    private static int ProbeBeetomInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BeetomInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 6) switch
                {
                    0 => BeetomInstructionProgramDefinitions.CrawlingLeft,
                    1 => BeetomInstructionProgramDefinitions.HopLeft,
                    2 => BeetomInstructionProgramDefinitions.DrainingLeft,
                    3 => BeetomInstructionProgramDefinitions.CrawlingRight,
                    4 => BeetomInstructionProgramDefinitions.HopRight,
                    _ => BeetomInstructionProgramDefinitions.DrainingRight,
                });
        }
        return checksum;
    }

    private static ushort ReadBeetomInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class BeetomInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BeetomInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Beetom mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BeetomInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BeetomInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                        {
                            ForbiddenReadAttempts++;
                            throw new InvalidOperationException(
                                $"Installed Beetom read visual-selector byte ${address:X6}.");
                        }
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
