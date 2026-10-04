using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static class TesterOptionsAudit
{
    internal static int Run(string installationRoot)
    {
        var defaults = SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents);
        Require(!defaults.GrantAllEquipment && !defaults.UnlockTourian, "shipped defaults disabled");
        var legacy = SuperMetroidGameOptionsIni.Parse("[Game]\nInfiniteAmmo=true");
        Require(!legacy.GrantAllEquipment && !legacy.UnlockTourian, "old INI defaults disabled");
        var enabled = SuperMetroidGameOptionsIni.Parse("[Game]\nGrantAllEquipment=true\nUnlockTourian=true");
        Require(enabled.GrantAllEquipment && enabled.UnlockTourian, "parse both settings");
        foreach (string key in new[] { "GrantAllEquipment", "UnlockTourian" })
        {
            Reject($"[Game]\n{key}=maybe");
            Reject($"[Game]\n{key}=true\n{key}=false");
        }
        var recording = new ControllerInputRecording
        {
            StartedUtc = DateTimeOffset.UtcNow, RomSha256 = new byte[32],
            InitialSaveRam = new byte[0x2000], GameOptions = enabled, ControllerInputs = [0]
        };
        using var stream = new MemoryStream();
        recording.Write(stream); stream.Position = 0;
        var restored = ControllerInputRecording.Read(stream);
        Require(restored.GameOptions.GrantAllEquipment && restored.GameOptions.UnlockTourian, "recording round trip");
        using var oldStream = new MemoryStream();
        (recording with { GameOptions = defaults }).Write(oldStream); oldStream.Position = 0;
        var old = ControllerInputRecording.Read(oldStream);
        Require(!old.GameOptions.GrantAllEquipment && !old.GameOptions.UnlockTourian, "old recording retains disabled flags");

        var installation = new GameInstallation(installationRoot);
        // Only these two explicit policy states: confirm requested on/off behavior, not a room sweep.
        foreach (bool on in new[] { false, true })
        {
            var bus = installation.OpenRuntimeAddressSpace();
            var game = new SuperMetroidGame(bus, on ? enabled : defaults, renderGameplayFrames: false);
            InstalledInputReplay.Bind(game, installation);
            typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(game, [false]);
            var runtime = game.RuntimeForVerification!;
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var samus = runtime.Samus!;
            Require(samus.MaxHealth == (on ? 1499 : 99), "new-game energy");
            Require(samus.MaxMissiles == (on ? 230 : 0), "new-game ammunition");
            if (on)
            {
                Require(samus.CollectedItems == 0xf32f && samus.EquippedItems == 0xf32f, "all equipment");
                Require(samus.CollectedBeams == 0x100f && samus.EquippedBeams == 0x100b, "all beams with legal Plasma equipment");
                Require(samus.ReserveEnergy == 400 && samus.MaxReserveEnergy == 400 && samus.ReserveTankMode == 1,
                    "full Auto reserves");
                Require(samus.Missiles == 230 && samus.SuperMissiles == 50 && samus.MaxSuperMissiles == 50 &&
                    samus.PowerBombs == 50 && samus.MaxPowerBombs == 50, "full capacities");
                Require(Enumerable.Range(0, Bank80SystemState.ItemBitByteCount * 8).All(runtime.System.HasCollectedItemBit), "pickups taken");
                samus.Health = 123; samus.Missiles = 2; samus.EquippedItems = 0; samus.EquippedBeams = 0;
                runtime.ApplyTesterInventory();
                Require(samus.Health == 123 && samus.Missiles == 2 && samus.EquippedItems == 0 && samus.EquippedBeams == 0,
                    "grant does not refill or re-equip existing player");
            }
            var saves = new SuperMetroidSaveRam(bus);
            saves.SaveSlot(0, new SuperMetroidSaveSnapshot());
            runtime.InitializeSavedGame(saves.ReadSlot(0)!);
            Require(runtime.Samus!.MaxHealth == (on ? 1499 : 99), "grant after save restore");
            runtime.LoadCartridgeRoomForDebug(0xa66a);
            for (int frame = 0; frame < 4; frame++) runtime.StepFrame(0);
            Require(runtime.Camera!.Scrolls.ReadState(1) == (on ? RoomScrollState.Green : RoomScrollState.RedBoundary),
                "Tourian passage scroll state");
            Require(runtime.TourianStatues.VerticalOffset == (on ? -TourianStatueRomData.DescentDistance : 0), "statue removed");
            for (int y = 12; y < 18; y++)
            for (int x = 6; x < 10; x++)
                if (on) Require(runtime.LevelData!.GetCollisionBlock(x, y).LevelWord == 0x00ff, "actual passage cleared");
            Require(!runtime.System.HasEvent(EventNumber.TourianUnlocked), "bypass does not save unlock event");
            Require(Enumerable.Range(0, Bank80SystemState.AreaCount).All(a => runtime.System.GetBossBitsRaw(a) == 0), "bosses remain alive");
            if (on)
            {
                runtime.LoadCartridgeRoomForDebug(0xa66a);
                for (int frame = 0; frame < 4; frame++) runtime.StepFrame(0);
                Require(runtime.LevelData!.GetCollisionBlock(6,12).LevelWord == 0x00ff, "bypass persists on room re-entry");
            }
            typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(game, [true]);
            Require(!game.RuntimeForVerification!.GrantAllEquipmentEnabled && !game.RuntimeForVerification.UnlockTourianEnabled,
                "attract runtime excludes both testing flags");
        }
        Console.WriteLine("PASS tester options: defaults/strict INI, recording flags, new/loaded inventory, one-time grant, legal beams, actual Tourian floor/scroll/re-entry with live bosses, attract exclusion.");
        return 0;
    }
    private static void Reject(string ini)
    {
        try { SuperMetroidGameOptionsIni.Parse(ini); }
        catch (InvalidDataException) { return; }
        throw new InvalidDataException("Invalid tester option was accepted.");
    }
    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("Tester option failed: " + property);
    }
}
