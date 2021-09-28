using Modding;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;

namespace HKVR
{
    public class HKVR : Mod
    {
        private GameObject _player;

        public override string GetVersion() => "1.0.0";

        public HKVR() : base("Hollow Knight VR") { }

        public override void Initialize()
        {
            GameManager.instance.StartCoroutine(InitXR());

            AssetManager.Initialize();
            On.HeroController.Start += OnHCStart;
        }

        private void OnHCStart(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            _player = Object.Instantiate(AssetManager.FetchAsset<GameObject>("Player"), self.transform);
            _player.transform.localPosition += Vector3.back * 25;
            _player.AddComponent<KeepWorldScalePositive>();

            Object.Destroy(GameObject.Find("_GameCameras/CameraParent/tk2dCamera"));
            GameCameras.instance.hudCamera.gameObject.SetActive(true);
            GameCameras.instance.hudCamera.transform.Find("Hud Canvas").position += Vector3.left * 10 + Vector3.up * 5;
        }
        private IEnumerator InitXR()
        {
            StopXR();

            XRSettings.LoadDeviceByName("OpenVR Display");
            XRSettings.LoadDeviceByName("OpenVR Input");

            var generalSettings = AssetManager.FetchAsset<XRGeneralSettings>("Standalone Settings");

            yield return new WaitWhile(() => XRGeneralSettings.Instance == null);

            var managerSettings = AssetManager.FetchAsset<XRManagerSettings>("Standalone Providers");

            XRGeneralSettings.Instance.Manager = managerSettings;
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            yield return new WaitUntil(() => XRGeneralSettings.Instance.Manager.isInitializationComplete);

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                LogError("XR initialization failed. Check Editor or Player log for details.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }

            yield return InitSteamVR();
        }

        private IEnumerator InitSteamVR()
        {
            SteamVR.Initialize(true);

            yield return new WaitUntil(() => SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess);

            Log("SteamVR successfully initialized.");
        }
        private void StopXR()
        {
            if (XRGeneralSettings.Instance == null) return;
            if (!XRGeneralSettings.Instance.Manager.isInitializationComplete) return;

            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}