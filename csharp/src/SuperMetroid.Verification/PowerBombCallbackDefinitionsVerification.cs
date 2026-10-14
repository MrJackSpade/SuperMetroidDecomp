using static ShotCallbackDefinitionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;

/// <summary>Cross-checks bank-qualified Power Bomb no-op callback identities against native enemy headers.</summary>
internal static class PowerBombCallbackDefinitionsVerification
{
    /// <summary>Finds the shared disassembly checkout used to enumerate named bank-$A0 enemy headers.</summary>
    internal static string FindCallbackDisassemblySource()
    {
        // Linked worktrees share the separately checked-out diagnostic reference.
        for (DirectoryInfo? directory = new(Environment.CurrentDirectory);
             directory is not null; directory = directory.Parent)
        {
            string source = Path.Combine(directory.FullName, "upstream-disassembly", "src");
            if (File.Exists(Path.Combine(source, "bank_A0.asm")))
                return source;
        }
        throw new DirectoryNotFoundException(
            "Callback verification requires upstream-disassembly/src/bank_A0.asm " +
            "in this checkout or an ancestor containing the shared reference checkout.");
    }

    /// <summary>Checks each native header's callback opcode and every bank/pointer pair against the compiled no-op set.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for power bomb callback definitions")]
    internal static void VerifyPowerBombCallbackDefinitions(SuperMetroidAddressSpace rom)
    {
        int headers = 0;
        var literalCallbacks = new HashSet<int>();
        foreach (string line in File.ReadLines(Path.Combine(FindCallbackDisassemblySource(), "bank_A0.asm")))
        {
            Match match = Regex.Match(line, @"^EnemyHeaders_\w+:\s*;([0-9A-F]{6});");
            if (!match.Success)
                continue;
            int address = Convert.ToInt32(match.Groups[1].Value, 16);
            byte bank = rom.ReadCartridgeByte(address + 12);
            ushort callback = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 40);
            bool expected = callback != 0 && rom.ReadCartridgeByte(((int)new SnesAddress(bank, callback))) == 0x6b;
            AssertEqual(expected, EnemyPowerBombCallbackDefinitions.IsLiteralNoOp(bank, callback),
                $"header {address:X6} native Power Bomb classification");
            if (expected)
                literalCallbacks.Add(((int)new SnesAddress(bank, callback)));
            headers++;
        }
        AssertTrue(headers > 150, "all named native enemy headers enumerated");
        AssertEqual(10, literalCallbacks.Count, "complete Power Bomb literal no-op identity set");
        // No other bank/pointer may accidentally alias a classified identity. A zero
        // pointer is always common damage, regardless of the bank's bytes at offset zero.
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(literalCallbacks.Contains(((int)new SnesAddress(checked((byte)bank), checked((ushort)pointer)))),
                EnemyPowerBombCallbackDefinitions.IsLiteralNoOp((byte)bank, (ushort)pointer),
                "Power Bomb bank-qualified callback identity");
        Console.WriteLine($"Power Bomb callbacks: {headers} named headers match native opcode classification; all 16777216 bank/pointer pairs preserve the ten literal no-ops and zero/common distinction.");
    }

    /// <summary>Runs the Power Bomb and shot-callback definition checks against the retail ROM.</summary>
    [VerificationCommand("--enemy-callback-definitions")]
    internal static int EnemyCallbackDefinitionsCommand()
    {
        var callbackRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPowerBombCallbackDefinitions), () => VerifyPowerBombCallbackDefinitions(callbackRom));
        Suite(nameof(VerifyShotCallbackDefinitions), () => VerifyShotCallbackDefinitions(callbackRom));
        return 0;
    }
}
