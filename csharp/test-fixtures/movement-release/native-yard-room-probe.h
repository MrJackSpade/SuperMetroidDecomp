// #377: all five retail Aqueduct Yards, full native enemy dispatcher and terrain.
// Requires native-release-probe.h. Dispatch before SDL with explicit error dialogs disabled.
#include "native-bounded-cpu.h"
int DiagnosticYardRoom(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "follow,frame,slot,cameraX,cameraY,samusX,samusY,facing,quake,rng,x,y,properties,list,timer,map,a,b,c,d,e,f,direction,behavior,ySpeed,xSpeed\n");
  fflush(f);
  for (int follow = 0; follow < 5; follow++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0xd5a7; random_number = 0x61;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2);
    ProbeRunBounded(0x82e7d3);
    ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
    if (num_enemies_in_room != 5) { fclose(f); return 8; }
    samus_health = samus_max_health = 999;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_x_pos = Get_MaridiaSnail(follow * 64)->base.x_pos - 64;
    samus_y_pos = Get_MaridiaSnail(follow * 64)->base.y_pos;
    for (int frame = 0; frame < 1200; frame++) {
      Enemy_MaridiaSnail *tracked = Get_MaridiaSnail(follow * 64);
      layer1_x_pos = tracked->base.x_pos > 128 ? tracked->base.x_pos - 128 : 0;
      layer1_y_pos = tracked->base.y_pos > 112 ? tracked->base.y_pos - 112 : 0;
      // Initially observed, then allowed to crawl; the second half adds the
      // real super-missile earthquake signal and terrain landing transitions.
      samus_pose_x_dir = frame == 0 ? 8 : 4;
      samus_pose = samus_pose_x_dir == 8 ? 1 : 2;
      earthquake_timer = frame == 600 ? 30 : 0; earthquake_type = 20;
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      oam_next_ptr = vram_write_queue_tail = 0;
      memset(enemy_drawing_queue_sizes, 0, 16);
      ProbeRunBounded(0xa08eb6); ProbeRunBounded(0xa08fd4);
      for (int slot = 0; slot < 5; slot++) {
        Enemy_MaridiaSnail *e = Get_MaridiaSnail(slot * 64);
        fprintf(f, "%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X%04X\n",
          follow,frame,slot,layer1_x_pos,layer1_y_pos,samus_x_pos,samus_y_pos,samus_pose_x_dir,earthquake_timer,random_number,
          e->base.x_pos,e->base.x_subpos,e->base.y_pos,e->base.y_subpos,e->base.properties,e->base.current_instruction,
          e->base.instruction_timer,e->base.spritemap_pointer,e->msl_var_A,e->msl_var_B,e->msl_var_C,e->msl_var_D,e->msl_var_E,e->msl_var_F,
          e->msl_var_07,e->msl_var_08,e->msl_var_01,e->msl_var_00,e->msl_var_03,e->msl_var_02);
      }
      fflush(f);
    }
  }
  fclose(f); return 0;
}
