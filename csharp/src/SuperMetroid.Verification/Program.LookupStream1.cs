using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        VerifyLookupStream1PowampMotion(rom);
        VerifyLookupStream1HibashiDragonFireball(rom);
        VerifyLookupStream1CommonFrames(rom);
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
    private static void VerifyLookupStream1PowampMotion(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        for (int phase = 0; phase < 12; phase++)
            AssertEqual(Word(0xa8c1a1 + 2 * phase), PowampMotionDefinitions.WiggleOffset(phase), "Powamp native wiggle triangle");
        for (int pose = 0; pose < 3; pose++)
        {
            AssertEqual(Word(0xa8c277 + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, false), "Powamp native rising balloon offset");
            AssertEqual(Word(0xa8c27d + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, true), "Powamp native sinking balloon offset");
        }
        for (int direction = 0; direction < 8; direction++)
        {
            AssertEqual(Word(0x86d21a + 2 * direction), PowampMotionDefinitions.SpikeXAcceleration(direction), "Powamp native spike X acceleration");
            AssertEqual(Word(0x86d22a + 2 * direction), PowampMotionDefinitions.SpikeYAcceleration(direction), "Powamp native spike Y acceleration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.WiggleOffset(invalid), "Powamp wiggle domain");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            for (int sinking = 0; sinking < 2; sinking++)
                AssertThrows<IndexOutOfRangeException>(() => PowampMotionDefinitions.BalloonOffset(invalid, sinking != 0), "Powamp balloon pose domain");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeXAcceleration(invalid), "Powamp X direction domain");
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeYAcceleration(invalid), "Powamp Y direction domain");
        }
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
    private static void VerifyLookupStream1LaunchRoles(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int index = 0; index < 5; index++)
        {
            var launch = SamusBombSpreadLaunchDefinitions.ForSlot(index);
            AssertEqual(Word(0x90d8cf + index * 2), launch.FuseTimer, "spread native fuse");
            AssertEqual(Word(0x90d8d9 + index * 2), launch.XVelocity, "spread native direction/magnitude X");
            AssertEqual(Word(0x90d8e3 + index * 2), launch.YSpeed, "spread native whole Y");
            AssertEqual(Word(0x90d8ed + index * 2), launch.YSubspeed, "spread native fraction Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SamusBombSpreadLaunchDefinitions.ForSlot(invalid), "spread native slot rejection");
        for (int index = 0; index < 22; index++)
        {
            var frame = HibashiDefinitions.ActivityFrame(index);
            AssertEqual(Word(0xa68dbb + index * 2), frame.YOffset, "Hibashi native collision rise");
            AssertEqual(Word(0xa68de7 + index * 2), frame.YRadius, "Hibashi native collision radius");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 22, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(invalid), "Hibashi native frame rejection");
        for (ushort selector = 0; selector < 6; selector++)
        {
            AssertEqual(Word(0xa2e5ef + selector * 2), DragonAnimationDefinitions.InstructionList((DragonAnimationSelector)selector), "Dragon native phase/facing program");
            var role = EscapeEtecoonDefinitions.Initialization(selector);
            int offset = selector & ~1;
            AssertEqual(Word(0xb3e718 + offset), role.XPosition, "Etecoon native role X");
            AssertEqual(Word(0xb3e71e + offset), role.YPosition, "Etecoon native role Y");
            AssertEqual(Word(0xb3e724 + offset), (ushort)role.PreInstruction, "Etecoon native role action");
            AssertEqual(Word(0xb3e72a + offset), role.InstructionList, "Etecoon native role program");
            AssertEqual(Word(0xb3e730 + offset), role.HorizontalSpeed, "Etecoon native role speed");
        }
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => DragonAnimationDefinitions.InstructionList((DragonAnimationSelector)invalid), "Dragon invalid selector rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeEtecoonDefinitions.Initialization(invalid), "Etecoon invalid role rejection");
        }
    }
    private static void VerifyLookupStream1SparkSpikePrograms(ISnesAddressSpace rom)
    {
        Check(0xf353, 0xf391, 17, 14,
            a => a < 0xf35f ? (a - 0xf353) % 4 == 2 : a >= 0xf363 && a < 0xf38f && (a - 0xf363) % 4 == 2,
            i => { var w = FallingSparkInstructionProgramDefinitions.MechanicsWord(i); return (w.Address, w.Value); },
            FallingSparkInstructionProgramDefinitions.PresentationWordAddress,
            FallingSparkInstructionProgramDefinitions.ReadMechanicsWord,
            FallingSparkInstructionProgramDefinitions.IsCompiledMechanicsByte);
        Check(0xd208, 0xd21a, 6, 3,
            a => a < 0xd214 && (a - 0xd208) % 4 == 2,
            i => { var w = PowampSpikeInstructionProgramDefinitions.MechanicsWord(i); return (w.Address, w.Value); },
            PowampSpikeInstructionProgramDefinitions.PresentationWordAddress,
            PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord,
            PowampSpikeInstructionProgramDefinitions.IsCompiledMechanicsByte);

        void Check(int first, int end, int mechanicsCount, int presentationCount, Func<int, bool> isVisual,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns)
        {
            int mechanical = 0, visual = 0;
            for (int address = first; address < end; address += 2)
            {
                bool art = isVisual(address);
                if (art)
                {
                    AssertEqual((ushort)address, presentation(visual++), "spark/spike original presentation ordering");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "spark/spike presentation excluded from mechanics");
                }
                else
                {
                    var actual = mechanics(mechanical++);
                    AssertEqual((ushort)address, actual.Address, "spark/spike original mechanics ordering");
                    ushort native = (ushort)(rom.ReadByte(0x860000 | address) | rom.ReadByte(0x860000 | (address + 1)) << 8);
                    AssertEqual(native, actual.Value, "spark/spike original mechanics word");
                    AssertEqual(native, read((ushort)address), "spark/spike direct mechanics read");
                }
                AssertEqual(!art, owns(0x860000 | address), "spark/spike low-byte ownership");
                AssertEqual(!art, owns(0x860000 | (address + 1)), "spark/spike high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "spark/spike unaligned word rejection");
            }
            AssertEqual(mechanicsCount, mechanical, "spark/spike mechanics extent");
            AssertEqual(presentationCount, visual, "spark/spike presentation extent");
            foreach (int invalid in new[] { -1, mechanicsCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "spark/spike mechanics index bounds");
            foreach (int invalid in new[] { -1, presentationCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "spark/spike presentation index bounds");
            foreach (int invalid in new[] { 0x850000 | first, 0x860000 | (first - 1), 0x860000 | end, int.MinValue, int.MaxValue })
                AssertTrue(!owns(invalid), "spark/spike outside byte-domain rejection");
        }
    }
    private static void VerifyLookupStream1CommonFrames(ISnesAddressSpace rom)
    {
        byte[] originalBanks = [0xa0, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3];
        AssertTrue(originalBanks.SequenceEqual(CommonEnemyEmptyExtendedFrameDefinitions.SupportedBanks), "common empty-frame original bank order");
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        {
            bool expected = originalBanks.Contains((byte)bank);
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameDefinitions.HasFrame((byte)bank, 0x804f), "common empty extended-frame exact bank domain");
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap((byte)bank, 0x804d), "common empty OAM exact bank domain");
            AssertTrue(!CommonEnemyEmptyExtendedFrameDefinitions.HasFrame((byte)bank, 0x8050), "common empty frame excludes neighboring pointer");
            if (expected)
            {
                AssertEqual((byte)0, rom.ReadByte(bank << 16 | 0x804d), "native common OAM has zero components");
                AssertEqual((byte)1, rom.ReadByte(bank << 16 | 0x804f), "native common extended frame has one component");
            }
        }
        var deletion = CommonEnemyProjectileInstructionProgramDefinitions.MechanicsWord(0);
        AssertEqual((ushort)0x84fc, deletion.Address, "shared projectile delete identity");
        AssertEqual((ushort)(rom.ReadByte(0x8684fc) | rom.ReadByte(0x8684fd) << 8), deletion.Value, "shared projectile delete native instruction");
        foreach (int invalid in new[] { int.MinValue, -1, 1, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CommonEnemyProjectileInstructionProgramDefinitions.MechanicsWord(invalid), "shared delete enumeration bounds");
    }
    private static void VerifyLookupStream1HibashiDragonFireball(ISnesAddressSpace rom)
    {
        Check(0x860000, 0xb4bf, 0xb4ef, 16, 8,
            address => (address - 0xb4bf) % 12 is 2 or 6,
            i => { var w = DragonFireballInstructionProgramDefinitions.MechanicsWord(i); return (w.Address, w.Value); },
            DragonFireballInstructionProgramDefinitions.PresentationWordAddress,
            DragonFireballInstructionProgramDefinitions.ReadMechanicsWord,
            DragonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte);
        Check(0xa60000, 0x8d1b, 0x8daf, 50, 24,
            address => address == 0x8dab || address >= 0x8d1f && address <= 0x8da3 && (address - 0x8d1f) % 6 == 0,
            i => { var w = HibashiInstructionProgramDefinitions.MechanicsWord(i); return (w.Address, w.Value); },
            HibashiInstructionProgramDefinitions.PresentationWordAddress,
            HibashiInstructionProgramDefinitions.ReadMechanicsWord,
            HibashiInstructionProgramDefinitions.IsCompiledMechanicsByte);
        void Check(int bank, int first, int end, int count, int visualCount, Func<int, bool> isVisual,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns)
        {
            int m = 0, p = 0;
            for (int address = first; address < end; address += 2)
            {
                bool visual = isVisual(address);
                if (visual)
                {
                    AssertEqual((ushort)address, presentation(p++), "Hibashi/Dragon native presentation order");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "Hibashi/Dragon presentation excluded from mechanics");
                }
                else
                {
                    var word = mechanics(m++);
                    AssertEqual((ushort)address, word.Address, "Hibashi/Dragon native control order");
                    ushort value = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
                    AssertEqual(value, word.Value, "Hibashi/Dragon original native control value");
                    AssertEqual(value, read((ushort)address), "Hibashi/Dragon direct control read");
                }
                AssertEqual(!visual, owns(bank | address), "Hibashi/Dragon low-byte ownership");
                AssertEqual(!visual, owns(bank | (address + 1)), "Hibashi/Dragon high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "Hibashi/Dragon unaligned reads rejected");
            }
            AssertEqual(count, m, "Hibashi/Dragon control count");
            AssertEqual(visualCount, p, "Hibashi/Dragon presentation count");
            foreach (int invalid in new[] { -1, count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "Hibashi/Dragon control index domain");
            foreach (int invalid in new[] { -1, visualCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "Hibashi/Dragon presentation index domain");
            foreach (int invalid in new[] { bank | (first - 1), bank | end, (bank ^ 0x10000) | first })
                AssertTrue(!owns(invalid), "Hibashi/Dragon external byte domain");
        }
    }
}