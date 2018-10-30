namespace ABObjects {

    public class ABTile {
        public int x;
        public int y;
        public char value;

        public ABTile() {
        }

        public ABTile(int x, int y, char value) {
            this.x = x;
            this.y = y;
            this.value = value;
        }
    }

    // Stores all information about an All Black room.
    public class ABRoom {
        public int originX;
        public int originY;
        public int dimension;

        public ABRoom() {
        }

        public ABRoom(int x, int y, int d) {
            originX = x;
            originY = y;
            dimension = d;
        }
    }

}