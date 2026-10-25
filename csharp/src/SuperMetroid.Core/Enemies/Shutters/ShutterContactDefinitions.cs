namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Samus's solid-enemy collision slots as the shutter reads them.</summary>
internal static class ShutterContactDefinitions
{
    /// <summary>The all-ones word a solid-enemy collision slot holds when no solid enemy touched Samus.</summary>
    internal const ushort NoSolidEnemy = ushort.MaxValue;
}
