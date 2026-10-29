namespace SuperMetroid.Core.Assets;

/// <summary>The shared sprite colour sets actors draw in, named by their role in the room (#1464).</summary>
internal enum SharedSpriteColours
{
    /// <summary>The common sprite colours a room load fills, which an enemy's hit flash shows.</summary>
    Common,

    /// <summary>The enemy projectiles' colours a room load fills, which an enemy the graphics set does not load draws in.</summary>
    EnemyProjectiles,

    /// <summary>The colours a frozen enemy shows: the colours of the beam that froze it.</summary>
    Frozen,

    /// <summary>The colours dead Sidehoppers fade in.</summary>
    DeadSidehopperFade,

    /// <summary>The colours the Ninja Space Pirates' gold wall colours load into.</summary>
    NinjaSpacePirateGoldWall,

    /// <summary>
    /// The colours a Ninja Space Pirate's divekick flashes back to and lands in: the set where the Metal Pirates room
    /// loads the gold Ninja Space Pirates' own colours.
    /// </summary>
    NinjaSpacePirateStance,
}
