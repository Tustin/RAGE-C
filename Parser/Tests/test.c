enum MainMenu {
    PLAYER_MENU,
    VEHICLE_MENU,
    WEAPON_MENU,
    WORLD_MENU,
};

enum PlayerMenu {
    GODMODE,
    INFINITE_AMMO,
};

static int currentMenu = 0;
static int currentOption = 0;
static bool open = false;

void draw_text(string text, int font, float x, float y, float size) {
    SET_TEXT_FONT(font);
    SET_TEXT_SCALE(size, size);
    SET_TEXT_COLOUR(255, 255, 255, 255);
    SET_TEXT_OUTLINE();
    BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
    ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text);
    END_TEXT_COMMAND_DISPLAY_TEXT(x, y);
}

void draw_background() {

}
void draw_menu() {
    int items = 5;
    int height = (float)22;
    draw_text("Goy Menu", 1, 0.839844, 0.191832, 1.05);
    if (currentMenu == MainMenu.PLAYER_MENU) {
        switch (currentOption) {
            case PlayerMenu.GODMODE:

            break;
            case PlayerMenu.INFINITE_AMMO:

            break;
        }
    }
}


void main() {
    while (true){
        draw_menu();

        wait(0);
    }
}
