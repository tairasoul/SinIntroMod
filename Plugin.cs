using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace IntroMusicAdder
{
    internal static class GameObjectExtensions {
        public static GameObject Find(this GameObject @object, string name) {
            return @object.transform.Find(name).gameObject;
        }
    }

    [BepInPlugin("tairasoul.vaproxy.sinintro", "SinIntro", "1.0.0")]
    public class Plugin: BaseUnityPlugin
    {
        // cutscene length: 21 seconds
        // sin intro: 16 seconds

        ConfigEntry<float> WaitTime;
        ConfigEntry<float> Volume;

        AudioSource source;
        bool init = false;
        void Awake() {
            WaitTime = Config.Bind("Times", "Wait", 4.7f, "Time to wait before starting the intro clip.");
            Volume = Config.Bind("Volume", "Volume", 0.5f, "How loud the clip will be.");
            Logger.LogInfo("sin intro awake!! real");
        }

        AudioClip GetSinIntro() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string IntroName = "IntroMusicAdder.sinintro";
            using Stream stream = assembly.GetManifestResourceStream(IntroName);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            return bundle.LoadAsset<AudioClip>("assets/Sin Intro.wav");
        }

        void Start() => Init();

        void OnDestroy() => Init();

        void Init() {
            if (init) return;
            init = true;
            GameObject SourceHolder = new("SinIntro");
            DontDestroyOnLoad(SourceHolder);
            source = SourceHolder.AddComponent<AudioSource>();
            source.clip = GetSinIntro();
            source.volume = Volume.Value;
            source.loop = false;
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => {
                if (scene.name != "Menu" && scene.name != "Intro") {
                    OnTrigger trigger = GameObject.Find("Director").Find("Cutscene2").Find("Cube (1)").GetComponent<OnTrigger>();
                    trigger.IN.AddListener(() => {
                        if (GameObject.Find("Director").Find("Cutscene2").activeSelf) {
                            Logger.LogInfo("Cutscene 2 started.");
                            StartCoroutine(DoScene());
                        }
                    });
                }
            };
        }
        
        internal IEnumerator DoScene() {
            yield return new WaitForSeconds(WaitTime.Value);
            source.Play();
        }
    }
}
