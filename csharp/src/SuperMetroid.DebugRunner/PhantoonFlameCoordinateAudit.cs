using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;

/// <summary>Compare the production flame-position helper with original 65816 $86:9BA2, not another host implementation.</summary>
internal static class PhantoonFlameCoordinateAudit
{
    public static int Run(string rom, string csv)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(csv))) !=
            "2B3ECC25B05166235C9B5D3EE88A309D93F10F6A1E97E75652386175C0E37464")
            throw new InvalidDataException("Expected the complete original-CPU flame coordinate capture.");
        var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
        var body = runtime.Enemies.Phantoon!.Body;
        body.XPosition = 128; body.YPosition = 112;
        var flame = runtime.Enemies.EnemyProjectiles[0];
        var position = typeof(RoomEnemySystem).GetMethod("PositionPhantoonFlameAroundBody", BindingFlags.NonPublic | BindingFlags.Instance)!;
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(csv).Skip(1))
        {
            ushort[] row = line.Split(',').Select(ushort.Parse).ToArray();
            position.Invoke(runtime.Enemies, [flame, body, row[0], row[1]]);
            ushort x = unchecked((ushort)(128 + row[2])), y = unchecked((ushort)(128 + row[3]));
            if (flame.XPosition != x || flame.YPosition != y)
            {
                if (failures++ < 8) Console.WriteLine($"angle={row[0]}, radius={row[1]}: managed ({flame.XPosition},{flame.YPosition}), native ({x},{y}).");
            }
            cases++;
        }
        Console.WriteLine($"Phantoon native coordinate comparison: {cases} cases, {failures} mismatches.");
        if (cases != 65536 || failures != 0) throw new InvalidDataException("Flame coordinate parity failed.");
        return 0;
    }
}
