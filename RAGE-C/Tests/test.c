void inVehicle() {
    if (is_ped_in_any_vehicle(player_ped_id(), 0)) {
        int vehicle = get_vehicle_ped_is_in(player_ped_id(), 0);
        if (get_entity_model(vehicle) == 0xB779A091) {
            set_entity_as_mission_entity(vehicle, 0, 1);
            delete_vehicle(&vehicle);
        }
    }
}
void main() {

}