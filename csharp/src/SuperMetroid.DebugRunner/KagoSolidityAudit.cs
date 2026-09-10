using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Compares the platform collision boundary against original CPU execution. This
/// deliberately isolates solidity; it does not simulate platform AI or establish
/// which controller timing produces penetration during an actual Kago.
/// </summary>
internal static class KagoSolidityAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "BC8C4237CCB67A556647F567E6429B0AD71E8A0F9D0FF25152B6143DD95B8C15")
            throw new InvalidDataException("Use the accepted native Kago solidity capture.");
        var rows = File.ReadAllLines(capture);
        if (rows.Length != 8641)
            throw new InvalidDataException("Incomplete Kago solidity matrix.");
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int rowIndex = 1, mismatches = 0, embeddedPasses = 0, touchingStops = 0;
        foreach (ushort definition in KagoSolidityFixtureData.PlatformDefinitions)
        foreach (byte pose in KagoSolidityFixtureData.Poses)
        foreach (SamusCollisionDirection direction in Enum.GetValues<SamusCollisionDirection>())
        for (int gap = -2; gap <= 2; gap++)
        foreach (ushort fraction in KagoSolidityFixtureData.Fractions)
        for (int mode = 0; mode < 3; mode++)
        {
            var samus = new SamusState { Pose = pose };
            samus.RefreshCollisionRadii(bus);
            var state = samus.Kinematics;
            state.XPosition = state.YPosition = 1024;
            state.XSubposition = state.YSubposition = fraction;
            ushort enemyWidth = ReadWord(bus, 0xa00000 + definition + 8);
            ushort enemyHeight = ReadWord(bus, 0xa00000 + definition + 10);
            bool horizontal = direction is SamusCollisionDirection.Left or SamusCollisionDirection.Right;
            int distance = (horizontal ? enemyWidth + state.XRadius : enemyHeight + state.YRadius) + gap;
            int signedDistance = direction is SamusCollisionDirection.Left or SamusCollisionDirection.Up
                ? -distance : distance;
            var body = new SolidEnemyCollisionBody(0,
                (ushort)(1024 + (horizontal ? signedDistance : 0)),
                (ushort)(1024 + (horizontal ? 0 : signedDistance)),
                enemyWidth, enemyHeight,
                FreezeTimer: (ushort)(mode == 2 ? 1 : 0),
                Properties: mode == 0 ? (ushort)EnemyProperties.SolidToSamus : (ushort)0);
            var result = SamusSolidEnemyCollision.Probe(state, [body], direction, 2, 0x8000);
            string actual = $"{definition:X4},{pose:X2},{(int)direction},{gap},{fraction:X4},{mode}," +
                $"{(result.Collided ? 0xffff : 0):X4},{result.Distance:X4},{result.DistanceSubposition:X4}," +
                $"{state.YSubposition:X4},{result.EnemyIndex ?? 0xffff:X4}";
            if (actual != rows[rowIndex++])
            {
                if (mismatches++ < 12)
                    Console.WriteLine($"Kago solidity: {actual} != {rows[rowIndex - 1]}");
            }
            if (mode == 1 && result.Collided)
                throw new InvalidDataException("An unfrozen, non-solid actor became a platform.");
            if (gap < 0 && mode != 1)
            {
                if (result.Collided || result.Distance != 2 || result.DistanceSubposition != 0x8000)
                    throw new InvalidDataException("Existing platform penetration must permit the requested move.");
                embeddedPasses++;
            }
            if (gap == 0 && mode != 1)
            {
                if (!result.Collided || result.Distance != 0 || result.DistanceSubposition != 0 ||
                    state.YSubposition != 0)
                    throw new InvalidDataException("Touching platform edge must stop movement and clear Y fraction.");
                touchingStops++;
            }
        }
        if (rowIndex != rows.Length || embeddedPasses != 2304 || touchingStops != 1152)
            throw new InvalidDataException("Kago solidity coverage changed.");
        Console.WriteLine($"Kago solidity: {rowIndex - 1} probes, {mismatches} mismatches; " +
            $"{embeddedPasses} embedded passes, {touchingStops} touching stops.");
        return mismatches == 0 ? 0 : 1;
    }
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));
}

/// <summary>Capture-only actor and pose identities; not new runtime dispatch values.</summary>
internal static class KagoSolidityFixtureData
{
    /// <summary>$A0 headers: vertical Kamer, horizontal Kamer, Kzan top and Kzan bottom.</summary>
    public static readonly ushort[] PlatformDefinitions = [0xd5ff, 0xd83f, 0xdfff, 0xe03f];

    /// <summary>Standing, morph ball, standing turns, crouches, crouch transitions and stand transitions, both facings.</summary>
    public static readonly byte[] Poses = [1, 2, 0x1d, 0x41, 0x25, 0x26, 0x27, 0x28, 0x35, 0x36, 0x3b, 0x3c];

    /// <summary>Exact, half-pixel and maximal fractional positions exercise outward rounding.</summary>
    public static readonly ushort[] Fractions = [0, 0x8000, 0xffff];
}
