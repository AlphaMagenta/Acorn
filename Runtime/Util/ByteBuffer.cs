using UnityEngine;
using System.Collections.Generic;

namespace Acorn {

    [System.Serializable]
    [PreferBinarySerialization]
    public class ByteBuffer {

        [SerializeField]
        [HideInInspector]
        private byte[] _buffer;

        public int size = 32;

        public byte[] buffer {
            get {
                if (_buffer == null || _buffer.Length != size * size) {
                    Clear();
                }
                return _buffer;
            }
        }

        public byte Get(Vector2Int xy) {
            return Get(xy.x, xy.y);
        }

        public byte Get(int x, int y) {
            if (x < 0 || y < 0 || x >= size || y >= size) {
                return 0;
            }
            return buffer[x + y * size];
        }

        public void Set(int x, int y, byte data) {
            if (x < 0 || y < 0 || x >= size || y >= size) {
                return;
            }
            buffer[x + y * size] = data;
        }

        public void Set(Vector2Int xy, byte data) {
            Set(xy.x, xy.y, data);
        }

        public bool IsWithinBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < size && y < size;
        }

        public void Clear() {
            _buffer = new byte[size * size];
        }

        // Traverse in row-major order
        public IEnumerable<Vector2Int> Traverse() {
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    yield return new Vector2Int(x, y);
                }
            }
        }

    }

}
