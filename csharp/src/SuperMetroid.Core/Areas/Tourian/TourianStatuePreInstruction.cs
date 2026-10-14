namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The bank-$86 pre-instructions that move Tourian statue unlocking effects.</summary>
internal enum TourianStatuePreInstruction : ushort
{
    /// <summary>$86:B977 splash follows water surface.</summary>
    SplashMotion = 0xb977,
    /// <summary>$86:B982 particle motion pre-instruction.</summary>
    ParticleMotion = 0xb982,
    /// <summary>$86:B9FD soul motion pre-instruction.</summary>
    SoulMotion = 0xb9fd,
}
