using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UdonSharpEditor;
using VRC.Udon;

namespace Mall1n.Canvas
{

    [CustomEditor(typeof(SwitchObjectsOnOff))]
    internal class SwitchObjectsOnOffEditor : Editor
    {
        public bool Enabled = false;

        private SwitchObjectsOnOff Script;
        private bool udonSharpInspector = true;

        private void OnEnable()
        {
            Script = (SwitchObjectsOnOff)target;
            if (Script == null) return;

            Undo.undoRedoPerformed += ApplyChanges;
        }

        public override void OnInspectorGUI()
        {


            EditorGUI.BeginChangeCheck();

            Enabled = EditorGUILayout.Toggle("Enabled Button", Script.Enabled);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Script, $"Bool Enabled {Script.GetType()} has been changed");
                Script.Enabled = Enabled;

                ApplyChanges();

                Script.ApplyProxyModifications();
            }

            if (UdonSharpInspector())
                if (!UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(Script))
                    DrawDefaultInspector();
        }

        void ApplyChanges()
        {
            CanvasColorController Colors = Script.transform.root.GetComponent<CanvasColorController>();
            Colors.UpdateProxy();

            Button button = Script.transform.GetComponent<Button>();
            Text text = Script.transform.GetComponent<Text>();
            if (button != null)
                if (Enabled == true)
                    SetColor(button, Colors.colorEnabled);
                else
                    SetColor(button, Colors.colorDisabled);

            var item = Script.transform.Find("Slider");
            if (item != null)
                item.gameObject.SetActive(Enabled);
            else if (text != null)
            {
                if (Enabled)
                {
                    SetColor(button, Colors.colorEnabled);
                    text.text = $"{text.text.Substring(0, text.text.IndexOf(":"))}: On";
                }
                else
                {
                    SetColor(button, Colors.colorDisabled);
                    text.text = $"{text.text.Substring(0, text.text.IndexOf(":"))}: Off";
                }
                UpdateScene();
            }
        }

        void SetColor(Button b, Color color)
        {
            ColorBlock colorBlock = b.colors;
            colorBlock.normalColor = color;
            b.colors = colorBlock;
        }

        bool UdonSharpInspector()
        {
            EditorGUI.BeginChangeCheck();

            udonSharpInspector = EditorGUILayout.Foldout(udonSharpInspector, "UdonSharp Inspector", true);

            EditorGUI.EndChangeCheck();
            return udonSharpInspector;
        }

        private void UpdateScene()
        {
            Transform t = Script.transform.root.Find("Download");
            if (t != null)
            {
                t.gameObject.SetActive(!t.gameObject.activeSelf);
                t.gameObject.SetActive(!t.gameObject.activeSelf);
            }
        }
    }
}

