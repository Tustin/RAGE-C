static int menuItems = 5;
static int foreachTest[] = {1, 2, 5, 8, 10};

int foo () {

    for (int i = 0; i < 5; i++) {
        int b = i;
    }

    foreach (item in foreachTest) {
        int a = item;
    }
    int testing = 1;
    return menuItems;
}