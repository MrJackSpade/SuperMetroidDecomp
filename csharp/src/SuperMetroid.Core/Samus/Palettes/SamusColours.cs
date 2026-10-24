using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Samus's colours (#1405): her suit's, which hold her visor's; and her beams', which the Crystal Flash and the Hyper Beam
/// also show. Her effects write them, her draws show them, and they outlive each Samus as the player does. Two of the
/// colours she shows are sprite colours others share: her body without the suit shows a whole sprite set, which her death
/// sequence loads and bosses load for their own sprites, and her grapple flare is the enemy projectiles' set's last colour.
/// </summary>
public sealed class SamusColours : IGameplayColourOwner, IInkRowColours
{
    /// <summary>The suit's colours.</summary>
    private readonly Bgr555[] suit = new Bgr555[SamusColourDefinitions.RowColours];

    /// <summary>The beams' colours.</summary>
    private readonly Bgr555[] beam = new Bgr555[SamusColourDefinitions.RowColours];

    /// <summary>For each ink row of Samus's sheets, the colours she holds in it, or null where she holds none.</summary>
    private readonly ColourRun?[] heldByInkRow;

    /// <summary>Lays out which colours each of her sheets' ink rows shows: hers, and the sprite colours she shares.</summary>
    /// <param name="spriteSets">Gameplay's sprite colour sets, which hold her suitless body's colours and her grapple flare's.</param>
    public SamusColours(SpriteColourSets spriteSets)
    {
        Ensure.NotNull(spriteSets);
        GrappleFlare = spriteSets.Set(SamusColourDefinitions.FlareSpriteSet).Slice(SamusColourDefinitions.FlareColour, 1);
        ColourRun suitless = spriteSets.Set(SamusColourDefinitions.SuitlessSpriteSet);
        heldByInkRow = new ColourRun?[SamusColourDefinitions.InkRows];
        heldByInkRow[SamusColourDefinitions.SuitInkRow] = Suit;
        heldByInkRow[SamusColourDefinitions.BeamInkRow] = Beam;
        heldByInkRow[SamusColourDefinitions.SuitlessInkRow] = suitless;
        heldByInkRow[SamusColourDefinitions.FlareInkRow] = GrappleFlare;
    }

    /// <summary>The suit's colours.</summary>
    public ColourRun Suit => new(suit, 0, suit.Length);

    /// <summary>The visor's colour, within the suit's.</summary>
    public ColourRun Visor => Suit.Slice(SamusColourDefinitions.VisorColour, 1);

    /// <summary>The beams' colours.</summary>
    public ColourRun Beam => new(beam, 0, beam.Length);

    /// <summary>The beam colours the Crystal Flash's body shows.</summary>
    public ColourRun CrystalFlashBody => Beam.Slice(0, SamusColourDefinitions.CrystalFlashBodyColours);

    /// <summary>The beam colours the Crystal Flash's bubble shows.</summary>
    public ColourRun CrystalFlashBubble =>
        Beam.Slice(SamusColourDefinitions.CrystalFlashBubbleFirstColour, SamusColourDefinitions.CrystalFlashBubbleColours);

    /// <summary>The beam colours the Hyper Beam's cycle shows.</summary>
    public ColourRun HyperBeam => Beam.Slice(SamusColourDefinitions.HyperBeamFirstColour, SamusColourDefinitions.HyperBeamColours);

    /// <summary>The grapple flare's colour: the enemy projectiles' sprite set's last colour, which she shares.</summary>
    public ColourRun GrappleFlare { get; }

    /// <inheritdoc />
    public IReadOnlyList<ColourRun> Stores => [Suit, Beam];

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
    public void Overlay(int inkRow, Span<Bgr555> colours)
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
    public Bgr555 At(int position)
    {
        Place place = Locate(position) ?? throw new ArgumentOutOfRangeException(nameof(position), position, "Samus holds no colour there.");
        return place.Colours[place.Index];
    }

    /// <inheritdoc />
    public void Set(int position, Bgr555 colour)
    {
        Place place = Locate(position) ?? throw new ArgumentOutOfRangeException(nameof(position), position, "Samus holds no colour there.");
        place.Colours[place.Index] = colour;
    }

    /// <summary>
    /// Where a colour addressed by its gameplay position lies among Samus's, for writers that still address colours by
    /// position: her suit's and her beams' rows.
    /// </summary>
    private Place? Locate(int position) => position switch
    {
        >= SamusPaletteDefinitions.Common.SamusObjPaletteStart and < SamusPaletteDefinitions.Common.SamusObjPaletteStart + SamusColourDefinitions.RowColours
            => new Place(suit, position - SamusPaletteDefinitions.Common.SamusObjPaletteStart),
        >= SamusProjectileRomData.Palettes.BeamFirstColour and < SamusProjectileRomData.Palettes.BeamFirstColour + SamusColourDefinitions.RowColours
            => new Place(beam, position - SamusProjectileRomData.Palettes.BeamFirstColour),
        _ => null,
    };

    /// <summary>One of Samus's colours: the colours it lies among and its index there.</summary>
    private readonly record struct Place(Bgr555[] Colours, int Index);
}
