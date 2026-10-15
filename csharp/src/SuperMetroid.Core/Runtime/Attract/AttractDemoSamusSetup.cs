namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>Native bank-$91 Samus setup dispatcher identities.</summary>
public enum AttractDemoSamusSetup : ushort
{
    /// <summary>$91:8A33, DemoSetFunc_0: front-facing Landing Site actor.</summary>
    LandingSite = 0x8a33,
    /// <summary>$91:8A3E, DemoSetFunc_3: left-facing grounded morph ball.</summary>
    MorphLeft = 0x8a3e,
    /// <summary>$91:8A43, DemoSetFunc_7: twenty health, standing left.</summary>
    LowHealthLeft = 0x8a43,
    /// <summary>$91:8A49, DemoSetFunc_2: standing left.</summary>
    StandingLeft = 0x8a49,
    /// <summary>$91:8A4E, DemoSetFunc_4: falling left.</summary>
    FallingLeft = 0x8a4e,
    /// <summary>$91:8A53, DemoSetFunc_1: standing right.</summary>
    StandingRight = 0x8a53,
    /// <summary>$91:8A68, DemoSetFunc_5: immediate diagonal-right shinespark.</summary>
    DiagonalShinespark = 0x8a68,
    /// <summary>$91:8A81, DemoSetFunc_6: immediate horizontal-left shinespark.</summary>
    HorizontalShinespark = 0x8a81,
}
