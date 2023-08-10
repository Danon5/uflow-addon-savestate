using NUnit.Framework;
using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Addon.SaveState.Core.Runtime;
using UFlow.Addon.Serialization.Core.Runtime;

namespace UFlow.Addon.SaveState.Tests {
    public sealed class SaveSerializerTests {
        [Test]
        public void ComponentSerializeDeserializeTest() {
            var buffer = new ByteBuffer();
            var test1 = new Test1 {
                someData1 = 1,
                someData2 = 2,
                someData3 = 3
            };
            SaveSerializationAPI.SerializeComponent(buffer, ref test1);
            buffer.ResetCursor();
            var result = SaveSerializationAPI.DeserializeComponent<Test1>(buffer);
            Assert.That(result.someData1, Is.EqualTo(1));
            Assert.That(result.someData2, Is.EqualTo(0));
            Assert.That(result.someData3, Is.EqualTo(3));
        }

        [Test]
        public void EntitySerializeDeserializeTest() {
            var buffer = new ByteBuffer();
            var world = new World();
            var entity = world.CreateEntity();
            entity.Set(new Test1 {
                someData1 = 1,
                someData2 = 2,
                someData3 = 3
            });
            SaveSerializationAPI.SerializeEntityAsNew(buffer, entity);
            buffer.ResetCursor();
            entity.Destroy();
            var deserializedEntity = SaveSerializationAPI.DeserializeEntityAsNew(buffer, world);
            ref var test1 = ref deserializedEntity.Get<Test1>();
            Assert.That(test1.someData1, Is.EqualTo(1));
            Assert.That(test1.someData2, Is.EqualTo(0));
            Assert.That(test1.someData3, Is.EqualTo(3));
            world.Destroy();
            ExternalEngineEvents.clearStaticCachesEvent?.Invoke();
        }
        
        [Test]
        public void EntitySerializeDeserializeArrayTest() {
            var buffer = new ByteBuffer();
            var world = new World();
            var entity = world.CreateEntity();
            entity.Set(new Test2 {
                someDataArray1 = new [] {
                    1,
                    2,
                    3
                }
            });
            SaveSerializationAPI.SerializeEntityAsNew(buffer, entity);
            buffer.ResetCursor();
            entity.Destroy();
            var deserializedEntity = SaveSerializationAPI.DeserializeEntityAsNew(buffer, world);
            ref var test2 = ref deserializedEntity.Get<Test2>();
            Assert.That(test2.someDataArray1[0], Is.EqualTo(1));
            Assert.That(test2.someDataArray1[1], Is.EqualTo(2));
            Assert.That(test2.someDataArray1[2], Is.EqualTo(3));
            world.Destroy();
            ExternalEngineEvents.clearStaticCachesEvent?.Invoke();
        }

        [Test]
        public void WorldSerializeDeserializeTest() {
            var buffer = new ByteBuffer();
            var world = new World();
            world.Set(new Test1 {
                someData1 = 1,
                someData2 = 2,
                someData3 = 3
            });
            var entity = world.CreateEntity();
            entity.Set(new Test1 {
                someData1 = 1,
                someData2 = 2,
                someData3 = 3
            });
            SaveSerializationAPI.SerializeWorld(buffer, world);
            buffer.ResetCursor();
            SaveSerializationAPI.DeserializeWorld(buffer, world);
            Assert.That(world.Get<Test1>().someData1, Is.EqualTo(1));
            Assert.That(world.Get<Test1>().someData2, Is.EqualTo(0));
            Assert.That(world.Get<Test1>().someData3, Is.EqualTo(3));
            Assert.That(entity.Get<Test1>().someData1, Is.EqualTo(1));
            Assert.That(entity.Get<Test1>().someData2, Is.EqualTo(0));
            Assert.That(entity.Get<Test1>().someData3, Is.EqualTo(3));
        }

        [EcsSerializable("SerializationTestsComp1")]
        private struct Test1 : IEcsComponent {
            [Save] public int someData1;
            public int someData2;
            [Save] public int someData3;
        }
        
        [EcsSerializable("SerializationTestsComp2")]
        private struct Test2 : IEcsComponent {
            [Save] public int[] someDataArray1;
        }
    }
}