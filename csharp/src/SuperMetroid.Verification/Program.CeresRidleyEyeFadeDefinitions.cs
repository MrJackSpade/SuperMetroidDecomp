using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresRidleyEyeFadeDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            CeresRidleyEyeFadeStep compiled = CeresRidleyEyeFadeDefinitions.Get(offset);
            AssertTrue(!compiled.IsComplete,
                $"Ceres Ridley eye-fade offset {offset} remains active");
            AssertEqual(
                rom.ReadByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress + offset),
                compiled.PaletteRow,
                $"Ceres Ridley eye-fade row {offset}");
        }

        AssertEqual((byte)0xff,
            rom.ReadByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                CeresRidleyEyeFadeDefinitions.PaletteStepCount),
            "Ceres Ridley eye-fade native terminator");
        AssertTrue(
            CeresRidleyEyeFadeDefinitions.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount).IsComplete,
            "Ceres Ridley eye-fade compiled terminator");
        AssertThrows<InvalidDataException>(
            () => CeresRidleyEyeFadeDefinitions.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount + 1),
            "Ceres Ridley eye fade rejects restored offsets beyond the authored schedule");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new CeresRidleyEyeFadeReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var cgram = new SnesCgram();
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        var tick = typeof(RoomEnemySystem).GetMethod(
            "TickCeresRidleyEyeFade", flags)!
            .CreateDelegate<Action<RidleyEnemyState>>(enemies);

        var state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.FadeInEyes,
            MovementAnimationEnabled = 0,
        };
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            state.FadePaletteOffset = offset;
            state.FunctionTimer = 0;
            tick(state);

            CeresRidleyEyeFadeStep expected = CeresRidleyEyeFadeDefinitions.Get(offset);
            AssertEqual(unchecked((ushort)(offset + 1)), state.FadePaletteOffset,
                $"production Ceres eye-fade offset {offset} advances");
            AssertEqual(RidleyAiFunction.FadeInEyes, state.Function,
                $"production Ceres eye-fade offset {offset} remains in fade phase");
            for (int color = 0; color < 3; color++)
            {
                int source = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows +
                    expected.PaletteRow * 6 + color * 2;
                ushort expectedColor = unchecked((ushort)(
                    rom.ReadByte(source) | rom.ReadByte(source + 1) << 8));
                AssertEqual(expectedColor, cgram.Colors[252 + color],
                    $"production Ceres eye-fade offset {offset} color {color}");
            }
        }

        state.FadePaletteOffset = CeresRidleyEyeFadeDefinitions.PaletteStepCount;
        state.FunctionTimer = 0;
        tick(state);
        AssertEqual((ushort)0, state.FadePaletteOffset,
            "production Ceres eye-fade terminator resets its offset");
        AssertEqual(RidleyAiFunction.FadeInBody, state.Function,
            "production Ceres eye-fade terminator enters body fade");
        AssertEqual((ushort)1, state.MovementAnimationEnabled,
            "production Ceres eye-fade terminator enables movement animation");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres eye fade performs no schedule reads");

        Console.WriteLine(
            "Ceres Ridley eye fade: all 64 row selections, the terminal handoff and " +
            "the complete production schedule pass with selector ROM reads forbidden.");
    }

    private sealed class CeresRidleyEyeFadeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress &&
                address <= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                    CeresRidleyEyeFadeDefinitions.PaletteStepCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ceres Ridley eye fade attempted schedule read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
