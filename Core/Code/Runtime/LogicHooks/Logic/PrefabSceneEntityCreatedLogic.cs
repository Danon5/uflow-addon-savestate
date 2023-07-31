using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Core.Runtime;
using UnityEngine.Scripting;

namespace UFlow.Addon.SaveState.Core.Runtime {
    [Preserve]
    public struct PrefabSceneEntityCreatedLogic : ILogic<PrefabSceneEntityCreatedHook> {
        public void Execute(in PrefabSceneEntityCreatedHook hook) {
            hook.sceneEntity.Entity.Set(new InstantiatedSceneEntity {
                sceneEntity = hook.sceneEntity,
                persistentKey = hook.persistentKey
            });
        }
    }
}