using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Addon.Serialization.Core.Runtime;
using UFlow.Core.Runtime;

// ReSharper disable StaticMemberInGenericType

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal static class ComponentSerialization<TAttribute, TComponent>
        where TAttribute : Attribute
        where TComponent : IEcsComponent {
        private static readonly List<ISerializer<TComponent>> s_fieldSerializers = new();
        private static readonly object[] s_objectBuffer = new object[1];
        
        static ComponentSerialization() {
            var objectType = typeof(TComponent);
            var unmanagedSerializerType = typeof(UnmanagedFieldSerializer<,>);
            var arraySerializerType = typeof(UnmanagedArrayFieldSerializer<,>);
            foreach (var field in UFlowUtils.Reflection.GetAllFieldsInTypeWithAttribute<TAttribute>(objectType)) {
                var genericType = field.FieldType.IsArray ? 
                    arraySerializerType.MakeGenericType(objectType, field.FieldType.GetElementType()) : 
                    unmanagedSerializerType.MakeGenericType(objectType, field.FieldType);
                s_objectBuffer[0] = field;
                s_fieldSerializers.Add(Activator.CreateInstance(genericType, s_objectBuffer) as ISerializer<TComponent>);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(in ByteBuffer buffer, ref TComponent obj) {
            foreach (var serializer in s_fieldSerializers)
                serializer.Serialize(buffer, ref obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(in ByteBuffer buffer, ref TComponent obj) {
            foreach (var serializer in s_fieldSerializers)
                serializer.Deserialize(buffer, ref obj);
        }
    }
}