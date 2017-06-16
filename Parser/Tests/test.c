// static int something = 5;
// static int something2 = 10;

// static int someArray[] = {1, 2};

//#include "a";

// struct myStruct {
//     int item = 5,
//     bool shit
// };

// static @myStruct structDecl;

static int FXDelay01 = 200;
void main() {
    if (FXDelay01 < GET_GAME_TIMER())
    {
        HAS_NAMED_PTFX_ASSET_LOADED("scr_oddjobtraffickingair");
        _USE_PARTICLE_FX_ASSET_NEXT_CALL("scr_oddjobtraffickingair");
        float TrailFX21 = _START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE_2("scr_drug_traffic_flare_L", PLAYER_PED_ID(), 0, 0, 0, 0, 0, 0, 0xfe2c, 0.7, false, false, false);
        float TrailFX21 = _START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE_2("scr_drug_traffic_flare_L", PLAYER_PED_ID(), 0, 0, 0, 0, 0, 0, 0x8cbd, 0.7, false, false, false);
        float TrailFX21 = _START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE_2("scr_drug_traffic_flare_L", PLAYER_PED_ID(), 0, 0, 0, 0, 0, 0, 0x3779, 0.7, false, false, false);
        float TrailFX21 = _START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE_2("scr_drug_traffic_flare_L", PLAYER_PED_ID(), 0, 0, 0, 0, 0, 0, 0x188e, 0.7, false, false, false);
        float TrailFX21 = _START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE_2("scr_drug_traffic_flare_L", PLAYER_PED_ID(), 0, 0, 0, 0, 0, 0, 0x2e28, 0.7, false, false, false);
        SET_PARTICLE_FX_LOOPED_COLOUR(TrailFX21, 255, 255, 255, 0);
        FXDelay01 = GET_GAME_TIMER() + 150;
    }
}