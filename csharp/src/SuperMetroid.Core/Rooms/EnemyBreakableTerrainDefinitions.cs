namespace SuperMetroid.Core.Rooms;

/// <summary>Native enemy/spike collision dispatch and its crumble actor.</summary>
public static class EnemyBreakableTerrainDefinitions
{
    /// <summary>$A0:C2DA, EnemyBlockCollisionReaction_Spike.PLMs; zero means solid.</summary>
    public const int ReactionTable = 0xa0c2da;
    /// <summary>$A0:C2C7 masks the BTS high bit before indexing the word table.</summary>
    public const byte ReactionIndexMask = 0x7f;
    /// <summary>$84:D094, PLMEntries_EnemyBreakableBlock; table entry $0F.</summary>
    public const ushort Header = 0xd094;
    /// <summary>$84:CD53, sound plus four timed crumble draws, then deletion.</summary>
    public const ushort InstructionList = 0xcd53;
}
