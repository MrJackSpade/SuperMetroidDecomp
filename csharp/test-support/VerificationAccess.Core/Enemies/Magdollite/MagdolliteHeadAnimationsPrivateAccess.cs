namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The private members of the Magdollite's head animations checks name (#2570), each an extension member of the
/// same name and signature reaching the private member through <see cref="PrivateState"/>, so checks name them as the
/// owner's own code does and production keeps them private.
/// </summary>
internal static class MagdolliteHeadAnimationsPrivateAccess
{
    /// <summary>The head animations' private static members checks name.</summary>
    extension(global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadAnimations)
    {
        /// <summary>The private property <c>StartingAt</c>, reached by name.</summary>
        internal static global::System.Collections.Generic.Dictionary<global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadSegment, global::SuperMetroid.Core.Animation.Animation<global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadSegment>> StartingAt
        {
            get => PrivateState.StaticGet<global::System.Collections.Generic.Dictionary<global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadSegment, global::SuperMetroid.Core.Animation.Animation<global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadSegment>>>(typeof(global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadAnimations), "StartingAt");
        }
    }
}
