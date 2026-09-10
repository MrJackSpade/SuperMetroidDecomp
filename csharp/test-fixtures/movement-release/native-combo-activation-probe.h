#include "native-bounded-cpu.h"

// #416: FireSBA handler boundary, including ammo debit before family rejection.
// Charge timing and input eligibility are deliberately separate from this oracle.
int DiagnosticComboActivation(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"beam,ammo,selected,left,existing,pb,hud,auto,count,cooldown,sba,carry,slots\n");
  for(int beam=0;beam<12;beam++) for(int ammo=0;ammo<3;ammo++)
  for(int selected=0;selected<2;selected++) for(int left=0;left<2;left++)
  for(int existing=0;existing<3;existing++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1000|beam; samus_power_bombs=ammo;
    hud_item_index=selected?3:0; samus_auto_cancel_hud_item_index=3;
    samus_x_pos=512; samus_y_pos=384; samus_pose_x_dir=left?4:8;
    projectile_bomb_pre_instructions[0]=existing==1?FUNC16(ProjPreInstr_IceSba):
      existing==2?FUNC16(ProjPreInstr_PlasmaSba):0;
    ProbeRunBounded(0x90ccc0);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%d,",
      beam,ammo,selected,left,existing,samus_power_bombs,hud_item_index,
      samus_auto_cancel_hud_item_index,projectile_counter,cooldown_timer,used_for_sba_attacksB60,g_snes->cpu->c);
    for(int i=0;i<4;i++) fprintf(f,"%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%04X%s",
      projectile_type[i],projectile_dir[i],projectile_bomb_pre_instructions[i],projectile_x_pos[i],projectile_y_pos[i],
      projectile_bomb_x_speed[i],projectile_bomb_y_speed[i],projectile_variables[i],projectile_timers[i],
      projectile_damage[i],projectile_x_radius[i],projectile_y_radius[i],projectile_bomb_instruction_ptr[i],projectile_bomb_instruction_timers[i],i==3?"":"/");
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
