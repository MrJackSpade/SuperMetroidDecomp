using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compares movement-owned echo slots and the real draw routine's low/high OAM output.</summary>
internal static class DraygonEchoComparison
{
    public static string? FindMismatch(ISnesAddressSpace bus, SamusState samus,
        ushort nmiFrameCounter, int frame, string[] row)
    {
        var speed = samus.HorizontalSpeed;
        string state = $"{speed.SpeedEchoIndex:X4},{speed.FirstSpeedEchoXPosition:X4},{speed.FirstSpeedEchoYPosition:X4},{speed.SecondSpeedEchoXPosition:X4},{speed.SecondSpeedEchoYPosition:X4}";
        string expectedState = string.Join(',', row[18..23]);
        if (state != expectedState) return $"slots {state} != {expectedState}";
        // Only flight/windup uses this extra, side-effect-free draw probe. Departing
        // echoes advance inside drawing, and must not be advanced twice per frame.
        if (frame < 150 || frame >= 175) return null;
        if ((speed.SpeedEchoIndex & 0x8000) != 0)
            throw new InvalidDataException("Echo OAM probe unexpectedly reached the mutating departure branch.");
        var oam = new OamBuffer();
        ushort cameraX = unchecked((ushort)(samus.XPosition - 128));
        ushort cameraY = unchecked((ushort)(samus.YPosition - 112));
        samus.Draw(bus, oam, cameraX, cameraY, nmiFrameCounter);
        oam.BeginFrame();
        samus.DrawSpeedBoosterEchoes(bus, oam, cameraX, cameraY);
        string draw = $"{oam.NextByteOffset:X4},{Convert.ToHexString(oam.LowTable[..oam.NextByteOffset])},{Convert.ToHexString(oam.HighTable)}";
        string expectedDraw = string.Join(',', row[23..]);
        return draw == expectedDraw ? null : $"OAM {draw} != {expectedDraw}";
    }
}
