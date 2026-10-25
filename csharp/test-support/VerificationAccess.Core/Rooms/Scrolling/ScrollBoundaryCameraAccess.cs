using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Verification access to <see cref="ScrollBoundaryCamera"/> members production does not use.</summary>
internal static class ScrollBoundaryCameraAccess
{
    /// <summary>Rejects debug movement distances that are zero or cross signed 16-bit wrap.</summary>
    extension(ScrollBoundaryCamera)
    {
        /// <summary>Accepts nonzero forward distances below the signed-wrap boundary.</summary>
        internal static void RequireDistance(ushort pixelDistance)
        {
            if (pixelDistance is 0 or >= 0x8000)
                throw new ArgumentOutOfRangeException(nameof(pixelDistance));
        }
    }

    /// <summary>Applies axis-specific debug camera stimuli before invoking the native scroll-boundary handlers.</summary>
    extension(ScrollBoundaryCamera self)
    {
        /// <summary>
        /// Replaces the integer X speed word, then moves the ideal and integer layer-one X words
        /// to <paramref name="target"/>; fractions and the Y axis are untouched.
        /// </summary>
        private void ApplyDebugStimulusX(ushort pixelDistance, ushort target)
        {
            self.Speed = self.Speed.WithX(SignedSixteenSixteen.FromParts(unchecked((short)pixelDistance), self.Speed.X.Fraction));
            self.IdealPosition = self.IdealPosition.WithX(target);
            self.Position = self.Position.WithX(self.Position.X.WithWhole(target));
        }

        /// <summary>The vertical counterpart of <see cref="ApplyDebugStimulusX"/>.</summary>
        private void ApplyDebugStimulusY(ushort pixelDistance, ushort target)
        {
            self.Speed = self.Speed.WithY(SignedSixteenSixteen.FromParts(unchecked((short)pixelDistance), self.Speed.Y.Fraction));
            self.IdealPosition = self.IdealPosition.WithY(target);
            self.Position = self.Position.WithY(self.Position.Y.WithWhole(target));
        }

        /// <summary>Applies a debug rightward stimulus, then runs <c>$80:A641</c>.</summary>
        internal void MoveRight(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusX(pixelDistance, unchecked((ushort)(self.PixelPosition.X + pixelDistance)));
            self.HandleScrollingRight();
        }

        /// <summary>Applies a debug leftward stimulus, then runs <c>$80:A6BB</c>.</summary>
        internal void MoveLeft(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusX(pixelDistance, unchecked((ushort)(self.PixelPosition.X - pixelDistance)));
            self.HandleScrollingLeft();
        }

        /// <summary>Applies a debug downward stimulus, then runs <c>$80:A893</c>.</summary>
        internal void MoveDown(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusY(pixelDistance, unchecked((ushort)(self.PixelPosition.Y + pixelDistance)));
            self.HandleScrollingDown();
        }

        /// <summary>Applies a debug upward stimulus, then runs <c>$80:A936</c>.</summary>
        internal void MoveUp(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusY(pixelDistance, unchecked((ushort)(self.PixelPosition.Y - pixelDistance)));
            self.HandleScrollingUp();
        }
    }
}
