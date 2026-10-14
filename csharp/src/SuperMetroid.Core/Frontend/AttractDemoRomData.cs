namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definition tables consumed by the title-demo loaders.</summary>
public static class AttractDemoRomData
{
    /// <summary>$82:8548 waits ninety NMI calls on the completed demo's final image.</summary>
    public const int FinalImageHoldFrames = 90;
    /// <summary>$82:8000 demo enemy-graphics loop starts at six and includes zero.</summary>
    public const int EnemyTransferFrames = 7;
    /// <summary>$80:8261 enables three sets before the completed-game SRAM marker.</summary>
    public const int DefaultSetCount = 3;
    /// <summary>Literal state writes in the demo setup routines.</summary>
    public static class SetupValues
    {
        /// <summary>$91:8A43 sets current energy to twenty without changing maximum energy.</summary>
        public const ushort LowHealth = 20;
        /// <summary>$82:891A writes the scroll cell at byte offset $21.</summary>
        public const int ChargeBeamScrollIndex = 0x21;
        /// <summary>$82:892B writes sixty to Kraid's body function timer.</summary>
        public const ushort KraidFunctionTimer = 60;
    }
    /// <summary>Number of physical entries in each retail demo-set pointer table.</summary>
    public const int SetCount = 4;
}
