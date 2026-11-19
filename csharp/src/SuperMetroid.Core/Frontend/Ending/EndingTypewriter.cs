using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One post-credits typed text (#1555): the item-percentage labels with their percentage and subtitle, or the final
/// message. It types its installed characters with the four-frame typewriter cadence, its typing played by the shared
/// animation player (<see cref="EndingTypewriterAnimations"/>, #2488), and shows what it has typed so far as glyph pictures
/// at their screen places; nothing is written to a page.
/// </summary>
internal sealed class EndingTypewriter
{
    /// <summary>Collected ammo and item counts used for the item percentage.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Whether the Japanese subtitle shows during the percentage hold.</summary>
    private readonly bool japaneseText;
    /// <summary>The text this typewriter types.</summary>
    private readonly EndingTextSequence sequence;

    /// <summary>Binds the installed text of <paramref name="sequence"/> without advancing its pacing.</summary>
    public EndingTypewriter(EndingTypewriterSetup setup, EndingTextSequence sequence)
    {
        Ensure.NotNull(setup);
        inventory = setup.Inventory;
        japaneseText = setup.JapaneseText;
        this.sequence = sequence;
        Characters = Ensure.NotNull(setup.Text).Compile(sequence).ToArray();
        AnimationSegments<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue> segments =
            EndingTypewriterAnimations.Segments(sequence, Characters.Length);
        typing = AnimationPlayer<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue>.Starting(
            segments, EndingTypewriterAnimations.Typing(segments, sequence));
    }

    /// <summary>The text's typing: its opening pause, its characters, the percentage's hold and its completion.</summary>
    private readonly AnimationPlayer<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue> typing;
    /// <summary>The number of characters typed so far.</summary>
    private int TypedCount => typing.Shown?.Characters ?? 0;
    /// <summary>The item percentage shown from the start of the percentage hold, or null before it.</summary>
    private int? shownPercentage;

    /// <summary>Set after the text and any percentage hold have finished.</summary>
    public bool Completed { get; private set; }
    /// <summary>Whether the text is the item percentage's, whose completion lets the scene move the text down (#2562).</summary>
    internal bool TypesItemPercentage => sequence == EndingTextSequence.ItemPercentage;

    /// <summary>The installed characters.</summary>
    private EndingTextCharacter[] Characters { get; set; }

    /// <summary>Binds the installed text when the game is bound to its installed content.</summary>
    /// <exception cref="InvalidDataException">The typed count exceeds the rebound text.</exception>
    public void BindText(EndingTypewriterText value)
    {
        Characters = Ensure.NotNull(value).Compile(sequence).ToArray();
        if (TypedCount > Characters.Length)
            throw new InvalidDataException($"Saved ending-text typed count {TypedCount} exceeds the rebound text.");
        AnimationSegments<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue> rebound =
            EndingTypewriterAnimations.Segments(sequence, Characters.Length);
        typing.Rebind(rebound, EndingTypewriterAnimations.Typing(rebound, sequence));
    }

    /// <summary>Runs the typing one update: the initial delay, four-frame character cadence, optional percentage hold, and completion handoff.</summary>
    public void Step() => typing.Step(CarryOut);

    /// <summary>Carries out <paramref name="cue"/> of the text's typing.</summary>
    /// <exception cref="InvalidOperationException">The cue is not one a text's typing raises.</exception>
    private void CarryOut(EndingTypewriterCue cue)
    {
        switch (cue)
        {
            case EndingTypewriterCue.ShowPercentage:
                shownPercentage = ItemPercentage();
                return;
            case EndingTypewriterCue.Complete:
                Completed = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined ending typewriter cue {cue}.");
        }
    }

    /// <summary>
    /// What the text shows now, in the order it was typed: each typed letter's glyph (a space shows nothing), then from the
    /// percentage hold on the percentage's digits and percent sign, and during the hold the Japanese subtitle when it is
    /// selected; the subtitle goes when the hold ends.
    /// </summary>
    internal IEnumerable<EndingShownText> Shown()
    {
        EndingTextCharacter[] typed = Characters;
        for (int index = 0; index < TypedCount; index++)
        {
            EndingTextCharacter character = typed[index];
            if (character.Character != ' ')
                yield return new(EndingPictureNames.Glyph(character.Style, character.Character), EndingResultTextLayout.GlyphCorner(character.Column, character.Row));
        }
        if (shownPercentage is not { } count)
            yield break;
        int hundreds = count / 100;
        int tens = count / 10 % 10;
        int units = count % 10;
        if (hundreds != 0)
            yield return new(EndingPictureNames.PercentDigit(hundreds), PercentageCorner(0));
        if (tens != 0 || hundreds != 0)
            yield return new(EndingPictureNames.PercentDigit(tens), PercentageCorner(1));
        yield return new(EndingPictureNames.PercentDigit(units), PercentageCorner(2));
        yield return new(EndingPictureNames.PercentSign, PercentageCorner(3));
        if (japaneseText && !Completed)
            yield return new(EndingPictureNames.JapaneseSubtitle, EndingResultTextLayout.JapaneseSubtitleCorner);
    }

    /// <summary>Counts collected expansions and items, clamped to 100.</summary>
    private int ItemPercentage()
    {
        int count = inventory.MaxHealth / SamusEnergyDefinitions.EnergyPerTank
                  + inventory.MaxReserveEnergy / SamusEnergyDefinitions.EnergyPerTank
                  + inventory.MaxMissiles / CollectibleCapacityDefinitions.MissileTankMissiles
                  + inventory.MaxSuperMissiles / CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles
                  + inventory.MaxPowerBombs / CollectibleCapacityDefinitions.PowerBombTankPowerBombs;
        count += (inventory.CollectedItems & EndingResultTextLayout.CollectibleItemMask).Count();
        count += (inventory.CollectedBeams & EndingResultTextLayout.CollectibleBeamMask).Count();
        return Math.Clamp(count, 0, 100);
    }

    /// <summary>The corner of the percentage's glyph <paramref name="place"/> glyphs right of the hundreds digit.</summary>
    private static Position16<ScreenPixels> PercentageCorner(int place) => new(
        EndingResultTextLayout.PercentageHundredsCorner.X + place * EndingResultTextLayout.GlyphPixels,
        EndingResultTextLayout.PercentageHundredsCorner.Y);
}
