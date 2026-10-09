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
    /// <summary>Address space supplying native text instruction words.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Mutable $400-word BG tilemap updated as text characters are revealed.</summary>
    private readonly ushort[] tilemap;
    /// <summary>Inventory values used to calculate the final collectible percentage.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Selects the Japanese subtitle path when the item percentage message is displayed.</summary>
    private readonly bool japaneseText;
    /// <summary>Current cursor in the native instruction stream.</summary>
    private ushort instructionPointer;
    /// <summary>Remaining frames before the next instruction or character is processed.</summary>
    private ushort instructionTimer = 1;
    /// <summary>Destination word for complete BG tilemap uploads.</summary>
    private readonly ushort tilemapDestination;
    /// <summary>Host presentation rebound after state restore to compile installed message text.</summary>
    [NonSerialized] private EndingTextPresentation? presentation;
    /// <summary>Compiled character program for the currently installed sequence.</summary>
    [NonSerialized] private EndingTextCharacter[]? installedProgram;
    /// <summary>Sequence identity determines whether item-percentage behavior follows the text.</summary>
    private readonly EndingTextSequence? installedSequence;
    /// <summary>Index of the next compiled character to install.</summary>
    private int installedCharacterIndex;
    /// <summary>Tracks whether the sequence's initial delay marker has been consumed.</summary>
    private bool installedInitialMarkerPending = true;
    /// <summary>Tracks the item-percentage hold between drawing the value and finishing the sequence.</summary>
    private bool installedHoldStarted;
    /// <summary>Whether all installed text and follow-up presentation work has completed.</summary>
    private bool installedCompleted;

    /// <summary>Creates a post-credit BG text state and compiles the selected installed sequence.</summary>
    /// <param name="bus">Address space used by native instruction-stream processing.</param>
    /// <param name="tilemap">Mutable $400-word BG tilemap receiving the text cells.</param>
    /// <param name="instructionPointer">Starting position in the text instruction stream.</param>
    /// <param name="inventory">Saved inventory values used by the item-percentage sequence.</param>
    /// <param name="japaneseText">Whether to draw the installed Japanese subtitle after the percentage.</param>
    /// <param name="tilemapDestination">VRAM word destination for tilemap uploads.</param>
    /// <param name="presentation">Compiled presentation assets and localized text.</param>
    /// <param name="installedSequence">Sequence whose compiled characters are advanced by this state.</param>
    /// <exception cref="ArgumentException">The supplied tilemap does not contain exactly $400 words.</exception>
    /// <exception cref="InvalidOperationException">Presentation assets or a sequence are missing.</exception>
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

    /// <summary>Whether the installed sequence and any percentage hold have finished.</summary>
    public bool Completed => installedCompleted;
    /// <summary>Whether completion requested the caller's item-percentage screen scroll.</summary>
    public bool RequestedItemPercentageScroll { get; private set; }

    /// <summary>Advances one frame of installed text and publishes tilemap writes when the sequence changes.</summary>
    /// <param name="vram">VRAM receiving the current complete text tilemap when an update requires publication.</param>
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

    /// <summary>Consumes the initial delay, reveals one character per cadence, then applies sequence-specific completion.</summary>
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

    /// <summary>Counts collectible upgrades and writes the clamped percentage and percent glyph to the tilemap.</summary>
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

    /// <summary>Writes one two-tile-high decimal digit at the supplied top-row tilemap index.</summary>
    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(EndingCreditsRomData.Text.DigitTopTile + digit));
        tilemap[topIndex + EndingCreditsRomData.Rendering.TilemapWidth] =
            unchecked((ushort)(EndingCreditsRomData.Text.DigitBottomTile + digit));
    }

    /// <summary>Transfers the complete tilemap to the configured VRAM word destination.</summary>
    private void Upload(SnesVram vram) =>
        vram.ExecuteWordTransfer(
            tilemap,
            tilemapDestination,
            wordIncrement: 1);

}

/// <summary>Inventory capacity and collectible bitfields captured for ending-text percentage calculation.</summary>
/// <param name="MaxHealth">Maximum energy capacity, including all acquired energy tanks.</param>
/// <param name="MaxReserveEnergy">Maximum reserve-energy capacity.</param>
/// <param name="MaxMissiles">Maximum missile capacity.</param>
/// <param name="MaxSuperMissiles">Maximum Super Missile capacity.</param>
/// <param name="MaxPowerBombs">Maximum Power Bomb capacity.</param>
/// <param name="CollectedItems">Collected item bitfield, filtered by the ending collectible mask.</param>
/// <param name="CollectedBeams">Collected beam bitfield, filtered by the ending collectible mask.</param>
internal readonly record struct EndingInventorySnapshot(
    ushort MaxHealth,
    ushort MaxReserveEnergy,
    ushort MaxMissiles,
    ushort MaxSuperMissiles,
    ushort MaxPowerBombs,
    ushort CollectedItems,
    ushort CollectedBeams);
