using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRidleyExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        ReadOnlySpan<ushort> spawnPointerOperands =
        [
            0xc933, 0xc93a, 0xc941, 0xc948, 0xc94f, 0xc956,
            0xc95d, 0xc964, 0xc96b, 0xc972, 0xc979, 0xc980,
        ];
        AssertEqual(spawnPointerOperands.Length, RidleyExplosionDefinitions.SpawnOrder.Length,
            "Ridley breakup spawn count");
        for (int index = 0; index < spawnPointerOperands.Length; index++)
        {
            ushort populationPointer = Word(rom, 0xa60000 | spawnPointerOperands[index]);
            ushort parameter = Word(rom, 0xa60000 | unchecked((ushort)(populationPointer + 12)));
            AssertEqual(parameter, RidleyExplosionDefinitions.SpawnOrder[index],
                $"Ridley breakup native spawn order {index}");
        }

        for (ushort parameter = 0; parameter <= RidleyExplosionParts.Claw; parameter += 2)
        {
            RidleyExplosionPartDefinition definition =
                RidleyExplosionDefinitions.GetPart(parameter);
            AssertEqual(parameter, definition.Parameter,
                $"Ridley breakup parameter ${parameter:X2}");
            AssertEqual(Word(rom, 0xa6c6ce + parameter), definition.Lifetime,
                $"Ridley breakup lifetime ${parameter:X2}");
            AssertEqual(Word(rom, 0xa6c6e6 + parameter), definition.InitializationRoutine,
                $"Ridley breakup initializer ${parameter:X2}");
        }

        int[] fixedTailOperands = [0xa6c710, 0xa6c728, 0xa6c740, 0xa6c758, 0xa6c770, 0xa6c788];
        for (int tailIndex = 0; tailIndex < fixedTailOperands.Length; tailIndex++)
        {
            ushort parameter = unchecked((ushort)(tailIndex * 2));
            AssertEqual(
                Word(rom, fixedTailOperands[tailIndex]),
                RidleyExplosionDefinitions.SelectTailInstructionList(parameter, 0),
                $"Ridley tail fragment instruction ${parameter:X2}");
        }
        for (int orientation = 0; orientation < 16; orientation++)
        {
            AssertEqual(
                Word(rom, 0xa6c7ba + orientation * 2),
                RidleyExplosionDefinitions.SelectTailInstructionList(
                    RidleyExplosionParts.TailTip,
                    orientation),
                $"Ridley tail-tip orientation {orientation}");
        }

        ushort[] bodyParameters =
        [
            RidleyExplosionParts.Wings,
            RidleyExplosionParts.Legs,
            RidleyExplosionParts.OpenHeadAndNeck,
            RidleyExplosionParts.Torso,
            RidleyExplosionParts.Claw,
        ];
        int[] xTables = [0xa6c804, 0xa6c836, 0xa6c868, 0xa6c89a, 0xa6c8cc];
        int[] yOperands = [0xa6c7f4, 0xa6c826, 0xa6c858, 0xa6c88a, 0xa6c8bc];
        int[] instructionTables = [0xa6c808, 0xa6c83a, 0xa6c86c, 0xa6c89e, 0xa6c8d0];
        for (int partIndex = 0; partIndex < bodyParameters.Length; partIndex++)
        for (int facingIndex = 0; facingIndex < 2; facingIndex++)
        {
            RidleyExplosionBodyPartDefinition definition =
                RidleyExplosionDefinitions.SelectBodyPart(
                    bodyParameters[partIndex],
                    facingRight: facingIndex != 0);
            AssertEqual(
                unchecked((short)Word(rom, xTables[partIndex] + facingIndex * 2)),
                definition.XOffset,
                $"Ridley body fragment {partIndex} facing {facingIndex} X offset");
            AssertEqual(
                unchecked((short)Word(rom, yOperands[partIndex])),
                definition.YOffset,
                $"Ridley body fragment {partIndex} Y offset");
            AssertEqual(
                Word(rom, instructionTables[partIndex] + facingIndex * 2),
                definition.InstructionList,
                $"Ridley body fragment {partIndex} facing {facingIndex} instruction");
        }

        for (int index = 0; index < 10; index++)
        {
            RidleyDeathExplosionPlacement placement =
                RidleyExplosionDefinitions.DeathExplosionPlacement(index);
            AssertEqual(
                unchecked((short)Word(rom, 0xa6c66e + index * 4)),
                placement.XOffset,
                $"Ridley death explosion {index} X offset");
            AssertEqual(
                unchecked((short)Word(rom, 0xa6c670 + index * 4)),
                placement.YOffset,
                $"Ridley death explosion {index} Y offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => RidleyExplosionDefinitions.DeathExplosionPlacement(-1),
            "Ridley death explosion negative index");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RidleyExplosionDefinitions.DeathExplosionPlacement(10),
            "Ridley death explosion index after authored cycle");

        VerifyRidleyExplosionProductionInitializer(rom);
        VerifyRidleyDeathExplosionProductionSpawns(rom);
        Console.WriteLine(
            "Ridley breakup definitions: all 12 lifetimes/callbacks, native spawn order, " +
            "six fixed tail selectors, 16 tip orientations, ten body records, ten death " +
            "explosion placements, 32 production initializations, and ten real death-effect " +
            "spawns match the cartridge with migrated reads forbidden.");
    }

    private static void VerifyRidleyDeathExplosionProductionSpawns(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int selectedIndex = 0; selectedIndex < 10; selectedIndex++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new RidleyExplosionDefinitionReadGuard(rom));
            var spawn = typeof(RoomEnemySystem)
                .GetMethod("SpawnSmallExplosionNearNorfairRidley", flags)!
                .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>(enemies);

            RoomEnemySlot body = enemies.Slots[0];
            body.XPosition = 0xfff8;
            body.YPosition = 0x0008;
            var state = new RidleyEnemyState
            {
                DeathExplosionTimer = 0,
                DeathExplosionCount = unchecked((ushort)((selectedIndex + 9) % 10)),
            };

            spawn(body, state);

            RoomEnemyProjectileSlot effect = enemies.EnemyProjectiles.Single(p => p.IsActive);
            short xOffset = unchecked((short)ReadRidleyExplosionWord(
                rom,
                0xa6c66e + selectedIndex * 4));
            short yOffset = unchecked((short)ReadRidleyExplosionWord(
                rom,
                0xa6c670 + selectedIndex * 4));
            AssertEqual((ushort)selectedIndex, state.DeathExplosionCount,
                $"Ridley death explosion {selectedIndex} cyclic index");
            AssertEqual((ushort)4, state.DeathExplosionTimer,
                $"Ridley death explosion {selectedIndex} timer reload");
            AssertEqual(unchecked((ushort)(body.XPosition + xOffset)), effect.XPosition,
                $"Ridley death explosion {selectedIndex} production X");
            AssertEqual(unchecked((ushort)(body.YPosition + yOffset)), effect.YPosition,
                $"Ridley death explosion {selectedIndex} production Y");
            AssertEqual(RoomEnemyProjectileKind.MiscDustExplosion, effect.Kind,
                $"Ridley death explosion {selectedIndex} actor kind");
            AssertEqual((ushort)0x0024, state.LastDeathSoundEffect,
                $"Ridley death explosion {selectedIndex} sound");
        }
    }

    private static void VerifyRidleyExplosionProductionInitializer(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new RidleyExplosionDefinitionReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x8130));
        var initialize = typeof(RoomEnemySystem)
            .GetMethod("InitializeNorfairRidleyExplosion", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot, RidleyEnemyState>>(enemies);

        RoomEnemySlot fragment = enemies.Slots[1];
        RoomEnemySlot body = enemies.Slots[0];
        body.XPosition = 0xfff8;
        body.YPosition = 0x0008;
        var state = new RidleyEnemyState
        {
            TailSegments = Enumerable.Range(0, 7)
                .Select(index => new RidleyTailSegment
                {
                    XPosition = unchecked((ushort)(0x1200 + index * 0x11)),
                    YPosition = unchecked((ushort)(0x3400 + index * 0x13)),
                })
                .ToArray(),
        };

        int cases = 0;
        for (ushort parameter = 0; parameter <= RidleyExplosionParts.Claw; parameter += 2)
        {
            int orientationCount = parameter == RidleyExplosionParts.TailTip ? 16 : 1;
            int facingCount = parameter > RidleyExplosionParts.TailTip ? 2 : 1;
            for (int facingIndex = 0; facingIndex < facingCount; facingIndex++)
            for (int orientation = 0; orientation < orientationCount; orientation++)
            {
                fragment.Clear();
                fragment.Parameter1 = parameter;
                state.FacingDirection = unchecked((ushort)facingIndex);
                state.TailSegments[5].Angle = 0;
                state.TailSegments[6].Angle = unchecked((ushort)(orientation << 4));

                initialize(fragment, body, state);

                AssertEqual((ushort)1, fragment.InstructionTimer,
                    "Ridley breakup production instruction timer");
                AssertEqual(EnemyPaletteBits.Palette7, fragment.PaletteIndex,
                    "Ridley breakup production palette");
                AssertEqual(
                    ReadRidleyExplosionWord(rom, 0xa6c6ce + parameter),
                    fragment.VariableF,
                    $"Ridley breakup production lifetime ${parameter:X2}");
                AssertEqual(unchecked((ushort)-0x0130), fragment.VariableB,
                    "Ridley breakup production signed random velocity");

                if (parameter <= RidleyExplosionParts.TailTip)
                {
                    int tailIndex = parameter >> 1;
                    AssertEqual(state.TailSegments[tailIndex].XPosition, fragment.XPosition,
                        $"Ridley breakup production tail X ${parameter:X2}");
                    AssertEqual(state.TailSegments[tailIndex].YPosition, fragment.YPosition,
                        $"Ridley breakup production tail Y ${parameter:X2}");
                    ushort expectedInstruction = parameter == RidleyExplosionParts.TailTip
                        ? ReadRidleyExplosionWord(rom, 0xa6c7ba + orientation * 2)
                        : ReadRidleyExplosionWord(
                            rom,
                            new[] { 0xa6c710, 0xa6c728, 0xa6c740, 0xa6c758, 0xa6c770, 0xa6c788 }
                                [tailIndex]);
                    AssertEqual(expectedInstruction, fragment.CurrentInstruction,
                        $"Ridley breakup production tail instruction ${parameter:X2}/{orientation}");
                }
                else
                {
                    int bodyIndex = (parameter - RidleyExplosionParts.Wings) >> 1;
                    int[] xTables = [0xa6c804, 0xa6c836, 0xa6c868, 0xa6c89a, 0xa6c8cc];
                    int[] yOperands = [0xa6c7f4, 0xa6c826, 0xa6c858, 0xa6c88a, 0xa6c8bc];
                    int[] instructionTables = [0xa6c808, 0xa6c83a, 0xa6c86c, 0xa6c89e, 0xa6c8d0];
                    short xOffset = unchecked((short)ReadRidleyExplosionWord(
                        rom,
                        xTables[bodyIndex] + facingIndex * 2));
                    short yOffset = unchecked((short)ReadRidleyExplosionWord(
                        rom,
                        yOperands[bodyIndex]));
                    AssertEqual(unchecked((ushort)(body.XPosition + xOffset)), fragment.XPosition,
                        $"Ridley breakup production body X ${parameter:X2}/{facingIndex}");
                    AssertEqual(unchecked((ushort)(body.YPosition + yOffset)), fragment.YPosition,
                        $"Ridley breakup production body Y ${parameter:X2}/{facingIndex}");
                    AssertEqual(
                        ReadRidleyExplosionWord(
                            rom,
                            instructionTables[bodyIndex] + facingIndex * 2),
                        fragment.CurrentInstruction,
                        $"Ridley breakup production body instruction ${parameter:X2}/{facingIndex}");
                }
                cases++;
            }
        }
        AssertEqual(32, cases, "Ridley breakup production initializer case count");
    }

    private static ushort ReadRidleyExplosionWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class RidleyExplosionDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is
            >= 0xa6c6ce and < 0xa6c6fe or
            >= 0xa6c66e and < 0xa6c696 or
            >= 0xa6c7ba and < 0xa6c7da or
            >= 0xa6c804 and < 0xa6c80c or
            >= 0xa6c836 and < 0xa6c83e or
            >= 0xa6c868 and < 0xa6c870 or
            >= 0xa6c89a and < 0xa6c8a2 or
            >= 0xa6c8cc and < 0xa6c8d4
                ? throw new InvalidOperationException(
                    $"Ridley breakup attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
