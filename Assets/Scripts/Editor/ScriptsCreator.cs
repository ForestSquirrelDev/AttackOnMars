using UnityEditor;
using UnityEngine;

namespace Editor {
    public static class ScriptsCreator {
        private static readonly string s_templatesRoot = $"{Application.dataPath}/ScriptTemplates";
    
        [MenuItem("Assets/Create/C# Struct", priority = 1)]
        private static void CreateStruct() {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{s_templatesRoot}/C# Struct Template.txt", "NewStruct.cs");
        }

        [MenuItem("Assets/Create/IComponentDataAuthoring", priority = 2)]
        private static void CreateComponentData() {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{s_templatesRoot}/C# IComponentDataAuthoring.txt", "NewComponentData.cs");
        }

        [MenuItem("Assets/Create/C# ECS SystemBase", priority = 3)]
        private static void CreateEcsSystem() {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
            $"{s_templatesRoot}/C# ECS System Template.txt", "NewEcsSystem.cs");
        }
    }
}