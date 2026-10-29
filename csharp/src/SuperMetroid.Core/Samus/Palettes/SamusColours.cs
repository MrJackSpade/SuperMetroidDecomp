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
/// bubble, the Hyper Beam's cycle, her suitless body's and the grapple flare. Which of her sheets' ink rows show them,
/// and so which sprite sets show her colours over their own, is this owner's alone (#1464).
/// </remarks>
public sealed class SamusColours : IGameplayColourOwner
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
        GrappleFlare = SamusColourDefinitions.FlareStore.In(spriteSets).Slice(SamusColourDefinitions.FlareColour, 1);
        Suitless = SamusColourDefinitions.SuitlessStore.In(spriteSets);
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

    /// <summary>The sprite colour stores her body shows: the one her suit shows over, and the one she shows without it.</summary>
    public static IReadOnlyList<SpriteColourStore> BodyStores { get; } =
        [SamusColourDefinitions.SuitStore, SamusColourDefinitions.SuitlessStore];

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
    public void Overlay(ImageColourGroup group, Span<RgbColor> colours)
    {
        // A held run fills its group from the group's start, or, for the flare's one colour, ends at the group's end; her
        // draws show their images' own colours everywhere else.
        if (heldByInkRow[group.Number] is not { } held)
            return;
        int first = held.Count == colours.Length ? 0 : colours.Length - held.Count;
        held.Colors.CopyTo(colours[first..]);
    }

    /// <inheritdoc />
    public void LoadStarting(GameplayStartingColours starting)
    {
        Ensure.NotNull(starting);
        Suit.CopyFrom(starting.SamusSuit);
        Beam.CopyFrom(starting.SamusBeams);
    }
}
