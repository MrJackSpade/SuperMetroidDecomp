namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>Native values the Skultera behavior check sets up. Only verification reads them.</summary>
internal static class SkulteraNativeValues
{
    /// <summary>A parameter one whose nonzero high byte starts Skultera swimming left, with speed index zero.</summary>
    internal const ushort StartsLeftParameter = 0x0100;
}
