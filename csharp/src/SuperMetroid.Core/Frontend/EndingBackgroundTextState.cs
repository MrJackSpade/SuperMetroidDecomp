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
    /// <summary>Stores bus.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Stores tilemap.</summary>
    private readonly ushort[] tilemap;
    /// <summary>Stores inventory.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Stores japanese text.</summary>
    private readonly bool japaneseText;
    /// <summary>Stores instruction pointer.</summary>
    private ushort instructionPointer;
    /// <summary>Stores instruction timer.</summary>
    private ushort instructionTimer = 1;
    /// <summary>Stores tilemap destination.</summary>
    private readonly ushort tilemapDestination;
    /// <summary>Stores presentation.</summary>
    [NonSerialized] private EndingTextPresentation? presentation;
    /// <summary>Stores installed program.</summary>
    [NonSerialized] private EndingTextCharacter[]? installedProgram;
    /// <summary>Stores installed sequence.</summary>
    private readonly EndingTextSequence? installedSequence;
    /// <summary>Stores installed character index.</summary>
    private int installedCharacterIndex;
    /// <summary>Stores installed initial marker pending.</summary>
    private bool installedInitialMarkerPending = true;
    /// <summary>Stores installed hold started.</summary>
    private bool installedHoldStarted;

    /// <summary>Initializes a new ending background text state instance.</summary>
    public EndingBackgroundTextState(ISnesAddressSpace bus, EndingBackgroundTextSetup setup)
    {
        this.bus = Ensure.NotNull(bus);
        this.tilemap = Ensure.NotNull(setup.Tilemap);
        Ensure.LengthEqual(setup.Tilemap, EndingCreditsRomData.Rendering.TilemapWords);
        this.instructionPointer = setup.InstructionPointer;
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        this.tilemapDestination = setup.TilemapDestination;
        this.presentation = setup.Presentation;
        this.installedSequence = setup.InstalledSequence;
        if (setup.Presentation is null || setup.InstalledSequence is null)
            throw new InvalidOperationException("Ending text requires installed presentation and sequence definitions.");
        installedProgram = setup.InstalledSequence is { } sequence
            ? setup.Presentation!.Compile(sequence).ToArray()
            : null;
    }

    /// <summary>Gets or sets completed.</summary>
    public bool Completed { get; private set; }
    /// <summary>Gets or sets requested item percentage scroll.</summary>
    public bool RequestedItemPercentageScroll { get; private set; }

    /// <summary>Steps.</summary>
    public void Step(SnesVram vram)
    {
        Ensure.NotNull(vram);
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

    /// <summary>Steps installed.</summary>
    private void StepInstalled(SnesVram vram)
    {
        if (Completed || instructionTimer-- != 1) return;
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
        Completed = true;
        Upload(vram);
    }

    /// <summary>Draws item percentage.</summary>
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

    /// <summary>Writes digit.</summary>
    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(EndingCreditsRomData.Text.DigitTopTile + digit));
        tilemap[topIndex + EndingCreditsRomData.Rendering.TilemapWidth] =
            unchecked((ushort)(EndingCreditsRomData.Text.DigitBottomTile + digit));
    }

    /// <summary>Upload.</summary>
    [PpuLayoutBoundary("codec: ending text staging tilemap DMA to its scene-selected VMADD")]
    private void Upload(SnesVram vram) =>
        vram.ExecuteWordTransfer(
            tilemap,
            tilemapDestination,
            wordIncrement: 1);

}

/// <summary>Represents ending inventory snapshot.</summary>
internal readonly record struct EndingInventorySnapshot(
    ushort MaxHealth,
    ushort MaxReserveEnergy,
    ushort MaxMissiles,
    ushort MaxSuperMissiles,
    ushort MaxPowerBombs,
    ushort CollectedItems,
    ushort CollectedBeams);
