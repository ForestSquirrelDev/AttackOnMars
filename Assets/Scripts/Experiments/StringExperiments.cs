using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Experiments {
    public class StringExperiments : MonoBehaviour {
        /*private void OnDrawGizmos() {
            string url = "oauth/test?state=xyz&id_token=eyJ0eXAiOiJqc29uIiwiYWxnIjoiUlMyNTYiLCJraWQiOiIwZjRhZjBhNTQ0NWZiNmE2NmUzMDNiMzY0ZWZmMWQwYTNkY2JhZjFkNTU2NDAxYWIwOTliMTNkYzc2MmNkNmUzIn0.eyJpYXQiOjE2NTc2MzEyMDEsImp0aSI6ImM4OTE4MjM5LWFlZjEtNGI2Mi05ZDI1LTdmMDUyZjFmNTllNSIsIm5iZiI6MTY1NzYzMTIwMSwic3ViIjoiNzYyMjg3MjQ2NzAxNzU3NCIsImF1ZCI6ImI4Y2FkMzY1LWZmMmMtNDAzNy04MjM3LWM5NzNmYWYxYTg1ZiIsImlzcyI6Imh0dHBzOi8vYXV0aC5sYXVuY2hlci5kZXYua2VmaXJnYW1lcy5ydSIsImV4cCI6MTY1NzcxNzYwMSwic2NvcGUiOiJwcm9maWxlIiwibmlja25hbWUiOiJQbGF5ZXIiLCJzb2NpYWwiOlt7ImVtYWlsIjoibC56aHlidWxpYUBrZWZpcmdhbWVzLmNvbSIsInVzZXJfaWQiOiIxMTQyNDQ1NTc3NDIyODczNzU1NDUiLCJ0eXBlIjoiZ29vZ2xlIn1dLCJub25jZSI6IjEyNDIifQ.b192OM-9LtnDIh-__O53nTFBvtyCQCFS8ELeHoWAk4q5FWWfEb9vukILY1vUJvT84ScgSKNX3eOhZIQQpGeE1-OsNuh-Kb5Z_jJAUfAbstVZErcpKKVLTJrC-mipELUy65VWTYN4oXfYJGAiECSSKb3e2Qy-9gDDFXK6KdJYDtD0fcXw3J0NFigMFwYLw0K55iB1AfzLqgjxWa2n08_Vo0d07QFCgpz3m3Ise3NpOQNCJ7t_WlwsTgDzZNhE-n1ZpSsIZqxV7DxqHyRW2wI5sIaRkAc02zqVaBjAvOULkJMQmuQfSs8LmKuZxAoCemkmibsRbBBMCLdwHN-idQoMIQ";
            var substrings = new List<string>();
            var pos = 0;
            while (pos < url.Length)
            {
                var length = Mathf.Clamp(url.Length - pos, 0, 50);
                if (length == 0)
                    break;
                substrings.Add(url.Substring(pos, length));
                pos += 50;
            }
            int j = 0;
            foreach (var urlPart in substrings)
            {
                Debug.Log($"SubString {j}. Url part: {urlPart}");
                j++;
            }
        }*/
    }
}