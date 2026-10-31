using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Publishes one of four transitional poses, chosen by whether Samus has any of the tested items and whether she is
/// moving vertically. Morph entry uses it to pick a Spring Ball or ordinary ball, grounded or falling.
/// </summary>
/// <param name="Item">The items tested.</param>
/// <param name="UnequippedGrounded">The pose taken without the items while not moving vertically.</param>
/// <param name="UnequippedAirborne">The pose taken without the items while moving vertically.</param>
/// <param name="EquippedGrounded">The pose taken with one of the items while not moving vertically.</param>
/// <param name="EquippedAirborne">The pose taken with one of the items while moving vertically.</param>
internal sealed record SamusAnimationItemAirborneTransition(
    SamusEquipmentFlags Item,
    SamusPoseId UnequippedGrounded,
    SamusPoseId UnequippedAirborne,
    SamusPoseId EquippedGrounded,
    SamusPoseId EquippedAirborne) : SamusAnimationInstruction;
