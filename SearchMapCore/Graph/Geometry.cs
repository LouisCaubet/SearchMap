using System;

namespace SearchMapCore.Graph {

    public class Location {

        public int x { get; set; }
        public int y { get; set; }

        public Location(Location toCopy) {
            x = toCopy.x;
            y = toCopy.y;
        }

        public Location(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Location() { }

        public double DistanceSquared(Location to) {
            return Math.Pow(x - to.x, 2) + Math.Pow(y - to.y, 2);
        }

        public double Distance(Location to) {
            return Math.Sqrt(DistanceSquared(to));
        }

        public void Translation(Vector v) {
            x += v.x;
            y += v.y;
        }

    }   

    public class Vector {

        public int x { get; set; }
        public int y { get; set; }

        public Vector(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Vector() : this(0, 0) { }

        public Vector(Location from, Location to) : 
            this(to.x - from.x, to.y - from.y) { }

        public void Scale(double scale) {

            x = (int)Math.Round(x * scale);
            y = (int)Math.Round(y * scale);

        }

        public double GetNorm() {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

    }

}
