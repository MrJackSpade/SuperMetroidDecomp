namespace SuperMetroid.Core.Game;

/// <summary>Native SRAM directory and per-slot payload layout used by bank <c>$81</c>.</summary>
/// <remarks>
/// Values in this catalog are byte offsets unless a summary explicitly calls out a CPU-bus
/// address. The serializer owns behavior; this type owns only the immutable cartridge data
/// structure so layout review does not require reading save/load control flow.
/// </remarks>
public static class SaveRamLayout
{
    /// <summary>The number of independently checksummed save payloads in the retail SRAM image.</summary>
    public const int SlotCount = 3;

    /// <summary>The byte length of one payload copied between SRAM and the WRAM mirror at <c>$7E:D7C0</c>.</summary>
    public const int SlotByteCount = 0x065c;

    /// <summary>SRAM offset of the three primary 16-bit slot checksums.</summary>
    public const int PrimaryChecksumOffset = 0x0000;

    /// <summary>SRAM offset of the three primary one's-complement checksum words.</summary>
    public const int PrimaryComplementOffset = 0x0008;

    /// <summary>SRAM offset of the selected-slot word; its complement immediately follows at <c>$1FEE</c>.</summary>
    public const int SelectedSlotOffset = 0x1fec;

    /// <summary>SRAM offset of the redundant three-word checksum directory near the end of the 8-KiB image.</summary>
    public const int BackupChecksumOffset = 0x1ff0;

    /// <summary>SRAM offset of the redundant three-word checksum-complement directory.</summary>
    public const int BackupComplementOffset = 0x1ff8;

    /// <summary>The SNES CPU-bus bank through which the cartridge exposes battery-backed SRAM.</summary>
    public const byte SramBank = 0x70;

    /// <summary>Mask that wraps logical SRAM accesses into the cartridge's 8-KiB <c>$70:0000-$70:1FFF</c> window.</summary>
    public const ushort SramOffsetMask = 0x1fff;

    /// <summary>Slot-relative offset of the 16-bit item-equipment mask mirrored from WRAM <c>$7E:D7C0</c>.</summary>
    public const int EquippedItemsOffset = 0x0000;

    /// <summary>Slot-relative offset of the 16-bit collected-item mask mirrored from WRAM <c>$7E:D7C2</c>.</summary>
    public const int CollectedItemsOffset = 0x0002;

    /// <summary>Slot-relative offset of the 16-bit equipped-beam mask mirrored from WRAM <c>$7E:D7C4</c>.</summary>
    public const int EquippedBeamsOffset = 0x0004;

    /// <summary>Slot-relative offset of the 16-bit collected-beam mask mirrored from WRAM <c>$7E:D7C6</c>.</summary>
    public const int CollectedBeamsOffset = 0x0006;

    /// <summary>Start of eleven 16-bit controller-binding words at WRAM <c>$7E:D7C8</c>; the first four are fixed D-pad masks.</summary>
    public const int ButtonConfigOffset = 0x0008;

    /// <summary>Slot-relative offset of the configurable Shoot-button SNES bit mask.</summary>
    public const int ShootButtonOffset = ButtonConfigOffset + 8;

    /// <summary>Slot-relative offset of the configurable Jump-button SNES bit mask.</summary>
    public const int JumpButtonOffset = ButtonConfigOffset + 10;

    /// <summary>Slot-relative offset of the configurable Dash-button SNES bit mask.</summary>
    public const int DashButtonOffset = ButtonConfigOffset + 12;

    /// <summary>Slot-relative offset of the configurable item-cancel SNES bit mask.</summary>
    public const int CancelButtonOffset = ButtonConfigOffset + 14;

    /// <summary>Slot-relative offset of the configurable item-select SNES bit mask.</summary>
    public const int SelectButtonOffset = ButtonConfigOffset + 16;

    /// <summary>Slot-relative offset of the configurable aim-down SNES bit mask.</summary>
    public const int AimDownButtonOffset = ButtonConfigOffset + 18;

    /// <summary>Slot-relative offset of the configurable aim-up SNES bit mask.</summary>
    public const int AimUpButtonOffset = ButtonConfigOffset + 20;

    /// <summary>Slot-relative offset of the 16-bit reserve-tank operating mode at WRAM <c>$7E:D7DE</c>.</summary>
    public const int ReserveModeOffset = 0x001e;

    /// <summary>Slot-relative offset of Samus's current energy word at WRAM <c>$7E:D7E0</c>.</summary>
    public const int HealthOffset = 0x0020;

    /// <summary>Slot-relative offset of Samus's maximum energy word at WRAM <c>$7E:D7E2</c>.</summary>
    public const int MaxHealthOffset = 0x0022;

    /// <summary>Slot-relative offset of the current missile-ammunition word at WRAM <c>$7E:D7E4</c>.</summary>
    public const int MissilesOffset = 0x0024;

    /// <summary>Slot-relative offset of the missile-capacity word at WRAM <c>$7E:D7E6</c>.</summary>
    public const int MaxMissilesOffset = 0x0026;

    /// <summary>Slot-relative offset of the current super-missile-ammunition word at WRAM <c>$7E:D7E8</c>.</summary>
    public const int SuperMissilesOffset = 0x0028;

    /// <summary>Slot-relative offset of the super-missile-capacity word at WRAM <c>$7E:D7EA</c>.</summary>
    public const int MaxSuperMissilesOffset = 0x002a;

    /// <summary>Slot-relative offset of the current power-bomb-ammunition word at WRAM <c>$7E:D7EC</c>.</summary>
    public const int PowerBombsOffset = 0x002c;

    /// <summary>Slot-relative offset of the power-bomb-capacity word at WRAM <c>$7E:D7EE</c>.</summary>
    public const int MaxPowerBombsOffset = 0x002e;

    /// <summary>Slot-relative offset of the native HUD item-selection word at WRAM <c>$7E:D7F0</c>.</summary>
    public const int HudItemOffset = 0x0030;

    /// <summary>Slot-relative offset of the reserve-tank energy capacity at WRAM <c>$7E:D7F2</c>.</summary>
    public const int MaxReserveEnergyOffset = 0x0032;

    /// <summary>Slot-relative offset of the currently stored reserve energy at WRAM <c>$7E:D7F4</c>.</summary>
    public const int ReserveEnergyOffset = 0x0034;
    /// <summary>$09D8, persistent excess missile accumulator.</summary>
    public const int ReserveMissilesOffset = 0x0036;
    /// <summary>$09E2, saved alternate/Japanese text setting.</summary>
    public const int JapaneseTextOffset = 0x0040;
    /// <summary>$7E:D91A, cumulative item-PLM setup count, not item percentage.</summary>
    public const int LoadedItemCountOffset = 0x015a;
    /// <summary>$70:1FE0-$1FEB, global completed-game signature used by $80:8261.</summary>
    public const int CompletionMarkerOffset = 0x1fe0;
    /// <summary>$80:824F writes these twelve bytes after the ending.</summary>
    public static ReadOnlySpan<byte> CompletionMarker => "supermetroid"u8;

    /// <summary>Slot-relative offset of the sub-second gameplay counter at WRAM <c>$7E:D7F8</c>.</summary>
    public const int GameTimeFramesOffset = 0x0038;

    /// <summary>Slot-relative offset of the gameplay seconds word at WRAM <c>$7E:D7FA</c>.</summary>
    public const int GameTimeSecondsOffset = 0x003a;

    /// <summary>Slot-relative offset of the gameplay minutes word at WRAM <c>$7E:D7FC</c>.</summary>
    public const int GameTimeMinutesOffset = 0x003c;

    /// <summary>Slot-relative offset of the gameplay hours word at WRAM <c>$7E:D7FE</c>.</summary>
    public const int GameTimeHoursOffset = 0x003e;

    /// <summary>Slot-relative offset of the Boolean moonwalk option word at WRAM <c>$7E:D802</c>.</summary>
    public const int MoonwalkOffset = 0x0042;

    /// <summary>Slot-relative offset of the cartridge debug-mode word at WRAM <c>$7E:D804</c>.</summary>
    public const int DebugFlagOffset = 0x0044;

    /// <summary>Slot-relative offset of the native new-file initialization marker at WRAM <c>$7E:D806</c>.</summary>
    public const int NewFileMarkerOffset = 0x0046;

    /// <summary>Slot-relative offset of the Boolean HUD item-cancel option at WRAM <c>$7E:D808</c>.</summary>
    public const int IconCancelOffset = 0x0048;

    /// <summary>Start of the eight-byte persistent event bitset at WRAM <c>$7E:D820</c>.</summary>
    public const int EventsOffset = 0x0060;

    /// <summary>Start of the per-area boss-state bytes at WRAM <c>$7E:D828</c>.</summary>
    public const int BossBitsOffset = 0x0068;

    /// <summary>Start of the 64-byte room Chozo-orb bitset at WRAM <c>$7E:D830</c>.</summary>
    public const int RoomChozoBitsOffset = 0x0070;

    /// <summary>Start of the 64-byte collected-item bitset at WRAM <c>$7E:D870</c>.</summary>
    public const int CollectedItemBitsOffset = 0x00b0;

    /// <summary>Start of the 64-byte opened-door bitset at WRAM <c>$7E:D8B0</c>.</summary>
    public const int OpenedDoorBitsOffset = 0x00f0;

    /// <summary>Start of the 16-byte used save-station and elevator bitset at WRAM <c>$7E:D8F8</c>.</summary>
    public const int UsedSaveStationsOffset = 0x0138;

    /// <summary>Start of the 12-byte acquired map-station bitset at WRAM <c>$7E:D908</c>.</summary>
    public const int MapStationsOffset = 0x0148;
    /// <summary>$7E:D914 loading_game_state, relative to the saved mirror at $7E:D7C0; consumed by $82:EEB4 before choosing intro or gameplay.</summary>
    public const int LoadingGameStateOffset = 0x0154;

    /// <summary>Slot-relative offset of the area's load-station index at WRAM <c>$7E:D916</c>.</summary>
    public const int SaveStationOffset = 0x0156;

    /// <summary>Slot-relative offset of the saved retail area index at WRAM <c>$7E:D918</c>.</summary>
    public const int AreaOffset = 0x0158;

    /// <summary>Start of the fixed-size packed explored-map payload at WRAM <c>$7E:D91C</c>.</summary>
    public const int CompressedMapDataOffset = 0x015c;

    /// <summary>Byte length of the cartridge's interleaved explored-map payload for one save slot.</summary>
    public const int CompressedMapDataByteCount = 0x0500;

    /// <summary>Number of Zebes area maps packed into SRAM; the Ceres prologue has no persisted map.</summary>
    public const int PackedMapAreaCount = 6;

    /// <summary>First CPU-bus byte of the cartridge's live SRAM mirror at $7E:D7C0.</summary>
    public const int WramMirrorAddress = 0x7ed7c0;
    /// <summary>First persistent event byte at $7E:D820.</summary>
    public const int EventsWramAddress = WramMirrorAddress + EventsOffset;
    /// <summary>First area-boss byte at $7E:D828.</summary>
    public const int BossBitsWramAddress = WramMirrorAddress + BossBitsOffset;
    /// <summary>First opened-Chozo-orb byte at $7E:D830.</summary>
    public const int RoomChozoBitsWramAddress = WramMirrorAddress + RoomChozoBitsOffset;
    /// <summary>First collected-item byte at $7E:D870.</summary>
    public const int CollectedItemBitsWramAddress = WramMirrorAddress + CollectedItemBitsOffset;
    /// <summary>First opened-door byte at $7E:D8B0.</summary>
    public const int OpenedDoorBitsWramAddress = WramMirrorAddress + OpenedDoorBitsOffset;
    /// <summary>First used save-station/elevator byte at $7E:D8F8.</summary>
    public const int UsedSaveStationsWramAddress = WramMirrorAddress + UsedSaveStationsOffset;
    /// <summary>First acquired-map-station byte at $7E:D908.</summary>
    public const int MapStationsWramAddress = WramMirrorAddress + MapStationsOffset;
    /// <summary>Saved frontend dispatcher word at $7E:D914.</summary>
    public const int LoadingGameStateWramAddress = WramMirrorAddress + LoadingGameStateOffset;

    /// <summary>$81:812B SaveSlotOffsets: first payload begins after the sixteen-byte checksum directory.</summary>
    private const ushort FirstSlotOffset = 0x0010;

    /// <summary>$81:812B SaveSlotOffsets: three adjacent payloads, each exactly $065C bytes long.</summary>
    public static ushort SlotOffset(int slot)
    {
        if ((uint)slot >= SlotCount) throw new IndexOutOfRangeException();
        return (ushort)(FirstSlotOffset + slot * SlotByteCount);
    }
}
