using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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

        Console.WriteLine(
            "Tourian statue eye positions: eight native words and all eight real eye/soul spawns pass with position tables forbidden.");
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
}
