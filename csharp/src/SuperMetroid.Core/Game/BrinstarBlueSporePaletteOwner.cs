namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive blue-spore room palette owner.</summary>
public enum BrinstarBlueSporePaletteOwner
{
    /// <summary>Palette-FX object $8D:F775, native Brinstar 1: the $ED99 loop animates BG palette 7 colors 1 through 3 in ordinary blue-spore rooms, without a boss-death pre-instruction.</summary>
    StandardRooms,
    /// <summary>Palette-FX object $8D:F779, native Brinstar 8: the $EE2D variant uses the same spore colors but installs $EEC5 to delete the object when the current area's mini-boss defeat bit ($0002) is set.</summary>
    SporeSpawnRoom,
}
