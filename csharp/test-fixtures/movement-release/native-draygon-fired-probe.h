#include "native-bounded-cpu.h"

// Original shared aiming routine, including out-of-range hardware byte truncation.
int DiagnosticEnemyAngle(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  static const int16 values[] = {-32768,-32767,-512,-260,-256,-255,-1,0,1,90,255,256,260,512,32767};
  fprintf(f, "x,y,angle\n");
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  for (int x = 0; x < 15; x++) for (int y = 0; y < 15; y++) {
    *(int16 *)(g_ram + 0x12) = values[x]; *(int16 *)(g_ram + 0x14) = values[y];
    ProbeRunBounded(0xa0c0af);
    fprintf(f, "%d,%d,%u\n", values[x],values[y],g_snes->cpu->a & 255);
  }
  fclose(f); return 0;
}

// Original game-state-eight trajectory, including live cannon damage/knockback.
int DiagnosticDraygonFired(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "release,scope,frame,input,frozen,health,timer,flash,map,x,y,samusX,samusY,pose,hud,type,shotX,shotY,samusHealth,function");
  for (int p = 0; p < 18; p++) fprintf(f, ",p%d_id,p%d_x,p%d_y,p%d_subX,p%d_subY,p%d_angle,p%d_pre",p,p,p,p,p,p,p);
  fprintf(f, "\n");
  static const int releases[] = {1574,1575,1580,1581,1581};
  for (int test = 0; test < 5; test++) {
    int release = releases[test], scope = test != 4;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0xda60; random_number = 0x61;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
    if (num_enemies_in_room != 4) { fclose(f); return 8; }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x8020; equipped_beams = collected_beams = 0x1008;
    game_state = 8; reg_INIDISP = 15;
    samus_health = samus_max_health = 999;
    samus_x_pos = samus_prev_x_pos = 256; samus_y_pos = samus_prev_y_pos = 443;
    layer1_x_pos = 160; layer1_y_pos = 277;
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
    uint16 previous = 0; int firstHit = -1;
    for (int frame = -2; frame < release + 250; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 3;
      uint16 input = frame == -2 ? 0x2000 : frame == -1 ? 0 : 0x800;
      if (frame >= release - 90 && frame < release) input |= 0x40;
      if (scope && firstHit >= 0 && (frame - firstHit - 1) % 64 < 60) input = 0x8000;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      oam_next_ptr = vram_write_queue_tail = vram_read_queue_tail = 0;
      EnemyData *body = gEnemyData(0); uint16 health = body->health;
      ProbeRunBounded(0x8884b9); ProbeRunBounded(0x808111); ProbeRunBounded(0x828b44);
      if (body->health < health && firstHit < 0) firstHit = frame;
      fprintf(f, "%d,%d,%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u",
        release, scope, frame, input, time_is_frozen_flag, body->health, body->invincibility_timer,
        body->flash_timer, body->spritemap_pointer, body->x_pos, body->y_pos,
        samus_x_pos,samus_y_pos,samus_pose,hud_item_index,projectile_type[0],projectile_x_pos[0],projectile_y_pos[0],
        samus_health,Get_Draygon(0)->draygon_var_A);
      for (int p = 0; p < 18; p++) {
        if (!eproj_id[p]) fprintf(f, ",0,0,0,0,0,0,0");
        else fprintf(f, ",%u,%u,%u,%u,%u,%u,%u",eproj_id[p],eproj_x_pos[p],eproj_y_pos[p],eproj_x_subpos[p],eproj_y_subpos[p],
          eproj_id[p] == 0x8e5e || eproj_id[p] == 0x8e50 ? g_word_7E97DC[p] : 0,eproj_pre_instr[p]);
      }
      fprintf(f, "\n");
      fflush(f);
    }
  }
  fclose(f); return 0;
}
