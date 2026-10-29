using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Samus's colours (#1405): her suit's, which hold her visor's; and her beams', which the Crystal Flash and the Hyper Beam
/// also show. Her effects write them, her draws show them, and they outlive each Samus as the player does. Two of the
/// colours she shows are sprite colours others share: her body without the suit shows a whole sprite set, which her death
/// sequence loads and bosses load for their own sprites, and her grapple flare is the enemy projectiles' set's last colour.
/// </summary>
/// <remarks>
/// Effects address her colours by what they are for: the suit's, the visor, the beams', the Crystal Flash's body and
/// bubble, the Hyper Beam's cycle, her suitless body's and the grapple flare. Where those lie among gameplay's colour
/// positions and her sheets' ink rows is this owner's alone, until owner-keyed colour sets replace positions (#1464).
/// </remarks>
public sealed class SamusColours : IGameplayColourOwner, IInkRowColours
{
    /// <summary>The suit's colours.</summary>
    private readonly RgbColor[] suit = new RgbColor[SamusColourDefinitions.RowColours];

    /// <summary>The beams' colours.</summary>
    private readonly RgbColor[] beam = new RgbColor[SamusColourDefinitions.RowColours];

    /// <summary>For each ink row of Samus's sheets, the colours she holds in it, or null where she holds none.</summary>
    private readonly ColourRun?[] heldByInkRow;

    /// <summary>Lays out which colours each of her sheets' ink rows shows: hers, and the sprite colours she shares.</summary>
    /// <param name="spriteSets">Gameplay's sprite colour sets, which hold her suitless body's colours and her grapple flare's.</param>
    public SamusColours(SpriteColourSets spriteSets)
    {
        Ensure.NotNull(spriteSets);
        GrappleFlare = spriteSets.Set(SamusColourDefinitions.FlareSpriteSet).Slice(SamusColourDefinitions.FlareColour, 1);
        Suitless = spriteSets.Set(SamusColourDefinitions.SuitlessSpriteSet);
        heldByInkRow = new ColourRun?[SamusColourDefinitions.InkRows];
        heldByInkRow[SamusColourDefinitions.SuitInkRow] = Suit;
        heldByInkRow[SamusColourDefinitions.BeamInkRow] = Beam;
        heldByInkRow[SamusColourDefinitions.SuitlessInkRow] = Suitless;
        heldByInkRow[SamusColourDefinitions.FlareInkRow] = GrappleFlare;
    }

    /// <summary>The suit's colours.</summary>
    public ColourRun Suit => new(suit, 0, suit.Length);

    /// <summary>The beams' colours.</summary>
    public ColourRun Beam => new(beam, 0, beam.Length);

    /// <summary>The beam colours the Crystal Flash's body shows.</summary>
    public ColourRun CrystalFlashBody => Beam.Slice(0, SamusColourDefinitions.CrystalFlashBodyColours);

    /// <summary>The beam colours the Crystal Flash's bubble shows.</summary>
    public ColourRun CrystalFlashBubble =>
        Beam.Slice(SamusColourDefinitions.CrystalFlashBubbleFirstColour, SamusColourDefinitions.CrystalFlashBubbleColours);

    /// <summary>The beam colours the Hyper Beam's cycle shows.</summary>
    public ColourRun HyperBeam => Beam.Slice(SamusColourDefinitions.HyperBeamFirstColour, SamusColourDefinitions.HyperBeamColours);

    /// <summary>
    /// Her suitless body's colours: a whole sprite set she shares, which her death sequence loads and bosses load for
    /// their own sprites.
    /// </summary>
    public ColourRun Suitless { get; }

    /// <summary>The grapple flare's colour: the enemy projectiles' sprite set's last colour, which she shares.</summary>
    public ColourRun GrappleFlare { get; }

    /// <inheritdoc />
    public IReadOnlyList<ColourRun> Stores => [Suit, Beam];

    /// <summary>Shows <paramref name="colour"/> as the visor's colour, leaving the rest of the suit's alone.</summary>
    public void ShowVisor(RgbColor colour) => Suit.Write(SamusColourDefinitions.VisorColour, colour);

    /// <summary>
    /// <paramref name="draws"/> with Samus's own draws given the colours she shows them in; the others are left to be
    /// resolved when the frame is published.
    /// </summary>
    public ImmutableArray<SpriteDraw> ColourOwnDraws(ImmutableArray<SpriteDraw> draws)
    {
        var coloured = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (SpriteDraw draw in draws)
        {
            coloured.Add(draw is ImageSpriteDraw image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
                ? image with { Colours = DrawColours.Owned(image, this) }
                : draw);
        }
        return coloured.MoveToImmutable();
    }

    /// <inheritdoc />
    public void Overlay(int inkRow, Span<RgbColor> colours)
    {
        // A held run fills its row from the row's start, or, for the flare's one colour, ends at the row's end; her draws
        // show their images' own colours everywhere else.
        if (heldByInkRow[Ensure.BetweenInclusive(0, SamusColourDefinitions.InkRows - 1, inkRow)] is not { } held)
            return;
        int first = held.Count == colours.Length ? 0 : colours.Length - held.Count;
        held.Colors.CopyTo(colours[first..]);
    }

    /// <inheritdoc />
    public bool Holds(int position) => Locate(position) is not null;

    /// <inheritdoc />
    public RgbColor At(int position)
    {
        Place place = Locate(position) ?? throw new ArgumentOutOfRangeException(nameof(position), position, "Samus holds no colour there.");
        return place.Colours[place.Index];
    }

    /// <inheritdoc />
    public void Set(int position, RgbColor colour)
    {
        Place place = Locate(position) ?? throw new ArgumentOutOfRangeException(nameof(position), position, "Samus holds no colour there.");
        place.Colours[place.Index] = colour;
    }

    /// <summary>
    /// Where a colour addressed by its gameplay position lies among Samus's, for the colour router, which still
    /// addresses colours by position: her suit's and her beams' rows.
    /// </summary>
    private Place? Locate(int position) => position switch
    {
        >= SamusColourDefinitions.SuitFirstPosition and < SamusColourDefinitions.SuitFirstPosition + SamusColourDefinitions.RowColours
            => new Place(suit, position - SamusColourDefinitions.SuitFirstPosition),
        >= SamusColourDefinitions.BeamFirstPosition and < SamusColourDefinitions.BeamFirstPosition + SamusColourDefinitions.RowColours
            => new Place(beam, position - SamusColourDefinitions.BeamFirstPosition),
        _ => null,
    };

    /// <summary>One of Samus's colours: the colours it lies among and its index there.</summary>
    private readonly record struct Place(RgbColor[] Colours, int Index);
}
