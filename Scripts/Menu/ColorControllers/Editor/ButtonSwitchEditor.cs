using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UdonSharpEditor;
using VRC.Udon;

namespace Mall1n.Canvas
{

    [CustomEditor(typeof(ButtonSwitch))]
    internal class ButtonSwitchEditor : Editor
    {
        public bool Enabled = false;

        private ButtonSwitch Script;
        private bool udonSharpInspector = true;

        private void OnEnable()
        {
            Script = (ButtonSwitch)target;
            if (Script == null) return;

            Undo.undoRedoPerformed += ApplyChanges;
        }

        public override void OnInspectorGUI()
        {


            EditorGUI.BeginChangeCheck();

            Enabled = EditorGUILayout.Toggle("Enabled button", Script.Enabled);

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
            if (button != null)
                if (Enabled == true)
                    SetColor(button, Colors.colorEnabled);
                else
                    SetColor(button, Colors.colorDisabled);

            var item = Script.transform.Find("Slider");
            if (item != null)
                item.gameObject.SetActive(Enabled);
            else UpdateScene();
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

