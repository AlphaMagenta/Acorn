using UnityEngine;

namespace Acorn {

    [System.Serializable]
    [PreferBinarySerialization]
    public class CellTerrainBuffer {

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

        public byte Get(Vector2Int coords) {
            return Get(coords.x, coords.y);
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

        public bool Contains(int x, int y) {
            return x >= 0 && y >= 0 && x < size && y < size;
        }

        public void Clear() {
            _buffer = new byte[size * size];
        }

    }

}
