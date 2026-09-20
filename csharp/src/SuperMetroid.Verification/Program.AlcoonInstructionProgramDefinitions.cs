using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private const ushort AlcoonInstructionAuditRoom = 0x93aa;

    private static void VerifyAlcoonInstructionProgramDefinitions()
    {
        VerifyAlcoonInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyAlcoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < AlcoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            AlcoonInstructionMechanicsWord definition =
                AlcoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadAlcoonInstructionWord(rom, definition.Address),
                $"Alcoon mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new AlcoonInstructionReadGuard(rom);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(rom, AlcoonInstructionAuditRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(rom, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            guard,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData);

        RoomEnemySlot actor = enemies.Slots[0];
        AlcoonEnemyState state = enemies.AlcoonStates[0] ??
            throw new InvalidDataException("Real Alcoon initializer omitted slot-zero state.");
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = unchecked((ushort)(actor.XPosition + 1)),
            YPosition = state.LandingYPosition,
        };
        samus.RefreshCollisionRadii(rom);
        samus.InitializeAnimation(rom);

        bool sawLeftFireball = false;
        bool sawRightFireball = false;
        for (int frame = 0; frame < 4_096 &&
             (guard.ObservedPresentationWords.Count < 42 ||
              !sawLeftFireball || !sawRightFireball); frame++)
        {
            int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
            ushort cameraX = unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX));
            enemies.StepFrame(cameraX, 0, false, samus, level: assets.LevelData);

            samus.XPosition = unchecked((ushort)(actor.XPosition +
                (unchecked((short)state.XVelocity) < 0 ? -32 : 32)));
            samus.YPosition = state.LandingYPosition;
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                sawLeftFireball |= unchecked((short)projectile.XVelocity) < 0;
                sawRightFireball |= unchecked((short)projectile.XVelocity) > 0;
            }
            enemies.StepEnemyProjectiles(assets.LevelData, samus: null, cameraX: 0, cameraY: 0);
        }

        AssertTrue(sawLeftFireball && sawRightFireball,
            "real Alcoon programs execute fire callbacks in both facings");
        AssertEqual(42, guard.ObservedPresentationWords.Count,
            "all reachable Alcoon spritemap operands remain cartridge reads");
        AssertTrue(!guard.ObservedPresentationWords.Contains(0xdc49) &&
                   !guard.ObservedPresentationWords.Contains(0xdcb9),
            "native start-walking callbacks skip the two trailing unreachable frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Alcoon mechanics byte");
        AssertThrows<InvalidDataException>(
            () => AlcoonInstructionProgramDefinitions.ReadMechanicsWord(0xdbeb),
            "Alcoon spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => AlcoonInstructionProgramDefinitions.ReadMechanicsWord(0xdcc7),
            "Alcoon constants following the programs are rejected as mechanics");

        _ = ProbeAlcoonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeAlcoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Alcoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Alcoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Alcoon instruction mechanics: 68 compiled words, all ten authored programs, " +
            "42 reachable live spritemap reads, both fire directions, and native callback " +
            "handoffs pass with mechanics bytes forbidden.");
    }

    private static int ProbeAlcoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += AlcoonInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 10) switch
                {
                    0 => AlcoonInstructionProgramDefinitions.WalkingLeft,
                    1 => AlcoonInstructionProgramDefinitions.FireLeft,
                    2 => AlcoonInstructionProgramDefinitions.AirborneLeftLookingUp,
                    3 => AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward,
                    4 => AlcoonInstructionProgramDefinitions.WalkingRight,
                    5 => AlcoonInstructionProgramDefinitions.FireRight,
                    6 => AlcoonInstructionProgramDefinitions.AirborneRightLookingUp,
                    7 => AlcoonInstructionProgramDefinitions.AirborneRightLookingForward,
                    8 => 0xdbff,
                    _ => 0xdc6f,
                });
        }
        return checksum;
    }

    private static ushort ReadAlcoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class AlcoonInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (AlcoonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Alcoon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < AlcoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        AlcoonInstructionProgramDefinitions.PresentationWordAddress(index);
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
