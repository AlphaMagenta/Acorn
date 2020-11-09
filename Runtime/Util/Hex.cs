using System.Collections.Generic;
using UnityEngine;

namespace Acorn {

    [System.Serializable]
    public struct Hex {

        public int q;
        public int r;

        public Hex(float q, float r) :
            this(Mathf.RoundToInt(q), Mathf.RoundToInt(r)) {}

        public Hex(int q, int r) {
            this.q = q;
            this.r = r;
        }

        public Vector2 ToPlanar(float radius = .5f) {
            return (Q_BASIS * q + R_BASIS * r) * radius;
        }

        public static Hex FromPlanar(float x, float y, float radius = .5f) {
            return FromPlanar(new Vector2(x, y), radius);
        }

        public static Hex FromPlanar(Vector2 planar, float radius = .5f) {
            float q = Vector2.Dot(planar, Q_INV) / radius;
            float r = Vector2.Dot(planar, R_INV) / radius;
            return new Hex(q, r);
        }

        public Vector2Int ToOffset() {
            return new Vector2Int(q + (r - (r & 1)) / 2, r);
        }

        public static Hex FromOffset(int col, int row) {
            return new Hex(col - (row - (row & 1)) / 2, row);
        }

        public IEnumerable<Hex> Neighbours() {
            foreach (Hex dir in AXIAL_DIRECTIONS) {
                yield return this + dir;
            }
        }

        public int DistanceTo(Hex other) {
            var s = -q - r;
            var otherS = -other.q - other.r;
            return (Mathf.Abs(q - other.q) + Mathf.Abs(r - other.r) + Mathf.Abs(s - otherS)) / 2;
        }

        public Hex GetNeighbour(int dir, int steps = 1) {
            Hex incr = AXIAL_DIRECTIONS[dir % AXIAL_DIRECTIONS.Length] * steps;
            return this + incr;
        }

        public override bool Equals(System.Object obj) {
            Hex hex = (Hex)obj;
            return (q == hex.q) && (r == hex.r);
        }

        public override int GetHashCode() {
            return q * 37 + r * 31;
        }

        public override string ToString() {
            return "(" + q + ";" + r + ")";
        }

        public static Vector2 Q_BASIS = new Vector2(2f, 0);
        public static Vector2 R_BASIS = new Vector2(1f, Mathf.Sqrt(3));
        public static Vector2 Q_INV = new Vector2(1f / 2, - Mathf.Sqrt(3) / 6);
        public static Vector2 R_INV = new Vector2(0, Mathf.Sqrt(3) / 3);

        public static Hex zero = new Hex(0, 0);

        public static Hex operator +(Hex a, Hex b) { return new Hex(a.q + b.q, a.r + b.r); }
        public static Hex operator -(Hex a, Hex b) { return new Hex(a.q - b.q, a.r - b.r); }
        public static Hex operator *(Hex a, int i) { return new Hex(a.q * i, a.r * i); }

        public static int[] DIRECTIONS = new int[] { 0, 1, 2, 3, 4, 5 };

        public static Hex[] AXIAL_DIRECTIONS = new Hex[] {
            new Hex(1, 0),
            new Hex(0, 1),
            new Hex(-1, 1),
            new Hex(-1, 0),
            new Hex(0, -1),
            new Hex(1, -1),
        };

        public static IEnumerable<Hex> Ring(Hex center, int radius) {
            Hex current = center + new Hex(0, -radius);
            foreach (Hex dir in AXIAL_DIRECTIONS) {
                for (int i = 0; i < radius; i++) {
                    yield return current;
                    current = current + dir;
                }
            }
        }

        public static IEnumerable<Hex> Spiral(Hex center, int minRadius, int maxRadius) {
            if (minRadius == 0) {
                yield return center;
                minRadius += 1;
            }
            for (int r = minRadius; r <= maxRadius; r++) {
                var ring = Ring(center, r);
                foreach (Hex hex in ring) {
                    yield return hex;
                }
            }
        }

        public static IEnumerable<Hex> Line(Hex from, Hex to) {
            var distance = from.DistanceTo(to);
            var a = from.ToPlanar();
            var b = to.ToPlanar();
            for (int i = 0; i <= distance; i++) {
                var t = (float)i / distance;
                var p = Vector2.Lerp(a, b, t);
                yield return Hex.FromPlanar(p);
            }
        }

    }

}
