// static int something = 5;
// static int something2 = 10;

// static int someArray[] = {1, 2};

//#include "a";

struct myStruct {
    int item = 5,
    bool shit
};

static @myStruct structDecl;

void main() {
    structDecl->item = 6;
    int item = structDecl->item;
    int hash = $"test";
}