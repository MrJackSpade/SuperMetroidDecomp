using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Charge-flare pictures by their number among a component's pictures, its phases in order, as the original programs and
/// the flare's selector table count them (#2285, #2396), and a charge's or the grapple's flare played to a numbered
/// picture and wait as the game plays it (#2553): checks compare what the flare shows with those numbers.
/// </summary>
internal static class ChargeFlareAnimationsAccess
{
    /// <summary>Pictures by number.</summary>
    extension(ChargeFlareAnimationDefinitions)
    {
        /// <summary>How many pictures <paramref name="component"/> has across its phases.</summary>
        internal static int PictureCount(ChargeFlareComponent component) => ChargeFlareAnimationDefinitions.PictureDurations(component).Count;

        /// <summary>The updates each picture of <paramref name="component"/> shows for, its phases in order.</summary>
        internal static IReadOnlyList<int> PictureDurations(ChargeFlareComponent component) => component == ChargeFlareComponent.MainFlare
            ? ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Durations(ChargeFlareAnimationDefinitions.MainFlareSegments, ChargeFlareAnimationDefinitions.MainFlareAnimation)
            : ChargeFlareNativePlay<SparkSegment, SparkPicture>.Durations(ChargeFlareAnimationDefinitions.SparkSegments, SparksAnimation(component));

        /// <summary>
        /// The pictures of <paramref name="component"/> its loop repeats, by number: from the first the loop shows to one
        /// past the last.
        /// </summary>
        internal static (int From, int End) LoopedPictures(ChargeFlareComponent component) => component == ChargeFlareComponent.MainFlare
            ? ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.LoopedPictures(ChargeFlareAnimationDefinitions.MainFlareAnimation.Animation)
            : ChargeFlareNativePlay<SparkSegment, SparkPicture>.LoopedPictures(SparksAnimation(component).Animation);

        /// <summary>The composition <paramref name="component"/>'s picture numbered <paramref name="number"/> shows in, facing <paramref name="facing"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number.</exception>
        internal static ProjectileSpriteId CompositionNumbered(ChargeFlareComponent component, HorizontalDirection facing, int number) =>
            component == ChargeFlareComponent.MainFlare ? ChargeFlarePictures.MainFlare(ChargeFlarePictureNumbers.MainFlare(number))
            : component == ChargeFlareComponent.SlowSparks ? ChargeFlarePictures.SlowSparks(facing, ChargeFlarePictureNumbers.SlowSparks(number))
            : ChargeFlarePictures.FastSparks(facing, ChargeFlarePictureNumbers.FastSparks(number));
    }

    /// <summary>The spark ring <paramref name="component"/>'s animation.</summary>
    private static LoopingAnimation<SparkSegment> SparksAnimation(ChargeFlareComponent component) => component == ChargeFlareComponent.SlowSparks
        ? ChargeFlareAnimationDefinitions.SlowSparksAnimation
        : ChargeFlareAnimationDefinitions.FastSparksAnimation;

    /// <summary>Plays a charge's flare to chosen pictures as the game plays it.</summary>
    extension(BeamChargeState self)
    {
        /// <summary>
        /// Starts the charge's flare, as a charge's first update does, and plays each component forward, as each update of a
        /// charge does, until it shows its picture numbered <paramref name="pictures"/>[place] with
        /// <paramref name="waits"/>[place] updates of the original's wait left, the components in
        /// <see cref="ChargeFlareComponent"/> order.
        /// </summary>
        internal void PlayFlareTo(IReadOnlyList<int> pictures, IReadOnlyList<int> waits)
        {
            self.StartChargeFlare();
            ChargeFlareComponent[] components = Enum.GetValues<ChargeFlareComponent>();
            for (int place = 0; place < components.Length; place++)
            {
                for (int update = UpdatesTo(components[place], pictures[place], waits[place]); update > 0; update--)
                    self.PlayFlareForward(components[place]);
            }
        }
    }

    /// <summary>Plays the grapple's own flare to a chosen picture as the game plays it.</summary>
    extension(SamusGrappleState self)
    {
        /// <summary>
        /// Starts the grapple's flare at its first picture and plays it forward until it shows the main flare's picture
        /// numbered <paramref name="number"/> with <paramref name="wait"/> updates of the original's wait left.
        /// </summary>
        internal void PlayFlareTo(int number, int wait)
        {
            self.Flare.Start();
            for (int update = UpdatesTo(ChargeFlareComponent.MainFlare, number, wait); update > 0; update--)
                self.Flare.PlayForward();
        }
    }

    /// <summary>
    /// The updates <paramref name="component"/>, started at its first picture, plays forward before it shows its picture
    /// numbered <paramref name="number"/> with <paramref name="wait"/> updates of the original's wait left: every update the
    /// pictures before it show for, then its own less the wait and one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number, or the picture never holds that wait.</exception>
    private static int UpdatesTo(ChargeFlareComponent component, int number, int wait)
    {
        IReadOnlyList<int> durations = ChargeFlareAnimationDefinitions.PictureDurations(component);
        int held = durations[Ensure.BetweenInclusive(0, durations.Count - 1, number)];
        return durations.Take(number).Sum() + held - 1 - Ensure.BetweenInclusive(0, held - 1, wait);
    }
}
