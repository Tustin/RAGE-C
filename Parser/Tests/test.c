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