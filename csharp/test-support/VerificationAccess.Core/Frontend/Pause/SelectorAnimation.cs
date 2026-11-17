namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Where the item selector's animation stands (#2359): its place, the look it shows and that look's updates left.</summary>
/// <param name="Place">Where the animation stands: the step after the one shown.</param>
/// <param name="Look">The look shown.</param>
/// <param name="UpdatesLeft">The updates the look shown has left.</param>
internal readonly record struct SelectorAnimation(SuperMetroid.Core.Animation.AnimationPlace Place, Artwork.PauseSelectorLook Look, int UpdatesLeft);
