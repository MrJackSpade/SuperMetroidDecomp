namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's or its invisible link's ignition and the link it names (#2346), created when the Super Missile is
/// fired or reflected or its link is created. A link never ignites: it stays pending, which the gold Ninja's armour reads.
/// </summary>
internal sealed partial class SuperMissileShotState : MissileShotState
{
    /// <summary>Creates a <see cref="SuperMissileShotState"/>.</summary>
    /// <param name="ignition">Where the shot starts in its ignition.</param>
    /// <param name="linkPlace">The link's position in its chain the shot starts naming, or null when it names none.</param>
    public SuperMissileShotState(SamusMissileIgnition.Unignited ignition, int? linkPlace)
        : base(ignition)
    {
        this.LinkChainPosition = linkPlace;
    }

    /// <summary>
    /// The link's position in its chain: the shot place whose projectile an ignited Super Missile moves as its invisible
    /// collision link, a quantity play observes (an ignited Super Missile's count carries it as a 128th of a pixel a place,
    /// <see cref="BeamFiredOverLastShot"/>), or null when it names none. A Super Missile, its link among them, names <see cref="UnlinkedSuperMissileLink.Place"/> from the update it
    /// is created until it ignites with a link of its own; a reflection leaves it naming none until it re-ignites.
    /// </summary>
    internal int? LinkChainPosition { get; set; }

    /// <inheritdoc/>
    internal override int? NamedLinkPlace => LinkChainPosition;

    /// <inheritdoc/>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam => Ignition.ReplacedByBeam(LinkChainPosition);
}
