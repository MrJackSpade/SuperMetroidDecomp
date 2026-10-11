using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    private void HandleEquipmentInput(SnesButton pressed, byte nmiFrameCounter8)
    {
        PauseEquipmentCategory dispatchedCategory = selectedCategory;
        if (dispatchedCategory == PauseEquipmentCategory.Reserves)
        {
            HandleReserveInput(pressed);
            MoveEquipmentSelector(pressed);
            UpdateReserveArrow(nmiFrameCounter8);
            return;
        }
        MoveEquipmentSelector(pressed);
        if ((pressed & SnesButton.A) == 0)
            return;
        if (dispatchedCategory == PauseEquipmentCategory.Beams && samus.CollectedBeams == 0)
            return;
        if (selectedCategory == PauseEquipmentCategory.Reserves)
            throw new NotSupportedException("Same-frame equipment toggle into reserve tables requires native out-of-table WRAM behavior.");

        // Dispatch precedes movement. Boots->Plasma therefore retains the Boots copy
        // length and bypasses the Weapons handler's Spazer/Plasma exclusion. Do not use
        // upstream C's 'Fixed var bug' category guard: it is absent from retail ROM.
        ushort previousBeams = samus.EquippedBeams;
        var target = PauseEquipmentCategories.Get(selectedCategory);
        ushort mask = ReadCategoryMask(target, selectedItem);
        bool wasEquipped = (GetEquippedBits(selectedCategory) & mask) != 0;
        if (selectedCategory == PauseEquipmentCategory.Beams)
            samus.EquippedBeams ^= mask;
        else
            samus.EquippedItems ^= mask;

        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        UpdateEquipmentLabel(selectedCategory, selectedItem,
            PauseEquipmentCategories.Get(dispatchedCategory).LabelWordCount, wasEquipped);
        if (dispatchedCategory == PauseEquipmentCategory.Beams)
        {
            ushort added = (ushort)(samus.EquippedBeams & ~previousBeams);
            if ((added & (ushort)SamusBeamFlags.Spazer) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Plasma) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Plasma);
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
            else if ((added & (ushort)SamusBeamFlags.Plasma) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Spazer) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Spazer);
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.SpazerItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
        }
        // Native patches only the chosen label, then refreshes the wireframe. Rebuilding
        // all labels would erase the intentional overlong copy that displays VAR.
        WriteSamusWireframe();
        UploadEquipmentTilemap();
    }

    private void UpdateEquipmentLabel(PauseEquipmentCategory categoryIndex, int item, int wordCount, bool disabled)
    {
        var category = PauseEquipmentCategories.Get(categoryIndex);
        if (categoryIndex == PauseEquipmentCategory.Beams &&
            item == PauseEquipmentCategories.PlasmaItem && wordCount > category.LabelWordCount)
            plasmaLabelOverrunActive = true;
        (mapPresentation ?? throw new InvalidOperationException(
            "Equipment labels require installed presentation assets."))
            .PauseEquipmentLabels.ApplyLabel(
                equipmentTilemap, categoryIndex, item, wordCount, disabled);
    }

    private bool TrySelectEquipment(PauseEquipmentCategory categoryIndex, int start, int step)
    {
        if (categoryIndex == PauseEquipmentCategory.Beams && samus.HyperBeam != 0)
            return false;
        var category = PauseEquipmentCategories.Get(categoryIndex);
        for (int item = start; item >= 0 && item < category.ItemCount; item += step)
        {
            if ((GetCollectedBits(categoryIndex) & ReadCategoryMask(category, item)) == 0)
            {
                // $82:B4B7 checks byte offset ten only AFTER an unsuccessful probe.
                // Thus directly entering the final suit item can succeed, but scanning
                // forward from an earlier missing item must not discover it.
                if (categoryIndex == PauseEquipmentCategory.Suits && step > 0 &&
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
        selectedCategory = PauseEquipmentCategory.Reserves;
        selectedItem = 0;
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
        return true;
    }

    private void MoveEquipmentSelector(SnesButton pressed)
    {
        bool left = (pressed & SnesButton.Left) != 0, right = (pressed & SnesButton.Right) != 0;
        bool up = (pressed & SnesButton.Up) != 0, down = (pressed & SnesButton.Down) != 0;
        PauseEquipmentCategory beams = PauseEquipmentCategory.Beams, suits = PauseEquipmentCategory.Suits, boots = PauseEquipmentCategory.Boots;
        switch (selectedCategory)
        {
            case PauseEquipmentCategory.Beams:
                if (right)
                {
                    if (!TrySelectEquipment(suits, up ? 0 : PauseEquipmentCategories.BeamRightSuitItem, 1) && !up)
                        TrySelectEquipment(boots, 0, 1);
                }
                else if (down) TrySelectEquipment(beams, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(beams, selectedItem - 1, -1)) TrySelectReserves();
                break;
            case PauseEquipmentCategory.Suits:
                if (left)
                {
                    if (down || !TrySelectReserves()) TrySelectEquipment(beams, 0, 1);
                }
                else if (up) TrySelectEquipment(suits, selectedItem - 1, -1);
                else if (down && !TrySelectEquipment(suits, selectedItem + 1, 1)) TrySelectEquipment(boots, 0, 1);
                break;
            case PauseEquipmentCategory.Boots:
                if (left)
                {
                    if (up || !TrySelectEquipment(beams, PauseEquipmentCategories.PlasmaItem, -1)) TrySelectReserves();
                }
                else if (down) TrySelectEquipment(boots, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(boots, selectedItem - 1, -1))
                    TrySelectEquipment(suits, PauseEquipmentCategories.Get(suits).ItemCount - 1, -1);
                break;
            case PauseEquipmentCategory.Reserves:
                if (right)
                {
                    if (down || !TrySelectEquipment(suits, 0, 1)) TrySelectEquipment(boots, 0, 1);
                }
                else if (up)
                {
                    if (SelectedReserveItem != PauseReserveItem.Mode)
                    {
                        selectedItem--;
                        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                    }
                }
                else if (down)
                {
                    if (SelectedReserveItem == PauseReserveItem.Transfer ||
                        samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode)
                        TrySelectEquipment(beams, 0, 1);
                    else
                    {
                        selectedItem++;
                        if (samus.ReserveEnergy == 0) TrySelectEquipment(beams, 0, 1);
                        else audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                    }
                }
                break;
        }
    }
}
