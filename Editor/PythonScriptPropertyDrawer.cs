using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PythonEngineUnity
{
    [CustomPropertyDrawer(typeof(PythonScript))]
    public class PythonScriptPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                var p_asset = property.FindPropertyRelative("m_asset");
                var asset = p_asset.objectReferenceValue as PythonScriptAsset;
                var parameters_p = property.FindPropertyRelative("m_parameters");
                var h = lineHeight * 2;
                if (p_asset.objectReferenceValue)
                    h += lineHeight + (asset.parameters.Length * lineHeight); 
                return h;
            }
            else
                return lineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var gui_e = GUI.enabled;

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (!(property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label)))
                return;

            var p_asset = property.FindPropertyRelative("m_asset");
            var last_asset = p_asset.objectReferenceValue;
            
            EditorGUI.indentLevel++;
            rect.y += lineHeight;
            rect = EditorGUI.IndentedRect(rect);

            EditorGUI.PropertyField(rect, p_asset);
            var assetChanged = p_asset.objectReferenceValue != last_asset;
            rect.y += lineHeight;

            if (p_asset.objectReferenceValue)
            {
                var asset = p_asset.objectReferenceValue as PythonScriptAsset;
                var parameters_p = property.FindPropertyRelative("m_parameters");

                EditorGUI.LabelField(rect, "Script Parameters");
                rect.y += lineHeight;
                EditorGUI.indentLevel++;
                

                for (int i = 0; i < asset.parameters.Length; i++)
                {
                    var param = asset.parameters[i];
                    SerializedProperty target = null;
                    for (int l = 0; l < parameters_p.arraySize; l++)
                    {
                        var element = parameters_p.GetArrayElementAtIndex(l);
                        if (element.FindPropertyRelative("id").stringValue == param.id)
                        {
                            target = element;
                            break;
                        }
                    }

                    if (target != null)
                        continue;

                    parameters_p.InsertArrayElementAtIndex(parameters_p.arraySize);
                    target = parameters_p.GetArrayElementAtIndex(parameters_p.arraySize-1);
                    target.FindPropertyRelative("id").stringValue = param.id;
                }

                for (int i = 0; i < parameters_p.arraySize; i++)
                {
                    var element = parameters_p.GetArrayElementAtIndex(i);
                    var source = asset.parameters.FirstOrDefault(x => x.id == element.FindPropertyRelative("id").stringValue);
                    if (source == null)
                        continue;

                    //rect = EditorGUI.IndentedRect(rect);
                    var r = EditorGUI.IndentedRect(rect);
                    EditorGUI.DrawRect(r, new Color(0, 0, 0, .05f));

                    var p_name = element.FindPropertyRelative("name");
                    var p_unlocked = element.FindPropertyRelative("unlocked");

                    const float bw = 24;
                    var r0 = new Rect(r.x, r.y, bw, r.height);
                    var r1 = new Rect(r.x + bw, r.y, bw, r.height);
                    var r3 = new Rect(rect.x + bw*2 + 3, rect.y, rect.width - bw*2 - 3, rect.height);
                    bool unlock = GUI.Button(r0, "U");
                    GUI.enabled = gui_e && p_unlocked.boolValue;
                    bool reset = GUI.Button(r1, "R");
                    GUI.enabled = gui_e;

                    if (unlock)
                    {
                        if (!p_unlocked.boolValue)
                        {
                            SerializedProperty prop;
                            switch (source.type)
                            {
                                case PythonScriptParamType.Num:
                                    prop = element.FindPropertyRelative("numValue");
                                    prop.floatValue = source.numValue;
                                    break;
                                case PythonScriptParamType.String:
                                    prop = element.FindPropertyRelative("stringValue");
                                    prop.stringValue = source.stringValue;
                                    break;
                                case PythonScriptParamType.Bool:
                                    prop = element.FindPropertyRelative("boolValue");
                                    prop.boolValue = source.boolValue;
                                    break;
                                case PythonScriptParamType.GameObject:
                                    prop = element.FindPropertyRelative("gameObjectValue");
                                    prop.objectReferenceValue = source.gameObjectValue;
                                    break;
                                case PythonScriptParamType.Transform:
                                    prop = element.FindPropertyRelative("transformValue");
                                    prop.objectReferenceValue = source.transformValue;
                                    break;
                                case PythonScriptParamType.Object:
                                    prop = element.FindPropertyRelative("objectValue");
                                    prop.objectReferenceValue = source.objectValue;
                                    break;
                            }
                        }
                        p_unlocked.boolValue = !p_unlocked.boolValue;
                    }

                    if (reset)
                    {
                        SerializedProperty prop;
                        switch (source.type)
                        {
                            case PythonScriptParamType.Num:
                                prop = element.FindPropertyRelative("numValue");
                                prop.floatValue = source.numValue;
                                break;
                            case PythonScriptParamType.String:
                                prop = element.FindPropertyRelative("stringValue");
                                prop.stringValue = source.stringValue;
                                break;
                            case PythonScriptParamType.Bool:
                                prop = element.FindPropertyRelative("boolValue");
                                prop.boolValue = source.boolValue;
                                break;
                            case PythonScriptParamType.GameObject:
                                prop = element.FindPropertyRelative("gameObjectValue");
                                prop.objectReferenceValue = source.gameObjectValue;
                                break;
                            case PythonScriptParamType.Transform:
                                prop = element.FindPropertyRelative("transformValue");
                                prop.objectReferenceValue = source.transformValue;
                                break;
                            case PythonScriptParamType.Object:
                                prop = element.FindPropertyRelative("objectValue");
                                prop.objectReferenceValue = source.objectValue;
                                break;
                        }
                    }

                    if (p_unlocked.boolValue)
                    {
                        string propName = "";
                        
                        switch (source.type)
                        {
                            case PythonScriptParamType.Num:
                                propName = "numValue";
                                break;
                            case PythonScriptParamType.String:
                                propName = "stringValue";
                                break;
                            case PythonScriptParamType.Bool:
                                propName = "boolValue";
                                break;
                            case PythonScriptParamType.GameObject:
                                propName = "gameObjectValue";
                                break;
                            case PythonScriptParamType.Transform:
                                propName = "transformValue";
                                break;
                            case PythonScriptParamType.Object:
                                propName = "objectValue";
                                break;
                        }
                        if (!string.IsNullOrEmpty(propName))
                        {
                            label = new GUIContent(source.name);
                            SerializedProperty prop = element.FindPropertyRelative(propName);
                            EditorGUI.PropertyField(r3, prop, label);
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        switch (source.type)
                        {
                            case PythonScriptParamType.Num:
                                EditorGUI.FloatField(r3, new GUIContent(source.name), source.numValue);
                                break;
                            case PythonScriptParamType.String:
                                EditorGUI.TextField(r3, new GUIContent(source.name), source.stringValue);
                                break;
                            case PythonScriptParamType.Bool:
                                EditorGUI.Toggle(r3, new GUIContent(source.name), source.boolValue);
                                break;
                            case PythonScriptParamType.GameObject:
                                EditorGUI.ObjectField(r3, new GUIContent(source.name), source.gameObjectValue, typeof(GameObject), false);
                                break;
                            case PythonScriptParamType.Transform:
                                EditorGUI.ObjectField(r3, new GUIContent(source.name), source.transformValue, typeof(Transform), false);
                                break;
                            case PythonScriptParamType.Object:
                                EditorGUI.ObjectField(r3, new GUIContent(source.name), source.objectValue, typeof(Object), false);
                                break;
                        }
                        GUI.enabled = gui_e;
                    }

                    rect.y += lineHeight;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

        }

        static float lineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    }
}
