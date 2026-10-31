using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Publishes one of two transitional poses, chosen by whether Samus has any of the tested items.</summary>
/// <param name="Item">The items tested.</param>
/// <param name="Unequipped">The pose taken without any of them.</param>
/// <param name="Equipped">The pose taken with one of them.</param>
internal sealed record SamusAnimationItemTransition(SamusEquipmentFlags Item, SamusPoseId Unequipped, SamusPoseId Equipped)
    : SamusAnimationInstruction;
