using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Samus's splashes, bubbles, lava spray and dust: the particles her presentation shows, each running its own animation
/// until it runs out (#1556). A new particle never displaces one already shown.
/// </summary>
/// <remarks>
/// What gameplay reads of these effects (whether a breath bubble or a lava spray may start, which decides the bubble's
/// random draw and the bubble and spray sounds) is not read from here: Samus's liquid state keeps it in
/// <see cref="BreathBubbleAndLavaSprayWaits"/>.
/// </remarks>
public sealed class SamusParticles
{
    /// <summary>The particles, oldest first.</summary>
    private readonly List<SamusParticle> _particles = [];

/// <summary>Removes every particle, as room and Samus initialization do.</summary>
    public void Clear() => _particles.Clear();

    /// <summary>Starts a particle on its first step; dust rises as it shows, and every other kind but lava spray stays put.</summary>
    /// <param name="kind">What it shows; lava spray starts through <see cref="SpawnLavaSpray"/>, which gives its drift.</param>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    internal void Spawn(SamusParticleKind kind, Position16<RoomPixels> position, SamusParticleStart start)
    {
        Offset16<RoomPixels> drift = Definition(kind).Drift
            ?? throw new ArgumentOutOfRangeException(nameof(kind), kind, "A particle that drifts to one side starts with its drift.");
        Add(new SamusParticle(kind, AnimationPlace.StartOfAnimation, start, position, drift));
    }

    /// <summary>Starts a particle of lava spray on its first step, rising and drifting to one side as it shows.</summary>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    /// <param name="horizontalDrift">The pixels it drifts sideways on each update it shows: one right or one left.</param>
    internal void SpawnLavaSpray(Position16<RoomPixels> position, SamusParticleStart start, int horizontalDrift)
    {
        if (horizontalDrift is not (1 or -1))
            throw new ArgumentOutOfRangeException(nameof(horizontalDrift), horizontalDrift, "Lava spray drifts one pixel to one side.");
        Add(new SamusParticle(SamusParticleKind.LavaSpray, AnimationPlace.StartOfAnimation, start, position,
            new Offset16<RoomPixels>(horizontalDrift, LavaSprayRise)));
    }

    /// <summary>How particles of <paramref name="kind"/> move and are placed.</summary>
    private static SamusParticleKindDefinition Definition(SamusParticleKind kind) => SamusParticleKindDefinitions.Kinds[kind];

    /// <summary>Adds a particle already under way, as a restored game does.</summary>
    private void Add(SamusParticle particle) => _particles.Add(Ensure.NotNull(particle));

    /// <summary>
    /// Runs every particle's update and draws those shown, the newest first, so a newer particle draws in front where
    /// particles overlap. A particle that runs out is removed and not drawn.
    /// </summary>
    /// <param name="sprites">Frame receiving the particles shown.</param>
    /// <param name="camera">Room-space camera X and Y origin in whole pixels.</param>
    /// <param name="waterSurfaceLine">
    /// The water surface's room Y as a diving splash follows it on each update, which for a room without a water surface is
    /// the line <see cref="SuperMetroid.Core.Compatibility.AbsentWaterSurfaceLine"/> gives.
    /// </param>
    /// <param name="sheets">The installed sheets every shown particle draws from.</param>
    public void UpdateAndDraw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        int waterSurfaceLine,
        InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheets);
        for (int index = _particles.Count - 1; index >= 0; index--)
        {
            SamusParticle particle = _particles[index];
            particle.Advance();
            if (particle.IsFinished)
            {
                _particles.RemoveAt(index);
                continue;
            }
            if (particle.IsHidden)
                continue;
            if (Definition(particle.Kind).FollowsWaterSurface)
                particle.HoldOnLine(waterSurfaceLine);
            sprites.Add(Draw(particle, camera, sheets));
        }
    }

    /// <summary>
    /// The draw of a shown particle wherever it lies; the display clips it (#1541). Splashes, spray and dust are eight-pixel
    /// pictures centred on the particle; the diving splash and bubbles are placed from it.
    /// </summary>
    private static SamusColouredDraw Draw(SamusParticle particle, Position16<RoomPixels> camera, InstalledSpriteSheets sheets)
    {
        string frame = SamusAtmosphereSheetDefinitions.FrameName(
            particle.Shown ?? throw new InvalidOperationException("A particle drawn shows no picture."));
        Offset16<ScreenPixels> place = particle.Position.RelativeToCamera(camera);
        if (Definition(particle.Kind).Centred)
            place -= Offset16<ScreenPixels>.Of(SmallParticleHalfWidth, SmallParticleHalfWidth);
        SpriteSheetFrame picture = sheets.Get(SpriteSheetId.SamusAtmosphere).Frame(frame);
        return new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusAtmosphere), picture, place);
    }
}
