using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Which sprite colour set each shared role uses (#1405).</summary>
internal static class SpriteColourSetDefinitions
{
    /// <summary>The set a room load fills with the common sprite colours, which an enemy's hit flash shows.</summary>
    public const int Common = 0;

    /// <summary>The set a room load fills with the enemy projectiles' colours.</summary>
    public const int EnemyProjectiles = 5;

    /// <summary>The set a frozen enemy shows: the colours of the beam that froze it.</summary>
    public const int Frozen = 6;

    /// <summary>The set the Chozo statues and the glass tube colour, with the one after it.</summary>
    public const int ChozoAndTube = 1;

    /// <summary>The sets the Chozo statues and the glass tube colour.</summary>
    public const int ChozoAndTubeSets = 2;

    /// <summary>The set dead Sidehoppers fade in, from its second colour.</summary>
    public const int DeadSidehopper = 1;

    /// <summary>The set the escape's typewriter text shows, whose two colours it copies from the background.</summary>
    public const int EscapeTypewriterText = 1;

    /// <summary>The first of the two typewriter text colours in its set.</summary>
    public const int EscapeTypewriterTextFirstColour = 13;

    /// <summary>The set the Ninja Space Pirates' gold wall colours load into.</summary>
    public const int NinjaSpacePirate = 7;

    /// <summary>The shared set each role row holds, which a row imported or recorded as a number denotes.</summary>
    public static IReadOnlyDictionary<int, SharedSpriteColouring> RoleRows { get; } = new Dictionary<int, SharedSpriteColouring>
    {
        [Common] = SharedSpriteColouring.Common,
        [EnemyProjectiles] = SharedSpriteColouring.EnemyProjectiles,
        [Frozen] = SharedSpriteColouring.Frozen,
    };
}
