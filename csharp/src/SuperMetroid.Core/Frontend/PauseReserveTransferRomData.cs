using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native equipment-screen reserve input and transfer definitions.</summary>
internal static class PauseReserveTransferRomData
{
    /// <summary>$82:AC87, first tank subdispatcher: mode selector.</summary>
    public const int ModeItem = 0;
    /// <summary>$82:AC89, second tank subdispatcher: manual energy transfer.</summary>
    public const int TransferItem = 1;
    /// <summary>$82:AEA9 stores reserve mode two when leaving AUTO.</summary>
    public const ushort ManualMode = 2;
    /// <summary>$82:BF04, ReserveTank_TransferEnergyPerFrame, consumed as a ROM word.</summary>
    public const int TransferAmount = 0x82bf04;
    /// <summary>$82:AF62-$AF75 rounds the initial countdown up to eight and tests its low three bits.</summary>
    public const ushort SoundCadenceMask = 7;
    /// <summary>$82:AF7A, manual reserve refill uses library-three sound $2D.</summary>
    public static readonly SoundEffectId RefillSound = new(SoundEffectLibrary.Library3, 0x2d);
    /// <summary>$82:AF7D calls QueueSound_Lib3_Max6.</summary>
    public const byte MaximumQueuedSounds = 6;
}
