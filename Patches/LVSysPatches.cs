using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Game.Simulation;
using HarmonyLib;
using Unity.Collections;
using Unity.Jobs;
using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Economy;
using Game.Prefabs;
using Game.Rendering;
using Game.UI;
using Game.UI.Tooltip;
using Game.Zones;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Game.Net;

namespace LandValueRemake
{
    [HarmonyPatch]
    public class LandValueRePatches
    {
            [HarmonyPatch(typeof(CellMapSystem<LandValueCell>), "GetData")]
            [HarmonyPrefix]
            public static bool LVGetData(CellMapSystem<LandValueCell> __instance, ref CellMapData<LandValueCell> __result, bool readOnly, ref JobHandle dependencies)
            {
                if (__instance.GetType().FullName == "Game.Simulation.LandValueSystem")
                {
                    __result = __instance.World.GetExistingSystemManaged<LandValueRemake.Systems.LandValueSystemRe>().GetData(readOnly, out dependencies);
                    return false;
                }
                return true;
            }

            [HarmonyPatch(typeof(CellMapSystem<LandValueCell>), "GetMap")]
            [HarmonyPrefix]
            public static bool LVGetMap(CellMapSystem<LandValueCell> __instance, ref NativeArray<LandValueCell> __result, bool readOnly, ref JobHandle dependencies)
            {
                if (__instance.GetType().FullName == "Game.Simulation.LandValueSystem")
                {
                    __result = __instance.World.GetExistingSystemManaged<LandValueRemake.Systems.LandValueSystemRe>().GetMap(readOnly, out dependencies);
                    return false;
                }
                return true;
            }

            [HarmonyPatch(typeof(CellMapSystem<LandValueCell>), "AddReader")]
            [HarmonyPrefix]
            public static bool LVAddReader(CellMapSystem<LandValueCell> __instance, JobHandle jobHandle)
            {
                string name = __instance.GetType().FullName;
                if (name == "Game.Simulation.LandValueSystem")
                {
                    __instance.World.GetExistingSystemManaged<LandValueRemake.Systems.LandValueSystemRe>().AddReader(jobHandle);
                    return false;
                }
                return true;
            }           

        }
    }

