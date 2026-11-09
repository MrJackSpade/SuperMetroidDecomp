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

    /// <summary>The particles shown or waiting to show, oldest first.</summary>
    public IReadOnlyList<SamusParticle> All => _particles;

    /// <summary>Removes every particle, as room and Samus initialization do.</summary>
    public void Clear() => _particles.Clear();

    /// <summary>Starts a particle on its first step; dust rises as it shows, and every other kind but lava spray stays put.</summary>
    /// <param name="kind">What it shows; lava spray starts through <see cref="SpawnLavaSpray"/>, which gives its drift.</param>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    internal void Spawn(SamusParticleKind kind, Position16<RoomPixels> position, SamusParticleStart start)
    {
        if (kind == SamusParticleKind.LavaSpray)
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Lava spray drifts outward and starts with its drift.");
        Add(new SamusParticle(kind, step: 0, start, position, kind == SamusParticleKind.Dust ? DustDrift : Offset16<RoomPixels>.Zero));
    }

    /// <summary>Starts a particle of lava spray on its first step, rising and drifting to one side as it shows.</summary>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    /// <param name="horizontalDrift">The pixels it drifts sideways on each update it shows: one right or one left.</param>
    internal void SpawnLavaSpray(Position16<RoomPixels> position, SamusParticleStart start, int horizontalDrift)
    {
        if (horizontalDrift is not (1 or -1))
            throw new ArgumentOutOfRangeException(nameof(horizontalDrift), horizontalDrift, "Lava spray drifts one pixel to one side.");
        Add(new SamusParticle(SamusParticleKind.LavaSpray, step: 0, start, position,
            new Offset16<RoomPixels>(horizontalDrift, LavaSprayRise)));
    }

    /// <summary>Adds a particle already under way, as a restored game does.</summary>
    internal void Add(SamusParticle particle) => _particles.Add(Ensure.NotNull(particle));

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
    /// <param name="sheets">The installed sheets every shown particle draws from; required when one is shown.</param>
    public void UpdateAndDraw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        int waterSurfaceLine,
        InstalledSpriteSheets? sheets = null)
    {
        Ensure.NotNull(sprites);
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
            if (particle.Kind == SamusParticleKind.DivingSplash)
                particle.HoldOnLine(waterSurfaceLine);
            sprites.Add(Draw(particle, index, camera, sheets));
        }
    }

    /// <summary>
    /// The draw of a shown particle wherever it lies; the display clips it (#1541). Splashes, spray and dust are eight-pixel
    /// pictures centred on the particle; the diving splash and bubbles are placed from it.
    /// </summary>
    private static ImageSpriteDraw Draw(SamusParticle particle, int index, Position16<RoomPixels> camera, InstalledSpriteSheets? sheets)
    {
        int step = particle.AnimationStep;
        string frame = particle.Kind switch
        {
            SamusParticleKind.SurfaceSplash => SamusAtmosphereSheetDefinitions.FootstepFrame(step),
            SamusParticleKind.LavaSpray or SamusParticleKind.Dust => SamusAtmosphereSheetDefinitions.LavaAndDustFrame(step),
            SamusParticleKind.DivingSplash => SamusAtmosphereSheetDefinitions.DivingSplashFrame(step),
            SamusParticleKind.Bubbles => SamusAtmosphereSheetDefinitions.BubblesFrame(step),
            _ => throw new InvalidDataException($"Undefined particle kind {particle.Kind}."),
        };
        int screenX = particle.Position.X - camera.X;
        int screenY = particle.Position.Y - camera.Y;
        if (particle.Kind is SamusParticleKind.SurfaceSplash or SamusParticleKind.LavaSpray or SamusParticleKind.Dust)
        {
            screenX = CoordinateAxis.Difference(screenX, SmallParticleHalfWidth);
            screenY = CoordinateAxis.Difference(screenY, SmallParticleHalfWidth);
        }
        SpriteSheetFrame picture = (sheets ?? throw new InvalidOperationException(
            "Samus's particles require the installed sprite sheets.")).Get(SpriteSheetId.SamusAtmosphere).Frame(frame);
        return new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusAtmosphere, index), picture,
            Position16<ScreenPixels>.Of(CoordinateAxis.Wrap(screenX), CoordinateAxis.Wrap(screenY)), SpriteMirror.None);
    }
}
