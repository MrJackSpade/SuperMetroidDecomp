using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What a Draygon wall cannon of one facing shows, and for how long (#2401).</summary>
/// <param name="Shield">The shield a whole cannon shows and then rests on.</param>
/// <param name="HitFlash">The shield's flash after a hit that does not destroy the cannon; the shield follows.</param>
/// <param name="Sparks">The destroyed cannon's sparks, looping for good.</param>
internal sealed partial record DraygonCannonLook(
    Animation<DraygonCannonSegment> Shield,
    Animation<DraygonCannonSegment> HitFlash,
    Animation<DraygonCannonSegment> Sparks);
