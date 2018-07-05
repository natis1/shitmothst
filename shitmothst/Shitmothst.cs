using System;
using System.Linq;
using Modding;

namespace shitmothst
{
    // ReSharper disable once InconsistentNaming because it's the name I want to appear on Modding API.
    // ReSharper disable once UnusedMember.Global because it's used implicitly but importing rider extensions is dumb.
    public class Shitmothst : Mod, ITogglableMod
    {
        private const string VERSION = "1.0.69";
        
        // after all other moth mods, before redwing
        private const int LOAD_ORDER = 89;
        private const int minApi = 44;


        private bool apiTooLow;
        private bool redwingInstalled;
        private bool blackmothInstalled;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = VERSION;
            apiTooLow = (Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi);

            if (apiTooLow)
                ver += " (Error: ModAPI too old... Minimum version is 44... seriously)";
            
            return ver;
        }

        public override void Initialize()
        {
            redwingInstalled = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes() where type.Namespace == "redwing" select type).Any();
            
            ModHooks.Instance.AfterSavegameLoadHook += saveGame;
            ModHooks.Instance.NewGameHook += addComponent;
        }
        
        private void saveGame(SaveGameData data)
        {
            addComponent();
        }
        
        private void addComponent()
        {
            log("Adding shitmothst to game.");

            GameManager.instance.gameObject.AddComponent<dash_hooks>();

            if (redwingInstalled)
            {
                log("Playing shitmothst with redwing... lol. Hope you told it not to use greymoth, or it's new" +
                    " enough to detect shitmothst");
            }
            else if (!redwingInstalled && !blackmothInstalled)
            {
                GameManager.instance.gameObject.AddComponent<ads_for_better_mods>();
            }
        }

        public override int LoadPriority()
        {
            return LOAD_ORDER;
        }

        public void Unload()
        {
            log("Disabling! If you see any more non-settings messages by this mod please report as an issue.");
            ModHooks.Instance.AfterSavegameLoadHook -= saveGame;
            ModHooks.Instance.NewGameHook -= addComponent;

        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Shitmothst] " + str);
        }


    }
}
