using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native Owtch and Stoke visual identities calculated from directional program and OAM layouts.</summary>
internal static class OwtchStokeVisualDefinitions
{
    /// <summary>$A2:A589 / Spritemap_Owtch_0 begins three one-object, seven-byte compositions.</summary>
    private const ushort OwtchFirstFrame = 0xa589;
    /// <summary>$A2:8ACA begins Stoke's left-facing maps: two, three, two, two, four OAM objects.</summary>
    private const ushort StokeFirstFrame = 0x8aca;
    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        if (enemyDefinition == RoomEnemySystem.OwtchDefinition)
        {
            int offset = address - OwtchInstructionProgramDefinitions.MovingLeft;
            int within = offset % 18;
            if ((uint)offset < 36 && within is >= 4 and <= 12 && within % 4 == 0)
            {
                int frame = (within - 4) / 4;
                if (offset >= 18) frame = 2 - frame;
                return (ushort)(OwtchFirstFrame + frame * 7);
            }
        }
        else if (enemyDefinition == RoomEnemySystem.StokeDefinition)
        {
            int offset = address - StokeInstructionProgramDefinitions.MovingLeft;
            if ((uint)offset < 76)
            {
                int frame = (offset % 38) switch
                {
                    4 => 0, 8 => 1, 12 or 24 => 2, 16 => 3, 32 => 4, _ => -1,
                };
                if (frame >= 0)
                    return (ushort)(StokeFirstFrame + offset / 38 * 75 + frame * 12 + (frame >= 2 ? 5 : 0));
            }
        }
        else throw new InvalidDataException($"Enemy ${enemyDefinition:X4} has no compiled Owtch/Stoke visuals.");
        throw new InvalidDataException($"Owtch/Stoke ${enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
