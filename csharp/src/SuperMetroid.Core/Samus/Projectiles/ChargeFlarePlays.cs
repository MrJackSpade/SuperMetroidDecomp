using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Builds each charge-flare component's ordinary animation, with its own pictures and compositions (#2553).</summary>
internal static class ChargeFlarePlays
{
    /// <summary>The main flare's ordinary animation, cleared on its first picture.</summary>
    internal static ChargeFlarePlay<MainFlareSegment, MainFlarePicture> MainFlare() =>
        new(MainFlareSegments, MainFlareAnimation, static (_, picture) => ChargeFlarePictures.MainFlare(picture));

    /// <summary>Component <paramref name="component"/>'s ordinary animation, cleared on its first picture.</summary>
    internal static IChargeFlarePlay Of(ChargeFlareComponent component) => Builders[component]();

    /// <summary>What builds each component's ordinary animation.</summary>
    private static IdentityTable<ChargeFlareComponent, Func<IChargeFlarePlay>> Builders { get; } = new(new Dictionary<ChargeFlareComponent, Func<IChargeFlarePlay>>
    {
        [ChargeFlareComponent.MainFlare] = static () => MainFlare(),
        [ChargeFlareComponent.SlowSparks] = static () => new ChargeFlarePlay<SparkSegment, SparkPicture>(SparkSegments, SlowSparksAnimation, ChargeFlarePictures.SlowSparks),
        [ChargeFlareComponent.FastSparks] = static () => new ChargeFlarePlay<SparkSegment, SparkPicture>(SparkSegments, FastSparksAnimation, ChargeFlarePictures.FastSparks),
    });
}
