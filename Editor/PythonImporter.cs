using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;

namespace PythonEngineUnity.Editor
{
    [ScriptedImporter(1, "py")]
    public class PythonImporter : ScriptedImporter
    {

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = AssetDatabase.LoadAssetAtPath<PythonScriptAsset>(ctx.assetPath);
            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<PythonScriptAsset>();
            }
            SerializedObject obj = new(asset);
            var text = File.ReadAllText(ctx.assetPath);

            obj.FindProperty("m_text").stringValue = text;

            // Test Script Type
            const string modulePattern = @"#module\s*(-?\d*)";
            Regex moduleRegex = new Regex(modulePattern, RegexOptions.Multiline);
            Match moduleMatch = moduleRegex.Match(text);

            var filePath = ctx.assetPath.Replace("Assets/Scripts/Python", ".").Replace(System.IO.Path.GetFileName(ctx.assetPath), "");
            obj.FindProperty("m_path").stringValue = filePath;

            if (moduleMatch.Success)
            {
                obj.FindProperty("m_type").enumValueIndex = 1;
                if (int.TryParse(moduleMatch.Groups[1].Value, out int priority))
                    obj.FindProperty("m_modulePriority").intValue = priority;
                else
                    obj.FindProperty("m_modulePriority").intValue = 0;
            }
            else
            {
                obj.FindProperty("m_type").enumValueIndex = 0;
                obj.FindProperty("m_modulePriority").intValue = 0;
            }

            const string paramsPattern = @"#\s*<param\s+id=(\d+)\s+name=(\w+)\s+type=(\w+)>(.*)<\/param>";
            var paramsMatches = Regex.Matches(text, paramsPattern);
            var paramsProperty = obj.FindProperty("m_params");
            paramsProperty.arraySize = 0;
            var paramsCount = 0;
            for(int i = 0; i < paramsMatches.Count; i++)
            { 
                var match = paramsMatches[i];
                var name = match.Groups[2].ToString();
                var id = match.Groups[1].ToString();
                var typeRaw = match.Groups[3].ToString();
                var valueRaw = match.Groups[4].ToString();
                object value = null;
                var type = PythonScriptParamType.Unedfined;
                try
                {
                    switch (typeRaw)
                    {
                        case "num":
                            if (!float.TryParse(valueRaw, out float numValue))
                                goto case "err";
                            value = numValue;
                            type = PythonScriptParamType.Num;
                            break;

                        case "string":
                            value = valueRaw;
                            type = PythonScriptParamType.String;
                            break;

                        case "bool":
                            if (valueRaw.ToLower() != "true" && valueRaw.ToLower() != "false")
                                goto case "err";
                            value = valueRaw.ToLower() == "true";
                            type = PythonScriptParamType.Bool;
                            break;

                        case "gameObject":
                            value = null;
                            type = PythonScriptParamType.GameObject;
                            break;

                        case "transform":
                            value = null;
                            type = PythonScriptParamType.Transform;
                            break;

                        case "object":
                            value = null;
                            type = PythonScriptParamType.Object;
                            break;

                        case "generic":
                            value = null;
                            type = PythonScriptParamType.Generic;
                            break;

                        case "err":
                            throw new System.FormatException($"param of file:{ctx.assetPath} name:{name} type:{typeRaw} value:{valueRaw}");

                        default:
                            throw new System.FormatException($"param of file:{ctx.assetPath} name:{name} type:{typeRaw} unkown type!");

                    }

                    paramsProperty.InsertArrayElementAtIndex(paramsCount);
                    var element = paramsProperty.GetArrayElementAtIndex(paramsCount);
                    element.FindPropertyRelative("m_name").stringValue = name;
                    element.FindPropertyRelative("m_id").stringValue = id;
                    element.FindPropertyRelative("m_type").enumValueIndex = (int)type;
                    switch(type)
                    {
                        case PythonScriptParamType.Num:
                            element.FindPropertyRelative("m_numValue").floatValue = (float)value;
                            break;

                        case PythonScriptParamType.String:
                            element.FindPropertyRelative("m_stringValue").stringValue = (string)value;
                            break;

                        case PythonScriptParamType.Bool:
                            element.FindPropertyRelative("m_boolValue").boolValue = (bool)value;
                            break;
                    }
                    paramsCount++;
                }
                catch(System.FormatException e)
                {
                    Debug.LogError(e);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }

            obj.ApplyModifiedProperties();
            ctx.AddObjectToAsset("main", asset);
            ctx.SetMainObject(asset);

        }


    }
}
