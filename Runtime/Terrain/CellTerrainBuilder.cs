using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    public class CellTerrainBuilder {

        private CellTerrainSettings settings;
        private CellTerrainBuffer buffer;

        private Mesh mainMesh;
        private Mesh collisionMesh;
        private MeshBuffer mainBuf;
        private MeshBuffer collisionBuf;

        public CellTerrainBuilder(
            CellTerrainBuffer buffer,
            CellTerrainSettings settings
        ) {
            this.buffer = buffer;
            this.settings = settings;
        }

        public Mesh GetMainMesh() {
            return mainMesh;
        }

        public Mesh GetCollisionMesh() {
            return collisionMesh;
        }

        public void GenerateMeshes() {
            this.mainBuf = new MeshBuffer();
            this.collisionBuf = new MeshBuffer();
            for (int y = 0; y < buffer.size; y++) {
                for (int x = 0; x < buffer.size; x++) {
                    var data = buffer.Get(x, y);
                    if ((data & 0b0011) == 0) {
                        continue;
                    }
                    var tile = CalcTileInfo(x, y, data);
                    DrawTile(tile);
                }
            }
            AssembleMainMesh();
            AssembleCollisionMesh();
        }

        void DrawTile(Tile tile) {
            DrawCap(tile);
            foreach (int i in tile.vacantDirs) {
                DrawLateralFaces(tile, i);
            }
        }

        void DrawCap(Tile tile) {
            if (settings.cullInsideCaps) {
                var culled = tile.hex.Neighbours().All(n => n.Neighbours().All(nn => ContainsCell(nn)));
                if (culled) {
                    return;
                }
            }
            var tileVerts = tile.cap.Select(v => CalcVertex(v, 0)).ToArray();
            var tileUvs = tile.cap.Select(v => Vector2.Scale(v, settings.uvScale)).ToArray();
            foreach (var tri in TILE_TRIS) {
                mainBuf.AddTriange(
                    mainBuf.AddVertex(tileVerts[tri[0]], tileUvs[tri[0]], tile.color),
                    mainBuf.AddVertex(tileVerts[tri[1]], tileUvs[tri[1]], tile.color),
                    mainBuf.AddVertex(tileVerts[tri[2]], tileUvs[tri[2]], tile.color));
            }
        }

        void DrawLateralFaces(Tile tile, int edgeDir) {
            for (int i = 1; i <= settings.lateralLoops; i++) {
                float t0 = (float)(i - 1) / settings.lateralLoops;
                float t1 = (float)i / settings.lateralLoops;
                var frontA = CalcLateralVertex(tile, edgeDir, true, t0);
                var frontB = CalcLateralVertex(tile, edgeDir, false, t0);
                var backA = CalcLateralVertex(tile, edgeDir, true, t1);
                var backB = CalcLateralVertex(tile, edgeDir, false, t1);
                var uvOffset = EDGE_UV_OFFSETS[edgeDir];
                var uvFrontA = Vector2.Scale(frontA.xy() + uvOffset * settings.uvBackOffset * t0 * t0, settings.uvScale);
                var uvFrontB = Vector2.Scale(frontB.xy() + uvOffset * settings.uvBackOffset * t0 * t0, settings.uvScale);
                var uvBackA = Vector2.Scale(backA.xy() + uvOffset * settings.uvBackOffset * t1 * t1, settings.uvScale);
                var uvBackB = Vector2.Scale(backB.xy() + uvOffset * settings.uvBackOffset * t1 * t1, settings.uvScale);
                mainBuf.AddTriange(
                    mainBuf.AddVertex(frontA, uvFrontA, tile.color),
                    mainBuf.AddVertex(frontB, uvFrontB, tile.color),
                    mainBuf.AddVertex(backB, uvBackB, tile.color));
                mainBuf.AddTriange(
                    mainBuf.AddVertex(frontA, uvFrontA, tile.color),
                    mainBuf.AddVertex(backB, uvFrontB, tile.color),
                    mainBuf.AddVertex(backA, uvBackA, tile.color));
            }
            if (settings.generateCollider) {
                var fA = CalcLateralVertex(tile, edgeDir, true, settings.colliderMidpoint).WithZ(0);
                var fB = CalcLateralVertex(tile, edgeDir, false, settings.colliderMidpoint).WithZ(0);
                var bA = CalcLateralVertex(tile, edgeDir, true, settings.colliderMidpoint).WithZ(+settings.depth);
                var bB = CalcLateralVertex(tile, edgeDir, false, settings.colliderMidpoint).WithZ(+settings.depth);
                collisionBuf.AddTriange(
                    collisionBuf.AddVertex(fA),
                    collisionBuf.AddVertex(fB),
                    collisionBuf.AddVertex(bB));
                collisionBuf.AddTriange(
                    collisionBuf.AddVertex(fA),
                    collisionBuf.AddVertex(bB),
                    collisionBuf.AddVertex(bA));
            }
        }

        Vector3 CalcLateralVertex(Tile tile, int edgeDir, bool isBefore, float t) {
            var vertexIndex = isBefore ? (edgeDir + 5) % 6 : edgeDir;
            float z = Mathf.Lerp(0, settings.depth, t);
            var basis = tile.cap[vertexIndex];
            var inflation = CalcInflateRatio(t) * settings.edgeInflate;
            bool isFree = tile.freeVerts.Contains(vertexIndex);
            // Calc the index of vertex which defines the direction of inflation,
            // based on whether the vertex is free or shared between edges of two tiles
            var inflateVertexIndex = isFree ? vertexIndex :
                isBefore ? (vertexIndex + 1) % 6 :
                    (vertexIndex + 5) % 6;
            var offset = TILE_VERTICES[inflateVertexIndex] * settings.radius * inflation;
            return CalcVertex(basis + offset, z);
        }

        Vector3 CalcVertex(Vector2 xy, float depth) {
            var xyz = new Vector3(xy.x, xy.y, depth);
            var noise = SampleNoise(xyz) * 2f - Vector3.one;
            return xyz + Vector3.Scale(noise, settings.displacement);
        }

        Vector3 SampleNoise(Vector3 point) {
            var coords = Vector3.Scale(point, settings.noiseScale);
            var noise = new Vector3(
                Utilx.PerlinNoise3D(coords + 100 * Vector3.right),
                Utilx.PerlinNoise3D(coords + 100 * Vector3.up),
                Utilx.PerlinNoise3D(coords + 100 * Vector3.forward));
            return noise;
        }

        Tile CalcTileInfo(int col, int row, byte data) {
            var color = GetColor(data);
            var hex = Hex.FromOffset(col, row);
            var center = hex.ToPlanar(settings.radius);
            var vacantDirs = GetVacantDirs(hex).ToArray();
            var freeVerts = CalcFreeVertices(vacantDirs).ToArray();
            var cap = TILE_VERTICES
                .Select((v, i) => {
                    var vert = freeVerts.Contains(i) ? v * settings.edgeVertsInset : v;
                    return vert * settings.radius + center;
                })
                .ToArray();
            return new Tile {
                col = col,
                row = row,
                data = data,
                hex = hex,
                color = color,
                center = center,
                cap = cap,
                vacantDirs = vacantDirs,
                freeVerts = freeVerts,
            };
        }

        // Calc which of the 6 vertices are inset towards the center based on
        // the adjacent vacant cells (if adjacent both adjacent dirs around the vertex are vacant,
        // then the vertex is free and can be inset)
        IEnumerable<int> CalcFreeVertices(IEnumerable<int> freeDirs) {
            for (int i = 0; i <= 5; i++) {
                var j = (i + 1) % 6;
                if (freeDirs.Contains(i) && freeDirs.Contains(j)) {
                    yield return i;
                }
            }
        }

        // Given t, returns inflation ratio (0 to 1), based on y = -4x^2 + 4x
        // which goes through (0, 0) and (1, 0) and has a peak at (.5, 1)
        float CalcInflateRatio(float t) {
            return -4 * t * t + 4 * t;
        }

        void AssembleMainMesh() {
            this.mainMesh = new Mesh();
            this.mainMesh.vertices = this.mainBuf.vertices.ToArray();
            this.mainMesh.uv = this.mainBuf.uvs.ToArray();
            this.mainMesh.colors = this.mainBuf.colors.ToArray();
            this.mainMesh.triangles = this.mainBuf.tris.ToArray();
            this.mainMesh.RecalculateBounds();
            this.mainMesh.RecalculateNormals();
            this.mainMesh.RecalculateTangents();
        }

        void AssembleCollisionMesh() {
            if (settings.generateCollider) {
                this.collisionMesh = new Mesh();
                this.collisionMesh.vertices = this.collisionBuf.vertices.ToArray();
                this.collisionMesh.triangles = this.collisionBuf.tris.ToArray();
                this.collisionMesh.RecalculateBounds();
            } else {
                this.collisionMesh = null;
            }
        }

        bool ContainsCell(Hex hex) {
            var offset = hex.ToOffset();
            var data = buffer.Get(offset.x, offset.y);
            // TODO refine this
            return (data & 0b0011) != 0;
        }

        Color GetColor(byte data) {
            var b = data & 0b0011;
            switch (b) {
                case 1: return Color.red;
                case 2: return Color.green;
                case 3: return Color.blue;
                default: return Color.black;
            }
        }

        IEnumerable<int> GetVacantDirs(Hex hex) {
            return Hex.DIRECTIONS.Where(dir => !ContainsCell(hex.GetNeighbour(dir)));
        }

        static Vector2[] TILE_VERTICES = new Vector2[] {
            new Vector2(1f, Mathf.Sqrt(3) / 3),
            new Vector2(0, 2 * Mathf.Sqrt(3) / 3),
            new Vector2(-1f, Mathf.Sqrt(3) / 3),
            new Vector2(-1f, -Mathf.Sqrt(3) / 3),
            new Vector2(0, -2 * Mathf.Sqrt(3) / 3),
            new Vector2(1f, -Mathf.Sqrt(3) / 3),
            new Vector3(0, 0),
        };

        static int[][] TILE_TRIS = new int[][] {
            new int[] { 6, 1, 0 },
            new int[] { 6, 2, 1 },
            new int[] { 6, 3, 2 },
            new int[] { 6, 4, 3 },
            new int[] { 6, 5, 4 },
            new int[] { 6, 0, 5 },
        };

        static Vector2[] EDGE_UV_OFFSETS = new Vector2[] {
            new Vector2(2f, 0),
            new Vector2(1f, Mathf.Sqrt(3)),
            new Vector2(-1f, Mathf.Sqrt(3)),
            new Vector2(-2f, 0),
            new Vector2(-1f, -Mathf.Sqrt(3)),
            new Vector2(1f, -Mathf.Sqrt(3)),
        };

        struct Tile {
            public int col;
            public int row;
            public byte data;
            public Hex hex;
            public Vector2 center;
            public Color color;

            public Vector2[] cap;
            public int[] vacantDirs;
            public int[] freeVerts;
        }

    }

}
