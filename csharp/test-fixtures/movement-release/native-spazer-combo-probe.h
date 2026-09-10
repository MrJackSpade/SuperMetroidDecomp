#include "native-bounded-cpu.h"
int DiagnosticSpazerCombo(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"left,hit,frame,count,cooldown,flare,sounds,slots\n");
  for(int left=0;left<2;left++) for(int hit=-1;hit<=60;hit=hit<0?10:hit==10?60:61) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1004; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=left?4:8;
    ProbeRunBounded(0x90ccc0);
    for(int frame=0;frame<200;frame++) {
      samus_x_pos=128+(left?-1:1)*(frame%9); samus_y_pos=128+left*64+(frame/7)%9;
      flare_counter=77; cooldown_timer=0;
      sfx_readpos[0]=sfx_writepos[0]=0; memset(sfx1_queue,0,16);
      if(frame==hit) projectile_dir[0]|=0x10;
      for(int i=3;i>=0;i--) if(projectile_bomb_instruction_ptr[i]) {
        projectile_index=i*2;
        ProbeRunBoundedRegisters(0x900000|projectile_bomb_pre_instructions[i],0,i*2,0);
        if(projectile_bomb_instruction_ptr[i]) ProbeRunBounded(0x9381e9);
      }
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,",left,hit,frame,projectile_counter,cooldown_timer,flare_counter);
      for(int i=0;i<sfx_writepos[0];i++) fprintf(f,"%02X",sfx1_queue[i]);
      fprintf(f,",");
      for(int i=0;i<4;i++) {
        if(!projectile_bomb_instruction_ptr[i]) fprintf(f,"0");
        else fprintf(f,"%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X",projectile_x_pos[i],projectile_y_pos[i],projectile_bomb_pre_instructions[i],
          projectile_variables[i],projectile_bomb_x_speed[i],projectile_bomb_y_speed[i],projectile_timers[i],projectile_bomb_x_subpos[i],projectile_bomb_y_subpos[i],projectile_unk_A[i],projectile_type[i],projectile_bomb_instruction_ptr[i],projectile_damage[i]);
        fprintf(f,i==3?"\n":"/");
      }
    }
  }
  fclose(f); return 0;
}
