using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.DesktopVerification.Harness;

/// <summary>Shared waits, assertions and host-handler access for the desktop verification checks.</summary>
internal static class DesktopVerificationSupport
{
    /// <summary>Polls for a rendered condition for up to ten seconds, surfacing worker faults while waiting.</summary>
    internal static async Task Until(Func<bool> condition, D3D11RenderWorker worker)
    {
        long deadline = Environment.TickCount64 + 10000;
        while (!condition())
        {
            worker.ThrowIfFaulted();
            if (Environment.TickCount64 >= deadline) throw new TimeoutException("Desktop GPU did not consume latest display.");
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Copies the repository ROM into <paramref name="directory"/>, with a copy of the repository
    /// installation where the desktop host opens the installation beside a ROM, and returns the ROM
    /// path. Each check gets its own installation without importing the ROM again; ROM import itself
    /// is covered by the asset-import verification.
    /// </summary>
    internal static string PlaceInstalledRom(string directory)
    {
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        PrivateInstallationCopy.CopyInstallation(RepositoryInstallation.Installation.Root,
            Path.Combine(directory, DesktopAccess.InstallationDirectoryName));
        return rom;
    }

    /// <summary>Raises a verification failure with the supplied context when an assertion is false.</summary>
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
