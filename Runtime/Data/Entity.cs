using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Acorn {

    [ExecuteInEditMode]
    public class Entity : MonoBehaviour {

        public static Entity[] GetRootEntities() {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .Select(_ => _.GetComponent<Entity>())
                .Where(_ => _ != null)
                .ToArray();
        }

        public static string SerializeRootEntities() {
            var kv = new KeyValueStore();
            foreach (var entity in GetRootEntities()) {
                kv[entity.uuid] = entity.type;
            }
            return kv.Stringify();
        }

        public static Entity[] DeserializeEntities(string data) {
            var kv = KeyValueStore.Parse(data);
            var entities = new List<Entity>();
            foreach (var e in kv) {
                var prefab = PrefabDatabase.Get().GetPrefab(e.Value);
                if (prefab == null) {
                    Debug.LogWarningFormat("Prefab {0} not found; we'll do our best, but the game is likely broken " +
                        "(I hate to scold you like this, but what were you thinking changing those prefab names like that? Huh?)",
                        e.Value);
                    continue;
                }
                var go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                var entity = go.GetComponent<Entity>();
                entity.uuid = e.Key;
                // TODO settle on load/save
                go.SendMessage("LoadState", entity.uuid);
                entities.Add(entity);
            }
            return entities.ToArray();
        }

        static Dictionary<string, GameObject> entitiesMap = new Dictionary<string, GameObject>();

        public string type = "";
        public string uuid = "";

        void OnDisable() {
            entitiesMap.Remove(uuid);
        }

        void Start() {
            Setup();
        }

        void Setup() {
            #if UNITY_EDITOR
            SetupPrefabId();
            #endif
            SetupEntityId();
            entitiesMap[uuid] = gameObject;
        }

        void SetupEntityId() {
            if (IsValidId()) {
                return;
            }
            // TODO add namespace support?
            uuid = System.Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(gameObject);
            #endif
        }

        bool IsValidId() {
            // Id must exist
            if (uuid == null || uuid == "") {
                return false;
            }
            // Id must be unique
            GameObject go = entitiesMap.Get(uuid);
            bool clash = go != null && go != gameObject;
            return !clash;
        }

        #if UNITY_EDITOR
        void SetupPrefabId() {
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this);
            if (prefab != null) {
                type = prefab.name;
                PrefabDatabase.Get();
            }
        }

        void OnValidate() {
            Setup();
        }
        #endif

    }

}
