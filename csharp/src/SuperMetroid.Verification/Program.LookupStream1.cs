using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        VerifyLookupStream1EnemyMovement(rom);
        VerifyLookupStream1CadencePrograms(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort medium = 0; medium < 3; medium++)
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotionDefinitions.Launch(medium, (variant & 1) != 0, (variant & 2) != 0);
                int address = 0x909eb9 + 12 * variant + 2 * medium;
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotionDefinitions.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * medium), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * medium), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotionDefinitions.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * medium), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * medium), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotionDefinitions.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * medium), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * medium), gravity.Fraction, "stream1 native fractional gravity");
        }
        foreach (ushort invalid in new ushort[] { 3, ushort.MaxValue })
        {
            for (int variant = 0; variant < 4; variant++)
                AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Launch(invalid, (variant & 1) != 0, (variant & 2) != 0), "stream1 invalid launch medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Knockback(invalid), "stream1 invalid hurt medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.BombJump(invalid), "stream1 invalid bomb medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Gravity(invalid), "stream1 invalid gravity medium");
        }
        for (int address = 0x90c254; address <= 0x90c28e; address++)
            AssertEqual(rom.ReadByte(address), SamusProjectileCooldownDefinitions.ReadByte(address), "stream1 all native cooldown bytes including padding");
        AssertEqual(rom.ReadByte(0x90c291), SamusProjectileCooldownDefinitions.ReadByte(0x90c291), "stream1 bounded spacetime beam cooldown");
        foreach (int invalid in new[] { int.MinValue, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileCooldownDefinitions.ReadByte(invalid), "stream1 invalid cooldown address");
    }
    private static void VerifyLookupStream1EnemyMovement(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort index = 0; index < 13; index++)
        {
            var intervals = BullMovementDefinitions.Intervals(index);
            AssertEqual(Word(0xa8d895 + 4 * index), intervals.Acceleration, "stream1 Bull acceleration interval");
            AssertEqual(Word(0xa8d897 + 4 * index), intervals.Deceleration, "stream1 Bull deceleration interval");
        }
        for (int direction = 0; direction < 10; direction++)
            AssertEqual(Word(0xa8d871 + 2 * direction), BullMovementDefinitions.ShotAngle(direction), "stream1 Bull compass angle");
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BullMovementDefinitions.ShotAngle(invalid), "stream1 Bull invalid direction");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacMovementDefinitions.TravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)direction), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)invalid), "stream1 Cacatac invalid direction selector");
    }
    private static void VerifyLookupStream1CadencePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int address = 0x90c481; address < 0x90c4b5; address++)
        {
            AssertEqual(rom.ReadByte(address), ChargeFlareAnimationDefinitions.ReadByte(address), "stream1 original flare pointer/cadence byte");
            if (address < 0x90c4b4)
                AssertEqual(Word(address), ChargeFlareAnimationDefinitions.ReadWord(address), "stream1 original flare unaligned word");
        }
        foreach (int invalid in new[] { int.MinValue, 0x90c480, 0x90c4b5, int.MaxValue })
            AssertThrows<InvalidDataException>(() => ChargeFlareAnimationDefinitions.ReadByte(invalid), "stream1 flare byte domain");
        Check(0xa80000, 0xd841, 0xd871, BullInstructionProgramDefinitions.MechanicsWordCount,
            BullInstructionProgramDefinitions.PresentationWordCount,
            i => { var word = BullInstructionProgramDefinitions.MechanicsWord(i); return (word.Address, word.Value); },
            BullInstructionProgramDefinitions.PresentationWordAddress, BullInstructionProgramDefinitions.ReadMechanicsWord,
            BullInstructionProgramDefinitions.IsCompiledMechanicsByte,
            a => a is 0xd843 or 0xd847 or 0xd84b or 0xd84f or 0xd85b or 0xd85f or 0xd863 or 0xd867);
        Check(0x860000, 0xd92e, 0xd96a, CacatacProjectileInstructionProgramDefinitions.MechanicsWordCount,
            CacatacProjectileInstructionProgramDefinitions.PresentationWordCount,
            i => { var word = CacatacProjectileInstructionProgramDefinitions.MechanicsWord(i); return (word.Address, word.Value); },
            CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress, CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord,
            CacatacProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte,
            a => (a - 0xd930) % 6 == 0);
        for (int address = 0xd840; address <= 0xd871; address++)
            AssertEqual(address is 0xd843 or 0xd847 or 0xd84b or 0xd84f or 0xd85b or 0xd85f or 0xd863 or 0xd867,
                BullInstructionProgramDefinitions.IsPresentationWord((ushort)address), "stream1 Bull exact presentation classification");

        void Check(int bank, int first, int end, int mechanicsCount, int presentationCount,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns, Func<int, bool> isVisual)
        {
            int mechanical = 0, visual = 0;
            for (int address = first; address < end; address += 2)
            {
                bool selectedVisual = isVisual(address);
                if (selectedVisual)
                {
                    AssertEqual((ushort)address, presentation(visual++), "stream1 native presentation operand ordering");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "stream1 presentation excluded from mechanics");
                }
                else
                {
                    var actual = mechanics(mechanical++);
                    AssertEqual((ushort)address, actual.Address, "stream1 native mechanics address ordering");
                    AssertEqual(Word(bank | address), actual.Value, "stream1 original program mechanics value");
                    AssertEqual(actual.Value, read((ushort)address), "stream1 direct program mechanics value");
                }
                AssertEqual(!selectedVisual, owns(bank | address), "stream1 program low-byte ownership");
                AssertEqual(!selectedVisual, owns(bank | (address + 1)), "stream1 program high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "stream1 unaligned program word rejection");
            }
            AssertEqual(mechanicsCount, mechanical, "stream1 exact mechanics count");
            AssertEqual(presentationCount, visual, "stream1 exact presentation count");
            AssertTrue(!owns(bank | (first - 1)) && !owns(bank | end) && !owns((bank ^ 0x10000) | first), "stream1 outside program byte rejection");
            foreach (int invalid in new[] { -1, mechanicsCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "stream1 mechanics enumeration bounds");
            foreach (int invalid in new[] { -1, presentationCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "stream1 presentation enumeration bounds");
        }
    }
    private static void VerifyLookupStream1ProjectileMotion(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(address), "stream1 all native projectile motion words");
        foreach (int invalid in new[] { int.MinValue, 0x90c2d0, 0x90c2d2, 0x90c37b, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileMotionDefinitions.ReadWord(invalid), "stream1 exact projectile motion address domain");
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePowerBeamVelocity",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        var guarded = new BeamSpeedRowAddressSpace(rom);
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectileSlot(0) { Type = combination, Direction = direction };
            initialize(guarded, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 + (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)), slot.XVelocity, "stream1 actual initializer X preserves adjacent missile-data overread");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)), slot.YVelocity, "stream1 actual initializer Y preserves adjacent missile-data overread");
        }
    }
    private static void VerifyLookupStream1Selection(ISnesAddressSpace rom)
    {
        for (int address = 0x9383c1; address < 0x9386db; address += 2)
        {
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, SamusProjectileSelectionDefinitions.ReadWord(address),
                $"stream1 original projectile selector/header {address:X6}");
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(address + 1),
                "stream1 unaligned selector/header rejection");
        }
        foreach (int invalid in new[] { int.MinValue, 0x9283c1, 0x9383bf, 0x9386db, 0x9483c1, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(invalid),
                "stream1 selector exact address-domain rejection");
    }
}
