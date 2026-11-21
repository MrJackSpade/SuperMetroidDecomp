using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Charge-flare pictures by their number among a component's pictures, its phases in order, as the original programs and
/// the flare's selector table count them (#2285, #2396), and a component's play read and set as the original's picture
/// number and wait (#2553): checks compare the shared player's play with those numbers. Each component's ordinary play
/// (<see cref="ChargeFlarePlay{TSegment, TPicture}"/>) and the Hyper Beam's count of it
/// (<see cref="HyperBeamFlareCount{TSegment, TPicture}"/>) keep their player private; these read it by name.
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
        internal static ProjectileSpriteId CompositionNumbered(ChargeFlareComponent component, HorizontalDirection facing, int number) => component switch
        {
            ChargeFlareComponent.MainFlare => ChargeFlarePictures.MainFlare(ChargeFlarePictureNumbers.MainFlare(number)),
            ChargeFlareComponent.SlowSparks => ChargeFlarePictures.SlowSparks(facing, ChargeFlarePictureNumbers.SlowSparks(number)),
            _ => ChargeFlarePictures.FastSparks(facing, ChargeFlarePictureNumbers.FastSparks(number)),
        };
    }

    /// <summary>The spark ring <paramref name="component"/>'s animation.</summary>
    private static LoopingAnimation<SparkSegment> SparksAnimation(ChargeFlareComponent component) => component == ChargeFlareComponent.SlowSparks
        ? ChargeFlareAnimationDefinitions.SlowSparksAnimation
        : ChargeFlareAnimationDefinitions.FastSparksAnimation;

    /// <summary>A charge's flare components as the original counts them.</summary>
    extension(BeamChargeState self)
    {
        /// <summary>Flare <paramref name="component"/> of the charge, read and set as the original counts it.</summary>
        internal ChargeFlareComponentNative Flare(ChargeFlareComponent component) => new(self, component);
    }

    /// <summary>The grapple's own flare as the original counts it.</summary>
    extension(SamusGrappleState self)
    {
        /// <summary>The number of the picture the grapple's flare shows among the main flare's pictures; setting it keeps the wait.</summary>
        internal int FlareFrame
        {
            get => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Picture(self.Flare)
                ?? throw new InvalidOperationException("The grapple's flare plays forward and always shows a picture.");
            set => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.ShowNative(self.Flare, value,
                ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Wait(self.Flare));
        }

        /// <summary>The grapple flare's wait as the original keeps it; setting it keeps the picture.</summary>
        internal WrappingCountdown FlareAnimationTimer
        {
            get => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Wait(self.Flare);
            set => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.ShowNative(self.Flare, self.FlareFrame, value);
        }
    }
}
