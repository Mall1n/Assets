
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using UnityEngine.UI;

namespace Mall1n.Spotify
{
    [CustomEditor(typeof(SpotifyMusic))]
    public class SpotifyMusicEditor : Editor
    {

        private SpotifyMusic SpotifyScript;

        List<Music> listMusic = new List<Music>();

        private bool udonSharpInspector = true;


        private bool AutoDetect = false;
        private bool PlayOnAwake = true;
        private GUIStyle StyleButtonBig;
        private GUIStyle StyleLabelBig;
        private GUIStyle StyleLabelSmall;
        private PlayMode playMode;
        private List<Image> SpotImages;
        private List<Text> SpotNameSongs;
        private List<Text> SpotArtists;
        private Image Shuffle;
        private Image Loop;
        private Image Shuffle_BS;
        private Image Loop_BS;
        private int list = 0;
        private float speed = 8.0f;

        private void OnEnable()
        {
            SpotifyScript = target as SpotifyMusic;
            if (SpotifyScript == null)
                return;

            SpotNameSongs = new List<Text>();
            SpotArtists = new List<Text>();
            SpotImages = new List<Image>();
            Image[] tempImages = SpotifyScript.transform.Find("ImageButtons/MaskMusic/ListMain")?.GetComponentsInChildren<Image>(true);
            if (tempImages != null && tempImages.Length != 0)
                SpotImages = new List<Image>(tempImages);
            for (int i = SpotImages.Count - 1; i >= 0; i--)
            {
                if (SpotImages[i].name.IndexOf("Spot") == -1)
                    SpotImages.RemoveAt(i);
                else
                {
                    Text temp = null;

                    temp = SpotImages[i].transform.Find("Text/NameSong")?.GetComponent<Text>();
                    if (temp != null) SpotNameSongs.Insert(0, temp);

                    temp = SpotImages[i].transform.Find("Text/Artist")?.GetComponent<Text>();
                    if (temp != null) SpotArtists.Insert(0, temp);
                }
            }

            if (SpotifyScript.images != null)
                RessurectListMusic();
            //Debug.Log($"list count = {SpotifyScript.images.Length}");

            Undo.undoRedoPerformed += RessurectListMusic;
        }

        public override void OnInspectorGUI()
        {
            if (SpotifyScript == null)
                return;

            StyleButtonBig = new GUIStyle(GUI.skin.button);
            StyleButtonBig.alignment = TextAnchor.MiddleCenter;
            StyleButtonBig.fontSize = 20;

            StyleLabelBig = new GUIStyle(GUI.skin.label);
            StyleLabelBig.alignment = TextAnchor.MiddleCenter;
            StyleLabelBig.fontSize = 20;

            StyleLabelSmall = new GUIStyle(GUI.skin.label);
            StyleLabelSmall.alignment = TextAnchor.MiddleCenter;
            StyleLabelSmall.wordWrap = true;

            GUILayout.Space(10);
            GUIButtonAddClip();
            GUILayout.Space(10);
            GUIAutoDetectToggle();
            GUIPlayOnAwake();
            GUILayout.Space(10);
            PlayModeDrop();
            GUILayout.Space(10);
            GUIInputField();
            GUIInputFieldSpeed();
            GUILayout.Space(10);
            ShowList();
            if (UdonSharpInspector())
            {
                if (!UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(SpotifyScript))
                    DrawDefaultInspector();
            }
        }

        // void GUIToggle(ref bool RefToUdonBool, ref bool BoolDefault, string nameToggle)
        // {
        //     EditorGUI.BeginChangeCheck();

        //     BoolDefault = EditorGUILayout.Toggle(nameToggle, RefToUdonBool);

        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         Undo.RecordObject(SpotifyScript, $"bool {RefToUdonBool} has been changed");
        //         RefToUdonBool = BoolDefault;
        //         UpdateListMusic();
        //         SpotifyScript.ApplyProxyModifications();
        //     }
        // }

        void GUIInputField()
        {
            EditorGUI.BeginChangeCheck();

            list = EditorGUILayout.IntField("Number of list in playlist", SpotifyScript.List);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpotifyScript, "int list has been changed");
                //Debug.Log($"{list} > {Mathf.Ceil((listMusic.Count - 1) / 9)}");
                if (list < 0)
                    list = 0;
                else if (list > Mathf.Ceil((listMusic.Count - 1) / 9))
                    list = (int)Mathf.Ceil((listMusic.Count - 1) / 9);

                SpotifyScript.List = list;
                UpdateImagesOnScene();
                //SpotifyScript.ApplyProxyModifications();
                //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
            }
        }

        void GUIInputFieldSpeed()
        {
            EditorGUI.BeginChangeCheck();

            speed = EditorGUILayout.Slider("Switching speed", SpotifyScript.speedSwitching, 2.0f, 20.0f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpotifyScript, "float speed has been changed");
                SpotifyScript.speedSwitching = speed;
                //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
                //potifyScript.ApplyProxyModifications();
                //UpdateImagesOnScene(listMusic.Count, list);
            }
        }

        void GUIAutoDetectToggle()
        {
            EditorGUI.BeginChangeCheck();

            AutoDetect = EditorGUILayout.Toggle("Try Auto Detect Name and Artist?", SpotifyScript.AutoDetect);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpotifyScript, $"bool {SpotifyScript.AutoDetect} has been changed");
                SpotifyScript.AutoDetect = AutoDetect;
                if (AutoDetect)
                    AutoDetectNameMusic();
                UpdateListMusic();
                //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
                //SpotifyScript.ApplyProxyModifications();
            }
        }

        void GUIPlayOnAwake()
        {
            EditorGUI.BeginChangeCheck();

            PlayOnAwake = EditorGUILayout.Toggle("Play On Awake", SpotifyScript.PlayOnAwake);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpotifyScript, $"bool {SpotifyScript.PlayOnAwake} has been changed");
                SpotifyScript.PlayOnAwake = PlayOnAwake;
                //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
                //SpotifyScript.ApplyProxyModifications();
            }
        }

        void GUIButtonAddClip()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("Add Clip", StyleButtonBig, GUILayout.Width(200), GUILayout.Height(32)))
            {
                Undo.RecordObject(SpotifyScript, "Added Music");
                listMusic.Add(new Music(this));
                UpdateListMusic();
                //SpotifyScript.ApplyProxyModifications();
            }
            GUILayout.Label("Edit playlist in spotify |", StyleLabelBig, GUILayout.Height(32));
            GUILayout.Label("It is not recommended to make any changes above UdonSharp Inspector", StyleLabelSmall);
            GUILayout.EndHorizontal();

            //EditorGUI.EndChangeCheck();
        }

        bool UdonSharpInspector()
        {
            EditorGUI.BeginChangeCheck();

            udonSharpInspector = EditorGUILayout.Foldout(udonSharpInspector, "UdonSharp Inspector", true);

            EditorGUI.EndChangeCheck();
            return udonSharpInspector;
        }

        void PlayModeDrop()
        {
            EditorGUI.BeginChangeCheck();

            PlayMode playMode = (PlayMode)EditorGUILayout.EnumPopup("PlayMode Type", (PlayMode)SpotifyScript.PlayMode);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpotifyScript, "Playmode has been Changed");
                SpotifyScript.PlayMode = (int)playMode;
                switch (playMode)
                {
                    case PlayMode.Default:
                        Loop.color = Color.white;
                        Shuffle.color = Color.white;
                        Loop_BS.color = Color.white;
                        Shuffle_BS.color = Color.white;
                        break;
                    case PlayMode.Repeat:
                        Loop.color = new Color(0.0f, 0.75f, 0.3f);
                        Shuffle.color = Color.white;
                        Loop_BS.color = new Color(0.0f, 0.75f, 0.3f);
                        Shuffle_BS.color = Color.white;
                        break;
                    case PlayMode.Shuffle:
                        Loop.color = Color.white;
                        Shuffle.color = new Color(0.0f, 0.75f, 0.3f);
                        Loop_BS.color = Color.white;
                        Shuffle_BS.color = new Color(0.0f, 0.75f, 0.3f);
                        break;
                }
                UpdateScene();
                //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
                //SpotifyScript.ApplyProxyModifications();
            }
        }

        void ShowList()
        {
            for (int i = 0; i < listMusic.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                listMusic[i].ShowBox();

                GUILayout.BeginVertical();

                if (GUILayout.Button("╳", StyleButtonBig, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    listMusic.RemoveAt(i);
                    if (listMusic.Count <= list * 9 && listMusic.Count != 0)
                    {
                        list--;
                        SpotifyScript.List = list;
                    }
                    UpdateListMusic();
                }
                GUILayout.Space(2);
                if (GUILayout.Button("↑", StyleButtonBig, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    if (i > 0)
                    {
                        listMusic.Insert(i - 1, listMusic[i]);
                        listMusic.RemoveAt(i + 1);
                        UpdateListMusic();
                    }
                }
                GUILayout.Space(2);
                if (GUILayout.Button("↓", StyleButtonBig, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    if (i < listMusic.Count - 1)
                    {
                        listMusic.Insert(i + 2, listMusic[i]);
                        listMusic.RemoveAt(i);
                        UpdateListMusic();
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        public void UpdateListMusic()
        {
            Undo.RecordObject(SpotifyScript, "AddMusicToUdon");

            // if (listMusic.Count <= list * 9)
            //     list--;

            int count = listMusic.Count;
            Sprite[] sprites = new Sprite[count];
            AudioClip[] audioSources = new AudioClip[count];
            string[] SongNames = new string[count];
            string[] SongArtists = new string[count];
            for (int i = 0; i < count; i++)
            {
                //listMusic[i].AutoDetectNameMusic();

                sprites[i] = listMusic[i].image;
                audioSources[i] = listMusic[i].music;
                SongNames[i] = listMusic[i].Name;
                SongArtists[i] = listMusic[i].Artist;
            }
            SpotifyScript.images = sprites;
            SpotifyScript.music = audioSources;
            SpotifyScript.SongsName = SongNames;
            SpotifyScript.Artists = SongArtists;

            //Debug.Log(count + " = Count");
            UpdateImagesOnScene();

            //UdonSharpEditorUtility.CopyProxyToUdon(SpotifyScript, ProxySerializationPolicy.All);
            //SpotifyScript.ApplyProxyModifications();
        }

        public void AutoDetectNameMusic()
        {
            foreach (var item in listMusic)
                item.AutoDetectNameMusic();
        }


        public void RessurectListMusic()
        {
            int count = SpotifyScript.images.Length;
            listMusic = new List<Music>();
            for (int i = 0; i < count; i++)
            {
                listMusic.Add(new Music(SpotifyScript.images[i], SpotifyScript.music[i], SpotifyScript.SongsName[i], SpotifyScript.Artists[i], this));
            }
            AutoDetect = SpotifyScript.AutoDetect;
            list = SpotifyScript.List;
            speed = SpotifyScript.speedSwitching;
            Shuffle = SpotifyScript.ShuffleImage;
            Loop = SpotifyScript.RepeatImage;
            Shuffle_BS = SpotifyScript.ShuffleImage_BS;
            Loop_BS = SpotifyScript.RepeatImage_BS;
            if (list * 9 >= listMusic.Count && listMusic.Count != 0)
                list--;
            UpdateImagesOnScene();
        }

        void UpdateImagesOnScene()
        {
            int listid = list * 9;
            int count = listMusic.Count - list * 9;
            if (count > 9) count = 9;
            for (int i = 0; i < count; i++)
            {
                if (SpotImages[i] != null)
                    SpotImages[i].gameObject.SetActive(true);
                if (listMusic[i + listid].image != null)
                {
                    SpotImages[i].sprite = listMusic[i + listid].image;
                    SpotImages[i].color = Color.white;
                }
                else
                {
                    SpotImages[i].sprite = null;
                    SpotImages[i].color = new Color(0.0f, 0.5f, 0.2f);
                }
                if (i < SpotNameSongs.Count)
                    if (listMusic[i + listid].Name.Length < 30)
                        SpotNameSongs[i].text = listMusic[i + listid].Name;
                    else SpotNameSongs[i].text = $"{listMusic[i + listid].Name.Substring(0, 30)}...";
                if (i < SpotArtists.Count)
                    if (listMusic[i + listid].Artist.Length < 25)
                        SpotArtists[i].text = listMusic[i + listid].Artist;
                    else SpotArtists[i].text = $"{listMusic[i + listid].Artist.Substring(0, 25)}...";
            }
            for (int i = count; i < SpotImages.Count; i++)
            {
                if (SpotImages[i] == null) continue;
                SpotImages[i].sprite = null;
                SpotImages[i].gameObject.SetActive(false);
            }

            UpdateScene();
        }

        private void UpdateScene()
        {
            if (SpotifyScript != null)
            {
                Transform temp = SpotifyScript.transform.Find("ActualPosition");
                if (temp.gameObject != null)
                {
                    temp.gameObject.SetActive(!temp.gameObject.activeSelf);
                    temp.gameObject.SetActive(!temp.gameObject.activeSelf);
                }
            }
        }

        internal protected class Music
        {
            public Sprite image;
            public AudioClip music;
            public string Name;
            public string Artist;

            private SpotifyMusicEditor spotifyMusicEditor;

            public Music(SpotifyMusicEditor s)
            {
                Name = "";
                Artist = "";
                this.spotifyMusicEditor = s;
            }

            public Music(Sprite image, AudioClip music, string Name, string Artist, SpotifyMusicEditor s) : this(s)
            {
                this.image = image;
                this.music = music;
                this.Name = Name;
                this.Artist = Artist;
            }


            public void ShowBox()
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);

                image = (Sprite)EditorGUILayout.ObjectField(image, typeof(Sprite), false, GUILayout.Height(96), GUILayout.Width(96));

                EndChangeCheck(false);

                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Song File", GUILayout.Width(96), GUILayout.Height(32));
                music = (AudioClip)EditorGUILayout.ObjectField(music, typeof(AudioClip), false, GUILayout.Height(32));
                EndChangeCheck(true);

                EditorGUI.BeginChangeCheck();

                GUILayout.EndHorizontal();

                Label(ref Name, "Song Name");

                Label(ref Artist, "Artist");

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                EndChangeCheck(false);

                void Label(ref string RefString, string NameLabel)
                {
                    EditorGUILayout.Space(8);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(NameLabel, GUILayout.Width(96));
                    RefString = EditorGUILayout.TextField(RefString);
                    GUILayout.EndHorizontal();
                }

                void EndChangeCheck(bool DetectNameMusic)
                {
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (DetectNameMusic)
                            AutoDetectNameMusic();
                        spotifyMusicEditor.UpdateListMusic();
                    }
                }
            }

            public void AutoDetectNameMusic()
            {
                if (spotifyMusicEditor.AutoDetect && music != null)
                {
                    int index = music.name.IndexOf(" - ");
                    if (index != -1)
                    {
                        //Debug.Log($"{music.name.Substring(0, index)} | {music.name.Substring(index + 3, music.name.Length - index - 3)}");
                        if (Artist == "")
                            Artist = music.name.Substring(0, index);
                        if (Name == "")
                            Name = music.name.Substring(index + 3, music.name.Length - index - 3);
                    }
                }
            }

        }

    }

    public enum PlayMode
    {
        Default,
        Repeat,
        Shuffle
    }

}

