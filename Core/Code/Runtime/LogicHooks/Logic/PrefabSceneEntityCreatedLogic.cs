using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    public struct PrefabSceneEntityCreatedLogic : ILogic<PrefabSceneEntityCreatedHook> {
        public void Execute(in PrefabSceneEntityCreatedHook hook) {
            hook.sceneEntity.Entity.Set(new InstantiatedSceneEntity {
                sceneEntity = hook.sceneEntity,
                persistentKey = hook.persistentKey
            });
        }
    }
}