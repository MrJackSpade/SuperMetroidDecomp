namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Cartridge definitions for PLM $84:B964, the old Tourian escape shaft fake wall that room
/// setup $8F:91A9 spawns in the Climb during the escape.
/// </summary>
internal static class OldTourianEscapeShaftWallPlmDefinitions
{
    /// <summary>$8F:91AD: the hardcoded spawn's block X.</summary>
    internal const int BlockX = 0x10;
    /// <summary>$8F:91AE: the hardcoded spawn's block Y.</summary>
    internal const int BlockY = 0x87;

    /// <summary>The wall's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Opens the shaft wall's two columns.</summary>
        OpenWall,
    }

    /// <summary>The draw that opens the shaft wall's two columns.</summary>
    internal static PlmFrameDraw<Draw> OpenWallDraw { get; } = PlmDraw.Of(Draw.OpenWall);

    /// <summary>$84:B927-$B92F: Samus must be strictly right of X $F0 and below Y $820.</summary>
    internal const ushort WakeTargetX = 0x00f0;
    /// <summary>$84:B927-$B92F: Samus must be below Y $820 as well as right of the X threshold.</summary>
    internal const ushort WakeTargetY = 0x0820;
}
