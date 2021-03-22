using System;
using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;


namespace BubBuy
{
    public class Main : IModuleNexus {

        public void initialize() {
            try {
                CommandWindow.Log("Starting BubBuy Module...");

                // http://wiki.unity3d.com/index.php?title=Singleton
                // https://forum.unity.com/threads/singleton-monobehaviour-script.99971/
                GameObject mObj = new GameObject();
                MBubBuy.Instance = mObj.AddComponent<MBubBuy>();
                mObj.name = typeof(MBubBuy).ToString() + " (Singleton)";
                GameObject.DontDestroyOnLoad(mObj);
                MBubBuy.Instance.init();

            } catch (Exception e) {
                CommandWindow.LogError("BubBuy_error_PIN1005: exception1 got cought: " + e.Message);
            }
        }

        public void shutdown() {

        }
    }
}
