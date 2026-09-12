#include "native-bounded-cpu.h"

// Room-local original-CPU controller trace for #402. No injected hits or freeze.
static int DiagnosticBotwoonProjectiles(const char *rom, const char *output, bool hyper) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  if (hyper) fprintf(f, "scope,");
  fprintf(f, "release,frame,input,frozen,health,timer,flash,map,x,y,samusX,samusY,pose,hud,type,shotX,shotY\n");
  for (int test = 0; test < (hyper ? 3 : 2); test++) {
    int release = test == 0 ? 296 : hyper ? 300 : 304;
    bool scope = !hyper || test != 2;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0xd95e; random_number = 0x61;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
    if (num_enemies_in_room != 1) { fclose(f); return 8; }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x8000; equipped_beams = collected_beams = 0x1008;
    if (hyper) { equipped_beams = collected_beams = 0x100c; hyper_beam_flag = 1; }
    game_state = 8; reg_INIDISP = 15;
    samus_health = samus_max_health = 999;
    samus_x_pos = samus_prev_x_pos = 192; samus_y_pos = samus_prev_y_pos = 187;
    layer1_x_pos = 96; layer1_y_pos = 21;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
    frame_handler_gamma = 0xe90e; samus_draw_handler = 0xeb52;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    hdma_objects_enable_flag = 0x8000;
    ProbeRunBounded(0x868000);
    ProbeRunBounded(0x89ab82);
    uint16 previous = 0; int firstHit = -1;
    for (int frame = -4; frame < release + (hyper ? 220 : 300); frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 6;
      uint16 input = frame == -3 ? 0x200 : frame == -1 ? 0x2000 :
        (hyper ? frame == release : frame >= release - 90 && frame < release) ? 0x40 : 0;
      if (scope && firstHit >= 0 && (frame - firstHit - 1) % 64 < 60) input = 0x8000;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      oam_next_ptr = vram_write_queue_tail = vram_read_queue_tail = 0;
      EnemyData *head = gEnemyData(0); uint16 health = head->health;
      ProbeRunBounded(0x8884b9); ProbeRunBounded(0x808111); ProbeRunBounded(0x828b44);
      if (head->health < health && firstHit < 0) firstHit = frame;
      if (hyper) fprintf(f, "%d,", scope);
      fprintf(f, "%d,%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",
        release, frame, input, time_is_frozen_flag, head->health, head->invincibility_timer,
        head->flash_timer, head->spritemap_pointer, head->x_pos, head->y_pos,
        samus_x_pos,samus_y_pos,samus_pose,hud_item_index,projectile_type[0],projectile_x_pos[0],projectile_y_pos[0]);
      fflush(f);
      if (hyper && head->health == 0) break;
    }
  }
  fclose(f); return 0;
}

int DiagnosticBotwoonFired(const char *rom, const char *output) {
  return DiagnosticBotwoonProjectiles(rom, output, false);
}

int DiagnosticBotwoonHyper(const char *rom, const char *output) {
  return DiagnosticBotwoonProjectiles(rom, output, true);
}
