namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The private members of <see cref="SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior"/> checks name (#2570), each an extension member of the
/// same name reaching the private member through <see cref="PrivateState"/>, so checks name them as the owner's own code
/// does and production keeps them private. A step that takes the segment's chain is handed the chain the segment's entity
/// holds from its setup (#2554), with the segments next to it there.
/// </summary>
internal static class ShaktoolBehaviorPrivateAccess
{
    /// <summary>The private instance members checks read or call.</summary>
    extension(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior self)
    {
        /// <summary>The private method <c>AdvanceAndOrientShaktoolCenter</c>, invoked by name with the centre's neighbours in its chain.</summary>
        internal void AdvanceAndOrientShaktoolCenter(global::SuperMetroid.Core.Enemies.RoomEnemyPlace place, global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentState state)
        {
            global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChain chain = self.ChainOf(place);
            global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentDefinition segment = SegmentOf(place);
            PrivateState.InvokeStatic(typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior), "AdvanceAndOrientShaktoolCenter", [typeof(global::SuperMetroid.Core.Enemies.RoomEnemyPlace), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentState), typeof(global::SuperMetroid.Core.Enemies.RoomEnemyPlace), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChainSegment), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChain)],
                new object?[] { place, state, chain.Before(segment), chain.After(segment), chain });
        }

        /// <summary>The private method <c>ApplyShaktoolCue</c>, invoked by name with the chain of the segment the cue plays in.</summary>
        internal void ApplyShaktoolCue(global::SuperMetroid.Core.Enemies.RoomEnemyPlace place, global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolCue cue) =>
            PrivateState.InvokeStatic(typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior), "ApplyShaktoolCue", [typeof(global::SuperMetroid.Core.Enemies.RoomEnemyPlace), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChain), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolCue)], new object?[] { place, self.ChainOf(place), cue });

        /// <summary>The chain <paramref name="segment"/>'s Shaktool is in, which its entity holds from its setup.</summary>
        /// <exception cref="InvalidOperationException">The place holds no Shaktool segment.</exception>
        internal global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChain ChainOf(global::SuperMetroid.Core.Enemies.RoomEnemyPlace segment) => EntityOf(segment).Chain;

        /// <summary>The private method <c>ReverseShaktoolAfterCollision</c>, invoked by name with the trailing saw's chain.</summary>
        internal void ReverseShaktoolAfterCollision(global::SuperMetroid.Core.Enemies.RoomEnemyPlace place, global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentState state, global::SuperMetroid.Core.Spatial.Position16<global::SuperMetroid.Core.Spatial.RoomPixels> previous) =>
            PrivateState.InvokeStatic(typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolBehavior), "ReverseShaktoolAfterCollision", [typeof(global::SuperMetroid.Core.Enemies.RoomEnemyPlace), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentState), typeof(global::SuperMetroid.Core.Spatial.Position16<global::SuperMetroid.Core.Spatial.RoomPixels>), typeof(global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolChain)],
                new object?[] { place, state, previous, self.ChainOf(place) });
    }

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

    /// <summary>The Shaktool segment <paramref name="place"/> holds.</summary>
    /// <exception cref="InvalidOperationException">The place holds no Shaktool segment.</exception>
    private static global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolEntity EntityOf(global::SuperMetroid.Core.Enemies.RoomEnemyPlace place) =>
        place.Entity as global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolEntity ?? throw new InvalidOperationException($"Enemy place {place.Ordinal} holds no Shaktool segment.");

    /// <summary>The segment of the chain <paramref name="place"/>'s Shaktool is, as its placement names it.</summary>
    private static global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegmentDefinition SegmentOf(global::SuperMetroid.Core.Enemies.RoomEnemyPlace place) => EntityOf(place).Settings.Segment;
}
