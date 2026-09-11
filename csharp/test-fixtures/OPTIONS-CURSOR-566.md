# Controller settings selector alignment (#566)

Affected player version: 0.2.0.

## Reproduction

Normal menu input selects Controller Settings and advances to END. Before the
fix, the missile was beside ANGLE DOWN instead: END's native screen Y is 184,
but the port subtracted the page's 32-pixel BG scroll again, placing it at 152.
Local before/after images from `--options-heading-audit` visibly confirm this.
The older heading test passed because it compared only the top 32 pixel rows.

The new `--options-cursor-audit <ROM> <local-output-directory>` also failed before
the fix on its first scrolling frame: 76 pixels differed from the native hidden
cursor composition. The port incorrectly kept drawing the selector during scroll.

## Native behavior and fix

Pinned bank-$82 sources agree: `$82:F2A9` reads screen-space X/Y pairs directly
from `$82:F31B` for controller state seven, without subtracting BG1 scroll.
The last two rows already contain the scrolled screen positions. Scroll states
nine/ten have null table pointers and use the off-screen origin `(384,16)`.
The heading has a separate pre-instruction that follows BG scrolling.

The shared options OAM builder now preserves those screen-space cursor anchors
and uses the native off-screen anchor during scrolling. Heading and BG movement
are unchanged; no row-specific visual correction was added.

## Verification

- 202 complete frame comparisons: all nine selections, both navigation directions,
  wraparound, and every scroll frame, in English and Japanese.
- The reference reads positions, hidden-origin operands, heading setup, and
  spritemaps from ROM independently of the managed coordinate selection.
- Reference composition retains the current animation phase and BG layers to
  isolate selector placement relative to the displayed labels. It does not claim
  independent validation of missile animation timing or label artwork.
- Every checked snapshot also matches direct options rendering.
- The existing five heading comparisons and Windows Release build pass.

This is a rendered production-menu repro and ROM-origin pixel comparison, not
an independently emulated full-menu recording. Screenshots remain local under
`csharp/test-temp/options-selector-566-before` and `options-selector-566-after`;
none are committed or publicly attached. Leave open for player confirmation.
