using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed owner of bank-$82's pause map/equipment menu.
/// </summary>
/// <remarks>
/// This object deliberately owns only the pause-menu-local WRAM/PPU image. The outer
/// <see cref="SuperMetroidGame"/> dispatcher retains native game states $0C-$12, while the
/// live <see cref="SamusState"/> remains the sole owner of inventory words. Consequently an
/// equipment-screen A press changes the same bits consumed by movement, rendering, and SRAM;
/// there is no host-only copy that can diverge from gameplay.
/// </remarks>
internal sealed class PauseMenuState
{
    private const ushort Bg1TilemapWord = 0x3000;
    private const ushort Bg2TilemapWord = 0x3800;
    private const ushort BlankEquipmentTilemapPointer = 0xc01a;
    private const ushort DisabledEquipmentPaletteBits = 0x0c00;
    private const ushort TilePaletteMask = 0x1c00;

    private static readonly EquipmentCategoryDefinition[] EquipmentCategories =
    [
        // Category zero is reserve tanks. Its two special controls do not exist on the
        // early-game route because maximum reserve energy is zero.
        new(0, 0, 0, 0, 0),
        new(0x82c06c, 0x82c08c, 0x82c04c, 5, 5), // Beams.
        new(0x82c076, 0x82c096, 0x82c056, 6, 9), // Suits/misc, including Morph and Bombs.
        new(0x82c082, 0x82c0a2, 0x82c062, 3, 9), // Boots.
    ];

    private readonly ISnesAddressSpace bus;
    private readonly SamusState samus;
    private readonly byte areaIndex;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly byte[] equipmentTilemap;
    private PauseMenuTransition transition;
    private int transitionBrightness = 15;
    private int selectedCategory;
    private int selectedItem;

    public PauseMenuState(ISnesAddressSpace bus, SamusState samus, byte areaIndex)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.samus = samus ?? throw new ArgumentNullException(nameof(samus));
        this.areaIndex = areaIndex < 7 ? areaIndex : (byte)0;

        // GameState_13 copies exactly these three cartridge ranges. VMADD is a word
        // address, hence the doubled byte destinations below.
        vram.LoadBytes(0x0000, RomDataReader.ReadFixedBank(bus, 0xb68000, 0x4000));
        vram.LoadBytes(0x4000, RomDataReader.ReadFixedBank(bus, 0xb6c000, 0x2000));
        vram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(bus, 0x9ab200, 0x2000));
        vram.LoadBytes(Bg2TilemapWord * 2, RomDataReader.ReadFixedBank(bus, 0xb6e000, 0x0800));
        cgram.LoadFromBus(bus, 0xb6f000);

        // $B6:E800 is the mutable equipment template normally copied to $7E:3800.
        // Preserve it as a byte array because the cartridge's offset tables contain WRAM
        // byte addresses rather than tilemap word indexes.
        equipmentTilemap = RomDataReader.ReadFixedBank(bus, 0xb6e800, 0x0800);
        RebuildEquipmentTilemap();
        LoadPauseMapTilemap();
        SelectFirstCollectedEquipment();
    }

    /// <summary>Zero for the map and one for the equipment page, matching WRAM $0753.</summary>
    public int ScreenMode { get; private set; }

    /// <summary>Low byte of the native category/item selector word.</summary>
    public int SelectedCategory => selectedCategory;

    /// <summary>High byte of the native category/item selector word.</summary>
    public int SelectedItem => selectedItem;

    /// <summary>
    /// Runs state-$0F menu input after the caller has latched NMI input and invoked the
    /// bank-$80 delayed-held filter. Returns true when Start requests game state $10.
    /// </summary>
    public bool Step(ushort delayedHeldInput, ushort newlyPressedInput)
    {
        SnesButton delayedPressed = (SnesButton)delayedHeldInput;
        SnesButton newlyPressed = (SnesButton)newlyPressedInput;
        if (transition != PauseMenuTransition.None)
        {
            StepPageTransition();
            return false;
        }

        // HandlePauseScreenStart runs from both stable pages. It is intentionally checked
        // after the delayed-held filter rather than from the raw NMI edge: a fresh press is
        // excluded for three frames by $80:8146 before becoming menu input.
        if ((delayedPressed & SnesButton.Start) != 0)
            return true;

        if (ScreenMode == 0)
        {
            if ((delayedPressed & SnesButton.R) != 0)
            {
                transition = PauseMenuTransition.MapToEquipmentFadeOut;
                transitionBrightness = 15;
            }
            return false;
        }

        if ((delayedPressed & SnesButton.L) != 0)
        {
            transition = PauseMenuTransition.EquipmentToMapFadeOut;
            transitionBrightness = 15;
            return false;
        }

        // EquipmentScreenMain consumes the ordinary $8F rising-edge word for D-pad/A;
        // only the shared L/R/Start chrome uses the delayed-held word at $05DF.
        HandleEquipmentInput(newlyPressed);
        return false;
    }

    /// <summary>Composes the cartridge's BG2 frame and current BG1 page at 256x224.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        Rgba32[] bg2 = SnesBgTilemapRenderer.Render4BppViewport(
            vram, cgram, Bg2TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
            vram, cgram, Bg1TilemapWord, 0, 0, 0, 256, 224, 64, 32);
        SnesLayerCompositor.Composite(output, bg2);
        SnesLayerCompositor.Composite(output, bg1);

        // Menu subindexes 2/4/5/7 fade the current page during an L/R transition. The
        // outer game-state fades are applied by SuperMetroidGame because they also cover
        // the gameplay frame before setup and after restoration.
        if (transition != PauseMenuTransition.None)
            MasterBrightnessFilter.Apply(output, transitionBrightness);
        return output;
    }

    private void StepPageTransition()
    {
        switch (transition)
        {
            case PauseMenuTransition.MapToEquipmentFadeOut:
            case PauseMenuTransition.EquipmentToMapFadeOut:
                transitionBrightness--;
                if (transitionBrightness > 0)
                    return;

                if (transition == PauseMenuTransition.MapToEquipmentFadeOut)
                {
                    ScreenMode = 1;
                    UploadEquipmentTilemap();
                    transition = PauseMenuTransition.MapToEquipmentFadeIn;
                }
                else
                {
                    ScreenMode = 0;
                    LoadPauseMapTilemap();
                    transition = PauseMenuTransition.EquipmentToMapFadeIn;
                }
                transitionBrightness = 0;
                return;

            case PauseMenuTransition.MapToEquipmentFadeIn:
            case PauseMenuTransition.EquipmentToMapFadeIn:
                transitionBrightness++;
                if (transitionBrightness >= 15)
                {
                    transitionBrightness = 15;
                    transition = PauseMenuTransition.None;
                }
                return;

            default:
                throw new InvalidDataException($"Invalid pause-page transition {transition}.");
        }
    }

    private void HandleEquipmentInput(SnesButton pressed)
    {
        if (selectedCategory is < 1 or > 3)
            return;

        EquipmentCategoryDefinition category = EquipmentCategories[selectedCategory];
        ushort collected = GetCollectedBits(selectedCategory);

        if ((pressed & SnesButton.Up) != 0)
        {
            for (int item = selectedItem - 1; item >= 0; item--)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedItem = item;
                return;
            }
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            for (int item = selectedItem + 1; item < category.ItemCount; item++)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedItem = item;
                return;
            }
        }
        else if ((pressed & SnesButton.A) != 0)
        {
            ushort mask = ReadCategoryMask(category, selectedItem);
            if ((collected & mask) == 0)
                return;

            // EquipmentScreenCategory_ButtonResponse toggles the live equipped word, then
            // recolors exactly this label. Rebuilding all labels is equivalent and avoids
            // retaining a second authoritative equipment state in the pause object.
            if (selectedCategory == 1)
                samus.EquippedBeams ^= mask;
            else
                samus.EquippedItems ^= mask;
            RebuildEquipmentTilemap();
            UploadEquipmentTilemap();
        }
    }

    private void SelectFirstCollectedEquipment()
    {
        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            EquipmentCategoryDefinition category = EquipmentCategories[categoryIndex];
            ushort collected = GetCollectedBits(categoryIndex);
            for (int item = 0; item < category.ItemCount; item++)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedCategory = categoryIndex;
                selectedItem = item;
                return;
            }
        }

        selectedCategory = 0;
        selectedItem = 0;
    }

    private ushort GetCollectedBits(int category) =>
        category == 1 ? samus.CollectedBeams : samus.CollectedItems;

    private ushort GetEquippedBits(int category) =>
        category == 1 ? samus.EquippedBeams : samus.EquippedItems;

    private void RebuildEquipmentTilemap()
    {
        // Restore the literal base before applying inventory-dependent labels. This makes
        // repeated A toggles idempotent and mirrors re-entering LoadEquipmentScreen...
        RomDataReader.ReadFixedBank(bus, 0xb6e800, equipmentTilemap.Length)
            .CopyTo(equipmentTilemap, 0);

        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            EquipmentCategoryDefinition category = EquipmentCategories[categoryIndex];
            ushort collected = GetCollectedBits(categoryIndex);
            ushort equipped = GetEquippedBits(categoryIndex);
            for (int item = 0; item < category.ItemCount; item++)
            {
                ushort destination = RomDataReader.ReadWordFixedBank(
                    bus,
                    category.OffsetTableAddress + item * 2);
                int destinationOffset = destination - 0x3800;
                int byteCount = category.LabelWordCount * 2;
                if (destinationOffset < 0 || destinationOffset + byteCount > equipmentTilemap.Length)
                {
                    throw new InvalidDataException(
                        $"Equipment label destination ${destination:X4} escapes $7E:3800-$3FFF.");
                }

                ushort mask = ReadCategoryMask(category, item);
                ushort source = (collected & mask) != 0
                    ? RomDataReader.ReadWordFixedBank(bus, category.TilemapPointerTableAddress + item * 2)
                    : BlankEquipmentTilemapPointer;
                CopyBank82Words(source, equipmentTilemap.AsSpan(destinationOffset, byteCount));
                if ((collected & mask) != 0 && (equipped & mask) == 0)
                    RecolorLabel(equipmentTilemap.AsSpan(destinationOffset, byteCount));
            }
        }

        WriteSamusWireframe();
    }

    private void WriteSamusWireframe()
    {
        ushort desired = (ushort)(samus.EquippedItems & 0x0101);
        ushort sourcePointer = 0;
        for (int index = 0; index < 4; index++)
        {
            if (RomDataReader.ReadWordFixedBank(bus, 0x82b257 + index * 2) != desired)
                continue;
            sourcePointer = RomDataReader.ReadWordFixedBank(bus, 0x82b25f + index * 2);
            break;
        }
        if (sourcePointer == 0)
            throw new InvalidDataException($"Pause wireframe table has no entry for items ${desired:X4}.");

        int sourceAddress = 0x820000 | sourcePointer;
        int destinationOffset = 472;
        for (int row = 0; row < 17; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
                equipmentTilemap[destinationOffset + column * 2] = unchecked((byte)word);
                equipmentTilemap[destinationOffset + column * 2 + 1] = unchecked((byte)(word >> 8));
                sourceAddress = 0x820000 | ((sourceAddress + 2) & 0xffff);
            }
            destinationOffset += 64;
        }
    }

    private void LoadPauseMapTilemap()
    {
        // kPauseMenuMapTilemaps is a table of long pointers to literal 64x32 tilemaps.
        // `$82:943D` dereferences words from that ROM image while applying explored-map
        // visibility; it does not call the decompressor. Treating $B5:9000 as compressed
        // therefore walks unrelated data looking for an impossible command terminator.
        int mapPointer = RomDataReader.ReadLongFixedBank(bus, 0x82964a + areaIndex * 3);
        vram.LoadBytes(
            Bg1TilemapWord * 2,
            RomDataReader.ReadFixedBank(bus, mapPointer, 0x1000));

        // The area name is a 24-byte bank-$82 tilemap fragment copied to VMADD $38AA.
        ushort labelPointer = RomDataReader.ReadWordFixedBank(bus, 0x82965f + areaIndex * 2);
        vram.LoadBytes(0x38aa * 2, RomDataReader.ReadFixedBank(bus, 0x820000 | labelPointer, 0x18));
    }

    private void UploadEquipmentTilemap() =>
        vram.LoadBytes(Bg1TilemapWord * 2, equipmentTilemap);

    private ushort ReadCategoryMask(EquipmentCategoryDefinition category, int item) =>
        RomDataReader.ReadWordFixedBank(bus, category.BitmaskTableAddress + item * 2);

    private void CopyBank82Words(ushort sourcePointer, Span<byte> destination)
    {
        int source = 0x820000 | sourcePointer;
        for (int index = 0; index < destination.Length; index++)
            destination[index] = bus.ReadByte(0x820000 | ((source + index) & 0xffff));
    }

    private static void RecolorLabel(Span<byte> bytes)
    {
        for (int offset = 0; offset < bytes.Length; offset += 2)
        {
            ushort word = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            word = (ushort)((word & ~TilePaletteMask) | DisabledEquipmentPaletteBits);
            bytes[offset] = unchecked((byte)word);
            bytes[offset + 1] = unchecked((byte)(word >> 8));
        }
    }

    private readonly record struct EquipmentCategoryDefinition(
        int OffsetTableAddress,
        int TilemapPointerTableAddress,
        int BitmaskTableAddress,
        int ItemCount,
        int LabelWordCount);
}

internal enum PauseMenuTransition
{
    None,
    MapToEquipmentFadeOut,
    MapToEquipmentFadeIn,
    EquipmentToMapFadeOut,
    EquipmentToMapFadeIn,
}
