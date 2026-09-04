using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused cartridge-backed regression for ordinary type-$B crumble blocks. The room is
/// synthetic so the exact contact boundary stays deterministic, while setup, instruction
/// timing, draw records, sound request, deletion, and restoration all execute from the
/// retail ROM through the production collision and PLM paths.
/// </summary>
internal static class CrumbleBlockAudit
{
    private const int Width = 4;
    private const int Height = 4;
    private const int CrumbleBlockIndex = 2 * Width + 1;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyCrumbleSequence(bus, behavior: 0, respawns: true);
        VerifyCrumbleSequence(bus, behavior: 4, respawns: false);

        Console.WriteLine(
            "Crumble-block audit passed: downward Samus contact activated the retail " +
            "four-frame delay, crumble sound/art, collision removal, permanent deletion, " +
            "and dimension-one respawn sequence.");
        return 0;
    }

    private static void VerifyCrumbleSequence(
        SuperMetroidAddressSpace bus,
        byte behavior,
        bool respawns)
    {
        var foreground = new ushort[Width * Height];
        var behaviors = new byte[foreground.Length];
        foreground[CrumbleBlockIndex] = 0xb321;
        behaviors[CrumbleBlockIndex] = behavior;
        RoomLevelData level = new(
            Width,
            Height,
            foreground,
            behaviors,
            new ushort[foreground.Length],
            ReadOnlySpan<byte>.Empty);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        var samus = new SamusKinematicsState
        {
            XPosition = 24,
            YPosition = 27,
            XRadius = 5,
            YRadius = 5,
        };

        BlockMoveResult contact = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus,
            displacement: 0x0001_0000,
            scanLeftToRight: true,
            includeSolidEnemies: false,
            plms: plms,
            publishDoorSideEffects: false);
        Require(contact.Collided && contact.AcceptedDisplacement == 0 && samus.YPosition == 27,
            $"BTS ${behavior:X2} contact did not retain the solid contact frame");
        Require(plms.ActiveCount == 1,
            $"BTS ${behavior:X2} contact did not allocate its crumble PLM");
        Require(level.GetCollisionBlockByIndex(CrumbleBlockIndex).LevelWord == 0x80bc,
            $"BTS ${behavior:X2} setup did not install its temporary solid word");

        var frameWords = new Dictionary<int, ushort>();
        for (int frame = 1; frame <= (respawns ? 51 : 17); frame++)
        {
            plms.Step(
                bus,
                level,
                streamer,
                layer1XPosition: 0,
                layer1YPosition: 0x8000,
                bg1XOffset: 0);
            frameWords[frame] = level.GetCollisionBlockByIndex(CrumbleBlockIndex).LevelWord;
            if (frame == 4)
            {
                Require(plms.SoundRequests.Count == 1 &&
                    plms.SoundRequests[0].SoundEffect.Value == 0x0a &&
                    plms.SoundRequests[0].MaximumQueued == 1,
                    $"BTS ${behavior:X2} did not queue retail crumble sound $0A");
            }
        }

        Require(frameWords[3] == 0x80bc && frameWords[4] == 0x0053,
            $"BTS ${behavior:X2} did not remove collision after setup's four-frame delay");
        Require(frameWords[respawns ? 12 : 8] == 0x0054 &&
            frameWords[respawns ? 18 : 12] == 0x0055 &&
            frameWords[respawns ? 22 : 16] == 0x00ff,
            $"BTS ${behavior:X2} crumble animation did not use retail frame timing");

        if (respawns)
        {
            Require(frameWords[38] == 0x0055 && frameWords[42] == 0x0054 &&
                frameWords[46] == 0x0053 && frameWords[50] == 0xb0bc,
                "Respawning crumble block did not run the reverse animation and restore collision");
            Require(plms.ActiveCount == 0,
                "Respawning crumble PLM did not delete one handler pass after restoration");
        }
        else
        {
            Require(frameWords[17] == 0x00ff && plms.ActiveCount == 0,
                "Permanent crumble block did not remain air after deleting its PLM");
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException(message + ".");
    }
}
