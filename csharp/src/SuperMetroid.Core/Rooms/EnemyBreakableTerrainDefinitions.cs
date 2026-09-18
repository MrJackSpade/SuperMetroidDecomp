namespace SuperMetroid.Core.Rooms;

/// <summary>Native enemy/spike collision dispatch and its crumble actor.</summary>
public static class EnemyBreakableTerrainDefinitions
{
    /// <summary>$84:D094, PLMEntries_EnemyBreakableBlock; table entry $0F.</summary>
    public const ushort Header = 0xd094;
    /// <summary>$84:CD53, sound plus four timed crumble draws, then deletion.</summary>
    public const ushort InstructionList = 0xcd53;

    /// <summary>
    /// Resolves <c>EnemyBlockCollisionReaction_Spike.PLMs</c> at
    /// <c>$A0:C2DA-$A0:C2F9</c>. The cartridge masks bit seven, treats entries zero
    /// through fourteen as solid, and spawns <see cref="Header"/> for entry fifteen.
    /// </summary>
    public static bool IsEnemyBreakable(byte behavior)
    {
        int index = behavior & 0x7f;
        if (index > 15)
        {
            throw new InvalidDataException(
                $"Enemy spike BTS ${behavior:X2} indexes past the sixteen-entry reaction table.");
        }

        return index == 15;
    }
}
