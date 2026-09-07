using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

try
{
    NativeConsole.SetErrorMode(NativeConsolePolicy.SuppressFaultDialogs);
    int errorMode = NativeConsole.WerSetFlags(NativeConsolePolicy.WerNoUi);
    if (errorMode < 0) System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(errorMode);
    string fixture = Path.GetFullPath(args[0]);
    int slot = int.Parse(args[1]);
    var seed = SuperMetroidAddressSpace.LoadRetailRom(args[2]);
    string output = Path.GetFullPath(args[3]);
    Directory.CreateDirectory(output);
    string temporary = Path.Combine(Path.GetTempPath(), "SuperMetroid-inspect-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(temporary);
    object loaded;
    string copy = Path.Combine(temporary, $"SuperMetroid-debug-slot-{slot}.smstate");
    try
    {
        File.Copy(Path.Combine(fixture, $"slot-{slot}.smstate"), copy);
        var type = Assembly.Load("SuperMetroid.Desktop").GetType("SuperMetroid.Desktop.DebuggerSaveStateStore", true)!;
        var store = RuntimeHelpers.GetUninitializedObject(type);
        type.GetField("directory", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(store, temporary);
        type.GetField("romDigest", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(store, SHA256.HashData(seed.Rom));
        loaded = type.GetMethod("Load")!.Invoke(store, [slot])!;
    }
    finally
    {
        if (File.Exists(copy)) File.Delete(copy);
        Directory.Delete(temporary);
    }
    var game = (SuperMetroidGame)loaded.GetType().GetProperty("Game")!.GetValue(loaded)!;
    var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame).GetProperty("RuntimeForVerification", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
    var frame = game.CurrentFrame;
    byte[] ports = new byte[4];
    FrontendFrame Step(ushort input)
    {
        game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
        var next = game.Step(input);
        foreach (var command in next.AudioCommands)
            if (command.Kind == SuperMetroid.Core.Audio.CartridgeAudioCommandKind.WritePort)
                ports[command.Port] = command.Value;
        return next;
    }
    PngWriter.WriteRgba(Path.Combine(output, $"slot-{slot}.png"), 256, 224, frame.Pixels);
    Console.WriteLine($"State={game.GameState}, room={runtime.ActiveRoom!.Pointer:X4}, Samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}, camera={runtime.Camera!.XPosition},{runtime.Camera.YPosition}");
    foreach (var enemy in runtime.Enemies.Slots.Where(e => e.EnemyDefinitionPointer != 0))
        Console.WriteLine($"Enemy {enemy.SlotIndex} def={enemy.EnemyDefinitionPointer:X4} xy={enemy.XPosition:X4},{enemy.YPosition:X4} map={enemy.SpritemapPointer:X4} tiles={enemy.VramTilesIndex:X4} palette={enemy.PaletteIndex:X4}");
    Console.WriteLine($"Crocomire phase={runtime.Enemies.Crocomire?.DeathSequenceIndex:X4}");
    if (slot == 1)
    {
        Console.WriteLine($"Grapple before: HUD={runtime.Samus.SelectedHudItem}, equipped={runtime.Samus.EquippedItems:X4}, phase={runtime.Samus.Grapple.Phase}");
        Step(0);
        Step(0x2000);
        Step(0);
        for (int tick = 0; tick < 60; tick++) Step(0x0040);
        Console.WriteLine($"Grapple after 60 Fire frames: HUD={runtime.Samus.SelectedHudItem}, phase={runtime.Samus.Grapple.Phase}");
    }
    if (game.GameState == SuperMetroidGameState.PausedB)
    {
        for (int tick = 0; tick < 30; tick++) Step(0);
        for (int tick = 0; tick < 12; tick++) Step(0x1000);
        for (int tick = 0; tick < 180 && game.GameState != SuperMetroidGameState.MainGameplay; tick++) Step(0);
        if (game.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidDataException($"Saved pause did not return to gameplay: {game.GameState}.");
        frame = Step(0);
        PngWriter.WriteRgba(Path.Combine(output, $"slot-{slot}-gameplay.png"), 256, 224, frame.Pixels);
    }
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}

internal static partial class NativeConsole
{
    [System.Runtime.InteropServices.LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint mode);
    [System.Runtime.InteropServices.LibraryImport("kernel32.dll")]
    internal static partial int WerSetFlags(uint flags);
}

internal static class NativeConsolePolicy
{
    /// <summary>SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX.</summary>
    internal const uint SuppressFaultDialogs = 0x8003;
    /// <summary>WER_FAULT_REPORTING_NO_UI.</summary>
    internal const uint WerNoUi = 0x20;
}
