namespace SuperMetroid.Core.Game;

/// <summary>Map exploration selected by the shared Ridley initializer at $A6:A0F5.</summary>
internal static class RidleyMapDefinitions
{
    /// <summary>$90:A84E selects the Ridley list only for boss ID five (Norfair).</summary>
    internal const ushort NorfairBossId = 5;

    /// <summary>$90:A89C: arena cells (0,0) and (0,1), followed by the terminator.</summary>
    internal const int ArenaScreenRows = 2;
}
