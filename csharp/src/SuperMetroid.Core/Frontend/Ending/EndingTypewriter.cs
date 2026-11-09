using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One post-credits typed text (#1555): the item-percentage labels with their percentage and subtitle, or the final
/// message. It types its installed characters with the four-frame typewriter cadence and shows what it has typed so far
/// as glyph pictures at their screen places; nothing is written to a page.
/// </summary>
internal sealed class EndingTypewriter
{
    /// <summary>Collected ammo and item counts used for the item percentage.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Whether the Japanese subtitle shows during the percentage hold.</summary>
    private readonly bool japaneseText;
    /// <summary>The text this typewriter types.</summary>
    private readonly EndingTextSequence sequence;
    /// <summary>The installed characters of <see cref="sequence"/>, or null while no ending text is bound.</summary>
    private EndingTextCharacter[]? characters;

    /// <summary>Binds the installed text of <paramref name="sequence"/> without advancing its pacing.</summary>
    public EndingTypewriter(EndingTypewriterSetup setup, EndingTextSequence sequence)
    {
        Ensure.NotNull(setup);
        inventory = setup.Inventory;
        japaneseText = setup.JapaneseText;
        this.sequence = sequence;
        characters = Ensure.NotNull(setup.Text).Compile(sequence).ToArray();
    }

    /// <summary>Remaining calls until the next character or hold transition.</summary>
    internal int FrameHold { get; set; } = 1;
    /// <summary>The number of characters typed so far.</summary>
    internal int TypedCount { get; set; }
    /// <summary>Whether the pause before the first character is still to begin.</summary>
    internal bool InitialPausePending { get; set; } = true;
    /// <summary>The item percentage shown from the start of the percentage hold, or null before it.</summary>
    private int? shownPercentage;

    /// <summary>Set after the text and any percentage hold have finished.</summary>
    public bool Completed { get; private set; }
    /// <summary>Set when the item-percentage text's hold ends and the scene may move the text down.</summary>
    public bool RequestedItemPercentageScroll { get; private set; }

    /// <summary>The installed characters.</summary>
    private EndingTextCharacter[] Characters => characters ??
        throw new InvalidOperationException("No ending text is bound.");

    /// <summary>Binds the installed text when the game is bound to its installed content.</summary>
    /// <exception cref="InvalidDataException">The typed count exceeds the rebound text.</exception>
    public void BindText(EndingTypewriterText? value)
    {
        characters = value?.Compile(sequence).ToArray();
        if (characters is not null && TypedCount > characters.Length)
            throw new InvalidDataException($"Saved ending-text typed count {TypedCount} exceeds the rebound text.");
    }

    /// <summary>Runs the initial delay, four-frame character cadence, optional percentage hold, and completion handoff.</summary>
    public void Step()
    {
        _ = Characters;
        if (Completed || --FrameHold != 0) return;
        if (InitialPausePending)
        {
            InitialPausePending = false;
            FrameHold = EndingTypewriterDefinitions.InitialDelayFrames;
            return;
        }
        if (TypedCount < Characters.Length)
        {
            TypedCount++;
            FrameHold = EndingTypewriterDefinitions.CharacterDelayFrames;
            return;
        }
        if (sequence == EndingTextSequence.ItemPercentage && shownPercentage is null)
        {
            shownPercentage = ItemPercentage();
            FrameHold = EndingTypewriterDefinitions.PercentageHoldFrames;
            return;
        }
        if (sequence == EndingTextSequence.ItemPercentage)
            RequestedItemPercentageScroll = true;
        Completed = true;
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
        int count = inventory.MaxHealth / 100
                  + inventory.MaxReserveEnergy / 100
                  + inventory.MaxMissiles / 5
                  + inventory.MaxSuperMissiles / 5
                  + inventory.MaxPowerBombs / 5;
        count += (inventory.CollectedItems & EndingResultTextLayout.CollectibleItemMask).Count();
        count += (inventory.CollectedBeams & EndingResultTextLayout.CollectibleBeamMask).Count();
        return Math.Clamp(count, 0, 100);
    }

    /// <summary>The corner of the percentage's glyph <paramref name="place"/> glyphs right of the hundreds digit.</summary>
    private static Position16<ScreenPixels> PercentageCorner(int place) => new(
        EndingResultTextLayout.PercentageHundredsCorner.X + place * EndingResultTextLayout.GlyphPixels,
        EndingResultTextLayout.PercentageHundredsCorner.Y);
}
