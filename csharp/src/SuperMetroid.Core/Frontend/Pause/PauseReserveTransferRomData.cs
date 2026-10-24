namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Native equipment-screen reserve input and transfer definitions.</summary>
internal static class PauseReserveTransferRomData
{
    /// <summary>$82:AC87, first tank subdispatcher: mode selector.</summary>
    public const int ModeItem = (int)PauseReserveItem.Mode;
    /// <summary>$82:AF62-$AF75 rounds the initial countdown up to eight and tests its low three bits.</summary>
    public const ushort SoundCadenceMask = 7;
    /// <summary>$82:AF7A, manual reserve refill uses library-three sound $2D.</summary>
    public static readonly SoundEffectId RefillSound = Library3Sound.IncrementalEnergy;
    /// <summary>$82:AF7D calls QueueSound_Lib3_Max6.</summary>
    public const byte MaximumQueuedSounds = 6;
}
