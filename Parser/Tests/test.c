// static int something = 5;
// static int something2 = 10;

// static int someArray[] = {1, 2};

//#include "a";

// struct myStruct {
//     int item = 5,
//     bool shit
// };

// static @myStruct structDecl;

void test(string name) {
    while(!HAS_SCRIPT_LOADED(name)) {
        REQUEST_SCRIPT(name);
        WAIT(0);
    }
}
void main() {
    test("test");
}