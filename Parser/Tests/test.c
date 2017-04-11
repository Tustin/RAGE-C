enum MainMenu {
    PLAYER_MENU,
    VEHICLE_MENU,
    WEAPON_MENU,
    WORLD_MENU,
};

static int id = PLAYER_PED_ID();

float testing(int something2, string theliberator) {
    int something = 0;
    int nothing;
    int someArray[5];
    someArray[4] = 105;
    for (int i = 0; i < 5; i++) {
        someArray[i] = i;
    }
}

void main() {
    //disable phone
    Global_10589[5] = 0;
    testing(1, "hi");
    while (true) {
        if (IS_PED_IN_ANY_VEHICLE(PLAYER_PED_ID(), 0)) {
            int vehicle = GET_VEHICLE_PED_IS_IN(PLAYER_PED_ID(), 0);
            SET_VEHICLE_MOD_KIT(vehicle, 0);
            SET_VEHICLE_COLOURS(vehicle, GET_RANDOM_INT_IN_RANGE(0, 255), GET_RANDOM_INT_IN_RANGE(0, 255));
            SET_VEHICLE_NUMBER_PLATE_TEXT(vehicle, "RAGE-C");
        }
        WAIT(0);
    }
}

