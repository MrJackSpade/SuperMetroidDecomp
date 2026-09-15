/* Original chase, attachment and button-escape callbacks, not a boost award. */
enum {
  DraygonChaseGrab = 0xa58e19,
  DraygonPlaceSamus = 0xa594a9,
  DraygonGrabGamma = 0xe2a1
};
static int draygon_grab_frame(int mode) { return 150 + mode % 4; }
static uint16 draygon_grab_input(int frame, int left, int mode) {
  uint16 forward = left ? 0x200 : 0x100;
  if (frame < 140) return 0x8000 | forward;
  if (frame == 140) return 0x410;
  if (frame < 150) return mode >= 4 ? 0 : 0x10;
  if (frame == 150) return 0x80;
  if (frame < 160) return 0x880;
  if (frame < 220) return frame & 1 ? 0x100 : 0x200;
  if (frame >= 340 && frame < 350) return forward;
  if (frame >= 360 && frame < 370) return 0x8000 | forward;
  return 0;
}
