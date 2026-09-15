using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Isolated native body-overlap phase, retaining the preceding earned boost state.</summary>
internal static class TemporaryBlueSandProbe
{
    public static void Run(ISnesAddressSpace bus, RoomLevelData level, SamusState samus, int mode)
    {
        Apply(bus, level, samus, mode);
        var body = samus.Kinematics;
        ushort counter = mode is 1 or 2 or 6 ? (ushort)0 : (ushort)0x0401;
        int displacement = mode switch { 1 or 3 or 6 => 0x12000, 4 => 0x14000, 5 => 0x1c000, _ => 0 };
        if (samus.HorizontalSpeed.SpeedBoostCounter != counter || body.ExtraYFixed != displacement)
            throw new InvalidDataException("Surface sand cancellation or sandfall displacement differs from native body sampling.");
    }

    /// <summary>Installs the isolated body-sample geometry without assuming the caller owns Blue Suit.</summary>
    public static void Apply(ISnesAddressSpace bus, RoomLevelData level, SamusState samus, int mode)
    {
        var body = samus.Kinematics;
        int row = (mode == 2 ? body.YPosition - body.YRadius : body.YPosition + body.YRadius - 1) >> 4;
        int index = row * level.WidthInBlocks + (body.XPosition >> 4);
        if (mode != 0)
        {
            level.SetForegroundEntry(index, mode == 7 ? (ushort)0x8000 : (ushort)0x3000);
            level.SetBehavior(index, mode is >= 3 and <= 5 ? (byte)(0x80 + mode) : (byte)0x82);
            if (mode == 6)
            {
                level.SetForegroundEntry(index + 1, 0x3000);
                level.SetBehavior(index + 1, 0x82);
                level.SetForegroundEntry(index, 0x5000);
                level.SetBehavior(index, 1);
            }
        }
        SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Maridia);
    }
}
