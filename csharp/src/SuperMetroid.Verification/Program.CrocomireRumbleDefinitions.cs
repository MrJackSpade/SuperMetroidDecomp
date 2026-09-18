using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireRumbleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa498ca;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        var expectedWords = new ushort[32];
        var assigned = new bool[32];
        foreach (CrocomireRumbleDefinition definition in CrocomireRumbleDefinitions.All)
        {
            int targetIndex = definition.TableOffset >> 1;
            expectedWords[targetIndex] = unchecked((ushort)definition.TargetYOffset);
            assigned[targetIndex] = true;
            if (!definition.HasTiming)
                continue;

            expectedWords[targetIndex + 1] = definition.Cooldown;
            expectedWords[targetIndex + 2] = definition.Delta;
            assigned[targetIndex + 1] = true;
            assigned[targetIndex + 2] = true;
        }

        for (int index = 0; index < expectedWords.Length; index++)
        {
            AssertTrue(assigned[index], $"Crocomire rumble source word {index} ownership");
            AssertEqual(ReadWord(rom, sourceAddress + index * 2), expectedWords[index],
                $"Crocomire rumble source word {index}");
        }

        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(1),
            "Crocomire odd rumble offset");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(8),
            "Crocomire timing word cannot become a target");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(0x40),
            "Crocomire rumble offset outside table");

        var guarded = new CrocomireRumbleReadGuard(rom);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var death = new CrocomireDeathState
        {
            RumbleYOffset = 0,
            RumbleCooldown = 10,
            RumbleDelta = 1,
        };
        RoomEnemySlot body = enemies.Slots[0];
        var state = new CrocomireEnemyState(body)
        {
            DeathSequenceIndex = CrocomireDeathPhases.RumbleHiddenWall,
            StepCounter = 4,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
        MethodInfo runRumble = typeof(RoomEnemySystem).GetMethod(
            "RunCrocomireWallRumble",
            flags)!;

        ushort referenceIndex = state.StepCounter;
        ushort referenceY = death.RumbleYOffset;
        ushort referenceCooldown = death.RumbleCooldown;
        ushort referenceDelta = death.RumbleDelta;
        ushort referenceDeathIndex = state.DeathSequenceIndex;
        int frames = 0;
        while (referenceIndex != 0x0080 && frames < 2048)
        {
            StepCrocomireRumbleReference(
                rom,
                ref referenceIndex,
                ref referenceY,
                ref referenceCooldown,
                ref referenceDelta,
                ref referenceDeathIndex);
            runRumble.Invoke(enemies, [state]);
            frames++;

            AssertEqual(referenceIndex, state.StepCounter,
                $"Crocomire rumble frame {frames} target index");
            AssertEqual(referenceY, death.RumbleYOffset,
                $"Crocomire rumble frame {frames} Y offset");
            AssertEqual(referenceCooldown, death.RumbleCooldown,
                $"Crocomire rumble frame {frames} cooldown");
            AssertEqual(referenceDelta, death.RumbleDelta,
                $"Crocomire rumble frame {frames} delta");
            AssertEqual(referenceDeathIndex, state.DeathSequenceIndex,
                $"Crocomire rumble frame {frames} death index");
        }

        AssertTrue(frames < 2048, "Crocomire rumble production sequence terminates");
        AssertEqual((ushort)0x0080, state.StepCounter,
            "Crocomire rumble production terminator handoff");
        AssertEqual((ushort)0x8080, death.RumbleYOffset,
            "Crocomire rumble production terminator Y marker");

        Console.WriteLine(
            $"Crocomire rumble definitions: all 32 native words and {frames} exact production frames pass with the source stream forbidden.");
    }

    private static void StepCrocomireRumbleReference(
        ISnesAddressSpace rom,
        ref ushort index,
        ref ushort yOffset,
        ref ushort cooldown,
        ref ushort delta,
        ref ushort deathIndex)
    {
        const int sourceAddress = 0xa498ca;
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        ushort target = ReadWord(rom, sourceAddress + index);
        if (target == 0x8080)
        {
            yOffset = 0x8080;
            index = 0x0080;
            deathIndex += 2;
            return;
        }

        if (yOffset == target)
        {
            if (unchecked((short)target) < 0)
            {
                if (cooldown != 0)
                {
                    cooldown--;
                    index -= 2;
                    return;
                }

                index += 2;
                cooldown = ReadWord(rom, sourceAddress + index);
                index += 2;
                delta = ReadWord(rom, sourceAddress + index);
            }

            index += 2;
            return;
        }

        yOffset = unchecked((short)(yOffset - target)) >= 0
            ? unchecked((ushort)(yOffset - delta))
            : unchecked((ushort)(yOffset + delta));
    }

    private sealed class CrocomireRumbleReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa498ca and < 0xa4990a
                ? throw new InvalidOperationException(
                    $"Crocomire rumble attempted migrated stream read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
