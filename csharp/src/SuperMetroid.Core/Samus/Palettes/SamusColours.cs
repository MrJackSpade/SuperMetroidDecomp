using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Samus's colour state (#1405, #1512): the shade her suit shows, the shades laid over it (her visor's), the shade her
/// beams show, the shades laid over them (the Crystal Flash's body and bubble, the Hyper Beam's cycle) and the shade her
/// body without the suit shows. Her effects set it, her draws publish it, and it outlives each Samus as the player does.
/// The import pre-colours every one of her sheets in every shade; she never selects a colour herself.
/// </summary>
/// <remarks>
/// A later colouring of the suit or the beams replaces everything laid over them, as a complete load of their colours did;
/// a shade laid over them replaces an earlier shade of the same effect and covers the rest. Her colours are her own:
/// another owner's colours never show on her draws, and hers never show on theirs (#1512).
/// </remarks>
public sealed class SamusColours
{
    /// <summary>The shades laid over the suit's, in the order they were laid; the last covers the others.</summary>
    internal readonly List<PictureShade> suitOverlays = [];

    /// <summary>The shades laid over the beams', in the order they were laid; the last covers the others.</summary>
    internal readonly List<PictureShade> beamOverlays = [];

    /// <summary>The shading her draws show, until her colours change.</summary>
    private PictureShading? shading;

    /// <summary>The shade the suit shows.</summary>
    public PictureShade Suit { get; private set; } = SamusShades.StartingSuit;

    /// <summary>The shade the beams show.</summary>
    public PictureShade Beams { get; private set; } = SamusShades.StartingBeams;

    /// <summary>The shade her body without the suit shows, or null for its own colours.</summary>
    public PictureShade? Suitless { get; private set; }

    /// <summary>The shade her grapple flare shows, or null for its own colour until she first fires the grapple.</summary>
    public PictureShade? GrappleFlare { get; private set; }

    /// <summary>The shading her draws show now: the suit's and the beams' shades, what lies over each, and the suitless body's.</summary>
    public PictureShading Shading => shading ??= Compose();

    /// <summary>Shows <paramref name="shade"/> as the suit's colours, replacing whatever was laid over them.</summary>
    public void ShowSuit(PictureShade shade)
    {
        Suit = shade;
        suitOverlays.Clear();
        shading = null;
    }

    /// <summary>Lays <paramref name="shade"/> over the suit's colours, replacing an earlier shade of its effect.</summary>
    public void LayOverSuit(PictureShade shade) => LayOver(suitOverlays, shade);

    /// <summary>Shows <paramref name="shade"/> as the visor's colour, leaving the rest of the suit's alone.</summary>
    public void ShowVisor(PictureShade shade) => LayOverSuit(shade);

    /// <summary>Shows <paramref name="shade"/> as the beams' colours, replacing whatever was laid over them.</summary>
    public void ShowBeams(PictureShade shade)
    {
        Beams = shade;
        beamOverlays.Clear();
        shading = null;
    }

    /// <summary>Lays <paramref name="shade"/> over the beams' colours, replacing an earlier shade of its effect.</summary>
    public void LayOverBeams(PictureShade shade) => LayOver(beamOverlays, shade);

    /// <summary>Shows <paramref name="shade"/> as her suitless body's colours.</summary>
    public void ShowSuitless(PictureShade shade)
    {
        Suitless = shade;
        shading = null;
    }

    /// <summary>Shows <paramref name="shade"/> as her grapple flare's colour.</summary>
    public void ShowGrappleFlare(PictureShade shade)
    {
        GrappleFlare = shade;
        shading = null;
    }

    /// <summary>Shows the suit's and the beams' colours as gameplay starts, and her suitless body and flare in their own.</summary>
    public void ShowStarting()
    {
        ShowSuit(SamusShades.StartingSuit);
        ShowBeams(SamusShades.StartingBeams);
        Suitless = null;
        GrappleFlare = null;
    }

    /// <summary>
    /// <paramref name="draws"/> with Samus's own draws given the shading she shows them in; the others are left to their
    /// owners.
    /// </summary>
    public ImmutableArray<SpriteDraw> ShadeOwnDraws(ImmutableArray<SpriteDraw> draws)
    {
        var shaded = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (SpriteDraw draw in draws)
        {
            shaded.Add(draw is ImageSpriteDraw image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
                ? image with { Shading = Shading }
                : draw);
        }
        return shaded.MoveToImmutable();
    }

    /// <summary>Lays <paramref name="shade"/> last among <paramref name="overlays"/>, removing an earlier shade of its effect.</summary>
    private void LayOver(List<PictureShade> overlays, PictureShade shade)
    {
        overlays.RemoveAll(laid => laid.Effect == shade.Effect);
        overlays.Add(shade);
        shading = null;
    }

    /// <summary>
    /// The shading of the current state: the suit's shades, the grapple flare's, the beams' (whose death whiteout also
    /// covers the flare), and the suitless body's last.
    /// </summary>
    private PictureShading Compose()
    {
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>();
        shades.Add(Suit);
        shades.AddRange(suitOverlays);
        if (GrappleFlare is { } flare)
            shades.Add(flare);
        shades.Add(Beams);
        shades.AddRange(beamOverlays);
        if (Suitless is { } suitless)
            shades.Add(suitless);
        return new PictureShading(shades.ToImmutable(), []);
    }
}
