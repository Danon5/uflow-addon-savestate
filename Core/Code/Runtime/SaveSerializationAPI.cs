using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Addon.Serialization.Core.Runtime;
using UFlow.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    public static class SaveSerializationAPI {
        private static readonly Dictionary<Type, MethodInfo> s_entityComponentSerializeCache = new();
        private static readonly Dictionary<Type, MethodInfo> s_entityComponentDeserializeCache = new();
        private static readonly Dictionary<Type, MethodInfo> s_worldComponentSerializeCache = new();
        private static readonly Dictionary<Type, MethodInfo> s_worldComponentDeserializeCache = new();
        private static readonly object[] s_singleObjectBuffer = new object[1];
        private static readonly object[] s_doubleObjectBuffer = new object[2];
        private static readonly HashSet<Type> s_componentTypeBuffer = new();
        private static readonly Queue<Type> s_componentRemoveQueue = new();
        private static Entity s_currentEntity;

        static SaveSerializationAPI() {
            var type = typeof(SaveSerializationAPI);
            foreach (var registeredType in SaveTypeMap.GetRegisteredTypesEnumerable())
                s_entityComponentSerializeCache.Add(registeredType, 
                    type.GetMethod(nameof(SerializeEntityComponent), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(registeredType));
            foreach (var registeredType in SaveTypeMap.GetRegisteredTypesEnumerable())
                s_entityComponentDeserializeCache.Add(registeredType, 
                    type.GetMethod(nameof(DeserializeEntityComponent), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(registeredType));
            foreach (var registeredType in SaveTypeMap.GetRegisteredTypesEnumerable())
                s_worldComponentSerializeCache.Add(registeredType, 
                    type.GetMethod(nameof(SerializeWorldComponent), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(registeredType));
            foreach (var registeredType in SaveTypeMap.GetRegisteredTypesEnumerable())
                s_worldComponentDeserializeCache.Add(registeredType, 
                    type.GetMethod(nameof(DeserializeWorldComponent), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(registeredType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeComponent<T>(in ByteBuffer buffer, ref T component) where T : IEcsComponent {
            buffer.Write(SaveTypeMap.GetHash(typeof(T)));
            ComponentSerialization<SaveAttribute, T>.Serialize(buffer, ref component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeserializeComponent<T>(in ByteBuffer buffer, ref T component) where T : IEcsComponent {
            buffer.ReadInt();
            ComponentSerialization<SaveAttribute, T>.Deserialize(buffer, ref component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeComponent<T>(in ByteBuffer buffer) where T : IEcsComponent, new() {
            var component = new T();
            buffer.ReadInt();
            ComponentSerialization<SaveAttribute, T>.Deserialize(buffer, ref component);
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeEntity(in ByteBuffer buffer, in Entity entity) => SerializeEntityInternal(buffer, entity, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity DeserializeEntity(in ByteBuffer buffer, in World world) => DeserializeEntityInternal(buffer, world, false);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeEntityAsNew(in ByteBuffer buffer, in Entity entity) => SerializeEntityInternal(buffer, entity, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity DeserializeEntityAsNew(in ByteBuffer buffer, in World world) {
            return DeserializeEntityInternal(buffer, world, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeWorld(in ByteBuffer buffer, in World world) {
            s_doubleObjectBuffer[0] = buffer;
            s_doubleObjectBuffer[1] = world;
            buffer.Write(world.NextEntityId);
            buffer.Write(world.ComponentCount);
            foreach (var componentType in world.ComponentTypes) {
                buffer.Write(SaveTypeMap.GetHash(componentType));
                s_worldComponentSerializeCache[componentType].Invoke(null, s_doubleObjectBuffer);
            }
            buffer.Write(world.EntityCount);
            foreach (var entity in world.GetEntitiesEnumerable())
                SerializeEntity(buffer, entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeserializeWorld(in ByteBuffer buffer, in World world) {
            s_doubleObjectBuffer[0] = buffer;
            s_doubleObjectBuffer[1] = world;
            world.IsDeserializing = true;
            world.ResetForDeserialization(buffer.ReadInt());
            var componentCount = buffer.ReadInt();
            for (var i = 0; i < componentCount; i++) {   
                var componentType = SaveTypeMap.GetType(buffer.ReadULong());
                s_worldComponentDeserializeCache[componentType].Invoke(null, s_doubleObjectBuffer);
            }
            var entityCount = buffer.ReadInt();
            for (var i = 0; i < entityCount; i++)
                DeserializeEntity(buffer, world);
            world.IsDeserializing = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SerializeEntityComponent<T>(in ByteBuffer buffer) where T : IEcsComponent {
            buffer.Write(s_currentEntity.IsEnabled<T>());
            ComponentSerialization<SaveAttribute, T>.Serialize(buffer, ref s_currentEntity.Get<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeserializeEntityComponent<T>(in ByteBuffer buffer) where T : IEcsComponent {
            var enabled = buffer.ReadBool();
            ComponentSerialization<SaveAttribute, T>.Deserialize(buffer, ref s_currentEntity.Set<T>(default, enabled));
            s_currentEntity.SetEnabled<T>(enabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SerializeWorldComponent<T>(in ByteBuffer buffer, in World world) where T : IEcsComponent {
            buffer.Write(world.IsEnabled<T>());
            ComponentSerialization<SaveAttribute, T>.Serialize(buffer, ref world.Get<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeserializeWorldComponent<T>(in ByteBuffer buffer, in World world) where T : IEcsComponent {
            var enabled = buffer.ReadBool();
            ComponentSerialization<SaveAttribute, T>.Deserialize(buffer, ref world.Set<T>(default, enabled));
            world.SetEnabled<T>(enabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SerializeEntityInternal(in ByteBuffer buffer, in Entity entity, bool asNew) {
            EntityPrefabMap.EnsureInitialized();
            s_singleObjectBuffer[0] = buffer;
            s_currentEntity = entity;
            var isInstantiatedSceneEntity = entity.Has<InstantiatedSceneEntity>();
            buffer.Write(isInstantiatedSceneEntity);
            if (isInstantiatedSceneEntity) {
                var persistentKey = entity.Get<InstantiatedSceneEntity>().persistentKey;
                buffer.Write(EntityPrefabMap.GetHash(persistentKey));
            }
            if (!asNew) {
                buffer.Write(entity.id);
                buffer.Write(entity.gen);
            }
            var componentCount = (byte)s_currentEntity.ComponentCount;
            if (isInstantiatedSceneEntity)
                componentCount--;
            buffer.Write(componentCount);
            var instantiatedSceneEntityType = typeof(InstantiatedSceneEntity);
            foreach (var componentType in entity.ComponentTypes) {
                if (componentType == instantiatedSceneEntityType) continue;
                buffer.Write(SaveTypeMap.GetHash(componentType));
                s_entityComponentSerializeCache[componentType].Invoke(null, s_singleObjectBuffer);
            }
            buffer.Write(entity.IsEnabled());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Entity DeserializeEntityInternal(in ByteBuffer buffer, in World world, bool asNew) {
            EntityPrefabMap.EnsureInitialized();
            s_singleObjectBuffer[0] = buffer;
            var isInstantiatedSceneEntity = buffer.ReadBool();
            if (asNew) {
                if (isInstantiatedSceneEntity) {
                    var persistentHash = buffer.ReadInt();
                    s_currentEntity = EntityPrefabMap.GetPrefab(persistentHash).Instantiate().AsEntity();
                }
                else
                    s_currentEntity = world.CreateEntity();
            }
            else {
                if (isInstantiatedSceneEntity) {
                    var persistentHash = buffer.ReadInt();
                    s_currentEntity = EntityPrefabMap.GetPrefab(persistentHash).Instantiate()
                        .AsEntityWithIdAndGen(buffer.ReadInt(), buffer.ReadUShort());
                }
                else
                    s_currentEntity = world.CreateEntityWithIdAndGen(buffer.ReadInt(), buffer.ReadUShort());
            }
            var componentCount = (int)buffer.ReadByte();
            s_componentTypeBuffer.Clear();
            for (var i = 0; i < componentCount; i++) {
                var componentType = SaveTypeMap.GetType(buffer.ReadULong());
                if (isInstantiatedSceneEntity)
                    s_componentTypeBuffer.Add(componentType);
                else
                    s_entityComponentDeserializeCache[componentType].Invoke(null, s_singleObjectBuffer);
            }
            if (isInstantiatedSceneEntity) {
                var instantiatedSceneEntityType = typeof(InstantiatedSceneEntity);
                foreach (var componentType in s_currentEntity.ComponentTypes) {
                    if (componentType == instantiatedSceneEntityType) continue;
                    if (!s_componentTypeBuffer.Contains(componentType))
                        s_componentRemoveQueue.Enqueue(componentType);
                }
                while (s_componentRemoveQueue.TryDequeue(out var componentType))
                    s_currentEntity.RemoveRaw(componentType);
                foreach (var componentType in s_componentTypeBuffer)
                    s_entityComponentDeserializeCache[componentType].Invoke(null, s_singleObjectBuffer);
            }
            s_currentEntity.SetEnabled(buffer.ReadBool());
            return s_currentEntity;
        }
    }
}