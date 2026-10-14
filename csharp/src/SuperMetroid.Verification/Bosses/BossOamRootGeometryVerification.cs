using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks boss OAM frame root pointers and native component counts.</summary>
internal static class BossOamRootGeometryVerification
{
    /// <summary>Checks Ridley's eleven selected OAM roots and the final single-part frame.</summary>
    internal static void VerifyRidleyOamRootGeometry(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = [
        0xe983, 0xe9a5, 0xe9c7, 0xe9e9, 0xea0b, 0xea2d,
        0xea4f, 0xea71, 0xea93, 0xeab5, 0xead7,
    ];
        Suite(nameof(VerifyBossOamRoots), () => VerifyBossOamRoots(rom, 0xa6, expected, BossOamFrameDefinitions.RidleyPointer,
            index => index == 10 ? 1 : 4));
    }

    /// <summary>Checks Draygon root ordering and per-frame component counts across its animations.</summary>
    internal static void VerifyDraygonOamRootGeometry(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = [
        0xa2df, 0xa2e9, 0xa2f3, 0xa2fd, 0xa307, 0xa311,
        0xa3c5, 0xa3cf, 0xa3d9, 0xa3e3,
        0xa40b, 0xa41d, 0xa42f, 0xa441, 0xa453, 0xa465, 0xa477, 0xa489,
        0xa4a3, 0xa4c5, 0xa4ef, 0xa521, 0xa55b, 0xa59d,
        0xa607, 0xa611, 0xa61b, 0xa625, 0xa62f, 0xa639,
        0xa6ed, 0xa6f7, 0xa701, 0xa70b,
        0xa779, 0xa78b, 0xa79d, 0xa7af, 0xa7c1, 0xa7d3, 0xa7e5, 0xa7f7,
        0xa811, 0xa833, 0xa85d, 0xa88f, 0xa8c9, 0xa90b,
    ];
        int[] components = [1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,3,4,5,6,7,8,8];
        Suite(nameof(VerifyBossOamRoots), () => VerifyBossOamRoots(rom, 0xa5, expected, BossOamFrameDefinitions.DraygonPointer,
            index => components[index % 24]));
    }

    /// <summary>Checks Spore Spawn's twelve OAM roots and one- or two-part geometry.</summary>
    internal static void VerifySporeSpawnOamRootGeometry(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = [
        0xee65, 0xee6f, 0xee79, 0xee8b, 0xee9d, 0xeeaf, 0xeec1,
        0xeed3, 0xeee5, 0xef3d, 0xef4f, 0xef61,
    ];
        Suite(nameof(VerifyBossOamRoots), () => VerifyBossOamRoots(rom, 0xa5, expected, BossOamFrameDefinitions.SporeSpawnPointer,
            index => index < 2 ? 1 : 2));
    }

    /// <summary>Compares recorded roots with compiled selectors and confirms each stride through native part counts.</summary>
    /// <remarks>Expected roots were captured from the published catalog before conversion.</remarks>
    [CpuBusBoundary("exact bus test: retail ROM oracle for boss OAM roots")]
    private static void VerifyBossOamRoots(SuperMetroidAddressSpace rom, byte bank,
        ushort[] expected, Func<int, ushort> actual, Func<int, int> componentCount)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], actual(index), "original selected OAM root");
            int address = ((int)new SnesAddress(bank, expected[index]));
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(componentCount(index), count, "native component count explains record geometry");
        }
        foreach (int invalid in new[] { -1, expected.Length, int.MaxValue })
        {
            bool rejected = false;
            try { _ = actual(invalid); }
            catch (IndexOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "OAM root index bounds");
        }
    }
}
