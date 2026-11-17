namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The landing Samus takes from an aim (#2323): with Shoot released, and with Shoot held.</summary>
/// <param name="ShootReleased">The landing she takes with Shoot released.</param>
/// <param name="ShootHeld">The landing she takes with Shoot held; a level aim fires as she lands, any other aim lands the same.</param>
internal sealed record SamusLandingChoice(SamusPoseId ShootReleased, SamusPoseId ShootHeld);
