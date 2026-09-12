using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledEnemyFireballLaunches(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort offset = 0; offset <= 4; offset += 2)
            AssertEqual(Word(0x869ef9 + offset), EnemyFireballLaunchDefinitions.AlcoonYVelocity(offset), "Alcoon native launch");
        for (int high = 0; high < 256; high++)
            for (int index = 0; index < 8; index++)
            {
                var pair = EnemyFireballLaunchDefinitions.NamiFuneVelocities((ushort)((high << 8) | index));
                AssertEqual(Word(0x86deb6 + index * 4), pair.Left, "NamiFune native left velocity");
                AssertEqual(Word(0x86deb8 + index * 4), pair.Right, "NamiFune native right velocity");
            }
        AssertThrows<InvalidDataException>(() => EnemyFireballLaunchDefinitions.AlcoonYVelocity(1), "Alcoon odd selector");
        AssertThrows<InvalidDataException>(() => EnemyFireballLaunchDefinitions.NamiFuneVelocities(8), "NamiFune out-of-table selector");
        Console.WriteLine("Enemy fireball launches: 19 native words and all 2048 valid high-byte/selector combinations match.");
    }
}
