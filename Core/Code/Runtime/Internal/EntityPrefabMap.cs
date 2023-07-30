using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Core.Runtime;
using UnityEngine;

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal static class EntityPrefabMap {
        private static readonly Dictionary<string, int> s_keyToHash = new();
        private static readonly Dictionary<int, ContentRef<GameObject>> s_hashToPrefab = new();
        private static bool s_initialized;

        public static void EnsureInitialized() {
            if (s_initialized) return;
            if (Root.Singleton == null) return;
            foreach (var contentRef in Root.Singleton.Context.contentModule.GetAllContentRefsEnumerable()) {
                if (contentRef is not ContentRef<GameObject> objectRef) continue;
                if (!objectRef.IsAssetAssigned() || !objectRef.Asset.TryGetComponent(out SceneEntity sceneEntity)) continue;
                var hash = sceneEntity.PersistentKey.GetHashCode();
                s_keyToHash.Add(sceneEntity.PersistentKey, hash);
                s_hashToPrefab.Add(hash, objectRef);
            }
            s_initialized = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHash(in string key) => s_keyToHash[key];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ContentRef<GameObject> GetPrefab(int hash) => s_hashToPrefab[hash];
    }
}