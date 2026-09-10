using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;

/// <summary>Checks positions and velocity, not just successful door completion.</summary>
internal static class SpinDoorNativeComparison
{
    public static string[] Load(string path)
    {
        string[] lines = File.ReadAllLines(path);
        string normalized = string.Join('\n', lines) + "\n";
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))) !=
            SpinDoorFixtureDefinitions.NativeTraceSha256 || lines.Length != 47)
            throw new InvalidDataException("Use the original-CPU spin-door-517 v1 trace.");
        return lines;
    }

    public static void VerifyFrame(SamusState samus, int frame, string[] native)
    {
        string actual = $"F{frame:D5},{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8}," +
            $"{samus.HorizontalSpeed.BaseFixed:X8},{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4}," +
            $"{samus.Kinematics.VerticalSpeedFixed:X8},{samus.Kinematics.YDirection:X4},{samus.Pose:X4}";
        // Header plus six completed cartridge loading stages precede gameplay.
        if (actual != native[frame + 7])
            throw new InvalidDataException($"Post-door native mismatch: {actual} != {native[frame + 7]}");
    }
}
