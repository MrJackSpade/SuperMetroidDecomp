using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One flare component of a charge, read and set as the original counts it: its picture number and wait.</summary>
/// <param name="charge">The charge whose flare it is.</param>
/// <param name="component">The component.</param>
internal sealed class ChargeFlareComponentNative(BeamChargeState charge, ChargeFlareComponent component)
{
    /// <summary>The component.</summary>
    internal ChargeFlareComponent Component { get; } = component;

    /// <summary>The component's ordinary play.</summary>
    private IChargeFlarePlay Play =>
        PrivateState.Field<BeamChargeState, IdentityTable<ChargeFlareComponent, IChargeFlarePlay>>(charge, "_flare")[Component];

    /// <summary>The Hyper Beam's count of the component, or none while the ordinary flare plays.</summary>
    private IHyperBeamFlareCount? Count =>
        PrivateState.Field<BeamChargeState, IdentityTable<ChargeFlareComponent, IHyperBeamFlareCount>?>(charge, "_hyperBeamCount")?[Component];

    /// <summary>
    /// The number of the picture the component shows among its pictures, or null once the Hyper Beam's count has stepped
    /// back past its first.
    /// </summary>
    internal int? NativePicture => (Count, Play) switch
    {
        (HyperBeamFlareCount<MainFlareSegment, MainFlarePicture> main, _) => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Picture(main),
        (HyperBeamFlareCount<SparkSegment, SparkPicture> sparks, _) => ChargeFlareNativePlay<SparkSegment, SparkPicture>.Picture(sparks),
        (null, ChargeFlarePlay<MainFlareSegment, MainFlarePicture> main) => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Picture(main),
        (null, ChargeFlarePlay<SparkSegment, SparkPicture> sparks) => ChargeFlareNativePlay<SparkSegment, SparkPicture>.Picture(sparks),
        _ => throw new InvalidOperationException($"Charge-flare {Component} plays in no form the original's count is read from."),
    };

    /// <summary>The ordinary flare's wait as the original keeps it.</summary>
    internal WrappingCountdown NativeWait => Play switch
    {
        ChargeFlarePlay<MainFlareSegment, MainFlarePicture> main => ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.Wait(main),
        ChargeFlarePlay<SparkSegment, SparkPicture> sparks => ChargeFlareNativePlay<SparkSegment, SparkPicture>.Wait(sparks),
        _ => throw new InvalidOperationException($"Charge-flare {Component} plays in no form the original's wait is read from."),
    };

    /// <summary>Shows the ordinary flare's picture numbered <paramref name="number"/> with the original's wait <paramref name="wait"/>.</summary>
    internal void ShowNative(int number, WrappingCountdown wait)
    {
        switch (Play)
        {
            case ChargeFlarePlay<MainFlareSegment, MainFlarePicture> main:
                ChargeFlareNativePlay<MainFlareSegment, MainFlarePicture>.ShowNative(main, number, wait);
                break;
            case ChargeFlarePlay<SparkSegment, SparkPicture> sparks:
                ChargeFlareNativePlay<SparkSegment, SparkPicture>.ShowNative(sparks, number, wait);
                break;
            default:
                throw new InvalidOperationException($"Charge-flare {Component} plays in no form the original's picture is set in.");
        }
    }
}
