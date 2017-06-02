static int menuItems = 5;
static int foreachTest[] = {1, 2, 5, 8, 10};

void foo () {
    foreach (item in foreachTest) {
        int b = item;
    }
}