/* Sweep a one/two-frame windup-to-launch delay with retained/released Dash
   and an optional midair reversal. Boost is earned on the same runway. */
static uint16 midair_input(int frame, int left, int mode) {
  uint16 forward = left ? 0x200 : 0x100;
  uint16 reverse = left ? 0x100 : 0x200;
  if (frame < 140) return 0x8000 | forward;
  if (frame == 140) return 0x410;
  if (frame < 150) return forward | ((mode & 2) ? 0x8000 : 0);
  if (frame < 160) return 0x80 | ((mode & 4) && frame >= 154 ? reverse : forward) | ((mode & 2) ? 0x8000 : 0);
  if (frame < 161 + (mode & 1)) return 0x800;
  if (frame < 186) return 0x880;
  if (frame >= 340 && frame < 350) return forward;
  if (frame >= 360 && frame < 370) return 0x8000 | forward;
  return 0;
}
