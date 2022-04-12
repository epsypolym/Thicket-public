using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thicket
{
    class SceneConstructor : MonoBehaviour
    {
        public GameObject lsc;
        public AssetBundle levelbundle;
        public AssetBundle common;
        public AssetBundle ggm;
        public GameObject preload;
        public GameObject firstroom;
        public GameObject finalroom;
        public FinalerPit finalerpit;
        public ThicketSceneInfo tsi;
        public static SpawnableObject[] bestiary;

        public void FetchPrefabs(string bundlename)
        { 
            common = AssetBundle.LoadFromFile(Path.Combine(Thicket.commondir, "common"));
            levelbundle = AssetBundle.LoadFromFile(Path.Combine(Thicket.modsdir, bundlename));
        }

        public void SpawnRequirements(string levelname) 
        {
            GameObject go = new GameObject("ee");
            go.SetActive(false);
            tsi = levelbundle.LoadAsset<GameObject>(levelname).GetComponent<ThicketSceneInfo>();

            var preload = new GameObject("preload");
            var mib = preload.AddComponent<MapInfoBase>();
            var smi = preload.AddComponent<StockMapInfo>();

            mib.layerName = tsi.layername;
            mib.levelName = tsi.levelname;

            smi.layerName = tsi.layername;
            smi.levelName = tsi.levelname;

            var sman = GameObject.Instantiate(common.LoadAsset<GameObject>("StatsManager"), go.transform);
            var statman = sman.GetComponent<StatsManager>();
            statman.secretObjects = new GameObject[] { };

            statman.killRanks = tsi.killRanks;
            statman.styleRanks = tsi.styleRanks;
            statman.timeRanks = tsi.timeRanks;

            statman.enabled = true; // enable stat manager because it's off by default for some weird reason

            bestiary = Resources.FindObjectsOfTypeAll<SpawnableObjectsDatabase>().First(x => x.name == "Bestiary Database").enemies;



            firstroom = GameObject.Instantiate(common.LoadAsset<GameObject>("FirstRoom"), go.transform);
            firstroom.transform.position = tsi.firstroomtransformposition;
            firstroom.transform.rotation = Quaternion.Euler(tsi.firstroomtransformrotation);
            firstroom.transform.parent = go.transform;


            // FINAL ROOM SPAWN, POSITION AND RECONFIG
            finalroom = GameObject.Instantiate(common.LoadAsset<GameObject>("FinalRoom"), go.transform);
            finalroom.transform.position = tsi.finalroomtransformposition;
            finalroom.transform.rotation = Quaternion.Euler(tsi.finalroomtransformrotation);
            finalroom.transform.GetChild(0).GetChild(4).gameObject.SetActive(true); // open final room doors (handle via trigger later)
            Component.Destroy(finalroom.transform.GetChild(5).GetChild(8).gameObject.GetComponent<FinalPit>()); // DESTROY FINAL PIT - DO NOT ENABLE - RISK OF SAVE CORRUPTION
            finalerpit = finalroom.transform.GetChild(5).GetChild(8).gameObject.AddComponent<FinalerPit>(); // add finaler pit level transitioner

           
            go.SetActive(true);
            GameObject.Find("GameController").GetComponent<AudioMixerController>().SendMessage("Awake");
           

        }

        IEnumerator Init() 
        {
            //yield return new WaitForSeconds(0.25f);
            //test
            GameObject.Find("Player/Main Camera").GetComponent<CameraController>().enabled = true;
            GameObject.Find("Player/Main Camera").GetComponent<CameraController>().SendMessage("Awake");
            GameObject.Find("GameController").GetComponent<PostProcessV2_Handler>().SendMessage("Start");
            yield return new WaitForEndOfFrame();
            
        }

        public void ConstructLevel(string levelname)
        {
            //all preload operations
            SpawnRequirements(levelname);
            var scene = GameObject.Instantiate(levelbundle.LoadAsset<GameObject>(levelname));
            var pmc = GameObject.Find("Player/Main Camera");


            if(tsi.skyboxtexture == null)
            {
                pmc.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                Shader skyboxshader = common.LoadAsset<Shader>("assets/shaders/special/ultrakill_skybox.shader");
                pmc.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
                var psky = pmc.AddComponent<Skybox>();
                Material joe = new Material(skyboxshader);
                joe.mainTexture = tsi.skyboxtexture;
                psky.material = joe;
            }



            levelbundle.Unload(false);
            common.Unload(false);

            StartCoroutine(Init());
            finalerpit.targetlevel = tsi.nextlevel;
            finalerpit.targetbundle = tsi.nextbundle;


        }

        public void Start()
        {
            FetchPrefabs(Thicket.targetbundle);
            ConstructLevel(Thicket.targetlevel);
        }

        IEnumerator enablelevelstats()
        {
            lsc.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            lsc.SetActive(true);
        }
    }
}
