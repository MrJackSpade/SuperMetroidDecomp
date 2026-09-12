using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

/// <summary>Replays the immutable production capture for #605 without modifying live state slots.</summary>
internal static class ShaktoolDigStateAudit
{
    public static int Run(string rom, string fixture)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string directory = Path.Combine(Path.GetTempPath(), $"SuperMetroid-dig-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var store = new DebuggerSaveStateStore(rom, bus.Rom, directory);
        DebuggerSaveStateLoadResult loaded;
        try
        {
            File.Copy(fixture, store.GetSlotPath(0));
            loaded = store.Load(0);
        }
        finally
        {
            File.Delete(store.GetSlotPath(0));
            Directory.Delete(directory);
        }
        foreach (string warning in loaded.Warnings) Console.WriteLine(warning);
        var runtime = loaded.Game.RuntimeForVerification!;
        if (runtime.ActiveRoom?.Pointer != ShaktoolDigDefinitions.Room)
            throw new InvalidDataException("Expected the preserved Shaktool room capture.");
        var level = runtime.LevelData!;
        ushort[] initial = level.ForegroundEntries.ToArray();
        for (int y = 4; y <= 11; y++)
        {
            var block = level.GetCollisionBlock(16, y);
            Console.WriteLine($"sand[16,{y}] word={block.LevelWord:X4}, BTS={block.Behavior:X2}");
        }
        string output = "csharp/test-temp/shaktool-dig-605";
        Directory.CreateDirectory(output);
        for (int frame = 0; frame <= 2400; frame++)
        {
            if (frame % 120 == 0)
            {
                int changed = initial.Where((word, index) => word != level.ForegroundEntries.Span[index]).Count();
                Console.WriteLine($"frame={frame}, Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition}, changed={changed}");
                foreach (var slot in runtime.Enemies.Slots.Where(s => s.EnemyDefinitionPointer == ShaktoolDigDefinitions.Enemy))
                    Console.WriteLine($"  segment={slot.Parameter2} xy={slot.XPosition}/{slot.YPosition} angle={slot.VariableB:X4} flags={slot.Parameter1:X4} pre={slot.VariableF:X4} list={slot.CurrentInstruction:X4} map={slot.SpritemapPointer:X4}");
            }
            if (frame is 0 or 1200 or 2400)
                PngWriter.WriteRgba(Path.Combine(output, $"frame-{frame:D4}.png"), 256, 224, SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (frame < 2400) loaded.Game.Step(0);
        }
        return 0;
    }
}

internal static class ShaktoolDigDefinitions
{
    /// <summary>RoomHeader_Shaktool at $8F:D8C5, the preserved #605 production room.</summary>
    public const ushort Room = 0xd8c5;
    /// <summary>EnemyHeaders_Shaktool at $A0:F07F, each of the seven linked segments.</summary>
    public const ushort Enemy = 0xf07f;
}
