using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

/// <summary>Loads immutable ticket fixtures through the production state reader without touching live slots.</summary>
internal static class DebuggerFixtureLoader
{
    public static DebuggerSaveStateLoadResult Load(string fixtureName, int slot)
    {
        string rom = Path.GetFullPath("Super Metroid.smc");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string directory = Path.Combine(Path.GetTempPath(), $"SuperMetroid-fixture-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var store = new DebuggerSaveStateStore(rom, bus.Rom, directory);
        string destination = store.GetSlotPath(slot);
        try
        {
            File.Copy(Path.Combine("csharp", "test-fixtures", fixtureName, $"slot-{slot}-named.smstate"), destination);
            return store.Load(slot);
        }
        finally
        {
            if (File.Exists(destination)) File.Delete(destination);
            Directory.Delete(directory);
        }
    }
}
