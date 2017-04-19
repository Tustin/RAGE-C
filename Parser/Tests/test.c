static string arrTest[5] = {"test", "aaa", "something1", "something2", "lastone"};
int intArr[3];

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
                show_notification("menu opened");   
            }
        } else {
            draw_menu();
            handle_input();
        }
        wait(0);
    }
}
