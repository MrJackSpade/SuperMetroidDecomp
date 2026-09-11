using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class YappingMawAudit
{
    public static int RunAudio(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, AuditRoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        // Check the ROM's actual LDA #$002F / JSL QueueSound_Lib2_Max6,
        // not just a matching name in annotated source.
        if (bus.ReadByte(0xa8a13e) != 0xa9 || ReadWord(bus, 0xa8a13f) != 0x002f ||
            bus.ReadByte(0xa8a141) != 0x22 || ReadWord(bus, 0xa8a142) != 0x90cb ||
            bus.ReadByte(0xa8a144) != 0x80)
            throw new InvalidDataException("Maw sound instruction differs from the audited ROM revision.");
        foreach (bool offscreen in new[] { false, true })
        {
            var loaded = Load(bus, room, assets);
            Step(loaded, assets, 0); Step(loaded, assets, 1);
            // Execute the authored attacking-list sound opcode through the real
            // enemy instruction dispatcher, with native on/offscreen computation.
            loaded.Actor.CurrentInstruction = 0x9f77;
            loaded.Actor.InstructionTimer = 1;
            loaded.Enemies.StepFrame(cameraX: (ushort)(offscreen ? 1024 : 0), cameraY: 0,
                timeIsFrozen: false, loaded.Samus, level: assets.LevelData, nmiFrameCounter8: 2);
            var requests = loaded.Enemies.SoundRequests.Where(request =>
                request.SoundEffect == new SoundEffectId(SoundEffectLibrary.Library2, 0x2f)).ToArray();
            if (requests.Length != (offscreen ? 0 : 1) || requests.Any(request => request.MaximumQueued != 6))
                throw new InvalidDataException($"Maw sound routing differs: offscreen={offscreen}, calls={requests.Length}.");
            if (loaded.Enemies.SoundRequests.Any(request => request.SoundEffect.Library == SoundEffectLibrary.Library1))
                throw new InvalidDataException("Maw attack unexpectedly published a weapon-library sound.");
            Console.WriteLine($"Candidate Yapping Maw: offscreen={offscreen}, library-two/$2F Max6 calls={requests.Length}.");
        }
        Console.WriteLine("This validates the candidate's opcode/routing, not the identity or exact trigger of the player's unidentified enemy.");
        return 0;
    }
}
