using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Addon.Serialization.Core.Runtime;
using UFlow.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal static class SaveTypeMap {
        private static readonly Dictionary<Type, ulong> s_typeToHash = new();
        private static readonly Dictionary<ulong, Type> s_hashToType = new();

        static SaveTypeMap() {
            foreach (var (type, att) in UFlowUtils.Reflection.GetAllInheritorsWithAttribute<IEcsComponent, EcsSerializableAttribute>()) {
                var hash = SerializationAPI.CalculateHash(att.persistentKey);
                s_typeToHash[type] = hash;
                s_hashToType[hash] = type;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetHash(in Type type) => s_typeToHash[type];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetType(ulong hash) => s_hashToType[hash];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetRegisteredTypesEnumerable() => s_typeToHash.Keys;
    }
}