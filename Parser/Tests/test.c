enum MainMenu {
    PLAYER_MENU,
    VEHICLE_MENU,
    WEAPON_MENU,
    WORLD_MENU,
};

static int id = PLAYER_PED_ID();
static int currentMenu = 0;
static bool open = false;

void main() {
    int someHack;
    switch (currentMenu) {
        case MainMenu.PLAYER_MENU:
        someHack = 1;
        break;
        case MainMenu.VEHICLE_MENU:
        someHack = 2;
        break;
        //etc...
    }
}
