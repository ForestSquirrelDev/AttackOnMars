using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Game.AddressableConfigs {
    public static class AddressablesLoader {
        private static readonly Dictionary<string, Object> s_objects = new Dictionary<string, Object>();

        public static T Get<T>(string addressableKey) where T : Object {
            if (s_objects.TryGetValue(addressableKey, out var config)) {
                return (T)config;
            }
            var addressableHandle = Addressables.LoadAssetAsync<T>(addressableKey);
            addressableHandle.WaitForCompletion();
            s_objects[addressableKey] = addressableHandle.Result;
            return addressableHandle.Result;
        }
    }
}