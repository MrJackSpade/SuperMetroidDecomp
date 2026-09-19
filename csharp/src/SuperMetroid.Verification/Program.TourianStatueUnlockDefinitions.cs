using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyTourianStatueUnlockDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        {
            TourianStatueEyePosition position =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            AssertEqual(ReadTourianEyeWord(rom, 0x86b90e + parameter), position.X,
                $"Tourian statue eye X parameter {parameter}");
            AssertEqual(ReadTourianEyeWord(rom, 0x86b916 + parameter), position.Y,
                $"Tourian statue eye Y parameter {parameter}");
        }

        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField(
            "_cgram", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new TourianStatueEyeReadGuard(rom);
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        for (int soulValue = 0; soulValue < 2; soulValue++)
        {
            bool soul = soulValue != 0;
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            cgramField.SetValue(enemies, new SnesCgram());
            enemies.SpawnTourianUnlockEffect(parameter, soul);

            TourianStatueEyePosition expected =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(expected.X, projectile.XPosition,
                $"production Tourian unlock X {parameter}/{soul}");
            AssertEqual(expected.Y, projectile.YPosition,
                $"production Tourian unlock Y {parameter}/{soul}");
            AssertEqual(soul ? TourianStatueRomData.Soul : TourianStatueRomData.EyeGlow,
                (ushort)projectile.Kind,
                $"production Tourian unlock kind {parameter}/{soul}");
            AssertEqual(soul ? 0xfc00 : 0,
                projectile.YVelocity,
                $"production Tourian unlock velocity {parameter}/{soul}");
        }

        foreach (ushort parameter in new ushort[] { 1, 3, 5, 7, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => TourianStatueUnlockDefinitions.EyePosition(parameter),
                $"invalid Tourian statue parameter {parameter}");
        }

        VerifyTourianStatueAnimatedTileMechanics(rom);

        Console.WriteLine(
            "Tourian statue eye positions: eight native words and all eight real eye/soul spawns pass with position tables forbidden.");
    }

    private static void VerifyTourianStatueAnimatedTileMechanics(
        SuperMetroidAddressSpace rom)
    {
        int mechanicsWordCount = 0;
        int presentationWordCount = 0;
        foreach (TourianStatueAnimatedTileProgramDefinition definition in
                 TourianStatueAnimatedTileMechanicsDefinitions.All)
        {
            for (int headerOffset = 0; headerOffset < 6; headerOffset += 2)
            {
                ushort pointer = unchecked((ushort)(definition.ObjectPointer + headerOffset));
                VerifyTourianStatueMechanicsWord(definition, rom, pointer);
                mechanicsWordCount++;
            }

            for (int programOffset = 0; programOffset <= 0x66; programOffset += 2)
            {
                ushort pointer = unchecked((ushort)(definition.ProgramStart + programOffset));
                if (definition.SourceOperandPointers.Contains(pointer))
                {
                    AssertTrue(!definition.TryReadMechanicsWord(pointer, out _),
                        $"statue $87:{definition.ObjectPointer:X4} leaves source " +
                        $"$87:{pointer:X4} presentation-owned");
                    presentationWordCount++;
                    continue;
                }

                VerifyTourianStatueMechanicsWord(definition, rom, pointer);
                mechanicsWordCount++;
            }
        }

        AssertEqual(4, TourianStatueAnimatedTileMechanicsDefinitions.All.Count,
            "Tourian statue animated-tile object count");
        AssertEqual(184, mechanicsWordCount,
            "Tourian statue compiled mechanics word count");
        AssertEqual(36, presentationWordCount,
            "Tourian statue live presentation-source word count");

        var guarded = new TourianStatueMechanicsForbiddenBus(rom);
        var runtime = new SuperMetroidRuntime(guarded, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertTrue(runtime.TourianStatues.Enabled,
            "retail Tourian statue room enables the production sequence");
        runtime.VramWrites.DrainTo(runtime.Vram, guarded);

        int steps = 0;
        for (; steps < 3000 && !TourianStatueGreyEventsAreSet(runtime); steps++)
        {
            runtime.TourianStatues.StepTiles(runtime);
            runtime.VramWrites.DrainTo(runtime.Vram, guarded);
        }

        AssertTrue(TourianStatueGreyEventsAreSet(runtime),
            "production Tourian sequence releases all four defeated-boss statues");
        AssertTrue(steps < 3000, "production Tourian sequence remains bounded");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production Tourian sequence performs no mechanics ROM reads");
        AssertEqual(36, guarded.ObservedSourceOperands.Count,
            "production Tourian sequence reads every live frame-source operand");
        Console.WriteLine(
            $"Tourian statue animated tiles: 184 mechanics words compiled, 36 source " +
            $"pointers live, and the real four-statue release completes in {steps} calls.");
    }

    private static bool TourianStatueGreyEventsAreSet(SuperMetroidRuntime runtime) =>
        runtime.System.HasEventRaw(0x0006) &&
        runtime.System.HasEventRaw(0x0007) &&
        runtime.System.HasEventRaw(0x0008) &&
        runtime.System.HasEventRaw(0x0009);

    private static void VerifyTourianStatueMechanicsWord(
        TourianStatueAnimatedTileProgramDefinition definition,
        ISnesAddressSpace rom,
        ushort pointer)
    {
        AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort compiled),
            $"statue $87:{definition.ObjectPointer:X4} catalogs $87:{pointer:X4}");
        AssertEqual(
            RomDataReader.ReadWordFixedBank(
                rom, RoomFxRomData.Banks.AnimatedTiles | pointer),
            compiled,
            $"statue $87:{definition.ObjectPointer:X4} cartridge $87:{pointer:X4}");
    }

    private static ushort ReadTourianEyeWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class TourianStatueEyeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86b90e and < 0x86b91e
                ? throw new InvalidOperationException(
                    $"Tourian statue unlock attempted migrated eye-position read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class TourianStatueMechanicsForbiddenBus(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public HashSet<ushort> ObservedSourceOperands { get; } = [];

        public byte ReadByte(int address)
        {
            SnesAddress snesAddress = SnesAddress.FromBusAddress(address);
            if (snesAddress.Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16))
            {
                foreach (TourianStatueAnimatedTileProgramDefinition definition in
                         TourianStatueAnimatedTileMechanicsDefinitions.All)
                {
                    if (definition.TryReadMechanicsWord(snesAddress.Offset, out _) ||
                        definition.TryReadMechanicsWord(
                            unchecked((ushort)(snesAddress.Offset - 1)), out _))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Tourian statue mechanics byte {snesAddress}.");
                    }

                    foreach (ushort sourcePointer in definition.SourceOperandPointers)
                    {
                        if (snesAddress.Offset == sourcePointer ||
                            snesAddress.Offset == unchecked((ushort)(sourcePointer + 1)))
                        {
                            ObservedSourceOperands.Add(sourcePointer);
                        }
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
