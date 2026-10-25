using SuperMetroid.Core.Progression;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One bank-$8C post-credit cinematic BG object, including the item-percentage opcodes.
/// </summary>
/// <remarks>
/// The final messages are two-tile-high glyph rectangles, not an installed desktop font.
/// Interpreting their ROM records preserves localization, spacing, palette bits, and the
/// four-frame typewriter cadence while keeping the staging tilemap debugger-visible.
/// </remarks>
internal sealed class EndingBackgroundTextState
{
    /// <summary>Mutable 32-by-32 staging tilemap uploaded after each revealed glyph.</summary>
    private readonly ushort[] tilemap;
    /// <summary>Collected ammo and item counts used for the final item-percentage message.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Selects the Japanese subtitle row during the percentage hold.</summary>
    private readonly bool japaneseText;
    /// <summary>Remaining calls until the next installed character or hold transition.</summary>
    internal WrappingCountdown InstructionTimer { get; set; } = WrappingCountdown.Start(1);
    /// <summary>Scene-selected background map the staging tilemap is uploaded into.</summary>
    private readonly EndingTextTilemap destinationTilemap;
    /// <summary>The installed ending text presentation; it is rebound after state restoration.</summary>
    [field: NonSerialized]
    [AllowNull]
    private EndingTextPresentation Presentation { get => field ?? throw new InvalidOperationException("No ending text presentation is installed."); set; }
    /// <summary>Compiled installed typewriter characters, omitted from serialized debugger state.</summary>
    [NonSerialized] private EndingTextCharacter[]? installedProgram;
    /// <summary>The installed text sequence's characters.</summary>
    private EndingTextCharacter[] InstalledProgram => installedProgram ??
        throw new InvalidOperationException("No ending text sequence is installed.");
    /// <summary>Selected completion or item-percentage program that controls terminal behavior.</summary>
    private readonly EndingTextSequence? installedSequence;
    /// <summary>Next installed character to reveal in the staging tilemap.</summary>
    internal int InstalledCharacterIndex { get; set; }
    /// <summary>Preserves the native initial pause before the first glyph.</summary>
    internal bool InstalledInitialMarkerPending { get; set; } = true;
    /// <summary>Tracks whether the item-percentage display hold has begun.</summary>
    private bool installedHoldStarted;

    /// <summary>Binds installed typewriter art and an existing scene tilemap without advancing its pacing.</summary>
    public EndingBackgroundTextState(EndingBackgroundTextSetup setup)
    {
        this.tilemap = Ensure.NotNull(setup.Tilemap);
        Ensure.LengthEqual(setup.Tilemap, EndingScreenDefinitions.TilemapWords);
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        this.destinationTilemap = setup.DestinationTilemap;
        Presentation = setup.Presentation;
        this.installedSequence = setup.InstalledSequence;
        if (setup.Presentation is null || setup.InstalledSequence is null)
            throw new InvalidOperationException("Ending text requires installed presentation and sequence definitions.");
        installedProgram = setup.InstalledSequence is { } sequence
            ? setup.Presentation.Compile(sequence).ToArray()
            : null;
    }

    /// <summary>Set after the installed text and any percentage hold have finished.</summary>
    public bool Completed { get; private set; }
    /// <summary>Set when the item-percentage text clears and the scene may scroll onward.</summary>
    public bool RequestedItemPercentageScroll { get; private set; }

    /// <summary>Consumes one ending text tick and uploads the tilemap when a glyph or terminal change appears.</summary>
    public void Step(EndingScreen screen)
    {
        Ensure.NotNull(screen);
        if (installedProgram is null)
            throw new InvalidOperationException(
                "Ending text host content must be rebound after state restoration.");
        StepInstalled(screen);
    }

    /// <summary>Rebinds current host content after debugger-state restoration.</summary>
    public void BindPresentation(EndingTextPresentation? value)
    {
        Presentation = value;
        installedProgram = installedSequence is { } sequence && value is not null
            ? value.Compile(sequence).ToArray()
            : null;
        if (installedProgram is not null && InstalledCharacterIndex > installedProgram.Length)
            throw new InvalidDataException(
                $"Saved ending-text cursor {InstalledCharacterIndex} exceeds the rebound program.");
    }

    /// <summary>Runs the initial delay, four-frame character cadence, optional percentage hold, and completion handoff.</summary>
    private void StepInstalled(EndingScreen screen)
    {
        if (Completed || !(InstructionTimer = InstructionTimer.Decrement().Remaining).IsZero) return;
        if (InstalledInitialMarkerPending)
        {
            InstalledInitialMarkerPending = false;
            InstructionTimer = WrappingCountdown.Start(EndingTextDefinitions.InitialDelayFrames);
            return;
        }
        if (InstalledCharacterIndex < InstalledProgram.Length)
        {
            EndingTextCharacter character = InstalledProgram[InstalledCharacterIndex++];
            int target = character.Row * EndingTextDefinitions.TilemapWidth + character.Column;
            tilemap[target] = character.TopWord.Raw;
            if (character.BottomWord is { } bottom)
                tilemap[target + EndingTextDefinitions.TilemapWidth] = bottom.Raw;
            InstructionTimer = WrappingCountdown.Start(EndingTextDefinitions.CharacterDelayFrames);
            Upload(screen);
            return;
        }
        if (installedSequence == EndingTextSequence.ItemPercentage && !installedHoldStarted)
        {
            DrawItemPercentage();
            if (japaneseText)
                Presentation.JapaneseSubtitle.CopyTo(tilemap.AsSpan(
                    EndingResultTextLayout.JapaneseSubtitleFirstCell,
                    EndingResultTextLayout.JapaneseSubtitleWords));
            installedHoldStarted = true;
            InstructionTimer = WrappingCountdown.Start(EndingTextDefinitions.PercentageHoldFrames);
            Upload(screen);
            return;
        }
        if (installedSequence == EndingTextSequence.ItemPercentage)
        {
            Array.Fill(tilemap, EndingScreenDefinitions.BlankTile,
                EndingResultTextLayout.JapaneseSubtitleFirstCell,
                EndingResultTextLayout.JapaneseSubtitleWords);
            RequestedItemPercentageScroll = true;
        }
        Completed = true;
        Upload(screen);
    }

    /// <summary>Counts collected expansions and item bits, clamps to 100, and writes native two-row digits and percent mark.</summary>
    private void DrawItemPercentage()
    {
        int count = inventory.MaxHealth / 100
                  + inventory.MaxReserveEnergy / 100
                  + inventory.MaxMissiles / 5
                  + inventory.MaxSuperMissiles / 5
                  + inventory.MaxPowerBombs / 5;
        count += BitOperations.PopCount(
            (inventory.CollectedItems & EndingResultTextLayout.CollectibleItemMask).ToNativeWord());
        count += BitOperations.PopCount(
            (inventory.CollectedBeams & EndingResultTextLayout.CollectibleBeamMask).ToNativeWord());
        count = Math.Clamp(count, 0, 100);

        int hundreds = count / 100;
        int tens = count / 10 % 10;
        int units = count % 10;
        if (hundreds != 0)
            WriteDigit(EndingResultTextLayout.PercentageHundredsTopIndex, hundreds);
        if (tens != 0 || hundreds != 0)
            WriteDigit(EndingResultTextLayout.PercentageHundredsTopIndex + 1, tens);
        WriteDigit(EndingResultTextLayout.PercentageHundredsTopIndex + 2, units);
        tilemap[EndingResultTextLayout.PercentageHundredsTopIndex + 3] =
            EndingResultTextLayout.PercentTopTile;
        tilemap[
            EndingResultTextLayout.PercentageHundredsTopIndex + 3 +
            EndingScreenDefinitions.TilemapWidth] =
            EndingResultTextLayout.PercentBottomTile;
    }

    /// <summary>Writes one decimal digit's top and bottom native BG words.</summary>
    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(EndingResultTextLayout.DigitTopTile + digit));
        tilemap[topIndex + EndingScreenDefinitions.TilemapWidth] =
            unchecked((ushort)(EndingResultTextLayout.DigitBottomTile + digit));
    }

    /// <summary>Transfers the current staging tilemap to the selected background map.</summary>
    private void Upload(EndingScreen screen) =>
        screen.Record(EndingUploadKind.TextTilemap, (int)destinationTilemap, tilemap);

}
