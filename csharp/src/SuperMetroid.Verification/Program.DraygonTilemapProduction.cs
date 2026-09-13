using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>#385: reconstruct fresh-room BG2 staging and native DMA, not merely retained current-command destinations.</summary>
    private static void VerifyDraygonTilemapProduction()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, DraygonProductionAuditDefinitions.Room);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram(); var cgram = new SnesCgram(); assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState { Health = 999, MaxHealth = 999, XPosition = 256, YPosition = 64,
            Pose = SamusPoseIds.FacingRightNormalPose };
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, vram, cgram,
            random.NextRandom, random.SetRandomNumber, readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData, samus: samus, isAreaBossDefeated: () => false);
        var boss = enemies.Draygon ?? throw new InvalidOperationException("Fresh Draygon population did not load.");
        // Observe which native enemy indexes were actually scheduled, without adding
        // a diagnostic callback to production. This does NOT independently verify AI
        // or queue construction; it isolates the downstream tilemap/DMA producer.
        var queues = (List<ushort>[])typeof(RoomEnemySystem).GetField("_drawQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(enemies)!;
        var staging = Enumerable.Repeat((ushort)0x0338, 2048).ToArray();
        var expectedVram = staging.ToArray();
        AssertEqual((ushort)0x0400, boss.Bg2TilemapSize, "A5:86D7 initializes the native DMA prefix size");
        for (int word = 0; word < expectedVram.Length; word++)
            AssertEqual(expectedVram[word], vram.ReadWord(0x4800 + word), "fresh Draygon BG2 starts entirely blank before any draw");
        var destinations = new HashSet<int>(); var streams = new HashSet<int>();
        var functions = new HashSet<DraygonAiFunction>();
        int commands = 0, transfers = 0; long comparisons = 0;
        var oam = new OamBuffer();
        for (int frame = 0; frame < 4096; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData, nmiFrameCounter8: unchecked((byte)frame));
            functions.Add(boss.Function);
            bool queuedTransfer = false;
            foreach (var queue in queues)
            foreach (ushort nativeIndex in queue)
            {
                var slot = enemies.Slots[nativeIndex / 64];
                if (((ushort)slot.ExtraProperties & 0x8004) != 0x8004) continue;
                int bank = slot.Definition.Bank << 16;
                int Word(int pointer) => bus.ReadByte(bank | (pointer & 65535)) | bus.ReadByte(bank | ((pointer + 1) & 65535)) << 8;
                int count = bus.ReadByte(bank | slot.SpritemapPointer);
                for (int component = 0; component < count; component++)
                {
                    int stream = Word(slot.SpritemapPointer + 2 + component * 8 + 4);
                    if (Word(stream) != 0xfffe) continue;
                    streams.Add(bank | stream); queuedTransfer = true;
                    int cursor = stream + 2;
                    for (int command = 0; ; command++)
                    {
                        int destination = Word(cursor); if (destination == 0xffff) break;
                        int words = Word(cursor + 2), first = (destination - 0x2000) / 2;
                        if (command >= 128 || (destination & 1) != 0 || words <= 0 || first < 0 || first + words > staging.Length)
                            throw new InvalidDataException("Invalid native Draygon command in independent producer audit.");
                        for (int i = 0; i < words; i++) { staging[first + i] = (ushort)Word(cursor + 4 + i * 2); destinations.Add(first + i); }
                        commands++; cursor += 4 + words * 2;
                    }
                }
            }
            // A0:9726 uploads only enemy_bg2_tilemap_size bytes from the beginning
            // of staging when any extended command ran. Compare untouched cells too.
            if (queuedTransfer)
            {
                staging.AsSpan(0, boss.Bg2TilemapSize / 2).CopyTo(expectedVram);
                transfers++;
            }
            oam.BeginFrame(); enemies.DrawLayers(oam, 0, 0, 0, 7); oam.FinalizeFrame();
            for (int word = 0; word < expectedVram.Length; word++)
            {
                ushort actual = vram.ReadWord(0x4800 + word);
                if (expectedVram[word] != actual)
                    throw new InvalidOperationException($"Fresh Draygon frame {frame}, BG2 word {word:X3}: native staging/DMA {expectedVram[word]:X4}, actual {actual:X4}; published={queuedTransfer}.");
                comparisons++;
            }
        }
        AssertTrue(commands > 0 && transfers > 0 && destinations.Count > 8 && streams.Count > 1,
            "fresh producer audit must cover newly published commands and more than retained eight-word fragments");
        Console.WriteLine($"Fresh Draygon production: {commands} commands, {transfers} transfers, {streams.Count} distinct streams, {destinations.Count} distinct destinations, {functions.Count} AI functions, {comparisons} complete-map word comparisons agree with native staging/DMA reconstruction.");
    }
}

internal static class DraygonProductionAuditDefinitions
{
    /// <summary>$8F:DA60, fresh two-screen-wide/tall Maridia Draygon room.</summary>
    internal const ushort Room = 0xda60;
}
