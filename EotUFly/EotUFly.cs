using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using HarmonyLib;
public interface INewHorizons;

namespace EotUFly
{
    public static class Extensions
    {
        public static void print<T>(this T t)
        {
            EotUFly.Instance.ModHelper.Console.WriteLine($"{t}");
        }
    }

    [HarmonyPatch]
    public class EotUFly : ModBehaviour
    {
        bool HasDone = false;

        public static EotUFly Instance;
        private void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        
        private void EotUSettings(string s)
        {
            if (s != "Eye of the Universe")
            {
                "Name is not eye".print();
                return;
            }

            if (HasDone)
            {
                return;
            }

            HasDone = true;


            s.print();


            var root = GameObject.Find("SolarSystemRoot");

            var ESAC1 = root.AddComponent<EyeStateActivationController>();
            ESAC1._object = GameObject.Find("SixthPlanet_Root");
            ESAC1._activeStates = [EyeState.WarpedToSurface, EyeState.IntoTheVortex];

            var ESAC2 = root.AddComponent<EyeStateActivationController>();
            ESAC2._object = GameObject.Find("SignalJammer_Pivot");
            ESAC2._activeStates = [EyeState.WarpedToSurface];

            var ESAC3 = root.AddComponent<EyeStateActivationController>();
            ESAC3._object = GameObject.Find("Sector_Campfire");
            ESAC3._activeStates = [EyeState.InstrumentHunt, EyeState.JamSession, EyeState.BigBang];

            var Eye = GameObject.Find("EyeoftheUniverse_Body").GetComponent<OWRigidbody>();

            var EyeRB = Eye.GetComponent<OWRigidbody>();
            var SectorSetter = GameObject.Find("Sector_EyeOfTheUniverse");
            SectorSetter.GetComponent<Sector>()._attachedOWRigidbody = EyeRB;

            var EyeQM = GameObject.Find("EyeoftheUniverse_Body");

            EyeQM.AddComponent<QuantumOrbit>();

            EyeQM.GetComponent<QuantumOrbit>()._orbitRadius = 6000;
            EyeQM.GetComponent<QuantumOrbit>()._stateIndex = 5;

            var locationsix = GameObject.Find("Sun_Body").GetComponent<QuantumOrbit>();
            Destroy(locationsix);

        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(EotUFly)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

                if (loadScene == OWScene.SolarSystem)
                {
                    var root = GameObject.Find("SolarSystemRoot");
                    var ESM = root.AddComponent<EyeStateManager>();
                    ESM._initialState = EyeState.WarpedToSurface;
                }
            };

            newHorizons.GetBodyLoadedEvent().AddListener(EotUSettings);

        }
    }
}
