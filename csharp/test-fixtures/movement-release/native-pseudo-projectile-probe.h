#include "native-bounded-cpu.h"

// #421: original projectile/Samus collision pass, independently of enemy touch.
int DiagnosticPseudoProjectile(const char *rom,const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status)return status;
  FILE *f=fopen(output,"wx");if(!f)return 4;
  uint8 *definition=(uint8*)RomFixedPtr(0x86f000), saved[14]; memcpy(saved,definition,14);
  memset(definition,0,14);definition[10]=0;definition[11]=0xf1;
  fprintf(f,"contact,inv,persist,disabled,offset,radius,health,timer,knock,flare,id,list,listTimer\n");
  const uint16 modes[]={0,3,4}, radii[]={0x404,0,0x400,4};
  for(int m=0;m<3;m++)for(int inv=0;inv<2;inv++)for(int persist=0;persist<2;persist++)
  for(int disabled=0;disabled<2;disabled++)for(int offset=-9;offset<=9;offset+=9)for(int r=0;r<4;r++) {
    cpu_reset(g_snes->cpu);memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false;g_snes->cpu->sp=0x1ff0;g_snes->cpu->dp=0;
    samus_contact_damage_index=modes[m];samus_invincibility_timer=inv ? 9 : 0;
    samus_health=99;samus_x_pos=samus_y_pos=128;samus_x_radius=5;samus_y_radius=12;flare_counter=120;
    eproj_id[17]=0xf000;eproj_x_pos[17]=128+offset;eproj_y_pos[17]=128;eproj_radius[17]=radii[r];
    eproj_properties[17]=40|(persist ? 0x4000:0)|(disabled ? 0x2000:0);
    eproj_instr_list_ptr[17]=0xf200;eproj_instr_timers[17]=7;
    ProbeRunBounded(0xa09894);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
      modes[m],inv,persist,disabled,offset,radii[r],samus_health,samus_invincibility_timer,samus_knockback_timer,
      flare_counter,eproj_id[17],eproj_id[17]?eproj_instr_list_ptr[17]:0,eproj_id[17]?eproj_instr_timers[17]:0);
  }
  memcpy(definition,saved,14);fclose(f);return 0;
}
