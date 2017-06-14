// static int something = 5;
// static int something2 = 10;

// static int someArray[] = {1, 2};

//#include "a";

// struct myStruct {
//     int item = 5,
//     bool shit
// };

// static @myStruct structDecl;

void handle_input() {
    if (delayed_key_press(Buttons.Dpad_Down) == true) {
        show_notification("pressed down");
        currentOption++;
        if (currentOption > SCRIPT_COUNT) {
            currentOption = 0;
        }
    } else if (delayed_key_press(Buttons.Dpad_Up) == true) {
        show_notification("pressed up");
        currentOption--;
        if (currentOption < 0) {
            currentOption = SCRIPT_COUNT;
        }
    } else if (delayed_key_press(Buttons.Button_Cross) == true) {
        show_notification("pressed x");
    } else if (delayed_key_press(Buttons.Button_Circle) == true) {
        open = false;
        show_notification("closed");
    }
}

void main() {
    test("test");
}