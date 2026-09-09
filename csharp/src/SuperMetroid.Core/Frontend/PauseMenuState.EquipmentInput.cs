using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    private void HandleEquipmentInput(SnesButton pressed)
    {
        int dispatchedCategory = selectedCategory;
        MoveEquipmentSelector(pressed);
        if (dispatchedCategory == PauseEquipmentCategories.Reserves || (pressed & SnesButton.A) == 0)
            return;
        if (dispatchedCategory == PauseEquipmentCategories.Beams && samus.CollectedBeams == 0)
            return;
        if (selectedCategory == PauseEquipmentCategories.Reserves)
            throw new NotSupportedException("Same-frame equipment toggle into reserve tables requires native out-of-table WRAM behavior.");

        // Dispatch precedes movement. Boots->Plasma therefore retains the Boots copy
        // length and bypasses the Weapons handler's Spazer/Plasma exclusion. Do not use
        // upstream C's 'Fixed var bug' category guard: it is absent from retail ROM.
        ushort previousBeams = samus.EquippedBeams;
        var target = PauseEquipmentCategories.Definitions[selectedCategory];
        ushort mask = ReadCategoryMask(target, selectedItem);
        bool wasEquipped = (GetEquippedBits(selectedCategory) & mask) != 0;
        if (selectedCategory == PauseEquipmentCategories.Beams)
            samus.EquippedBeams ^= mask;
        else
            samus.EquippedItems ^= mask;

        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        UpdateEquipmentLabel(selectedCategory, selectedItem,
            PauseEquipmentCategories.Definitions[dispatchedCategory].LabelWordCount, wasEquipped);
        if (dispatchedCategory == PauseEquipmentCategories.Beams)
        {
            ushort added = (ushort)(samus.EquippedBeams & ~previousBeams);
            if ((added & (ushort)SamusBeamFlags.Spazer) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Plasma) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Plasma);
                UpdateEquipmentLabel(PauseEquipmentCategories.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Definitions[PauseEquipmentCategories.Beams].LabelWordCount, true);
            }
            else if ((added & (ushort)SamusBeamFlags.Plasma) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Spazer) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Spazer);
                UpdateEquipmentLabel(PauseEquipmentCategories.Beams, PauseEquipmentCategories.SpazerItem,
                    PauseEquipmentCategories.Definitions[PauseEquipmentCategories.Beams].LabelWordCount, true);
            }
        }
        // Native patches only the chosen label, then refreshes the wireframe. Rebuilding
        // all labels would erase the intentional overlong copy that displays VAR.
        WriteSamusWireframe();
        UploadEquipmentTilemap();
    }

    private void UpdateEquipmentLabel(int categoryIndex, int item, int wordCount, bool disabled)
    {
        var category = PauseEquipmentCategories.Definitions[categoryIndex];
        int offset = RomDataReader.ReadWordFixedBank(bus, category.OffsetTableAddress + item * 2) -
            PauseEquipmentCategories.TilemapWramBase;
        Span<byte> label = equipmentTilemap.AsSpan(offset, wordCount * 2);
        if (disabled)
            RecolorLabel(label);
        else
            CopyBank82Words(RomDataReader.ReadWordFixedBank(bus, category.TilemapPointerTableAddress + item * 2), label);
    }

    private bool TrySelectEquipment(int categoryIndex, int start, int step)
    {
        if (categoryIndex == PauseEquipmentCategories.Beams && samus.HyperBeam != 0)
            return false;
        var category = PauseEquipmentCategories.Definitions[categoryIndex];
        for (int item = start; item >= 0 && item < category.ItemCount; item += step)
        {
            if ((GetCollectedBits(categoryIndex) & ReadCategoryMask(category, item)) == 0)
            {
                // $82:B4B7 checks byte offset ten only AFTER an unsuccessful probe.
                // Thus directly entering the final suit item can succeed, but scanning
                // forward from an earlier missing item must not discover it.
                if (categoryIndex == PauseEquipmentCategories.Suits && step > 0 &&
                    item + step >= category.ItemCount - 1)
                    return false;
                continue;
            }
            selectedCategory = categoryIndex;
            selectedItem = item;
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
            return true;
        }
        return false;
    }

    private bool TrySelectReserves()
    {
        if (samus.MaxReserveEnergy == 0) return false;
        selectedCategory = PauseEquipmentCategories.Reserves;
        selectedItem = 0;
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
        return true;
    }

    private void MoveEquipmentSelector(SnesButton pressed)
    {
        bool left = (pressed & SnesButton.Left) != 0, right = (pressed & SnesButton.Right) != 0;
        bool up = (pressed & SnesButton.Up) != 0, down = (pressed & SnesButton.Down) != 0;
        int beams = PauseEquipmentCategories.Beams, suits = PauseEquipmentCategories.Suits, boots = PauseEquipmentCategories.Boots;
        switch (selectedCategory)
        {
            case PauseEquipmentCategories.Beams:
                if (right)
                {
                    if (!TrySelectEquipment(suits, up ? 0 : PauseEquipmentCategories.BeamRightSuitItem, 1) && !up)
                        TrySelectEquipment(boots, 0, 1);
                }
                else if (down) TrySelectEquipment(beams, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(beams, selectedItem - 1, -1)) TrySelectReserves();
                break;
            case PauseEquipmentCategories.Suits:
                if (left)
                {
                    if (down || !TrySelectReserves()) TrySelectEquipment(beams, 0, 1);
                }
                else if (up) TrySelectEquipment(suits, selectedItem - 1, -1);
                else if (down && !TrySelectEquipment(suits, selectedItem + 1, 1)) TrySelectEquipment(boots, 0, 1);
                break;
            case PauseEquipmentCategories.Boots:
                if (left)
                {
                    if (up || !TrySelectEquipment(beams, PauseEquipmentCategories.PlasmaItem, -1)) TrySelectReserves();
                }
                else if (down) TrySelectEquipment(boots, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(boots, selectedItem - 1, -1))
                    TrySelectEquipment(suits, PauseEquipmentCategories.Definitions[suits].ItemCount - 1, -1);
                break;
            case PauseEquipmentCategories.Reserves:
                if (right)
                {
                    if (down || !TrySelectEquipment(suits, 0, 1)) TrySelectEquipment(boots, 0, 1);
                }
                else if (down) TrySelectEquipment(beams, 0, 1);
                break;
        }
    }
}
