using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal static class SaveTypeMap {
        private static readonly Dictionary<Type, int> s_typeToHash = new();
        private static readonly Dictionary<int, Type> s_hashToType = new();
        private static bool s_initialized;

        static SaveTypeMap() {
            foreach (var type in UFlowUtils.Reflection.GetAllInheritorsWithAttribute<IEcsComponent, EcsSerializableAttribute>()) {
                var hash = type.GetCustomAttribute<EcsSerializableAttribute>().persistentKey.GetHashCode();
                s_typeToHash[type] = hash;
                s_hashToType[hash] = type;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHash(in Type type) => s_typeToHash[type];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetType(int hash) => s_hashToType[hash];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetRegisteredTypesEnumerable() => s_typeToHash.Keys;
    }
}