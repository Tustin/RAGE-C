int items = 50;
int id = PLAYER_PED_ID();

void main() {
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

float testing() {
    int something = 0;
    switch (IS_CONTROL_JUST_PRESSED(2, 0xcc)) {
        case 0:
        something = 0;
        break;
        case 1:
        something = 1;
        break;
    }
}