using UnityEngine;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;
using System.Linq;

namespace PythonEngineUnity
{
    [System.Serializable]
    public class PythonScript
    {

        [SerializeField] PythonScriptAsset m_asset;
        [SerializeField] List<PythonScriptParamOverride> m_parameters = new List<PythonScriptParamOverride>();

        public dynamic Instantiate(out ScriptScope scope, string sourceString)
        {
            if (m_asset)
            {
                scope = PythonManager.instance.CreateScope();
                PythonManager.instance.GetSource(asset).Execute(scope);
                return scope.Engine.CreateScriptSourceFromString(sourceString).Execute(scope);
            }
            else
                throw new System.InvalidOperationException("ScriptAsset is missing");
        }

        public bool ApplyParametersToDict(dynamic obj, string dictFieldName = ".dict")
        {
            System.Type type = obj.GetType();
            var field = type.GetField(dictFieldName);
            var dict = field.GetValue(obj) as PythonDictionary;

            foreach (var param in parameters)
            {
                var paramSource = asset.parameters.FirstOrDefault(x => x.id == param.id);
                if (paramSource == null)
                    continue;

                param.source = paramSource;
                dict[paramSource.name] = param.GetValue();
            }

            return true;
        }

        public PythonScriptAsset asset => m_asset;

        public List<PythonScriptParamOverride> parameters => m_parameters;

    }

    [System.Serializable]
    public class PythonScriptParamOverride
    {

        public string id;
        public float numValue;
        public string stringValue;
        public bool boolValue;
        public GameObject gameObjectValue;
        public Transform transformValue;
        public Object objectValue;
        public bool unlocked;

        public dynamic GetValue()
        {
            return source.type switch
            {
                PythonScriptParamType.Num => unlocked ? numValue : source.numValue,
                PythonScriptParamType.String => unlocked ? stringValue : source.stringValue,
                PythonScriptParamType.Bool => unlocked ? boolValue : source.boolValue,
                PythonScriptParamType.GameObject => unlocked ? gameObjectValue : source.gameObjectValue,
                PythonScriptParamType.Transform => unlocked ? transformValue : source.transformValue,
                PythonScriptParamType.Object => unlocked ? objectValue : source.objectValue,
                PythonScriptParamType.Generic => null,
                _ => null,
            };
        }

        public PythonScriptParam source { get; set; }
    }
}
