"""Validate an lsnes movie and write its controller-1 frames as SNES $4218 words.

The capture adapter replays these words exactly as lsnes does: one line per emulated
frame, every controller poll inside that frame returning the line. Anything this model
cannot reproduce faithfully is refused rather than approximated.
"""


def run():
    import argparse
    import hashlib
    import struct
    import sys
    from pathlib import Path

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("movie", type=Path)
    parser.add_argument("--rom", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from lsmv_movie import read_frames
    words = read_frames(args.movie, args.rom.read_bytes())

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes(struct.pack(f"<{len(words)}H", *words))
    print(f"{len(words)} frames; movie sha256 {hashlib.sha256(args.movie.read_bytes()).hexdigest().upper()}")
    return 0


if __name__ == "__main__":
    import sys
    try:
        if sys.platform == "win32":
            import ctypes
            ctypes.windll.kernel32.SetErrorMode(0x0001 | 0x0002 | 0x8000)
        raise SystemExit(run())
    except Exception:
        import traceback
        traceback.print_exc(file=sys.stderr)
        raise SystemExit(1)
