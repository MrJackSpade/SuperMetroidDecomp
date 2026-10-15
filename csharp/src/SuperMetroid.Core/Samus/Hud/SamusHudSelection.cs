using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>One frame of HandleSwitchingHudSelection ($90:C4B5): HUD item cycling and availability.</summary>
internal sealed class SamusHudSelection
{
    /// <summary>Samus state whose selected item and native stabilization counter this handler updates.</summary>
    private readonly SamusState _samus;

    /// <summary>Binds HUD selection logic to the owning Samus state.</summary>
    internal SamusHudSelection(SamusState owner)
    {
        _samus = owner;
    }
    /// <summary>
    /// WRAM <c>$0AAA</c>: one on a selection change, then two after the selection remains
    /// stable for another frame. The arm-cannon cover animation uses this two-frame gate.
    /// </summary>
    public ushort HudItemChangedThisFrame { get; set; }

    /// <summary>
    /// Ports <c>HandleSwitchingHudSelection</c> at <c>$90:C4E7</c> for the retail default
    /// item-switch and item-cancel bindings.
    /// </summary>
    /// <remarks>
    /// The selected item is a six-entry index, not a bit field: zero is beams/nothing,
    /// one missiles, two Super Missiles, three Power Bombs, four Grapple, and five X-ray.
    /// A new Select edge advances through that ring, calling each cartridge handler to
    /// skip unavailable ammo/equipment. Y cancels to zero. Holding Y while pressing Select
    /// records the chosen item for native auto-cancel behavior.
    /// </remarks>
    /// <returns>Whether an item's switched-to handler ran.</returns>
    public HudSelectionOutcome HandleHudSelection(
        SnesButton controllerInput,
        SnesButton controllerNewInput,
        SnesButton itemSwitchButton = SnesButton.Select,
        SnesButton itemCancelButton = SnesButton.Y)
    {
        ushort previousSelection = _samus.SelectedHudItem;
        SnesButton switchMask = itemSwitchButton;
        SnesButton cancelMask = itemCancelButton;
        bool itemHandlerAccepted = (controllerNewInput & (cancelMask | switchMask)) != 0;

        if ((controllerNewInput & cancelMask) != 0)
        {
            _samus.AutoCancelHudItemIndex = 0;
            _samus.SelectedHudItem = 0;
        }
        else if ((controllerNewInput & switchMask) != 0)
        {
            // `$90:C51D-$C53A` wraps after X-ray and repeatedly invokes the selected
            // handler while it returns carry set. The loop always terminates because
            // entry zero is the unconditional Nothing handler.
            do
            {
                _samus.SelectedHudItem++;
                if (_samus.SelectedHudItem >= 6)
                    _samus.SelectedHudItem = 0;
            }
            while (!IsHudItemAvailable(_samus.SelectedHudItem));

            _samus.AutoCancelHudItemIndex = (controllerInput & cancelMask) != 0
                ? _samus.SelectedHudItem
                : (ushort)0;
        }

        if (_samus.SelectedHudItem != previousSelection)
        {
            HudItemChangedThisFrame = 1;
        }
        else
        {
            // Native saturates this word at two; it is a short stabilization counter,
            // never a general elapsed-time counter.
            HudItemChangedThisFrame = HudItemChangedThisFrame >= 2
                ? (ushort)2
                : unchecked((ushort)(HudItemChangedThisFrame + 1));
        }
        return new HudSelectionOutcome(itemHandlerAccepted);
    }

    /// <summary>Applies the native switched-to handlers' ammo and equipment admission rules.</summary>
    private bool IsHudItemAvailable(ushort index) => index switch
    {
        0 => true,
        1 => _samus.Missiles != 0,
        2 => _samus.SuperMissiles != 0,
        3 => _samus.PowerBombs != 0,
        4 => _samus.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        5 => _samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new InvalidDataException(
            $"HUD item index {index} escaped the cartridge's six-entry switch table."),
    };
}
