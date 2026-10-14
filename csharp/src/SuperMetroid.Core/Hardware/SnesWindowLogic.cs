namespace SuperMetroid.Core.Hardware;

/// <summary>Two-bit WBGLOG/WOBJLOG operation when both windows are enabled.</summary>
public enum SnesWindowLogic : byte
{
    /// <summary>Includes a pixel when either enabled window includes it.</summary>
    Or = 0,
    /// <summary>Includes a pixel only when both enabled windows include it.</summary>
    And = 1,
    /// <summary>Includes a pixel when exactly one of the two enabled windows includes it.</summary>
    Xor = 2,
    /// <summary>Includes a pixel when both enabled windows agree, including pixels outside both.</summary>
    Xnor = 3,
}
