using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "PrefabDatabase", menuName = "System / PrefabDatabase")]
    public class PrefabDatabase : ScriptableObject {

        private static PrefabDatabase instance;

        public static PrefabDatabase Get() {
            if (instance == null) {
                instance = Resources.Load<PrefabDatabase>("System/PrefabDatabase");
                instance.Init();
            }
            return instance;
        }

        public GameObject[] prefabs;

        public void Init() {
            #if UNITY_EDITOR
            LoadPrefabsFromAssets();
            #endif
            Debug.Log("Initialized Prefab Database");
        }

        public GameObject GetPrefab(string prefabName) {
            // TODO replace with a map
            return prefabs.FirstOrDefault(_ => _.name == prefabName);
        }

        #if UNITY_EDITOR
        public void LoadPrefabsFromAssets() {
            prefabs = UnityEditor.AssetDatabase.FindAssets("", new string[] { "Assets/Prefabs" })
                .Select(guid => UnityEditor.AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path))
                .Where(_ => _ != null)
                .OrderBy(_ => _.name)
                .ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif

    }

}
