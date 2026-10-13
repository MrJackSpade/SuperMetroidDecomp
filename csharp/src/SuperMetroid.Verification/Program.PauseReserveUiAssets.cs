using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using static PauseEquipmentRulesVerification;

/// <summary>Compares installed pause reserve tiles, arrow color phases, and rendering with native source data.</summary>
internal static class PauseReserveUiAssetsVerification
{
    /// <summary>Checks reserve labels, digits, and arrow phases against native rendering, then verifies edits, rebinding, and invalid assets.</summary>
    internal static void VerifyPauseReserveUiAssets(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseReserveUiDefinitions.FileName);
        string path = Path.Combine(overrides, PauseReserveUiDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(stockBytes, MapPresentationFormat.JsonOptions);
        var guard = NewPauseReserveUiReadGuard(bus);

        var nativeSamus = Samus(); var installedSamus = Samus();
        var native = new PauseMenuState(bus, new PauseMenuEntry { Samus = nativeSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = original });
        var installed = new PauseMenuState(guard, new PauseMenuEntry { Samus = installedSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = original });
        EnterEquipment(native); EnterEquipment(installed);
        for (int frame = 0; frame < 64; frame++)
        {
            native.Step(0, 0, nmiFrameCounter8: (byte)frame);
            installed.Step(0, 0, nmiFrameCounter8: (byte)frame);
            AssertTrue(native.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(installed.CaptureRenderSnapshot().Memory.Vram),
                "installed reserve labels/digits/arrow tile palettes match native with their ROM sources forbidden");
            AssertTrue(native.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(installed.CaptureRenderSnapshot().Memory.Cgram),
                "all 32 reserve-arrow color phases match native");
            AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()),
                "reserve UI migration retains native pixels");
        }

        var labels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        labels["Mode"] = labels["Mode"] with { Cells = labels["Mode"].Cells.Select((cell, index) =>
            index == 0 ? cell with { FlipX = !cell.FlipX } : cell).ToArray() };
        var digitCells = document.Digits.Cells.ToArray();
        digitCells[5] = digitCells[5] with { FlipY = !digitCells[5].FlipY };
        var frames = document.Arrow.Frames.ToArray();
        frames[7] = frames[7] with { Color6 = frames[7].Color6 with { Red = (frames[7].Color6.Red + 1) % 32 } };
        var editedDocument = document with
        {
            Labels = labels,
            Digits = document.Digits with { Cells = digitCells },
            Arrow = document.Arrow with { Frames = frames },
        };
        using (var output = File.Create(path)) PauseReserveUiPresentation.Write(output, editedDocument);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "reserve UI override changes selected content identity");
        var stockPause = new PauseMenuState(guard, new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = original });
        var editedPause = new PauseMenuState(guard, new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = edited });
        AssertTrue(stockPause.ReadReserveSupplyDigit(2) != editedPause.ReadReserveSupplyDigit(2),
            "edited reserve digit reaches the actual mutable equipment page");
        EnterEquipment(stockPause); EnterEquipment(editedPause);
        for (int frame = 0; frame <= 7; frame++)
        {
            stockPause.Step(0, 0, nmiFrameCounter8: (byte)frame);
            editedPause.Step(0, 0, nmiFrameCounter8: (byte)frame);
        }
        AssertTrue(!stockPause.Render().AsSpan().SequenceEqual(editedPause.Render()),
            "edited reserve label/digit/arrow presentation changes visible menu pixels");

        var cursorBeforeRestore = (editedPause.SelectedCategory, editedPause.SelectedItem);
        using var captured = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(captured, editedPause); captured.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(captured);
        restored.BindMapPresentation(original);
        AssertEqual(cursorBeforeRestore, (restored.SelectedCategory, restored.SelectedItem),
            "reserve UI content rebind preserves the live equipment cursor");
        AssertTrue(!editedPause.Render().AsSpan().SequenceEqual(restored.Render()),
            "restored reserve UI adopts current content without changing reserve mechanics");
        AssertTrue(stockPause.Render().AsSpan().SequenceEqual(restored.Render()),
            "restored reserve UI rebind reproduces the current stock presentation at the retained phase");

        void Reject(PauseReserveUiDocument invalid) => AssertThrows<InvalidDataException>(() =>
            PauseReserveUiPresentation.Write(new MemoryStream(), invalid), "invalid reserve UI schema rejected");
        Reject(document with { Version = 2 });
        Reject(document with { Labels = [] });
        Reject(document with { Digits = document.Digits with { Cells = document.Digits.Cells[..9] } });
        Reject(document with { Arrow = document.Arrow with { Cells = document.Arrow.Cells[..9] } });
        Reject(document with { Arrow = document.Arrow with { EnabledPalette = 8 } });
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "invalid reserve UI override is never silently replaced");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
                "reserve UI override cannot conceal missing stock");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
                "reserve UI override cannot conceal failed provenance");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause reserve UI: labels, ten digits and 32 arrow phases match native with guarded reads; edits, restore and strict failures pass.");

        static SamusState Samus() => new()
        {
            MaxHealth = 499, Health = 499, MaxReserveEnergy = 400, ReserveEnergy = 245,
            ReserveTankMode = 1,
        };
        static void EnterEquipment(PauseMenuState pause)
        {
            byte nmiFrame = 0; ChangePausePage(pause, SnesButton.R, input => pause.Step(input, input, nmiFrameCounter8: nmiFrame++));
            AssertEqual(1, pause.ScreenMode, "reserve UI fixture enters equipment");
        }
    }

    /// <summary>Forbids the migrated reserve tilemap and arrow color ranges during installed menu playback.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause reserve UI tilemap ranges")]
    private static CartridgeReadGuard NewPauseReserveUiReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Block(int start, int count) { for (int i = 0; i < count; i++) blocked.Add(start + i); }
        Block(0x82c068, 4); Block(0x82c088, 4); Block(0x82bf22, 16);
        Block(0x82ad5d, 64); Block(0x82ad9d, 64);
        for (int index = 0; index < 2; index++)
        {
            int sourceAddress = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c088 + index * 2);
            Block(sourceAddress, 14);
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read reserve UI at {address:X6}.");
    }
}
