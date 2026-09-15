/* Native visual probe: the viewport follows Samus, but echo positions remain
   the world coordinates produced by the real movement routines. */
enum { SamusBodyDraw = 0x9085e2, SamusEchoDraw = 0x9087bd,
       OamLowStart = 0x370, OamHighStart = 0x570, OamHighBytes = 32 };
static void draw_echo_probe(void) {
  layer1_x_pos = samus_x_pos - 128;
  layer1_y_pos = samus_y_pos - 112;
  oam_next_ptr = 0;
  memset(g_ram + OamHighStart, 0, OamHighBytes);
  run(SamusBodyDraw);
  oam_next_ptr = 0;
  memset(g_ram + OamHighStart, 0, OamHighBytes);
  run(SamusEchoDraw);
}
static void print_echo_probe(int frame) {
  printf(",%04X,%04X,%04X,%04X,%04X,%04X,", speed_echoes_index,
    speed_echo_xpos[0],speed_echo_ypos[0],speed_echo_xpos[1],speed_echo_ypos[1],oam_next_ptr);
  if (frame >= 150 && frame < 175)
    for (int i = 0; i < oam_next_ptr; i++) printf("%02X",g_ram[OamLowStart+i]);
  printf(",");
  if (frame >= 150 && frame < 175)
    for (int i = 0; i < OamHighBytes; i++) printf("%02X",g_ram[OamHighStart+i]);
}
