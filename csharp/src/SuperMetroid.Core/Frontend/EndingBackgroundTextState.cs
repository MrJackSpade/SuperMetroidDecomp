using System.Numerics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

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
    private readonly ISnesAddressSpace bus;
    private readonly ushort[] tilemap;
    private readonly EndingInventorySnapshot inventory;
    private readonly bool japaneseText;
    private ushort instructionPointer;
    private ushort instructionTimer = 1;
    private readonly ushort tilemapDestination;
    [NonSerialized] private EndingTextPresentation? presentation;
    [NonSerialized] private EndingTextCharacter[]? installedProgram;
    private readonly EndingTextSequence? installedSequence;
    private int installedCharacterIndex;
    private bool installedInitialMarkerPending = true;
    private bool installedHoldStarted;
    private bool installedCompleted;

    public EndingBackgroundTextState(
        ISnesAddressSpace bus,
        ushort[] tilemap,
        ushort instructionPointer,
        EndingInventorySnapshot inventory,
        bool japaneseText,
        ushort tilemapDestination = EndingCreditsRomData.Rendering.PostCreditsTilemapWord,
        EndingTextPresentation? presentation = null,
        EndingTextSequence? installedSequence = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
        if (tilemap.Length != EndingCreditsRomData.Rendering.TilemapWords)
            throw new ArgumentException("Ending BG tilemap must contain exactly $400 words.", nameof(tilemap));
        this.instructionPointer = instructionPointer;
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.tilemapDestination = tilemapDestination;
        this.presentation = presentation;
        this.installedSequence = installedSequence;
        if (presentation is null || installedSequence is null)
            throw new InvalidOperationException("Ending text requires installed presentation and sequence definitions.");
        installedProgram = installedSequence is { } sequence
            ? presentation!.Compile(sequence).ToArray()
            : null;
    }

    public bool Completed => installedCompleted;
    public bool RequestedItemPercentageScroll { get; private set; }

    public void Step(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (installedProgram is null)
            throw new InvalidOperationException(
                "Ending text host content must be rebound after state restoration.");
        StepInstalled(vram);
    }

    /// <summary>Rebinds current host content after debugger-state restoration.</summary>
    public void BindPresentation(EndingTextPresentation? value)
    {
        presentation = value;
        installedProgram = installedSequence is { } sequence && value is not null
            ? value.Compile(sequence).ToArray()
            : null;
        if (installedProgram is not null && installedCharacterIndex > installedProgram.Length)
            throw new InvalidDataException(
                $"Saved ending-text cursor {installedCharacterIndex} exceeds the rebound program.");
    }

    private void StepInstalled(SnesVram vram)
    {
        if (installedCompleted || instructionTimer-- != 1) return;
        if (installedInitialMarkerPending)
        {
            installedInitialMarkerPending = false;
            instructionTimer = EndingTextDefinitions.InitialDelayFrames;
            return;
        }
        if (installedCharacterIndex < installedProgram!.Length)
        {
            EndingTextCharacter character = installedProgram[installedCharacterIndex++];
            int target = character.Row * EndingTextDefinitions.TilemapWidth + character.Column;
            tilemap[target] = character.TopWord;
            if (character.BottomWord is { } bottom)
                tilemap[target + EndingTextDefinitions.TilemapWidth] = bottom;
            instructionTimer = EndingTextDefinitions.CharacterDelayFrames;
            Upload(vram);
            return;
        }
        if (installedSequence == EndingTextSequence.ItemPercentage && !installedHoldStarted)
        {
            DrawItemPercentage();
            if (japaneseText)
                presentation!.JapaneseSubtitle.CopyTo(tilemap.AsSpan(
                    EndingCreditsRomData.Text.JapaneseSubtitleDestination,
                    EndingCreditsRomData.Text.JapaneseSubtitleWords));
            installedHoldStarted = true;
            instructionTimer = EndingTextDefinitions.PercentageHoldFrames;
            Upload(vram);
            return;
        }
        if (installedSequence == EndingTextSequence.ItemPercentage)
        {
            Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Text.JapaneseSubtitleDestination,
                EndingCreditsRomData.Text.JapaneseSubtitleWords);
            RequestedItemPercentageScroll = true;
        }
        installedCompleted = true;
        Upload(vram);
    }

    private void DrawItemPercentage()
    {
        int count = inventory.MaxHealth / 100
                  + inventory.MaxReserveEnergy / 100
                  + inventory.MaxMissiles / 5
                  + inventory.MaxSuperMissiles / 5
                  + inventory.MaxPowerBombs / 5;
        count += BitOperations.PopCount((uint)(
            inventory.CollectedItems & EndingCreditsRomData.Text.CollectibleItemMask));
        count += BitOperations.PopCount((uint)(
            inventory.CollectedBeams & EndingCreditsRomData.Text.CollectibleBeamMask));
        count = Math.Clamp(count, 0, 100);

        int hundreds = count / 100;
        int tens = count / 10 % 10;
        int units = count % 10;
        if (hundreds != 0)
            WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex, hundreds);
        if (tens != 0 || hundreds != 0)
            WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex + 1, tens);
        WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex + 2, units);
        tilemap[EndingCreditsRomData.Text.PercentageHundredsTopIndex + 3] =
            EndingCreditsRomData.Text.PercentTopTile;
        tilemap[
            EndingCreditsRomData.Text.PercentageHundredsTopIndex + 3 +
            EndingCreditsRomData.Rendering.TilemapWidth] =
            EndingCreditsRomData.Text.PercentBottomTile;
    }

    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(EndingCreditsRomData.Text.DigitTopTile + digit));
        tilemap[topIndex + EndingCreditsRomData.Rendering.TilemapWidth] =
            unchecked((ushort)(EndingCreditsRomData.Text.DigitBottomTile + digit));
    }

    private void Upload(SnesVram vram) =>
        vram.ExecuteWordTransfer(
            tilemap,
            tilemapDestination,
            wordIncrement: 1);

}

internal readonly record struct EndingInventorySnapshot(
    ushort MaxHealth,
    ushort MaxReserveEnergy,
    ushort MaxMissiles,
    ushort MaxSuperMissiles,
    ushort MaxPowerBombs,
    ushort CollectedItems,
    ushort CollectedBeams);
