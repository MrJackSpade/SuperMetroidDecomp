using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Which sprite colour store each shared role uses (#1405, #1464).</summary>
internal static class SpriteColourSetDefinitions
{
    /// <summary>The store a room load fills with the common sprite colours, which an enemy's hit flash shows.</summary>
    public static readonly SpriteColourStore Common = SpriteColourStore.Common;

    /// <summary>The store a room load fills with the enemy projectiles' colours.</summary>
    public static readonly SpriteColourStore EnemyProjectiles = SpriteColourStore.EnemyProjectiles;

    /// <summary>The store a frozen enemy shows: the colours of the beam that froze it.</summary>
    public static readonly SpriteColourStore Frozen = SpriteColourStore.SamusBeams;

    /// <summary>The stores the Chozo statues and the glass tube colour, in the order their colours are written.</summary>
    public static readonly ImmutableArray<SpriteColourStore> ChozoAndTube =
        [SpriteColourStore.EnemiesFirst, SpriteColourStore.EnemiesSecond];

    /// <summary>The store dead Sidehoppers fade in, from its second colour.</summary>
    public static readonly SpriteColourStore DeadSidehopper = SpriteColourStore.EnemiesFirst;

    /// <summary>The store the Ninja Space Pirates' gold wall colours load into.</summary>
    public static readonly SpriteColourStore NinjaSpacePirate = SpriteColourStore.EnemiesFourth;

    /// <summary>
    /// The store a Ninja Space Pirate's divekick flashes back to and lands in. The Metal Pirates room loads the gold
    /// Ninja Space Pirates' own colours there; the escape's Climb loads its silver one into another store, so there the
    /// divekick leaves it in whatever colours this store holds, as on the cartridge.
    /// </summary>
    public static readonly SpriteColourStore NinjaSpacePirateStance = SpriteColourStore.EnemiesFirst;
}
