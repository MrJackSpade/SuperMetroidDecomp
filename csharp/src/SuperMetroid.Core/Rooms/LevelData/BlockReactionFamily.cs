namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The reactions a reacting collision kind reads from its block's behaviour (#1722): each family is one reaction type, read
/// by the kinds <see cref="BlockReactionFamilies.Of"/> names. Its values are arbitrary.
/// </summary>
public enum BlockReactionFamily
{
    /// <summary>Shootable blocks and shootable air read a <see cref="ShootableReaction"/>.</summary>
    Shootable,

    /// <summary>Bombable blocks and bombable air read a <see cref="BombableReaction"/>.</summary>
    Bombable,

    /// <summary>Special blocks read a <see cref="SpecialBlockReaction"/>.</summary>
    Special,

    /// <summary>Special-air blocks read a <see cref="SpecialAirReaction"/>.</summary>
    SpecialAir,

    /// <summary>Spike blocks and spike air read a <see cref="SpikeReaction"/>.</summary>
    Spike,

    /// <summary>Grapple blocks read a <see cref="GrappleReaction"/>.</summary>
    Grapple,
}
