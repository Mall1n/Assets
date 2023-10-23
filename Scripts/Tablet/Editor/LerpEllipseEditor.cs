using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UdonSharpEditor;
using VRC.Udon;

namespace Mall1n.Canvas
{

    [CustomEditor(typeof(LerpEllipse))]
    internal class LerpEllipseEditor : Editor
    {
        public bool LeftPosition = false;


        [ColorUsage(true, true)]
        public Color SliderColor;
        [ColorUsage(true, true)]
        public Color DisabledColor;
        [ColorUsage(true, true)]
        public Color EnabledColor;

        private float Range = 0.03f;

        private LerpEllipse Script;
        private bool udonSharpInspector = true;

        private void OnEnable()
        {
            Script = (LerpEllipse)target;
            if (Script == null) return;

            Undo.undoRedoPerformed += ApplyChanges;
        }

        public override void OnInspectorGUI()
        {

            GUILeftPosition();
            GUILayout.Space(8);
            GUIColor(ref SliderColor, ref Script.SliderColor, "Slider Color");
            GUIColor(ref DisabledColor, ref Script.DisabledColor, "Disabled Color");
            GUIColor(ref EnabledColor, ref Script.EnabledColor, "Enabled Color");


            if (UdonSharpInspector())
                if (!UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(Script))
                    DrawDefaultInspector();
        }

        void ApplyChanges()
        {
            Transform transform = Script.settings;
            if (transform != null)
            {
                Transform EllipseLong = transform.Find("EllipseLong");
                if (EllipseLong != null)
                {
                    SpriteRenderer EllipseColor = EllipseLong.GetComponent<SpriteRenderer>();
                    if (EllipseColor != null)
                    {
                        if (LeftPosition)
                            EllipseColor.color = Script.DisabledColor;
                        else EllipseColor.color = Script.EnabledColor;
                    }


                }
                Transform Ellipse = transform.Find("Ellipse");
                if (Ellipse != null)
                {
                    if (LeftPosition)
                        Ellipse.localPosition = new Vector3(-Range, 0, 0);
                    else Ellipse.localPosition = new Vector3(Range, 0, 0);

                    SpriteRenderer EllipseSprite = Ellipse.GetComponent<SpriteRenderer>();
                    if (EllipseSprite != null)
                    {
                        EllipseSprite.color = Script.SliderColor;
                    }
                }

            }
            UpdateScene();
        }

        void GUILeftPosition()
        {
            EditorGUI.BeginChangeCheck();

            LeftPosition = EditorGUILayout.Toggle("Left position button", Script.left);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Script, $"Bool LeftPosition {Script.GetType()} has been changed");
                Script.left = LeftPosition;

                ApplyChanges();

                Script.ApplyProxyModifications();
            }
        }

        private void GUIColor(ref Color color, ref Color colorScript, string NameGUI)
        {
            EditorGUI.BeginChangeCheck();

            color = EditorGUILayout.ColorField(new GUIContent(NameGUI), colorScript, true, true, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Script, $"Color {nameof(color)}|{NameGUI} has been changed");
                colorScript = color;
                ApplyChanges();
                Script.ApplyProxyModifications();
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
            Transform t;
            t = Script.settings.transform.Find("Ellipse");
            if (t != null)
            {
                t.gameObject.SetActive(!t.gameObject.activeSelf);
                t.gameObject.SetActive(!t.gameObject.activeSelf);
            }
        }
    }
}

