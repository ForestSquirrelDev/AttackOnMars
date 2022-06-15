using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class LeaksKekwDetector {
    private static int _counter;
    [MenuItem("Tools/KekwLeaksTest")]
    public static void KekwLeaksTest() {
        _counter = 0;
        _ = Test();
    }

    private static async Task Test() {
        if (_counter >= 15) return;
        _counter++;
        await Task.Delay(10000);
        if (EditorApplication.isPlaying) {
            EditorApplication.isPlaying = false;
        } else if (UnityEditor.EditorApplication.isPlaying == false) {
            _ = Test();
            UnityEditor.EditorApplication.isPlaying = true;
        }
        Debug.Log($"Yeps");
    }
}
