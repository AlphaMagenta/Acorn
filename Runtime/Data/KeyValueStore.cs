using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace Acorn {

    public class KeyValueStore : Dictionary<string, string> {

        public string Stringify(int estimatedEntrySize = 64) {
            var sb = new StringBuilder(estimatedEntrySize * Count);
            foreach (var e in this) {
                sb.Append(e.Key);
                sb.Append("=");
                sb.Append(e.Value);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public static KeyValueStore Parse(string data) {
            var store = new KeyValueStore();
            using (var reader = new StringReader(data)) {
                while (true) {
                    var line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    var i = line.IndexOf("=");
                    if (i == -1) {
                        // It's a malformed entry, let's skip it
                        continue;
                    }
                    var key = line.Substring(0, i);
                    var value = line.Substring(i + 1);
                    store[key] = value;
                }
            }
            return store;
        }

    }
}
