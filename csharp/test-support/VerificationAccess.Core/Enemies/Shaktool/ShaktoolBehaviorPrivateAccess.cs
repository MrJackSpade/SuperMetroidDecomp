namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The private members of <see cref="SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior"/> checks name (#2570), each an extension member of the
/// same name reaching the private member through <see cref="PrivateState"/>, so checks name them as the owner's own code
/// does and production keeps them private.
/// </summary>
internal static class ShaktoolBehaviorPrivateAccess
{
    /// <summary>The private static members checks name.</summary>
    extension(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior)
    {
        /// <summary>The private method <c>OrbitDisplacement</c>, invoked by name.</summary>
        internal static global::SuperMetroid.Core.Spatial.FixedVector<global::SuperMetroid.Core.Spatial.RoomPixels> OrbitDisplacement(global::SuperMetroid.Core.Spatial.TrigonometryAngle angle) =>
            PrivateState.InvokeStatic<global::SuperMetroid.Core.Spatial.FixedVector<global::SuperMetroid.Core.Spatial.RoomPixels>>(typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior), "OrbitDisplacement", [typeof(global::SuperMetroid.Core.Spatial.TrigonometryAngle)], new object?[] { angle });

        /// <summary>The private method <c>SineVelocity</c>, invoked by name.</summary>
        internal static global::SuperMetroid.Core.Spatial.SignedEightEight SineVelocity(global::SuperMetroid.Core.Spatial.TrigonometryAngle angle) =>
            PrivateState.InvokeStatic<global::SuperMetroid.Core.Spatial.SignedEightEight>(typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior), "SineVelocity", [typeof(global::SuperMetroid.Core.Spatial.TrigonometryAngle)], new object?[] { angle });
    }
}
