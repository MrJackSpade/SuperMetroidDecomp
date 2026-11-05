using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="LayeredScene"/> members production does not use.</summary>
internal static class LayeredRenderSnapshotAccess
{
    /// <summary>Exposes <see cref="LayeredScene"/> state that only verification reads.</summary>
    extension(LayeredScene self)
    {
        /// <summary>
        /// Every sprite part the scene shows, frontmost first (#1514): its paint's, then each sprite layer's, from the
        /// frontmost sprite layer back.
        /// </summary>
        internal PlacedSpritePart[] SpriteParts() => PlacedSpritePart.Of(self.SpriteDraws);

        /// <summary>
        /// This scene painted from <paramref name="nativeColours"/>, a native 256-colour image, as a frame's
        /// <c>WithNativeColours</c> paints it (#1514): its paint so, and each sprite layer's draws in the native colours
        /// their inks name. A glow over a sprite layer glows over that layer's repainted copy.
        /// </summary>
        internal LayeredScene WithNativeColours(ReadOnlySpan<RgbColor> nativeColours)
        {
            FramePaint paint = self.Paint.WithNativeColours(nativeColours);
            var layers = new DisplayLayer[self.layers.Length];
            for (int index = 0; index < layers.Length; index++)
            {
                layers[index] = self.layers[index] is SpritesLayer sprites
                    ? new SpritesLayer(new FramePaint(paint.Backdrop, sprites.Draws).WithNativeColours(nativeColours).SpriteDraws, sprites.Blend, sprites.Tint)
                    : self.layers[index];
            }
            return new LayeredScene(paint, Reglowed(self.layers, layers), self.Brightness);
        }

        /// <summary>
        /// This scene with its one sprite layer showing an OAM fixture's records in place of its draws, converted and
        /// coloured from <paramref name="nativeColours"/> as <c>FramePaint.FromOam</c> converts them (#1514), over the
        /// native colour 0 backdrop.
        /// </summary>
        /// <exception cref="InvalidOperationException">The scene has other than one sprite layer.</exception>
        internal LayeredScene WithNativeColoursAndOam(ReadOnlySpan<byte> vram, ReadOnlySpan<RgbColor> nativeColours,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection)
        {
            FramePaint native = FramePaint.FromOam(vram, nativeColours, oamUpload, spriteCount, objectSelection);
            var layers = new DisplayLayer[self.layers.Length];
            int replaced = 0;
            for (int index = 0; index < layers.Length; index++)
            {
                if (self.layers[index] is SpritesLayer sprites)
                {
                    layers[index] = new SpritesLayer(native.SpriteDraws, sprites.Blend, sprites.Tint);
                    replaced++;
                }
                else
                    layers[index] = self.layers[index];
            }
            if (replaced != 1)
                throw new InvalidOperationException($"An OAM fixture replaces a scene's one sprite layer, not {replaced}.");
            return new LayeredScene(new FramePaint(native.Backdrop), Reglowed(self.layers, layers), self.Brightness);
        }

        /// <summary>
        /// This scene with <paramref name="hidden"/> drawn by none of its sprite layers (#1514): each sprite layer keeps its
        /// other draws in their order, and a glow over a sprite layer glows over that layer's copy.
        /// </summary>
        internal LayeredScene WithoutDraws(IReadOnlyCollection<SpriteDraw> hidden)
        {
            var layers = new DisplayLayer[self.layers.Length];
            for (int index = 0; index < layers.Length; index++)
            {
                layers[index] = self.layers[index] is SpritesLayer sprites
                    ? new SpritesLayer([.. sprites.Draws.Where(draw => !hidden.Contains(draw))], sprites.Blend, sprites.Tint)
                    : self.layers[index];
            }
            return new LayeredScene(self.Paint, Reglowed(self.layers, layers), self.Brightness);
        }

        /// <summary>
        /// The same snapshot without its <typeparamref name="TLayer"/> operations: the other layers keep their order, and the
        /// memory, object selection and brightness are shared. Allocates one layer array, which the result owns.
        /// </summary>
        /// <remarks>Removing layers keeps every validated invariant, including the fused gameplay base staying first.</remarks>
        public LayeredScene Without<TLayer>() where TLayer : DisplayLayer
        {
            int kept = 0;
            foreach (DisplayLayer layer in self.layers)
            {
                if (layer is not TLayer)
                    kept++;
            }
            var remaining = new DisplayLayer[kept];
            int next = 0;
            foreach (DisplayLayer layer in self.layers)
            {
                if (layer is not TLayer)
                    remaining[next++] = layer;
            }
            return new LayeredScene(self.Paint, remaining, self.Brightness);
        }
        /// <summary>A new array of this snapshot's <typeparamref name="TLayer"/> operations, in application order.</summary>
        public TLayer[] LayersOf<TLayer>() where TLayer : DisplayLayer
        {
            int count = 0;
            foreach (DisplayLayer layer in self.layers)
            {
                if (layer is TLayer)
                    count++;
            }
            var result = new TLayer[count];
            int next = 0;
            foreach (DisplayLayer layer in self.layers)
            {
                if (layer is TLayer match)
                    result[next++] = match;
            }
            return result;
        }
    }

    /// <summary>
    /// <paramref name="replaced"/>, layer for layer a copy of <paramref name="original"/>, with each glow over one of the
    /// original's sprite layers glowing over the sprite layer that replaced it.
    /// </summary>
    private static DisplayLayer[] Reglowed(DisplayLayer[] original, DisplayLayer[] replaced)
    {
        for (int index = 0; index < replaced.Length; index++)
        {
            if (replaced[index] is AddedPicturedPlaneLayer { GlowsOverSprites: { } sprites } glow)
                replaced[index] = glow with { GlowsOverSprites = (SpritesLayer)replaced[Array.FindIndex(original, layer => ReferenceEquals(layer, sprites))] };
        }
        return replaced;
    }
}
