using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EotUFly
{
    [Harmony]
    internal class MyPatchClass
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.ChangeQuantumState))]
        public static bool ChangeQuantumState(QuantumMoon __instance, bool skipInstantVisibilityCheck, ref bool __result)
        {
            bool flag = false;
            if (__instance._isPlayerInside && __instance._hasSunCollapsed)
            {
                __result = false;
                return false;
            }
            if (Time.time - __instance._playerWarpTime < 1f)
            {
                __result = false;
                return false;
            }
            if (__instance._stateIndex == 5 && __instance._isPlayerInside && !__instance.IsPlayerEntangled())
            {
                __result = false;
                return false;
            }
            for (int i = 0; i < 10; i++)
            {
                int num = ((__instance._collapseToIndex != -1) ? __instance._collapseToIndex : __instance.GetRandomStateIndex());
                int num2 = -1;
                for (int j = 0; j < __instance._orbits.Length; j++)
                {
                    if (__instance._orbits[j].GetStateIndex() == num)
                    {
                        num2 = j;
                        break;
                    }
                }
                if (num2 == -1)
                {
                    Debug.LogError("QUANTUM MOON FAILED TO FIND ORBIT FOR STATE " + num);
                }
                float num3 = ((num2 != -1) ? __instance._orbits[num2].GetOrbitRadius() : 10000f);
                OWRigidbody oWRigidbody = ((num2 != -1) ? __instance._orbits[num2].GetAttachedOWRigidbody() : Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody());
                Vector3 onUnitSphere = Random.onUnitSphere;
                if (num == 6)
                {
                    onUnitSphere.y = 0f;
                    onUnitSphere.Normalize();
                }
                Vector3 position = onUnitSphere * num3 + oWRigidbody.GetWorldCenterOfMass();
                if (!Physics.CheckSphere(position, __instance._sphereCheckRadius, OWLayerMask.physicalMask) || __instance._collapseToIndex != -1)
                {
                    __instance._visibilityTracker.transform.position = position;
                    if (!Physics.autoSyncTransforms)
                    {
                        Physics.SyncTransforms();
                    }
                    if (skipInstantVisibilityCheck || __instance.IsPlayerEntangled() || !__instance.CheckVisibilityInstantly())
                    {
                        __instance._moonBody.transform.position = position;
                        if (!Physics.autoSyncTransforms)
                        {
                            Physics.SyncTransforms();
                        }
                        __instance._visibilityTracker.transform.localPosition = Vector3.zero;
                        __instance._constantForceDetector.AddConstantVolume(oWRigidbody.GetAttachedGravityVolume(), inheritForceAcceleration: true, clearOtherFields: true);
                        Vector3 vector = oWRigidbody.GetVelocity();
                        if (__instance._useInitialMotion)
                        {
                            InitialMotion component = oWRigidbody.GetComponent<InitialMotion>();
                            vector = ((component != null) ? component.GetInitVelocity() : Vector3.zero);
                            __instance._useInitialMotion = false;
                        }
                        __instance._moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(oWRigidbody, __instance._moonBody, Random.Range(0, 360)) + vector);
                        __instance._useInitialMotion = false;
                        __instance._lastStateIndex = __instance._stateIndex;
                        __instance._stateIndex = num;
                        __instance._collapseToIndex = -1;
                        flag = true;
                        for (int k = 0; k < __instance._stateSkipCounts.Length; k++)
                        {
                            __instance._stateSkipCounts[k] = ((k != __instance._stateIndex) ? (__instance._stateSkipCounts[k] + 1) : 0);
                        }
                        break;
                    }
                    __instance._visibilityTracker.transform.localPosition = Vector3.zero;
                }
                else
                {
                    Debug.LogError("Quantum moon orbit position occupied! Aborting collapse.");
                }
            }
            if (flag)
            {
                if (__instance._isPlayerInside)
                {
                    __instance.SetSurfaceState(__instance._stateIndex);
                }
                else
                {
                    __instance.SetSurfaceState(-1);
                }
                GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", __instance._moonBody);
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EyeStateManager), nameof(EyeStateManager.Awake))]
        public static bool Awake(EyeStateManager __instance)
        {
            if (PlayerData.GetWarpedToTheEye())
            {
                __instance._initialState = EyeState.AboardVessel;
                return false;
            }
            return false;
        }


    }
}
