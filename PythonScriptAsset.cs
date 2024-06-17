using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PythonEngineUnity
{
    public class PythonScriptAsset : ScriptableObject
    {


        [SerializeField] string m_path;
        [SerializeField] string m_text;
        [SerializeField] PythonScriptType m_type;
        [SerializeField] int m_modulePriority;
        [SerializeField] PythonScriptParam[] m_params;
         
        public string path => m_path;

        public string text => m_text;

        public PythonScriptType type => m_type;

        public int modulePriority => m_modulePriority;

        public PythonScriptParam[] parameters => m_params;

        public string fullPath => path + name;

    }

    [System.Serializable]
    public class PythonScriptParam
    {
        [SerializeField] string m_name;
        [SerializeField] string m_id;
        [SerializeField] PythonScriptParamType m_type;
        [SerializeField] float m_numValue;
        [SerializeField] string m_stringValue;
        [SerializeField] bool m_boolValue;
        [SerializeField] GameObject m_gameObjectValue;
        [SerializeField] Transform m_transformValue;
        [SerializeField] Object m_objectValue;

        public string name => m_name;
        public string id => m_id;
        public PythonScriptParamType type => m_type;
        public float numValue => m_numValue;
        public string stringValue => m_stringValue;
        public bool boolValue => m_boolValue;
        public GameObject gameObjectValue => m_gameObjectValue;
        public Transform transformValue => m_transformValue;
        public Object objectValue => m_objectValue;

        public dynamic GetValue()
        {
            return type switch
            {
                PythonScriptParamType.Num => m_numValue,
                PythonScriptParamType.String => m_stringValue,
                PythonScriptParamType.Bool => m_boolValue,
                PythonScriptParamType.GameObject => m_gameObjectValue,
                PythonScriptParamType.Transform => m_transformValue,
                PythonScriptParamType.Object => m_objectValue,
                PythonScriptParamType.Generic => null,
                _ => null,
            };
        }
    }

    public enum PythonScriptParamType
    {
        Unedfined,
        Num,
        String,
        Bool,
        GameObject,
        Transform,
        Object,
        Generic
    }

    public enum PythonScriptType
    {
        Script,
        Module
    }
}
