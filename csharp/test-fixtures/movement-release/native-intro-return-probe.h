#include "native-bounded-cpu.h"

// #511: original-CPU intro alpha/beta after knockback recovery. This isolates the
// return input stream; it does not claim to reproduce the earlier Rinka hit.
int DiagnosticIntroReturn(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_width_in_blocks = room_height_in_blocks = 16;
  room_width_in_scrolls = room_height_in_scrolls = 1;
  room_size_in_blocks = 512;
  interactive_enemy_indexes[0] = 0xffff;
  memcpy(level_data, RomPtr(0x8cbec3), 448);
  fx_y_pos = lava_acid_y_pos = 0xffff;
  cinematic_function = 0xaf6c;
  samus_health = samus_max_health = 99;
  samus_x_pos = samus_prev_x_pos = 204; samus_y_pos = samus_prev_y_pos = 115;
  samus_pose = samus_prev_pose = 2; samus_pose_x_dir = samus_prev_pose_x_dir = 4;
  samus_x_radius = 5; samus_y_radius = 21;
  samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
  samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
  grapple_beam_function = 0xc4f0;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  fprintf(f, "frame,input,pose,x,y,animation,timer\n");
  for (int frame = 0; frame < 80; frame++) {
    uint16 input = frame >= 1 && frame <= 20 ? 0x200 : 0;
    if (frame >= 9 && frame <= 16) input |= 0x80;
    uint16 edge = frame == 1 ? 0x200 : frame == 9 ? 0x80 : 0;
    controller1_input_for_demo = input; controller1_new_input_for_demo = edge;
    joypad1_lastkeys = input; joypad1_newkeys = edge;
    nmi_frame_counter_word = nmi_frame_counter_byte = frame;
    ProbeRunBounded(0x90e6c9);
    ProbeRunBounded(0x90e833);
    fprintf(f, "%d,%04X,%02X,%u,%u,%u,%u\n", frame,input,samus_pose,
      samus_x_pos,samus_y_pos,samus_anim_frame,samus_anim_frame_timer);
  }
  fclose(f); return 0;
}
