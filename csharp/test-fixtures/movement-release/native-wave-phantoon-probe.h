#include "native-bounded-cpu.h"

// Real Phantoon definition, extended hitboxes and shot callback. Particle masks
// seed repeated contacts without manufacturing a complete boss-room playthrough.
int DiagnosticWavePhantoon(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int maps[]={0xdedd,0xdee7,0xdef1}, functions[]={0xd60d,0xd678};
  fprintf(f,"mask,map,function,pass,health,phase,timer,props,accum,reaction,count,types\n");
  for(int mask=0;mask<16;mask++) for(int m=0;m<3;m++) for(int fn=0;fn<2;fn++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1001; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    ProbeRunBounded(0x90ccc0);
    for(int i=0;i<4;i++) {
      projectile_x_pos[i]=projectile_y_pos[i]=(mask&(1<<i))?128:1024;
      projectile_index=i*2; ProbeRunBoundedRegisters(0x9381e9,0,i*2,0);
      EnemyData *part=gEnemyData(i*64); part->enemy_ptr=0xe4bf+i*64; part->bank=0xa7;
    }
    Enemy_Phantoon *body=Get_Phantoon(0), *tentacles=Get_Phantoon(128);
    body->base.x_pos=body->base.y_pos=128; body->base.health=2500;
    body->base.spritemap_pointer=maps[m]; body->base.extra_properties=4;
    body->phant_var_F=functions[fn]; body->phant_var_E=60;
    for(int pass=0;pass<4;pass++) {
      cur_enemy_index=0; ProbeRunBounded(0xa09b7f);
      // Only collision-marked particles run deletion here; nonhits stay at their
      // seeded contact positions. Natural movement/lifetime has its own oracle.
      for(int i=3;i>=0;i--) if(projectile_type[i] && (projectile_dir[i]&0xf0)) {
        projectile_index=i*2; ProbeRunBoundedRegisters(0x90da08,0,i*2,0);
      }
      fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X%04X%04X%04X\n",
        mask,maps[m],functions[fn],pass,body->base.health,body->phant_var_F,
        body->phant_var_E,body->base.properties,tentacles->phant_var_B,
        tentacles->phant_parameter_2,projectile_counter,
        projectile_type[0],projectile_type[1],projectile_type[2],projectile_type[3]);
    }
  }
  fclose(f); return 0;
}
