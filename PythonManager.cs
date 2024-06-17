using System.Collections.Generic;
using UnityEngine;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace PythonEngineUnity
{
    public class PythonManager : MonoBehaviour
    {

        static PythonManager _instance;

        public static PythonManager instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("$python").AddComponent<PythonManager>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        PythonAssembly pythonAssembly;
        Dictionary<string, PythonScriptAsset> rawModules = new();
        Dictionary<string, ScriptScope> loadedModules = new();
        Dictionary<PythonScriptAsset, string> fixedAssets = new();
        //Dictionary<string, List<PythonScriptAsset>> rawModules;

        private void Awake()
        {
            pythonAssembly = Resources.Load<PythonAssembly>("PythonAssembly");
            engine = Python.CreateEngine(AppDomain.CurrentDomain);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                engine.Runtime.LoadAssembly(assembly);

            // WORK PATHS
            // ~ Get Raw Modules
            foreach(var asset in pythonAssembly.scripts)
                rawModules[asset.path + asset.name] = asset;

            // ~ Work importers
            const string importPattern = @"from\s+(.*[^\s])[\s]+import\s+([A-Za-z][A-Za-z0-9,]*)";
            foreach (var asset in pythonAssembly.scripts)
            {
                var text = asset.text;
                var matches = Regex.Matches(text, importPattern);
                foreach (Match match in matches)
                {
                    var targetPath = match.Groups[1].ToString();
                    var targetItem = match.Groups[2].ToString();
                    var targetItems = targetItem.Split(',');
                    var path = asset.path;
                    var steps = new List<string>(targetPath.Split('.'));
                    var dots = "";

                    for (int i = 0; i < steps.Count; i++)
                    {
                        var step = steps[i];
                        if (string.IsNullOrEmpty(step))
                            dots += ".";
                        else
                        {
                            dots = "";
                            path += step + "/";
                        }

                        if (dots == "..")
                        {
                            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                                path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                            path = Path.GetDirectoryName(path) + "/";
                            dots = "";
                        }

                    }

                    path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (rawModules.TryGetValue(path, out PythonScriptAsset targetAsset))
                    {
                        foreach (var item in targetItems)
                        {
                            var cItem = item.Replace(" ", string.Empty);
                            text = text.Replace(match.Value, $"{cItem} = external_module('{path}', '{cItem}')");
                        }
                    }
                }
                fixedAssets[asset] = text;
            }
            mainScope = CreateScope();
        }

        public dynamic ImportExternalModule(ScriptScope targetScope, string modulePath, string targetItem)
        {
            var asset = rawModules[modulePath];
            var fixedStr = fixedAssets[asset];
            if (!loadedModules.TryGetValue(modulePath, out ScriptScope moduleScope))
                loadedModules[modulePath] = moduleScope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromString(fixedStr);
            source.Execute(moduleScope);
            return moduleScope.GetVariable(targetItem);
        }

        public ScriptScope CreateScope()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("external_module", new System.Func<string, string, dynamic>( (a0, a1) => ImportExternalModule(scope, a0, a1)));
            return scope;
        }

        public ScriptSource GetSource(PythonScriptAsset asset)
            => engine.CreateScriptSourceFromString(fixedAssets[asset]);

        public ScriptEngine engine { get; private set; }

        public ScriptScope mainScope { get; private set; }

    }
}
