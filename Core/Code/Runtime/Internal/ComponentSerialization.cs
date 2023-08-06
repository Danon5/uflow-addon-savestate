using System;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Addon.Serialization.Core.Runtime;

// ReSharper disable StaticMemberInGenericType

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal static class ComponentSerialization<TAttribute, TComponent>
        where TAttribute : Attribute
        where TComponent : IEcsComponent {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(in ByteBuffer buffer, ref TComponent obj) {
            foreach (var serializer in FieldSerializerCache<TComponent>.AsEnumerableWithAttribute<TAttribute>())
                serializer.Serialize(buffer, ref obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(in ByteBuffer buffer, ref TComponent obj) {
            foreach (var serializer in FieldSerializerCache<TComponent>.AsEnumerableWithAttribute<TAttribute>())
                serializer.Deserialize(buffer, ref obj);
        }
    }
}