#include "native-bounded-cpu.h"

// #604: original game-state-eight execution, intact retail room and normal PB input.
int DiagnosticNoobTubeControl(const char *rom, const char *output, int wake) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_ptr = 0xcefb; random_number = 0x61;
  ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
  // Preserve the authored population and native descending slot order.
  uint16 population = get_RoomDefRoomstate(roomdefroomstate_ptr)->room_plm_header_ptr;
  for (uint16 p = population; get_RoomPlmEntry(p)->plm_header_ptr_; p += 6)
    ProbeRunBoundedRegisters(0x84846a, 0, p, 0);
  ProbeRunBounded(0x8483ad);
  ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
  equipped_items = collected_items = 0x1005;
  game_state = 8; reg_INIDISP = 15;
  samus_health = samus_max_health = 999;
  samus_power_bombs = samus_max_power_bombs = 10;
  samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 395;
  layer1_x_pos = 32; layer1_y_pos = 229;
  samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
  samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
  frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
  frame_handler_gamma = 0xe90e; samus_draw_handler = 0xeb52;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  hdma_objects_enable_flag = 0x8000;
  ProbeRunBounded(0x868000); ProbeRunBounded(0x89ab82);
  fprintf(f, "frame,input,x,y,pose,animation,timer,alpha,beta,pre,instruction,pbStatus,pbCount\n");
  uint16 previous = 0;
  for (int frame = 0; frame < 700; frame++) {
    nmi_frame_counter_word = nmi_frame_counter_byte = frame + 1;
    uint16 input = frame == 0 || frame == 12 ? 0x400 : 0;
    if (frame == 24) input = 0x2000;
    if (frame == 26) input = 0x40;
    if (frame == 40) input = 0x800;
    if (wake >= 0 && frame >= wake && frame < wake + 30) input = 0x100;
    joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
    oam_next_ptr = vram_write_queue_tail = vram_read_queue_tail = 0;
    ProbeRunBounded(0x8884b9); ProbeRunBounded(0x808111); ProbeRunBounded(0x828b44);
    uint16 pre = 0, instruction = 0;
    for (int p = 0; p < 40; p++) if (plm_header_ptr[p] == 0xd70c) {
      pre = plm_pre_instrs[p]; instruction = plm_instr_list_ptrs[p];
    }
    fprintf(f, "%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n", frame,input,samus_x_pos,samus_y_pos,
      samus_pose,samus_anim_frame,samus_anim_frame_timer,frame_handler_alfa,frame_handler_beta,
      pre,instruction,power_bomb_explosion_status,samus_power_bombs);
  }
  fclose(f); return 0;
}
