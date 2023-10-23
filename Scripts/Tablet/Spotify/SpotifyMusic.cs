using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
//using System;

namespace Mall1n.Spotify
{
    public class SpotifyMusic : UdonSharpBehaviour
    {
        public bool AutoDetect = false;
        public bool PlayOnAwake = false;

        [Range(0, 2)]
        public int PlayMode = 0; // 0 - Default | 1 - Repeat | 2 - Shuffle

        public AudioSource audioSource;
        public int Id = -1;
        public int List;
        [Range(2.0f, 20.0f)]
        public float speedSwitching = 8.0f;
        public Transform ListOfMusicMain;
        public Transform ListOfMusicSecond;
        public Transform BottomSpritesMain;
        public Transform SpritesBigScreen;

        public Image ToLeft = null;
        public Image ToRight = null;
        public Sprite[] images;
        public AudioClip[] music;
        public string[] SongsName;
        public string[] Artists;


        private int[] MusicId;
        private int[] MusicIdDefault;
        private bool isSwitching = false, Shuffled = false, Repeat = false, Paused = true, IdLog = false;
        private Text[] idsMain;
        private Text[] idsSecond;
        private Vector3 vectorMove;
        private Transform[] ListOfSpotsMain;
        private Transform[] ListOfSpotsSecond;
        private Transform[] EqualizersMain;
        private Transform[] EqualizersSecond;
        private BoxCollider[] TriggersMain;
        private float SecondsDeltaTime = 0;
        private int CurrentMusicTime = 0;
        private int TotalMusicTime = 0;

        #region Region MainSprites
        private Text Artist;
        private Text SongName;
        private Image MusicImage;
        private Image PlayImage;
        private Image PauseImage;
        public Image ShuffleImage;
        public Image RepeatImage;
        private Text TotalTime;
        private Text CurrentTime;
        private Slider MusicTimeSlider;
        #endregion


        #region Region BigScreenSprites
        private Text Artist_BS;
        private Text SongName_BS;
        private Image MusicImage_BS;
        private Image PlayImage_BS;
        private Image PauseImage_BS;
        public Image ShuffleImage_BS;
        public Image RepeatImage_BS;
        private Text TotalTime_BS;
        private Text CurrentTime_BS;
        private Slider MusicTimeSlider_BS;
        #endregion

        [Space]
        [ColorUsage(true, true)]
        public Color ColorEqualizer;

        void Start()
        {
            CheckMusic();
            EqualizerSetColor();
            Id = 0;

            PlayImage = BottomSpritesMain.Find("PlaySwitch/Play").GetComponent<Image>();
            PauseImage = BottomSpritesMain.Find("PlaySwitch/Pause").GetComponent<Image>();
            TotalTime = BottomSpritesMain.Find("Time/TotalTime").GetComponent<Text>();
            CurrentTime = BottomSpritesMain.Find("Time/CurrentTime").GetComponent<Text>();
            MusicTimeSlider = BottomSpritesMain.Find("TimeLine").GetComponent<Slider>();
            Artist = BottomSpritesMain.Find("Text/Artist").GetComponent<Text>();
            SongName = BottomSpritesMain.Find("Text/NameSong").GetComponent<Text>();
            MusicImage = BottomSpritesMain.Find("MaskImage/MusicIcon").GetComponent<Image>();

            PlayImage_BS = SpritesBigScreen.Find("PlaySwitch/Play").GetComponent<Image>();
            PauseImage_BS = SpritesBigScreen.Find("PlaySwitch/Pause").GetComponent<Image>();
            TotalTime_BS = SpritesBigScreen.Find("Time/TotalTime").GetComponent<Text>();
            CurrentTime_BS = SpritesBigScreen.Find("Time/CurrentTime").GetComponent<Text>();
            MusicTimeSlider_BS = SpritesBigScreen.Find("TimeLine").GetComponent<Slider>();
            Artist_BS = SpritesBigScreen.Find("Text/Artist").GetComponent<Text>();
            SongName_BS = SpritesBigScreen.Find("Text/NameSong").GetComponent<Text>();
            MusicImage_BS = SpritesBigScreen.Find("MaskImage/MusicIcon").GetComponent<Image>();

            TriggersMain = ListOfMusicMain.GetComponentsInChildren<BoxCollider>(true);
            TriggersMain = ResizeT(TriggersMain, "InteractTrigger", "AreaTrigger");

            ListOfSpotsMain = GetSpots(ListOfMusicMain, ListOfSpotsMain);
            ListOfSpotsSecond = GetSpots(ListOfMusicSecond, ListOfSpotsSecond);
            EqualizersMain = new Transform[ListOfSpotsMain.Length];
            for (int i = 0; i < EqualizersMain.Length; i++)
            {
                EqualizersMain[i] = ListOfSpotsMain[i].Find("Equalizer");
            }
            EqualizersSecond = new Transform[ListOfSpotsSecond.Length];
            for (int i = 0; i < EqualizersSecond.Length; i++)
            {
                EqualizersSecond[i] = ListOfSpotsSecond[i].Find("Equalizer");
            }

            MusicIdDefault = new int[music.Length];

            for (int i = 0; i < MusicIdDefault.Length; i++)
                MusicIdDefault[i] = i;
            MusicIdDefault = DeleteArrayId();
            MusicId = new int[MusicIdDefault.Length];
            MusicIdDefault.CopyTo(MusicId, 0);

            if (PlayMode == 1) Repeat = true;
            else Repeat = false;
            if (PlayMode == 2)
            {
                Shuffled = false;
                Id = Random.Range(0, MusicId.Length);
            }
            else Shuffled = true;

            ShuffleModeSwitch(); 

            PlaySong();
            if (!PlayOnAwake)
            {
                Pause();
            }

            DefineMain(List);
            CheckArrows();

        }


        private void Update()
        {
            if (isSwitching)
            {
                ListOfMusicMain.localPosition = Vector3.Lerp(ListOfMusicMain.localPosition, vectorMove, speedSwitching * Time.deltaTime);
                ListOfMusicSecond.localPosition = Vector3.Lerp(ListOfMusicSecond.localPosition, Vector3.zero, speedSwitching * Time.deltaTime);
                if (ListOfMusicSecond.localPosition.x < 0.1f && ListOfMusicSecond.localPosition.x > -0.1f)
                {
                    isSwitching = false;
                    DefineMain(List);
                    CheckArrows();
                    ListOfMusicMain.localPosition = Vector3.zero;
                    TriggersSwitch(TriggersMain, true);
                    ListOfMusicSecond.gameObject.SetActive(false);
                }
            }

            //if (audioSource.isPlaying)
            if (!Paused)
            {
                SecondsDeltaTime += Time.deltaTime;
                if (Mathf.Floor(SecondsDeltaTime) != CurrentMusicTime)
                {
                    CurrentMusicTime = (int)Mathf.Floor(SecondsDeltaTime);
                    if (CurrentMusicTime != TotalMusicTime)
                    {
                        string s = (CurrentMusicTime % 60).ToString();
                        string result = $"{Mathf.Floor(CurrentMusicTime / 60)}:{(s = s.Length == 1 ? $"0{s}" : s)}";
                        CurrentTime.text = result;
                        CurrentTime_BS.text = result;
                        float Value = (float)CurrentMusicTime / (float)TotalMusicTime;
                        MusicTimeSlider.value = Value;
                        MusicTimeSlider_BS.value = Value;
                    }
                    else
                    {
                        if (!Repeat) IdAdd();
                        PlaySong();
                    }
                }
            }
            // else if (!audioSource.isPlaying && !Paused)
            // {

            // }

        }

        private void EqualizerSetColor()
        {
            Transform[] t = transform.GetComponentsInChildren<Transform>(true);
            if (t != null)
                foreach (var item in t)
                {
                    if (item.name == "Equalizer")
                    {
                        Image image = item.GetComponent<Image>();
                        if (image != null)
                            image.color = ColorEqualizer;
                    }
                }
        }

        public void PauseFromCanvas()
        {
            if (!Paused)
                Pause();
        }

        public void PlayFromCanvas()
        {
            if (Paused)
                Play();
        }

        public void SwitchPause()
        {
            if (!Paused)
                Pause();
            else
                Play();
        }

        private void Pause()
        {
            audioSource.Pause();
            PauseImageOn();
            Paused = true;

            int i = MusicId[Id] - List * 9;
            if (i < 10 && i > -1)
                StateEqualizerMain(i, false);
        }

        private void Play()
        {
            audioSource.Play();
            PlayImageOn();
            Paused = false;

            int i = MusicId[Id] - List * 9;
            if (i < 10 && i > -1)
                StateEqualizerMain(i, true);
        }

        private void StateEqualizerMain(int id, bool start)
        {
            Animator t = EqualizersMain[id].GetComponent<Animator>();
            if (t != null)
            {
                if (start)
                    t.StopPlayback();
                else
                    t.StartPlayback();
            }
        }

        private void StateEqualizerSecond(int id, bool start)
        {
            Animator t = EqualizersSecond[id].GetComponent<Animator>();
            if (t != null)
            {
                if (start)
                    t.StopPlayback();
                else
                    t.StartPlayback();
            }
        }

        private void UpdatePauseImage()
        {
            if (Paused)
                PauseImageOn();
            else
                PlayImageOn();
        }

        private void PauseImageOn()
        {
            PlayImage.gameObject.SetActive(true);
            PlayImage_BS.gameObject.SetActive(true);
            PauseImage.gameObject.SetActive(false);
            PauseImage_BS.gameObject.SetActive(false);
        }

        private void PlayImageOn()
        {
            PlayImage.gameObject.SetActive(false);
            PlayImage_BS.gameObject.SetActive(false);
            PauseImage.gameObject.SetActive(true);
            PauseImage_BS.gameObject.SetActive(true);
        }

        private void CheckMusic()
        {
            int count = 0;
            for (int i = 0; i < music.Length; i++)
                if (music[i] == null) count++;
            if (count == 0) this.enabled = false;
            if (count == music.Length) this.enabled = false;
            if (audioSource == null) this.enabled = false;
        }

        public void ShuffleModeSwitch()
        {
            if (!Shuffled)
            {
                ShuffleImage.color = new Color(0.0f, 0.75f, 0.3f);
                ShuffleImage_BS.color = new Color(0.0f, 0.75f, 0.3f);
                int temp = MusicId[0];
                MusicId[0] = MusicId[Id];
                MusicId[Id] = temp;
                Id = 0;
                for (int i = 1; i < MusicId.Length; i++)
                {
                    int j = UnityEngine.Random.Range(i, MusicId.Length);
                    temp = MusicId[j];
                    MusicId[j] = MusicId[i];
                    MusicId[i] = temp;
                }
                Shuffled = true;
            }
            else
            {
                for (int i = 0; i < MusicIdDefault.Length; i++)
                    if (MusicIdDefault[i] == MusicId[Id]) { Id = i; break; }
                MusicIdDefault.CopyTo(MusicId, 0);
                ShuffleImage.color = Color.white;
                ShuffleImage_BS.color = Color.white;
                Shuffled = false;
            }
        }

        public void RepeatSwitch()
        {
            if (!Repeat)
            {
                RepeatImage.color = new Color(0.0f, 0.75f, 0.3f);
                RepeatImage_BS.color = new Color(0.0f, 0.75f, 0.3f);
                Repeat = true;
            }
            else
            {
                RepeatImage.color = Color.white;
                RepeatImage_BS.color = Color.white;
                Repeat = false;
            }
        }


        void CheckArrows()
        {
            if (music.Length <= (List + 1) * 9)
                SetArrow(ToRight, Color.grey, false);
            else SetArrow(ToRight, Color.white, true);
            if (List == 0)
                SetArrow(ToLeft, Color.grey, false);
            else SetArrow(ToLeft, Color.white, true);
        }

        private void SetArrow(Image image, Color color, bool b)
        {
            image.color = color;
            image.transform.GetChild(0).gameObject.SetActive(b);
        }

        public void SwitchToRight()
        {
            if (!isSwitching && music.Length > (List + 1) * 9)
            {
                TriggersSwitch(TriggersMain, false);

                List++;

                ListOfMusicSecond.localPosition = new Vector3(175.0f, 0.0f, 0.0f);
                vectorMove = new Vector3(-175.0f, 0.0f, 0.0f);

                DefineSecond(List);

                isSwitching = true;
            }
        }

        public void SwitchToLeft()
        {
            if (!isSwitching && List != 0)
            {
                List--;

                ListOfMusicSecond.localPosition = new Vector3(-175.0f, 0.0f, 0.0f);
                vectorMove = new Vector3(175.0f, 0.0f, 0.0f);

                DefineSecond(List);

                isSwitching = true;
            }
        }

        private void DefineMain(int list)
        {
            int count = music.Length - list * 9;
            int id = -1;
            if (count > 9) count = 9;
            if (IdLog)
                idsMain = new Text[count];

            if (ListOfSpotsMain != null)
            {
                for (int i = 0; i < ListOfSpotsMain.Length; i++)
                {
                    if (i < count)
                    {
                        id = i + list * 9;

                        if (MusicId[Id] == id)
                        {
                            EqualizersMain[i].gameObject.SetActive(true);
                            if (Paused)
                            {
                                StateEqualizerMain(i, false);
                            }
                            else StateEqualizerMain(i, true);
                        }
                        else EqualizersMain[i].gameObject.SetActive(false);

                        Transform transformTrigger = ListOfSpotsMain[i].Find("Trigger");
                        if (transformTrigger != null)
                        {
                            UdonBehaviour u = (UdonBehaviour)transformTrigger.GetComponent(typeof(UdonBehaviour));
                            u.SetProgramVariable("SendId", id);
                        }

                        if (IdLog)
                        {
                            idsMain[i] = ListOfSpotsMain[i].Find("Text/id").GetComponent<Text>();
                            idsMain[i].text = id.ToString();
                        }

                        ListOfSpotsMain[i].gameObject.SetActive(true);

                        Image image = ListOfSpotsMain[i].GetComponent<Image>();
                        if (image != null)
                        {
                            image.sprite = images[id];
                            if (image.sprite == null)
                                image.color = new Color(0.0f, 0.5f, 0.2f);
                            else image.color = Color.white;
                        }
                        ListOfSpotsMain[i].Find("Text/NameSong").GetComponent<Text>().text = SongsName[id];
                        ListOfSpotsMain[i].Find("Text/Artist").GetComponent<Text>().text = Artists[id];

                        if (transformTrigger != null)
                        {
                            if (music[id] == null)
                                transformTrigger.gameObject.SetActive(false);
                            else transformTrigger.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        ListOfSpotsMain[i].gameObject.SetActive(false);
                    }

                }
            }
        }

        private void DefineSecond(int list)
        {
            int count = music.Length - list * 9;
            int id = -1;
            if (count > 9) count = 9;
            if (IdLog)
                idsSecond = new Text[count];
            if (ListOfSpotsSecond != null)
            {
                ListOfMusicSecond.gameObject.SetActive(true);
                for (int i = 0; i < ListOfSpotsSecond.Length; i++)
                {
                    if (i < count)
                    {
                        id = i + list * 9;

                        if (MusicId[Id] == id)
                        {
                            EqualizersSecond[i].gameObject.SetActive(true);
                            if (Paused)
                            {
                                StateEqualizerSecond(i, false);
                            }
                            else StateEqualizerSecond(i, true);
                        }
                        else EqualizersSecond[i].gameObject.SetActive(false);

                        Transform transformTrigger = ListOfSpotsMain[i].Find("Trigger");
                        if (transformTrigger != null)
                        {
                            UdonBehaviour u = (UdonBehaviour)transformTrigger.GetComponent(typeof(UdonBehaviour));
                            u.SetProgramVariable("SendId", id);
                        }

                        if (IdLog)
                        {
                            idsSecond[i] = ListOfSpotsSecond[i].Find("Text/id").GetComponent<Text>();
                            idsSecond[i].text = id.ToString();
                        }

                        ListOfSpotsSecond[i].gameObject.SetActive(true);

                        Image image = ListOfSpotsSecond[i].GetComponent<Image>();
                        if (image != null)
                        {
                            image.sprite = images[id];
                            if (image.sprite == null)
                                image.color = new Color(0.0f, 0.5f, 0.2f);
                            else image.color = Color.white;
                        }
                        ListOfSpotsSecond[i].Find("Text/NameSong").GetComponent<Text>().text = SongsName[id];
                        ListOfSpotsSecond[i].Find("Text/Artist").GetComponent<Text>().text = Artists[id];
                    }
                    else ListOfSpotsSecond[i].gameObject.SetActive(false);
                }
                //ListOfMusicSecond.gameObject.SetActive(true);
            }
        }

        private void DefineEqualizer()
        {
            int count = music.Length - List * 9;
            int id = -1;
            if (count > 9) count = 9;

            for (int i = 0; i < EqualizersMain.Length; i++)
            {
                id = i + List * 9;

                if (i < count)
                {
                    if (MusicId[Id] == id) EqualizersMain[i].gameObject.SetActive(true);
                    else EqualizersMain[i].gameObject.SetActive(false);
                }
                else break;
            }
        }

        public void SetCustomMusic()
        {
            if (Id != -1)
            {
                for (int i = 0; i < MusicId.Length; i++)
                {
                    if (MusicId[i] != Id) continue;
                    Id = i;
                    break;
                }
                if (audioSource.clip == music[MusicId[Id]])
                    SwitchPause();
                else
                    PlaySong();
            }
        }

        private void IdAdd()
        {
            Id++; if (Id == MusicIdDefault.Length) Id = 0;
        }

        private void IdMinus()
        {
            Id--; if (Id == -1) Id = MusicIdDefault.Length - 1;
        }

        public void PreviousSong()
        {
            IdMinus();

            PlaySong();
        }

        public void NextSong()
        {
            IdAdd();

            PlaySong();
        }

        private void PlaySong()
        {
            if (audioSource != null && Id != -1)
            {
                audioSource.clip = music[MusicId[Id]];
                audioSource.Play();
                Artist.text = Artists[MusicId[Id]];
                Artist_BS.text = Artists[MusicId[Id]];
                SongName.text = SongsName[MusicId[Id]];
                SongName_BS.text = SongsName[MusicId[Id]];
                MusicImage.sprite = images[MusicId[Id]];
                MusicImage_BS.sprite = images[MusicId[Id]];
                TotalMusicTime = (int)Mathf.Ceil(audioSource.clip.length);
                CurrentMusicTime = 0;
                SecondsDeltaTime = 0;
                MusicTimeSlider.value = 0;
                MusicTimeSlider_BS.value = 0;
                Paused = false;

                if (TotalTime != null)
                {
                    string s = (TotalMusicTime % 60).ToString();
                    TotalTime.text = $"{Mathf.Floor(TotalMusicTime / 60)}:{(s = s.Length == 1 ? $"0{s}" : s)}";
                }
                if (CurrentTime != null) CurrentTime.text = "0:00";

                if (TotalTime_BS != null)
                {
                    string s = (TotalMusicTime % 60).ToString();
                    TotalTime_BS.text = $"{Mathf.Floor(TotalMusicTime / 60)}:{(s = s.Length == 1 ? $"0{s}" : s)}";
                }
                if (CurrentTime_BS != null) CurrentTime_BS.text = "0:00";

                DefineEqualizer();
                UpdatePauseImage();
            }
        }

        private void TriggersSwitch(BoxCollider[] t, bool b)
        {
            foreach (BoxCollider item in t)
                if (item.transform.parent.gameObject.activeSelf == true)
                    item.gameObject.SetActive(b);
        }

        Transform[] GetSpots(Transform t, Transform[] aim)
        {
            int count = 0;
            foreach (Transform item in t)
                if (item.name.IndexOf("Spot") != -1)
                    count++;
            aim = new Transform[count];
            count = 0;
            foreach (Transform item in t)
                if (item.name.IndexOf("Spot") != -1)
                {
                    aim[count] = item;
                    count++;
                }
            return aim;
        }

        private BoxCollider[] ResizeT(BoxCollider[] t, string s, string ss)
        {
            int Count = 0;
            foreach (BoxCollider item in t)
                if (item.name.IndexOf(s) != -1 || item.name.IndexOf(ss) != -1)
                    Count++;
            BoxCollider[] temp = new BoxCollider[Count];
            Count = 0;
            foreach (BoxCollider item in t)
                if (item.name.IndexOf(s) != -1 || item.name.IndexOf(ss) != -1)
                {
                    temp[Count] = item;
                    Count++;
                }
            return t;
        }

        private int[] DeleteArrayId()
        {
            int length = MusicIdDefault.Length;
            for (int i = 0; i < MusicIdDefault.Length; i++)
            {
                if (music[i] == null)
                {
                    MusicIdDefault[i] = -1;
                    length--;
                }
            }
            int[] temp = new int[length];
            length = 0;
            for (int i = 0; i < MusicIdDefault.Length; i++)
                if (MusicIdDefault[i] != -1)
                {
                    temp[length] = MusicIdDefault[i];
                    length++;
                }
            return temp;
        }

    }

}
