using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UdonSharpEditor;
using UdonSharp;


namespace Mall1n.Canvas
{

    [CustomEditor(typeof(CanvasColorController))]
    internal class CanvasColorControllerEditor : Editor
    {

        private CanvasColorController CanvasScript;

        public Font fontText;


        [ColorUsage(true, true)]
        private Color colorLight;
        [ColorUsage(true, true)]
        private Color colorEnabled;
        [ColorUsage(true, true)]
        private Color colorDisabled;


        [ColorUsage(true, true)]
        public Color SliderColor;
        [ColorUsage(true, true)]
        public Color SliderBackgroundLight;
        [ColorUsage(true, true)]
        public Color SliderBackgroundDark;


        [ColorUsage(true, true)]
        public Color StripsColor;




        private bool udonSharpInspector = false;

        private void OnEnable()
        {
            CanvasScript = (CanvasColorController)target;
            if (CanvasScript == null) return;

            Undo.undoRedoPerformed += ColorsSend;
            Undo.undoRedoPerformed += ColorsSendStrips;
            Undo.undoRedoPerformed += FontSendStrips;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Text Font");
            GUIFontText();
            GUILayout.Label("Buttons");
            GUIColor(ref colorLight, ref CanvasScript.colorLight, "Color Light (NonButton Text)");
            GUIColor(ref colorEnabled, ref CanvasScript.colorEnabled, "Color Enabled");
            GUIColor(ref colorDisabled, ref CanvasScript.colorDisabled, "Color Disabled");
            GUILayout.Label("Sliders");
            GUIColor(ref SliderColor, ref CanvasScript.SliderColor, "Color Slider");
            GUIColor(ref SliderBackgroundLight, ref CanvasScript.SliderBackgroundLight, "Color SliderBackgroundLight");
            GUIColor(ref SliderBackgroundDark, ref CanvasScript.SliderBackgroundDark, "Color SliderBackgroundDark");
            GUILayout.Label("Strips");
            GUIColorStrips();


            if (UdonSharpInspector())
                if (!UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(CanvasScript))
                    DrawDefaultInspector();
        }


        private void GUIFontText()
        {
            EditorGUI.BeginChangeCheck();

            fontText = (Font)EditorGUILayout.ObjectField("Font Text", CanvasScript.fontText, typeof(Font), false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(CanvasScript, $"Color Strips has been changed");
                CanvasScript.fontText = fontText;
                FontSendStrips();
                CanvasScript.ApplyProxyModifications();
            }
        }

        private void GUIColorStrips()
        {
            EditorGUI.BeginChangeCheck();

            StripsColor = EditorGUILayout.ColorField(new GUIContent("Color Strips"), CanvasScript.StripsColor, true, true, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(CanvasScript, $"Color Strips has been changed");
                CanvasScript.StripsColor = StripsColor;
                ColorsSendStrips();
                CanvasScript.ApplyProxyModifications();
            }
        }

        private void GUIColor(ref Color color, ref Color colorScript, string NameGUI)
        {
            EditorGUI.BeginChangeCheck();

            color = EditorGUILayout.ColorField(new GUIContent(NameGUI), colorScript, true, true, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(CanvasScript, $"Color {nameof(color)}|{NameGUI} has been changed");
                colorScript = color;
                ColorsSend();
                CanvasScript.ApplyProxyModifications();
            }
        }

        private void GUIColorSlider(ref Color color, ref Color colorScript, string NameGUI)
        {
            EditorGUI.BeginChangeCheck();

            color = EditorGUILayout.ColorField(new GUIContent(NameGUI), colorScript, true, true, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(CanvasScript, $"Color {nameof(color)}|{NameGUI} has been changed");
                colorScript = color;
                ColorsSend();
                CanvasScript.ApplyProxyModifications();
            }
        }

        bool UdonSharpInspector()
        {
            EditorGUI.BeginChangeCheck();

            udonSharpInspector = EditorGUILayout.Foldout(udonSharpInspector, "UdonSharp Inspector", true);

            EditorGUI.EndChangeCheck();
            return udonSharpInspector;
        }

        private void ColorsSend()
        {
            Text[] transforms = CanvasScript.transform.GetComponentsInChildren<Text>(true);
            if (transforms != null)
                foreach (var item in transforms)
                {
                    if (item == null) continue;

                    Button button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        if (item.transform.Find("Slider") != null)
                        {
                            Slider slider = item.transform.Find("Slider").GetComponent<Slider>();
                            if (slider != null)
                            {
                                if (slider.gameObject.activeSelf == true)
                                {
                                    SetColorButton(button, CanvasScript.colorEnabled);
                                }
                                else
                                {
                                    SetColorButton(button, CanvasScript.colorDisabled);
                                }

                                SetColorSlider(slider);
                            }
                        }
                        else
                        {
                            if (SwitchObjectsOnOff(button)) continue;
                            else if (Pillowcolliders(button)) continue;
                            else if (ChangeMaterial(button)) continue;
                            else if (SwitchColliders(button)) continue;
                            else if (RespawnObjects(button)) continue;
                        }
                    }
                    else
                    {
                        SetColorText(item, CanvasScript.colorLight);
                    }
                }

            UpdateScene();
        }


        private void ColorsSendStrips()
        {
            Image[] images = CanvasScript.transform.GetComponentsInChildren<Image>();
            if (images != null)
                foreach (var item in images)
                {
                    if (item == null) continue;
                    if (item.transform.name.IndexOf("Slide") != -1)
                    {
                        SetColorImage(item, StripsColor);
                    }
                }
            UpdateScene();
        }

        private void FontSendStrips()
        {
            Text[] texts = CanvasScript.transform.GetComponentsInChildren<Text>();
            if (texts != null)
                foreach (var item in texts)
                {
                    if (item == null) continue;
                    item.font = CanvasScript.fontText;
                }
            UpdateScene();
        }

        // void SetColor<T>(T item, Color color)
        // {
        //     ColorBlock colorBlock = (item as Button).colors;
        //     colorBlock.normalColor = color;
        //     (item as Button).colors = colorBlock;
        // }

        bool RespawnObjects(Button item)
        {
            var scriptSwitch = item.transform.GetComponent<RespawnObjectsByMaster>();
            if (scriptSwitch != null)
            {
                SetColorButton(item, CanvasScript.colorEnabled);
                return true;
            }
            return false;
        }

        bool SwitchObjectsOnOff(Button item)
        {
            var scriptSwitch = item.transform.GetComponent<SwitchObjectsOnOff>();
            if (scriptSwitch != null)
            {
                if ((bool)scriptSwitch.GetComponent<UdonSharpBehaviour>().GetProgramVariable("Enabled") == true)
                    SetColorButton(item, CanvasScript.colorEnabled);
                else
                    SetColorButton(item, CanvasScript.colorDisabled);
                return true;
            }
            return false;
        }

        bool Pillowcolliders(Button item)
        {
            var scriptSwitch = item.transform.GetComponent<Pillowcolliders>();
            if (scriptSwitch != null)
            {
                if ((bool)scriptSwitch.GetComponent<UdonSharpBehaviour>().GetProgramVariable("Enabled") == true)
                    SetColorButton(item, CanvasScript.colorEnabled);
                else
                    SetColorButton(item, CanvasScript.colorDisabled);
                return true;
            }
            return false;
        }

        bool ChangeMaterial(Button item)
        {
            var scriptSwitch = item.transform.GetComponent<ChangeMaterial>();
            if (scriptSwitch != null)
            {

                if ((bool)scriptSwitch.GetComponent<UdonSharpBehaviour>().GetProgramVariable("Enabled") == true)
                    SetColorButton(item, CanvasScript.colorEnabled);
                else
                    SetColorButton(item, CanvasScript.colorDisabled);
                return true;
            }
            return false;
        }

        bool SwitchColliders(Button item)
        {
            var scriptSwitch = item.transform.GetComponent<SwitchColliders>();
            if (scriptSwitch != null)
            {
                //Debug.Log($"scriptSwitch Type = {scriptSwitch.GetType()} | scriptSwitch.Enabled = {scriptSwitch.Enabled}");
                if ((bool)scriptSwitch.GetComponent<UdonSharpBehaviour>().GetProgramVariable("Enabled") == true)
                    SetColorButton(item, CanvasScript.colorEnabled);
                else
                    SetColorButton(item, CanvasScript.colorDisabled);
                return true;
            }
            return false;
        }

        void SetColorImage(Image item, Color color)
        {
            item.color = color;
        }

        void SetColorText(Text item, Color color)
        {
            item.color = color;
        }

        void SetColorButton(Button item, Color color)
        {
            ColorBlock colorBlock = item.colors;
            colorBlock.normalColor = color;
            item.colors = colorBlock;
        }

        void SetColorSlider(Slider item)
        {
            ColorBlock colorBlock = item.colors;
            colorBlock.normalColor = CanvasScript.SliderColor;
            item.colors = colorBlock;
            item.transform.Find("Background").GetComponent<Image>().color = CanvasScript.SliderBackgroundDark;
            item.transform.Find("Fill Area/Fill").GetComponent<Image>().color = CanvasScript.SliderBackgroundLight;
        }

        private void UpdateScene()
        {
            Transform t = CanvasScript.transform.root.Find("Download");
            if (t != null)
            {
                t.gameObject.SetActive(!t.gameObject.activeSelf);
                t.gameObject.SetActive(!t.gameObject.activeSelf);
            }
        }
    }

}
