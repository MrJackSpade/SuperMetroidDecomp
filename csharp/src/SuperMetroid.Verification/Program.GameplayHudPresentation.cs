using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameplayHudPresentationAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, GameplayHudDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.GameplayHudPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed gameplay HUD JSON is the deterministic cartridge extraction");
        GameplayHudPresentation presentation = GameplayHudPresentation.Load(new MemoryStream(extracted));
        for (int index = 0; index < GameplayHudDefinitions.TopRowByteCount; index++)
            AssertEqual(bus.ReadByte(GameplayHudDefinitions.TopRowAddress + index),
                presentation.TopRowTransfer.Span[index], $"stock immutable HUD row byte {index}");
        var guard = new GameplayHudReadGuard(bus);
        var topRowRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(guard)
            { MapPresentation = original };
        topRowRuntime.VramWrites.Enqueue(GameplayHudDefinitions.TopRowByteCount,
            GameplayHudDefinitions.TopRowAddress, 0x5800);
        topRowRuntime.VramWrites.DrainTo(topRowRuntime.Vram, guard, topRowRuntime);
        AssertTrue(topRowRuntime.Vram.Bytes.Slice(0xb000, GameplayHudDefinitions.TopRowByteCount)
            .SequenceEqual(presentation.TopRowTransfer.Span),
            "queued immutable HUD row uses installed visual data without source reads");
        ushort equipment = (ushort)(SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.GrappleBeam);
        var snapshot = new HudSnapshot(
            Health: 345, MaxHealth: 499,
            Missiles: 123, MaxMissiles: 230,
            SuperMissiles: 45, MaxSuperMissiles: 50,
            PowerBombs: 6, MaxPowerBombs: 10,
            EquippedItems: equipment, SelectedItem: 2,
            ReserveHealth: 50, ReserveMode: 1);
        var native = new HudState();
        var installed = new HudState();
        installed.BindPresentation(presentation);
        native.Initialize(bus, snapshot);
        installed.Initialize(guard, snapshot);
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed HUD template, icons, tanks, counters, AUTO and highlight match native");

        var samus = new SamusState
        {
            Health = 287,
            MaxHealth = 499,
            Missiles = 98,
            MaxMissiles = 230,
            SuperMissiles = 32,
            MaxSuperMissiles = 50,
            PowerBombs = 4,
            MaxPowerBombs = 10,
            EquippedItems = equipment,
            SelectedHudItem = 3,
            ReserveEnergy = 25,
            ReserveTankMode = 1,
        };
        native.UpdateGameplayCounters(bus, samus);
        installed.UpdateGameplayCounters(guard, samus);
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed live HUD updates match native without presentation ROM reads");
        AssertEqual(native.SelectionSoundRequestedThisFrame, installed.SelectionSoundRequestedThisFrame,
            "installed HUD retains selection-sound mechanics");

        var nativeSystem = new Bank80SystemState();
        var installedSystem = new Bank80SystemState();
        nativeSystem.SetAreaMapAcquired(AreaId.Crateria);
        installedSystem.SetAreaMapAcquired(AreaId.Crateria);
        native.UpdateMinimap(bus, nativeSystem, AreaId.Crateria, 23, 0, 144, 80,
            0x0440, 0x04bb, 0, presentationMap: original.Get(AreaId.Crateria));
        installed.UpdateMinimap(guard, installedSystem, AreaId.Crateria, 23, 0, 144, 80,
            0x0440, 0x04bb, 0, presentationMap: original.Get(AreaId.Crateria));
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed minimap anchor matches native and needs no HUD visual ROM data");

        var document = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(extracted,
            MapPresentationFormat.JsonOptions)!;
        document = document with
        {
            Digits = document.Digits with { HealthAnchor = new(4, 2) },
            MinimapAnchor = new(25, 0),
        };
        document.TopRow[0] = document.TopRow[0] with { FlipX =
            !document.TopRow[0].FlipX };
        document.Icons["Missile"].Cells[0] = document.Icons["Missile"].Cells[0] with { FlipX = true };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, GameplayHudDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameplayHudPresentation.Write(output, document);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "gameplay HUD override changes selected catalog identity");
        var editedTopRowRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(guard)
            { MapPresentation = edited };
        editedTopRowRuntime.VramWrites.Enqueue(GameplayHudDefinitions.TopRowByteCount,
            GameplayHudDefinitions.TopRowAddress, 0x5800);
        editedTopRowRuntime.VramWrites.DrainTo(editedTopRowRuntime.Vram, guard,
            editedTopRowRuntime);
        AssertTrue(editedTopRowRuntime.Vram.Bytes.Slice(0xb000,
                GameplayHudDefinitions.TopRowByteCount)
            .SequenceEqual(edited.GameplayHud.TopRowTransfer.Span),
            "edited top-row cell reaches live BG3 VRAM");
        AssertTrue(!edited.GameplayHud.TopRowTransfer.Span.SequenceEqual(
                original.GameplayHud.TopRowTransfer.Span),
            "edited top-row cell changes the native transfer");

        var editedHud = new HudState();
        editedHud.BindPresentation(edited.GameplayHud);
        editedHud.Initialize(guard, snapshot);
        var stockHud = new HudState();
        stockHud.BindPresentation(original.GameplayHud);
        stockHud.Initialize(guard, snapshot);
        AssertEqual(stockHud.Tiles[70], editedHud.Tiles[68],
            "edited health anchor moves its first digit through production HUD setup");
        AssertEqual(stockHud.Tiles[71], editedHud.Tiles[69],
            "edited health anchor moves its second digit through production HUD setup");
        AssertTrue(editedHud.Tiles[10] != stockHud.Tiles[10],
            "edited missile cell changes the installed icon");

        var rebind = new HudState();
        rebind.BindPresentation(original.GameplayHud);
        rebind.Initialize(guard, snapshot);
        var rebindSystem = new Bank80SystemState();
        rebindSystem.SetAreaMapAcquired(AreaId.Crateria);
        rebind.UpdateMinimap(guard, rebindSystem, AreaId.Crateria, 23, 0, 144, 80,
            0x0440, 0x04bb, 8, presentationMap: original.Get(AreaId.Crateria));
        ushort[] logicalMap = Enumerable.Range(0, 15)
            .Select(i => rebind.Tiles[(i / 5) * 32 + 26 + i % 5]).ToArray();
        rebind.BindPresentation(edited.GameplayHud, samus);
        for (int i = 0; i < logicalMap.Length; i++)
            AssertEqual(logicalMap[i], rebind.Tiles[(i / 5) * 32 + 25 + i % 5],
                "HUD content rebind carries logical minimap cells to edited anchor");

        File.WriteAllText(replacement, "{ broken gameplay HUD JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt gameplay HUD override fails loudly");
        using (var output = File.Create(replacement))
            GameplayHudPresentation.Write(output, document);

        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(Stream.Null,
            document with { SelectedPalette = 8 }), "HUD rejects invalid highlight palette");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(Stream.Null,
            document with { Template = document.Template[..^1] }), "HUD rejects incomplete template");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(Stream.Null,
            document with { TopRow = document.TopRow[..^1] }), "HUD rejects incomplete immutable row");
        var overlap = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(extracted,
            MapPresentationFormat.JsonOptions)!;
        overlap = overlap with { Digits = overlap.Digits with { HealthAnchor = new(8, 0) } };
        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(Stream.Null, overlap),
            "HUD rejects overlapping visual owners");
        var missingIcon = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(extracted,
            MapPresentationFormat.JsonOptions)!;
        missingIcon.Icons.Remove("XRay");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(Stream.Null, missingIcon),
            "HUD rejects missing named icons");

        Console.WriteLine("Gameplay HUD presentation: exact stock/live parity, ROM guard, layout/art edits, rebind and strict failures pass.");
    }

    private sealed class GameplayHudReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (InRange(address, GameplayHudDefinitions.TopRowAddress, GameplayHudDefinitions.TopRowByteCount) ||
                InRange(address, GameplayHudDefinitions.TemplateAddress, GameplayHudDefinitions.CellCount * 2) ||
                InRange(address, GameplayHudDefinitions.IconTableAddress, 44) ||
                InRange(address, GameplayHudDefinitions.HealthDigitsAddress, 20) ||
                InRange(address, GameplayHudDefinitions.AmmoDigitsAddress, 20) ||
                InRange(address, GameplayHudDefinitions.AutoReserveTableAddress, 24))
                throw new InvalidOperationException($"Unexpected gameplay HUD presentation ROM read ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
        private static bool InRange(int address, int start, int length) =>
            (uint)(address - start) < (uint)length;
    }
}
