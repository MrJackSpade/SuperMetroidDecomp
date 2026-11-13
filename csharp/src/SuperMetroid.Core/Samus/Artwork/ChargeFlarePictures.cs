using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which composition a charge-flare component shows for its picture and Samus's facing (#1624). The main flare's
/// pictures are the same either way; each spark ring shows its own facing's six compositions.
/// </summary>
/// <remarks>
/// A component shows only its own pictures. A place outside them, which only the Hyper Beam's backward count reaches
/// once it has run a component past its first picture, shows nothing: it does not draw the neighbouring run's
/// picture there, which was a presentation quirk (owner decision, 2026-10-12).
/// </remarks>
public static class ChargeFlarePictures
{
    /// <summary>The main flare's compositions, by picture.</summary>
    private static readonly ProjectileSpriteId[] MainFlare =
        [.. MainFlarePictures.Select(main => new ProjectileSpriteId(ProjectileSpriteGroup.FlareMain, main))];

    /// <summary>
    /// The composition <paramref name="component"/> shows at picture <paramref name="phase"/> facing left or right, or
    /// null when the place is none of the component's pictures.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <param name="facingLeft">Whether Samus faces left.</param>
    /// <param name="phase">The component's picture, 0 through 65535.</param>
    public static ProjectileSpriteId? Of(ChargeFlareComponent component, bool facingLeft, int phase)
    {
        ProjectileSpriteId[] pictures = PicturesOf(component, facingLeft);
        return Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, phase) < pictures.Length ? pictures[phase] : null;
    }

    /// <summary>The sprite source instance that tells the flare's three components' draws apart.</summary>
    internal static int DrawSlot(ChargeFlareComponent component) => component switch
    {
        ChargeFlareComponent.MainFlare => 0,
        ChargeFlareComponent.SlowSparks => 1,
        ChargeFlareComponent.FastSparks => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

    /// <summary><paramref name="component"/>'s compositions for the facing, by picture.</summary>
    private static ProjectileSpriteId[] PicturesOf(ChargeFlareComponent component, bool facingLeft) => component switch
    {
        ChargeFlareComponent.MainFlare => MainFlare,
        ChargeFlareComponent.SlowSparks => Sparks[facingLeft ? ProjectileSpriteGroup.FlareSlowSparksLeft : ProjectileSpriteGroup.FlareSlowSparksRight],
        ChargeFlareComponent.FastSparks => Sparks[facingLeft ? ProjectileSpriteGroup.FlareFastSparksLeft : ProjectileSpriteGroup.FlareFastSparksRight],
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

    /// <summary>Each spark family's compositions, by picture.</summary>
    private static readonly Dictionary<ProjectileSpriteGroup, ProjectileSpriteId[]> Sparks =
        Groups.Where(sparks => sparks.Group != ProjectileSpriteGroup.FlareMain).ToDictionary(sparks => sparks.Group, sparks => sparks.Members.ToArray());
}
