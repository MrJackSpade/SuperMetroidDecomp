using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Real vertical collision against speed/bomb terrain after full or partial earned boost.</summary>
internal static class TemporaryBlueTerrainProbe
{
    public static string Run(ISnesAddressSpace bus, RoomLevelData level, SamusState samus, int mode, bool fullBoost)
    {
        var body = samus.Kinematics;
        bool up = (mode & 2) != 0;
        int row = (up ? body.YPosition - body.YRadius - 4 : body.YPosition + body.YRadius + 3) >> 4;
        int first = row * level.WidthInBlocks + ((body.XPosition - body.XRadius) >> 4);
        int last = row * level.WidthInBlocks + ((body.XPosition + body.XRadius - 1) >> 4);
        ushort original = mode < 4 ? (ushort)0xb123 : (ushort)0xf123;
        for (int index = first; index <= last; index++)
        {
            level.SetForegroundEntry(index, original);
            level.SetBehavior(index, (byte)(mode < 4 ? 0x0e + (mode & 1) : mode & 1));
        }
        // An empty native actor pool isolates allocation and immediate collision
        // mutation. No PLM instruction tick or renderer update is substituted here.
        var plms = new RoomPlmSystem();
        uint before = body.YFixed;
        BlockMoveResult movement = SamusBlockCollision.MoveVertical(bus, level, body,
            (up ? -4 : 4) << 16, scanLeftToRight: true, plms: plms);
        ushort firstWord = level.GetCollisionBlockByIndex(first).LevelWord;
        ushort lastWord = level.GetCollisionBlockByIndex(last).LevelWord;
        ushort expected = fullBoost ? mode < 4 ? (ushort)0x00b6 : (ushort)0x0058 : original;
        if (movement.Collided == fullBoost || firstWord != expected || lastWord != expected ||
            plms.ActiveCount != (fullBoost ? last - first + 1 : 0))
            throw new InvalidDataException("Earned boost did not produce native collision, tile mutation and PLM allocation.");
        if (fullBoost && body.YFixed != unchecked(before + (uint)((up ? -4 : 4) << 16)))
            throw new InvalidDataException("Breaking terrain must admit the entire requested displacement.");
        return $"{(movement.Collided ? 1 : 0):X4},{firstWord:X4},{lastWord:X4},{plms.ActiveCount:X4}";
    }
}
