namespace SuperMetroid.Core.Game;

using SuperMetroid.Core.Rooms;

/// <summary>Crocomire's bridge threshold latches and cross-bank arena publications.</summary>
public sealed partial class RoomEnemySystem
{
    private void PublishCrocomirePlm(byte blockX, byte blockY, ushort header) =>
        _crocomirePlmRequests.Add(new CrocomirePlmRequest(blockX, blockY, header));

    /// <summary>Ports the staged $0600/$0620/$0630 bridge effects in $A4:8D5E.</summary>
    private void HandleCrocomireBridgeApproach(RoomEnemySlot body)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (unchecked((short)(body.XPosition - 1536)) < 0)
        {
            death.BridgeDustAt1536Spawned = false;
            death.BridgeBlockAt1568Crumbling = false;
            death.BridgeBlocksAt1584Crumbling = false;
            return;
        }

        if (unchecked((short)(body.XPosition - 1552)) < 0)
        {
            if (!death.BridgeDustAt1536Spawned)
            {
                death.BridgeDustAt1536Spawned = true;
                SpawnRoomGraphicsDustExplosion(1536, 176, animationIndex: 0x0015);
            }
            return;
        }

        if (unchecked((short)(body.XPosition - 1568)) < 0)
        {
            death.BridgeBlockAt1568Crumbling = false;
            death.BridgeBlocksAt1584Crumbling = false;
            return;
        }

        if (unchecked((short)(body.XPosition - 1584)) < 0)
        {
            if (!death.BridgeBlockAt1568Crumbling)
            {
                death.BridgeBlockAt1568Crumbling = true;
                PublishCrocomirePlm(
                    0x61, 0x0b, RoomPlmHeaders.CrumbleCrocomireBridgeBlock);
                SpawnRoomGraphicsDustExplosion(1568, 176, animationIndex: 0x0015);
            }
            return;
        }

        if (unchecked((short)(body.XPosition - 1600)) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            PublishCrocomirePlm(
                0x62, 0x0b, RoomPlmHeaders.CrumbleCrocomireBridgeBlock);
            PublishCrocomirePlm(
                0x63, 0x0b, RoomPlmHeaders.CrumbleCrocomireBridgeBlock);
            SpawnRoomGraphicsDustExplosion(1584, 176, animationIndex: 0x0015);
        }
    }

    /// <summary>
    /// Ports <c>Crocomire_8EE5</c>: clear all ten bridge blocks and burst dust.
    /// The ten native PLM operand records at $A4:8EE9 + 8*i exactly use
    /// block X=$61+i, block Y=$0B, and header $B74F for i=0..9. The
    /// inclusive production loop preserves their ascending publication
    /// order; $A4:8F35 starts dust setup, not an eleventh clear record.
    /// The seven dust positions in the pinned NTSC J/U v1.0 ROM follow the
    /// exact bounded rule X=$0600+$0010*i for i=0..6; Y=$00B0 at i=0 or 2
    /// and $00C0 otherwise. Native $A4:8F35-$8FB4 encodes these as ordered
    /// immediate operands, not a contiguous table. All seven pairs and their
    /// order match the local span; there is no eighth dust spawn.
    /// </summary>
    private void PublishCrocomireBridgeCollapsePlms()
    {
        for (byte blockX = 0x61; blockX <= 0x6a; blockX++)
            PublishCrocomirePlm(
                blockX, 0x0b, RoomPlmHeaders.ClearCrocomireBridgeBlock);

        ReadOnlySpan<(ushort X, ushort Y)> dustPositions =
        [
            (1536, 176),
            (1552, 192),
            (1568, 176),
            (1584, 192),
            (1600, 192),
            (1616, 192),
            (1632, 192),
        ];
        foreach ((ushort x, ushort y) in dustPositions)
            SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 0x0015);

        PublishCrocomirePlm(0x4e, 0x03, RoomPlmHeaders.CreateCrocomireInvisibleWall);
    }
}
