namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A7 code pointers consumed by translated Phantoon dispatchers.</summary>
internal static class PhantoonInstructionCodes
{
    /// <summary><c>PlayPhantoonMaterializationSFX</c> at $A7:CEED.</summary>
    public const ushort PlayPhantoonMaterializationSFX = 0xceed;

    /// <summary><c>SetupEyeOpenPhantoonState</c> at $A7:D03F.</summary>
    public const ushort SetupEyeOpenPhantoonState = 0xd03f;

    /// <summary><c>PickNewPhantoonPattern</c> at $A7:D076.</summary>
    public const ushort PickNewPhantoonPattern = 0xd076;

    /// <summary><c>SpawnCasualFlame</c> at $A7:CF5E.</summary>
    public const ushort SpawnCasualFlame = 0xcf5e;

}
