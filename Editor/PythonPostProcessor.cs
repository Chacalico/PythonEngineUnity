using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace PythonEngineUnity
{
    public class PythonPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            int op = 0;

            var assembly = AssetDatabase.LoadAssetAtPath<PythonAssembly>("Assets/Resources/PythonAssembly.asset");
            if (!assembly)
            {
                assembly = ScriptableObject.CreateInstance<PythonAssembly>();
                assembly.name = "PythonAssembly";
                AssetDatabase.CreateAsset(assembly, "Assets/Resources/PythonAssembly.asset");
                AssetDatabase.ImportAsset("Assets/Resources/PythonAssembly.asset");
            }


            foreach (var path in importedAssets)
                if (path.EndsWith(".py"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<PythonScriptAsset>(path);
                    if (!assembly.scripts.Contains(asset))
                    {
                        assembly.scripts.Add(asset);
                        op++;
                    }
                }


            if (op > 0)
            {
                assembly.scripts.RemoveAll(x => !x);
                assembly.scripts.Sort((x, y) => Comparer<int>.Default.Compare(x.modulePriority, y.modulePriority));
                EditorUtility.SetDirty(assembly);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
