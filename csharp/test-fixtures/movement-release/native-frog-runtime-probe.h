#include "native-bounded-cpu.h"

// #410 room-local original-CPU gameplay trace. Decompression only uses the
// existing asset decoder; all observed movement/projectile/PLM routines are ROM.
int DiagnosticFrogRuntime(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
  room_width_in_blocks = 128; room_height_in_blocks = 16;
  room_width_in_scrolls = 8; room_height_in_scrolls = 1;
  room_size_in_blocks = 4096; area_index = 2;
  const uint8 *state = RomFixedPtr(0x8fb113);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  DecompressToMem(level, g_ram + 0x10000);
  memcpy(BTS, (uint8 *)level_data + 4096, 2048);
  memcpy(scrolls, RomFixedPtr(0x8f0000 | state[14] | state[15] << 8), 8);
  up_scroller = 0x70; down_scroller = 0xa0;
  interactive_enemy_indexes[0] = 0xffff;
  samus_pose = samus_prev_pose = 6; samus_pose_x_dir = samus_prev_pose_x_dir = 4;
  samus_x_pos = samus_prev_x_pos = 1237; samus_y_pos = samus_prev_y_pos = 139;
  samus_x_radius = 5; samus_y_radius = 21;
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
  samus_anim_frame_timer = 5; samus_health = 99; equipped_beams = 5;
  button_config_shoot_x = 0x40; button_config_run_b = 0x8000; button_config_jump_a = 0x80;
  button_config_up = 0x800; button_config_down = 0x400;
  button_config_left = 0x200; button_config_right = 0x100;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  layer1_x_pos = 1109; layer1_y_pos = 0; plm_flag = 0x8000;
  fprintf(f, "frame,x,subx,y,pose,slots,camera\n");
  for (int frame = 0; frame < 900; frame++) {
    joypad1_lastkeys = 0x8250; joypad1_newkeys = frame ? 0 : 0x8250;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
    ProbeRunBounded(0x90ec22); ProbeRunBounded(0x918000); ProbeRunBounded(0x909c5b);
    ProbeRunBounded(0x90ac1c); ProbeRunBounded(0x90b80d);
    ProbeRunBounded(0x90aece); ProbeRunBounded(0x90eb02);
    ProbeRunBounded(0x90a337); ProbeRunBounded(0x908000);
    ProbeRunBounded(0x91e8b6); ProbeRunBounded(0x91eb88);
    ProbeRunBounded(0x90eab3); ProbeRunBounded(0x9094ec); ProbeRunBounded(0x8485b4);
    int active = 0; for (int i = 0; i < 40; i++) if (plm_header_ptr[i]) active++;
    fprintf(f, "%d,%d,%d,%d,%02X,%d,%d\n", frame, samus_x_pos, samus_x_subpos,
      samus_y_pos, samus_pose, active, layer1_x_pos);
    vram_write_queue_tail = 0;
    if (samus_x_pos < 800) break;
  }
  fclose(f); return 0;
}
