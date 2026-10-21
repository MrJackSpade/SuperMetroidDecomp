using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="ProjectileSpriteCatalog"/> members production does not use.</summary>
internal static class ProjectileSpriteCatalogAccess
{
    /// <summary>Exposes <see cref="ProjectileSpriteCatalog"/> state that only verification reads.</summary>
    extension(ProjectileSpriteCatalog self)
    {
        /// <summary>Emits one installed projectile composition in authored OAM order using native projectile coordinate wrapping, without advancing animation or collision state.</summary>
        /// <param name="id">Bank-$93 spritemap offset selected by a timed projectile record, not an instruction-list pointer or projectile family.</param>
        /// <param name="oam">Destination OAM buffer; the native wrapping insertion cursor advances for every part.</param>
        /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
        /// <exception cref="InvalidDataException">The requested sprite identity is not installed.</exception>
        public void Draw(ProjectileSpriteId id, OamBuffer oam, Position16<OamPixels> origin)
        {
            if (!self.frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id.Name}.");
            for (int index = 0; index < parts.PartCount; index++)
            {
                var part = parts.Part(index);
                oam.AddProjectileSpritePart(part.X, part.Y.Raw, part.Attributes, origin);
            }
        }

        /// <summary>Draws the composition stored at bank-$93 <paramref name="address"/> without consulting ROM.</summary>
        public void Draw(int address, OamBuffer oam, Position16<OamPixels> origin) =>
            self.Draw(ProjectileSpriteNative.Projectile(address), oam, origin);
    }
}
