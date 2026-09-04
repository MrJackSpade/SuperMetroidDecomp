using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Retail identities and authored block words used by issue #257's audit.</summary>
internal static class BreakableBlockInitialStateAuditDefinitions
{
    /// <summary><c>$8F:A408</c>, room <c>$01/$28</c> (Below Spazer).</summary>
    public const ushort RoomHeader = 0xa408;

    /// <summary>
    /// The respawning bomb block at room block <c>(8,7)</c>. Its concealed visual index is
    /// <c>$142</c>; bank-$84 deliberately changes it to <c>$058</c> only after activation.
    /// </summary>
    public const int RespawningBombBlockIndex = 232;

    /// <summary>The complete level word authored for the concealed bomb block.</summary>
    public const ushort ConcealedBombBlockWord = 0xf142;

    /// <summary>The temporary solid word installed by setup <c>$84:CEDA</c>.</summary>
    public const ushort ActivatedBombBlockWord = 0x8058;

    /// <summary>The air tile used while the respawning block is absent.</summary>
    public const ushort BrokenBombBlockWord = 0x00ff;

    /// <summary>The solid post-respawn word synthesized by the retail instruction list.</summary>
    public const ushort RespawnedBombBlockWord = 0xf058;

    /// <summary>
    /// Visibly metallic floor blocks that prompted the report. These are ordinary type-$8
    /// solids authored as visual block <c>$05F</c>, not breakable-block reveal graphics.
    /// </summary>
    public static readonly int[] AuthoredMetalFloorBlocks = [902, 905, 907];

    /// <summary>The complete cartridge-authored word for each listed metal floor block.</summary>
    public const ushort AuthoredMetalFloorWord = 0x805f;
}

/// <summary>
/// Cartridge-backed regression for issue #257. It distinguishes the conspicuous metal
/// floor decoration from the room's actual respawning bomb block, then drives that real
/// block through the production bank-$84 PLM lifecycle.
/// </summary>
internal static class BreakableBlockInitialStateAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            BreakableBlockInitialStateAuditDefinitions.RoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomLevelData level = assets.LevelData;

        Require(room.Identity == new RoomIdentity(AreaId.Brinstar, 0x28),
            $"Expected room $01/$28, found {room.Identity}");
        Require(
            level.GetCollisionBlockByIndex(
                BreakableBlockInitialStateAuditDefinitions.RespawningBombBlockIndex).LevelWord ==
            BreakableBlockInitialStateAuditDefinitions.ConcealedBombBlockWord,
            "The real respawning bomb block was not cartridge-authored with its concealed tile");

        foreach (int blockIndex in BreakableBlockInitialStateAuditDefinitions.AuthoredMetalFloorBlocks)
        {
            Require(
                level.GetCollisionBlockByIndex(blockIndex).LevelWord ==
                BreakableBlockInitialStateAuditDefinitions.AuthoredMetalFloorWord,
                $"Metal floor block {blockIndex} no longer matches the room's authored solid tile");
        }

        // Activate the exact room block rather than a synthetic stand-in. The projectile
        // collision dispatcher passes the same BTS and bomb-family word to this setup.
        var plms = new RoomPlmSystem();
        int target = BreakableBlockInitialStateAuditDefinitions.RespawningBombBlockIndex;
        RoomCollisionBlock block = level.GetCollisionBlockByIndex(target);
        Require(
            plms.TrySpawnBombReactionBlock(
                level,
                target,
                block.Bts,
                new SamusProjectileTypeWord((ushort)SamusProjectileFamily.Bomb)),
            "The real respawning bomb block did not allocate a bank-$84 PLM");
        Require(
            level.GetCollisionBlockByIndex(target).LevelWord ==
                BreakableBlockInitialStateAuditDefinitions.ActivatedBombBlockWord,
            "Bomb-block setup did not install the native temporary solid/reveal word");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        bool observedBrokenState = false;
        bool observedRespawnedState = false;
        for (int frame = 0; frame < 450 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(
                bus,
                level,
                streamer,
                layer1XPosition: 0,
                layer1YPosition: 0,
                bg1XOffset: 0);
            ushort word = level.GetCollisionBlockByIndex(target).LevelWord;
            observedBrokenState |=
                word == BreakableBlockInitialStateAuditDefinitions.BrokenBombBlockWord;
            observedRespawnedState |=
                word == BreakableBlockInitialStateAuditDefinitions.RespawnedBombBlockWord;
        }

        Require(observedBrokenState,
            "The real respawning bomb block never reached its broken air tile");
        Require(observedRespawnedState,
            "The real respawning bomb block never reached its native post-respawn tile");
        Require(plms.ActiveCount == 0,
            "The real respawning bomb-block PLM did not finish its instruction list");

        Console.WriteLine(
            "Breakable-block initial-state audit passed: room $01/$28 begins with the " +
            "concealed $F142 bomb block, preserves its cartridge-authored $805F metal " +
            "floor blocks, and reaches $00FF/$F058 only after activation.");
        return 0;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException(message + ".");
    }
}
