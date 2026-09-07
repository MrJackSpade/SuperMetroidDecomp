using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Tracks actual APU upload/track commands while booting an ordinary saved game.</summary>
internal static class SavedRoomMusicAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var station = LoadStationEntry.Load(bus, AreaId.Norfair, 0);
        var room = CartridgeRoomHeader.Load(bus, station.RoomPointer);
        byte data = room.State.MusicDataIndex, track = room.State.MusicTrackIndex;
        Console.WriteLine($"Norfair station 0: room {room.Pointer:X4}, music data/track {data:X2}/{track:X2}.");
        if (data == 0 || track == 0) throw new InvalidDataException("Fixture requires an explicit room music bank and track.");
        var saves = new SuperMetroidSaveRam(bus);
        saves.SaveSlot(0, new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Norfair, SaveStation = 0, Health = 99, MaxHealth = 99 });
        saves.SelectSlot(0);
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        var ports = new byte[4];
        int lastUpload = 0;
        bool loadingSavedRoom = false;
        var savedRoomTracks = new List<byte>();
        int pointer = AudioRomData.Assets.MusicPointerTable + data;
        int expectedUpload = bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8 | bus.ReadByte(pointer + 2) << 16;
        FrontendFrame frame = default;
        void Step(ushort input = 0)
        {
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            frame = game.Step(input);
            foreach (var command in frame.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.Upload) lastUpload = command.UploadAddress;
                else
                {
                    ports[command.Port] = command.Value;
                    if (loadingSavedRoom && command.Port == AudioRomData.Apu.MusicPort && command.Value != 0)
                    {
                        if (lastUpload != expectedUpload)
                            throw new InvalidDataException("Saved-game track started before its room music bank was uploaded.");
                        savedRoomTracks.Add(command.Value);
                    }
                }
            }
        }
        void Until(Func<bool> condition, int limit)
        {
            for (int tick = 0; tick < limit && !condition(); tick++) Step();
            if (!condition()) throw new InvalidDataException($"Saved-room music fixture stalled in {frame.GameState}/{frame.Phase}.");
        }
        Step(); Step((ushort)SnesButton.Start);
        Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen), 150);
        Step((ushort)SnesButton.Start);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus, 150);
        for (int tick = 0; tick < 16; tick++) Step();
        Step((ushort)SnesButton.A);
        Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu, 200);
        for (int tick = 0; tick < 16; tick++) Step();
        Step((ushort)SnesButton.A);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMap && frame.Phase == "Area", 200);
        Step((ushort)SnesButton.Start);
        Until(() => frame.Phase == "Room", 100);
        loadingSavedRoom = true;
        Step((ushort)SnesButton.Start);
        Until(() => game.GameState == SuperMetroidGameState.MainGameplay, 150);
        for (int tick = 0; tick < 500; tick++) Step();
        if (lastUpload != expectedUpload || ports[0] != track)
            throw new InvalidDataException($"Saved room plays bank {lastUpload:X6}/track {ports[0]:X2}, expected {expectedUpload:X6}/{track:X2}.");
        if (!savedRoomTracks.SequenceEqual(new byte[] { 1, track }))
            throw new InvalidDataException($"Expected appearance then room music, received {string.Join(",", savedRoomTracks)}.");
        Console.WriteLine("Saved Norfair room uses its own uploaded music bank and room track after appearance.");
        return 0;
    }
}
