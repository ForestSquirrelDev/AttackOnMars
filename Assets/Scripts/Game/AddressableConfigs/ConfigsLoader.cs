using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Game.AddressableConfigs {
    public static class ConfigsLoader {
        private static readonly Dictionary<string, Object> s_configs = new Dictionary<string, Object>();

        public static T Get<T>(string addressableKey) where T : Object {
            if (s_configs.TryGetValue(addressableKey, out var config)) {
                return (T)config;
            }
            var addressableHandle = Addressables.LoadAssetAsync<T>(addressableKey);
            addressableHandle.WaitForCompletion();
            s_configs[addressableKey] = addressableHandle.Result;
            return addressableHandle.Result;
        }
    }
}