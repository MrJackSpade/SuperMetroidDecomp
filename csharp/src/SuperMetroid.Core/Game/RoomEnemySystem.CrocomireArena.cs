namespace SuperMetroid.Core.Game;

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
                PublishCrocomirePlm(0x61, 0x0b, 0xb74b);
                SpawnRoomGraphicsDustExplosion(1568, 176, animationIndex: 0x0015);
            }
            return;
        }

        if (unchecked((short)(body.XPosition - 1600)) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            PublishCrocomirePlm(0x62, 0x0b, 0xb74b);
            PublishCrocomirePlm(0x63, 0x0b, 0xb74b);
            SpawnRoomGraphicsDustExplosion(1584, 176, animationIndex: 0x0015);
        }
    }

    /// <summary>Ports <c>Crocomire_8EE5</c>: clear all ten bridge blocks and burst dust.</summary>
    private void PublishCrocomireBridgeCollapsePlms()
    {
        for (byte blockX = 0x61; blockX <= 0x6a; blockX++)
            PublishCrocomirePlm(blockX, 0x0b, 0xb74f);

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

        PublishCrocomirePlm(0x4e, 0x03, 0xb757);
    }
}
