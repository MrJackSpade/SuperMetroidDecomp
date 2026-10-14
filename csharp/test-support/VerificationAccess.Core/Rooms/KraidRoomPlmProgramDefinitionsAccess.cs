using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="KraidRoomPlmProgramDefinitions"/> members production does not use.</summary>
internal static class KraidRoomPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Kraid room PLM words for bounded cartridge comparison.</summary>
    extension(KraidRoomPlmProgramDefinitions)
    {
        /// <summary>Visits decoded Kraid ceiling-crumble words across the room PLM interval.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            for (int address = KraidRoomPlmProgramDefinitions.CrumbleCeilingBackground1; address < KraidRoomPlmProgramDefinitions.EndExclusive; address++)
                if (KraidRoomPlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out _))
                    yield return (ushort)address;
        }
    }
}
