using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which composition a charge-flare component shows for its picture and Samus's facing (#1624). The main flare's
/// pictures are the same either way; each spark ring shows its own facing's six compositions.
/// </summary>
/// <remarks>
/// The flare's draw lays its pictures out one run after another, the main flare's thirty, then the right-facing slow and
/// fast sparks and the left-facing slow and fast sparks, and takes a component's picture within a cycle of 256. A place
/// past the component's own run, which only the Hyper Beam's backward count wrapped past a first picture reaches, shows
/// the picture laid out there, and a place past the last run shows nothing.
/// </remarks>
public static class ChargeFlarePictures
{
    /// <summary>Every picture in the order the flare's draw lays them out.</summary>
    private static readonly ProjectileSpriteId[] LaidOut =
    [
        .. MainFlarePictures.Select(main => new ProjectileSpriteId(ProjectileSpriteGroup.FlareMain, main)),
        .. SparkRuns.SelectMany(run => Groups.Single(group => group.Group == run).Members),
    ];

    /// <summary>
    /// The composition <paramref name="component"/> shows at picture <paramref name="phase"/> facing left or right, or
    /// null when the place shows no flare picture.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <param name="facingLeft">Whether Samus faces left.</param>
    /// <param name="phase">The component's picture, 0 through 65535.</param>
    public static ProjectileSpriteId? Of(ChargeFlareComponent component, bool facingLeft, int phase)
    {
        int place = RunStart(component, facingLeft) + Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, phase) % DrawnPictureCycle;
        return place < LaidOut.Length ? LaidOut[place] : null;
    }

    /// <summary>The sprite source instance that tells the flare's three components' draws apart.</summary>
    internal static int DrawSlot(ChargeFlareComponent component) => component switch
    {
        ChargeFlareComponent.MainFlare => 0,
        ChargeFlareComponent.SlowSparks => 1,
        ChargeFlareComponent.FastSparks => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

    /// <summary>Where <paramref name="component"/>'s run of pictures starts for the facing.</summary>
    private static int RunStart(ChargeFlareComponent component, bool facingLeft) => component switch
    {
        ChargeFlareComponent.MainFlare => 0,
        ChargeFlareComponent.SlowSparks => StartOf(facingLeft ? ProjectileSpriteGroup.FlareSlowSparksLeft : ProjectileSpriteGroup.FlareSlowSparksRight),
        ChargeFlareComponent.FastSparks => StartOf(facingLeft ? ProjectileSpriteGroup.FlareFastSparksLeft : ProjectileSpriteGroup.FlareFastSparksRight),
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

    /// <summary>Where spark family <paramref name="group"/>'s run starts among the laid-out pictures.</summary>
    private static int StartOf(ProjectileSpriteGroup group) => Array.IndexOf(LaidOut, new ProjectileSpriteId(group, 0));
}
