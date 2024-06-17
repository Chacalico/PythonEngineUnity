using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PythonEngineUnity
{
    public class PythonAssembly : ScriptableObject
    {

        public string[] includedAssemblies;
        public List<PythonScriptAsset> scripts = new List<PythonScriptAsset>();

    }
}
