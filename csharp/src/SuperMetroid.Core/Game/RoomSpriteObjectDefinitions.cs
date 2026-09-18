namespace SuperMetroid.Core.Game;

/// <summary>Compiled initial instruction selectors for bank-$B4 room sprite objects.</summary>
internal static class RoomSpriteObjectDefinitions
{
    /// <summary>
    /// <c>SpriteObject_DrawInst_Pointers</c> at <c>$B4:BDA8-$B4:BE23</c>.
    /// Entries are indexed directly by the native object number passed to
    /// <c>CreateSpriteAtPos</c>; the selected mixed instruction programs remain in bank $B4.
    /// </summary>
    private static readonly ushort[] InstructionPointers =
    [
        0xbe5a, 0xbe6c, 0xbe86, 0xbea4, 0xbebe, 0xbed4, 0xbeea, 0xbf04,
        0xbf12, 0xbf1c, 0xbf32, 0xbf44, 0xbf56, 0xbf8e, 0xbfa0, 0xbfb2,
        0xbfc4, 0xbfd2, 0xc014, 0xc026, 0xc040, 0xc05e, 0xc080, 0xc0fe,
        0xc10c, 0xc132, 0xc154, 0xc176, 0xbf68, 0xbf74, 0xc198, 0xc1ac,
        0xc1c0, 0xc1d4, 0xc1e8, 0xc1fc, 0xc210, 0xc224, 0xc238, 0xc258,
        0xc2a0, 0xc2bc, 0xc304, 0xc30a, 0xc33e, 0xc35c, 0xc37a, 0xbe54,
        0xc390, 0xc3a2, 0xc3ba, 0xc436, 0xc4b6, 0xc536, 0xc5b2, 0xc5c6,
        0xc5d8, 0xc5de, 0xc5e4, 0xc608, 0xc61c, 0xbe24,
    ];

    /// <summary>Returns the bank-$B4 instruction list for one native object number.</summary>
    internal static ushort InstructionPointer(RoomSpriteObjectKind kind)
    {
        uint index = (ushort)kind;
        if (index >= InstructionPointers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind), kind,
                "Room sprite object number must be zero through $3D.");
        }

        return InstructionPointers[index];
    }
}
