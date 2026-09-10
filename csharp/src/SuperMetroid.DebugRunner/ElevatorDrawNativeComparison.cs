using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Compares #516's forward-facing sprite records with original CPU output.
/// This does not claim parity for the arrival camera or final PPU composition.
/// </summary>
internal static class ElevatorDrawNativeComparison
{
    public static int Run(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.XPosition = 128;
        var oam = new OamBuffer();
        string[] native = File.ReadAllLines(nativeCsv);
        const int expectedCases = 39 * 17;
        if (native.Length != expectedCases + 1 ||
            native[0] != "worldY,cameraY,count,lowOam,highOam")
            throw new InvalidDataException("Incomplete or unrecognized original-CPU elevator draw trace.");
        int row = 1;
        for (ushort worldY = 256; worldY <= 294; worldY++)
        for (ushort cameraY = 0; cameraY <= 32; cameraY += 2)
        {
            samus.YPosition = worldY;
            oam.BeginFrame();
            if (!samus.Draw(bus, oam, 0, cameraY))
                throw new InvalidDataException("Even-NMI forward-facing Samus unexpectedly hidden.");
            string actual = $"{worldY},{cameraY},{oam.NextByteOffset / 4}," +
                Convert.ToHexString(oam.LowTable[..oam.NextByteOffset]) + "," +
                Convert.ToHexString(oam.HighTable);
            if (actual != native[row])
                throw new InvalidDataException($"Elevator draw differs at row {row}.\nNative: {native[row]}\nManaged: {actual}");
            row++;
        }
        Console.WriteLine($"Elevator draw: {expectedCases} original-CPU cases match every emitted low/high OAM byte. " +
            "Camera trajectory and visible scene composition are not covered by this comparison.");
        return 0;
    }
}
