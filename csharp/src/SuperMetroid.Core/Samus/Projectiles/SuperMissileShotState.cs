namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's or its invisible link's ignition and the link it names (#2346), created when the Super Missile is
/// fired or reflected or its link is created. A link never ignites: it stays pending, which the gold Ninja's armour reads.
/// </summary>
/// <param name="ignition">Where the shot starts in its ignition.</param>
/// <param name="linkPlace">The place the shot starts naming as its link, or null when it names none.</param>
internal sealed class SuperMissileShotState(SamusMissileIgnition.Unignited ignition, int? linkPlace) : MissileShotState(ignition)
{
    /// <summary>
    /// The place whose projectile an ignited Super Missile moves as its invisible collision link, or null when it names
    /// none. A Super Missile, its link among them, names <see cref="UnlinkedSuperMissileLink.Place"/> from the update it
    /// is created until it ignites with a link of its own; a reflection leaves it naming none until it re-ignites.
    /// </summary>
    internal int? LinkPlace { get; set; } = linkPlace;

    /// <inheritdoc/>
    private protected override int? LinkPlaceFound => LinkPlace;
}
