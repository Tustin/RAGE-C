static string mainMenu[4] = {"Player", "Vehicle", "Weapons", "World"};

enum Menus {
    MAIN_MENU,
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
static int currentMenuHeight;
static int lastButtonPress = 0;
static int buttonPressDelay = 200; //I used this in The Tesseract

void draw_text(string text, int font, float x, float y, float size) {
    SET_TEXT_FONT(font);
    SET_TEXT_SCALE(size, size);
    SET_TEXT_COLOUR(255, 255, 255, 255);
    SET_TEXT_OUTLINE();
    BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
    ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text);
    END_TEXT_COMMAND_DISPLAY_TEXT(x, y);
}

void show_notification(string message) {
    BEGIN_TEXT_COMMAND_PRINT("STRING");
    ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(message);
    END_TEXT_COMMAND_PRINT(2000, 1);
}

void draw_menu_title(string title) {
    draw_text(title, 1, 0.839844, 0.191832, 1.05);
}

void draw_menu_option(string option, int index) {
    float temp = (float)index * 0.026094;
    draw_text(option, 1, 0.729657, 0.25647797 + temp, 0.437500);
}

void draw_background(int items) {
    float tmpHeight = (float)items * 0.026094
    tmpHeight = tmpHeight * 0.030943;
    DRAW_RECT(0.840576, 0.248187 + tmpHeight, 0.249766, tmpHeight, 0, 0, 0, 60); //background
}

//Draws menu and the items
void draw_menu() {
    draw_menu_title("Goy Menu");
    int menuItems = 0;
    switch (currentMenu) {
        case Menus.MAIN_MENU:
        for (int i = 0; i < 4; i++) {        
            draw_menu_option(mainMenu[i], i);
        }
        menuItems = 4;
        break;
        case Menus.PLAYER_MENU:
        menuItems = 2;
        break;
    }

    draw_background(menuItems);
}

//Returns whether a key has been pressed while accounting for the last button press delay
bool delayed_key_press(int control) {
    if (GET_GAME_TIMER() - lastButtonPress < buttonPressDelay) {
        return false;
    }
    
    if (IS_DISABLED_CONTROL_PRESSED(2, control)) {
        lastButtonPress = GET_GAME_TIMER();
        return true;
    }
    return false;
}

void disable_phone() {
    mainMenu[0] = "test";
    string item_1 = mainMenu[0];
    if (mainMenu[0] == "test") {

    }
    Global_87755.imm_16884.imm_2153[0][1].imm_42 = 5;
    // if (Global_10589 == 1) {
    //     Global_10589 = 0;
    //     bool testing = Global_10589;
    // }
}

//Deals with user input
void handle_input() {
    //Down
    if (delayed_key_press(203) == true) {
        show_notification("pressed down");
        currentOption++;
        switch (currentMenu) {
            case Menus.MAIN_MENU:
            if (currentOption == 3) {
                currentOption = 0;
            }
            break;
        }
    } else if (delayed_key_press(204) == true) {
        show_notification("pressed left");
    } else if (delayed_key_press(195) == true) {
        open = false;
        show_notification("closed");

    }
}

void main() {
    while (true) 
    {
        if (!open) {
            if (IS_DISABLED_CONTROL_PRESSED(2, 0xcc) && IS_DISABLED_CONTROL_PRESSED(2, 0xc9)) {
                open = true;
                currentMenu = 0;
                currentOption = 0;
                lastButtonPress = GET_GAME_TIMER();
                string first = "menu ";
                first . "opened";
                show_notification(first);   
            }
        } else {
            draw_menu();
            handle_input();
        }
        wait(0);
    }
}
