"""Read an lsnes movie's controller-1 frames as SNES $4218 words.

One line per emulated frame; lsnes answers every poll inside a frame with that line.
Anything the power-on capture model cannot reproduce faithfully is refused.
"""

import hashlib
import zipfile

CORE_PREFIX = "bsnes v085"


def read_frames(movie_path, rom_bytes):
    """Return the movie's per-frame input words after validating it against the ROM."""
    archive = zipfile.ZipFile(movie_path)
    names = set(archive.namelist())

    def member(name):
        return archive.read(name).decode("utf-8").strip()

    if member("systemid") != "lsnes-rr1":
        raise ValueError("Not an lsnes rr1 movie")
    if member("gametype") != "snes_ntsc":
        raise ValueError("Only NTSC SNES movies are supported")
    if not member("coreversion").startswith(CORE_PREFIX):
        raise ValueError(f"Movie core {member('coreversion')!r} is not the bsnes v085 the adapter builds")
    # Non-default core settings, savestate starts and initial SRAM change power-on state
    # or poll timing; the adapter models only lsnes's defaults from power-on.
    for unsupported in ("settings", "savestate", "moviesram", "anchorsave", "rtc.second"):
        for name in names:
            if name == unsupported or name.startswith(unsupported + "."):
                raise ValueError(f"Movie member {name!r} is not supported by the power-on adapter")
    if member("rom.sha256").lower() != hashlib.sha256(rom_bytes).hexdigest():
        raise ValueError("The movie was recorded with a different ROM")

    # lsnes gamepad field order is B Y Select Start Up Down Left Right A X L R, the
    # libsnes joypad ids 0..11; in the $4218 word id n is bit 15 - n.
    words = []
    for number, line in enumerate(archive.read("input").decode("utf-8").splitlines()):
        if not line.startswith("F"):
            raise ValueError(f"Input line {number} is a subframe; subframe polls are not modelled")
        system, separator, pad = line.partition("|")
        if separator != "|" or "|" in pad or len(pad) != 12:
            raise ValueError(f"Input line {number} is not exactly one 12-button gamepad")
        fields = system.split()
        if len(fields) != 3 or fields[0] != "F." or fields[1:] != ["0", "0"]:
            raise ValueError(f"Input line {number} requests a reset; resets are not modelled")
        word = 0
        for index, character in enumerate(pad):
            if character != ".":
                word |= 1 << (15 - index)
        words.append(word)
    if not words:
        raise ValueError("The movie has no frames")
    return words
