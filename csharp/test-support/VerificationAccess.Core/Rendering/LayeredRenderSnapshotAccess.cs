namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="LayeredRenderSnapshot"/> members production does not use.</summary>
internal static class LayeredRenderSnapshotAccess
{
    /// <summary>Exposes <see cref="LayeredRenderSnapshot"/> state that only verification reads.</summary>
    extension(LayeredRenderSnapshot self)
    {
        /// <summary>
        /// The same snapshot without its <typeparamref name="TLayer"/> operations: the other layers keep their order, and the
        /// memory, object selection and brightness are shared. Allocates one layer array, which the result owns.
        /// </summary>
        /// <remarks>Removing layers keeps every validated invariant, including the fused gameplay base staying first.</remarks>
        public LayeredRenderSnapshot Without<TLayer>() where TLayer : RenderLayer
        {
            int kept = 0;
            foreach (RenderLayer layer in self.layers)
            {
                if (layer is not TLayer)
                    kept++;
            }
            var remaining = new RenderLayer[kept];
            int next = 0;
            foreach (RenderLayer layer in self.layers)
            {
                if (layer is not TLayer)
                    remaining[next++] = layer;
            }
            return new LayeredRenderSnapshot(self.Memory, remaining, self.ObjectSelection, self.Brightness);
        }
        /// <summary>A new array of this snapshot's <typeparamref name="TLayer"/> operations, in application order.</summary>
        public TLayer[] LayersOf<TLayer>() where TLayer : RenderLayer
        {
            int count = 0;
            foreach (RenderLayer layer in self.layers)
            {
                if (layer is TLayer)
                    count++;
            }
            var result = new TLayer[count];
            int next = 0;
            foreach (RenderLayer layer in self.layers)
            {
                if (layer is TLayer match)
                    result[next++] = match;
            }
            return result;
        }
    }
}
