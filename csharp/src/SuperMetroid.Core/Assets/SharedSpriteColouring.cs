namespace SuperMetroid.Core.Assets;

/// <summary>The colours of a shared sprite colour set, named by its role (#1464).</summary>
/// <param name="Set">The shared set shown.</param>
internal sealed record SharedSpriteColouring(SharedSpriteColours Set) : SpriteColouring
{
    /// <summary>The common sprite colours, which an enemy's hit flash shows.</summary>
    internal static SharedSpriteColouring Common { get; } = new(SharedSpriteColours.Common);

    /// <summary>The enemy projectiles' colours.</summary>
    internal static SharedSpriteColouring EnemyProjectiles { get; } = new(SharedSpriteColours.EnemyProjectiles);

    /// <summary>The colours a frozen enemy shows.</summary>
    internal static SharedSpriteColouring Frozen { get; } = new(SharedSpriteColours.Frozen);

    /// <summary>The colours dead Sidehoppers fade in.</summary>
    internal static SharedSpriteColouring DeadSidehopperFade { get; } = new(SharedSpriteColours.DeadSidehopperFade);

    /// <summary>The gold wall colours a Ninja Space Pirate's divekick flashes in.</summary>
    internal static SharedSpriteColouring NinjaSpacePirateGoldWall { get; } = new(SharedSpriteColours.NinjaSpacePirateGoldWall);

    /// <summary>The colours a Ninja Space Pirate's divekick flashes back to and lands in.</summary>
    internal static SharedSpriteColouring NinjaSpacePirateStance { get; } = new(SharedSpriteColours.NinjaSpacePirateStance);
}
