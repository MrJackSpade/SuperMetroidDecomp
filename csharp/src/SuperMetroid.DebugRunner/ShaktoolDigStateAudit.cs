using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;
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
        int firstDig = -1, firstBlock = -1;
        for (int frame = 0; frame <= 6000; frame++)
        {
            if (firstDig < 0)
                for (int index = 0; index < initial.Length; index++)
                    if (initial[index] == 0xa110 && level.ForegroundEntries.Span[index] != initial[index])
                    { firstDig = frame; firstBlock = index; Console.WriteLine($"First dig frame={frame}, block={index}"); break; }
            if (firstDig >= 0 && frame - firstDig <= 13)
            {
                Console.WriteLine($"dig+{frame - firstDig}: word={level.ForegroundEntries.Span[firstBlock]:X4}");
                if (level.ForegroundEntries.Span[firstBlock] != ShaktoolDigDefinitions.CrumbleWords[Math.Min((frame - firstDig) / 4, 3)])
                    throw new InvalidDataException("Production-state crumble artwork/timing differs from the native instruction list.");
            }
            if (frame % 120 == 0)
            {
                int changed = initial.Where((word, index) => word != level.ForegroundEntries.Span[index]).Count();
                Console.WriteLine($"frame={frame}, Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition}, changed={changed}");
                foreach (var slot in runtime.Enemies.Slots.Where(s => s.EnemyDefinitionPointer == ShaktoolDigDefinitions.Enemy))
                    Console.WriteLine($"  segment={slot.Parameter2} xy={slot.XPosition}/{slot.YPosition} angle={slot.VariableB:X4} flags={slot.Parameter1:X4} pre={slot.VariableF:X4} list={slot.CurrentInstruction:X4} map={slot.SpritemapPointer:X4}");
            }
            if (frame is 0 or 1200 or 2400 or 6000 || firstDig >= 0 && frame - firstDig is 0 or 4 or 8 or 12)
                PngWriter.WriteRgba(Path.Combine(output, $"frame-{frame:D4}.png"), 256, 224, SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (frame < 6000) loaded.Game.Step(0);
        }
        if (initial.AsSpan().SequenceEqual(level.ForegroundEntries.Span))
            throw new InvalidDataException("Shaktool never removed or animated any sand in 6000 production-state frames.");
        for (int y = 5; y <= 10; y++)
            Console.WriteLine($"row {y}: {string.Concat(Enumerable.Range(15, 42).Select(x => level.GetCollisionBlock(x, y).CollisionType == SuperMetroid.Core.Rooms.RoomCollisionType.Air ? '.' : '#'))}");
        int removed = 0;
        for (int index = 0; index < initial.Length; index++)
            if (initial[index] == 0xa110)
            {
                if (level.ForegroundEntries.Span[index] != ShaktoolDigDefinitions.CrumbleWords[3])
                    throw new InvalidDataException($"Sand block {index} did not finish clearing.");
                removed++;
            }
        if (removed != 216 || firstDig != 1046)
            throw new InvalidDataException($"Production digging trajectory changed: {removed} blocks, first frame {firstDig}.");
        for (int frame = 0; frame < 900 && runtime.Samus!.XPosition < 848; frame++)
        {
            ushort input = (ushort)SnesButton.Right;
            if (frame % 80 < 40) input |= runtime.ControllerBindings.Jump;
            loaded.Game.Step(input);
            if (runtime.ActiveRoom?.Pointer != ShaktoolDigDefinitions.Room)
                throw new InvalidDataException("Passage test left the reported room.");
        }
        Console.WriteLine($"Passage attempt: Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition}");
        if (runtime.Samus.XPosition < 848)
            throw new InvalidDataException("Controller-driven Samus did not traverse the cleared sand passage.");
        return 0;
    }
}

internal static class ShaktoolDigDefinitions
{
    /// <summary>RoomHeader_Shaktool at $8F:D8C5, the preserved #605 production room.</summary>
    public const ushort Room = 0xd8c5;
    /// <summary>EnemyHeaders_Shaktool at $A0:F07F, each of the seven linked segments.</summary>
    public const ushort Enemy = 0xf07f;
    /// <summary>Native CD53 draw sequence: Respawn1x1_0/1/2/3, held for 4/4/4/1 frames.</summary>
    public static ReadOnlySpan<ushort> CrumbleWords => [0x0053, 0x0054, 0x0055, 0x00ff];
}
