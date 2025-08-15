//
//	#     #
//	##    #  ######  #    #  ######   ####   #    #   ####
//	# #   #  #       ##  ##  #       #       #    #  #
//	#  #  #  #####   # ## #  #####    ####   #    #   ####
//	#   # #  #       #    #  #            #  #    #       #
//	#    ##  #       #    #  #       #    #  #    #  #    #
//	#     #  ######  #    #  ######   ####    ####    ####
//
// Created by Nemesus (STANDALONE)
// Website: https://nemesus.de
// Youtube: https://youtube.nemesus.de

// ONLY EDIT IF YOU KNOW WHAT YOU ARE DOING!

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nEUP_Menu_Client
{
    public class Main : BaseScript
    {
        public static bool showMenu = false;
        private int cam = -1;
        private bool portraitActive = false;

        public Main()
        {
            Tick += OnTick;

            EventHandlers["nEUPMenu:startMenu"] += new Action(OnStartMenu);
            EventHandlers["nEUPMenu:showMenu"] += new Action<string>(OnShowMenu);

            RegisterCommand("nEUPMenu", new Action<int, List<object>, string>((source, args, raw) =>
            {
                OnStartMenu();
            }), false);

            RegisterNuiCallbackType("setEUPOutfit");
            EventHandlers["__cfx_nui:setEUPOutfit"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                if (data.TryGetValue("outfitId", out var outfitId))
                {
                    TriggerServerEvent("nEUPMenu:setEUPOutfit", int.Parse(outfitId.ToString()));
                }
                cb("ok");
            });

            RegisterNuiCallbackType("hideEupMenu");
            EventHandlers["__cfx_nui:hideEupMenu"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                SetNuiFocus(false, false);
                showMenu = false;
                ResetPortraitCam();
                cb("ok");
            });

            RegisterCommand("male", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SetFreemodePed("MALE");
            }), false);

            RegisterCommand("female", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SetFreemodePed("FEMALE");
            }), false);
        }

        private async void SetFreemodePed(string gender)
        {
            uint model = (uint)GetHashKey("mp_m_freemode_01");

            if (gender.ToUpper() == "FEMALE")
            {
                model = (uint)GetHashKey("mp_f_freemode_01");
            }

            RequestModel(model);
            while (!HasModelLoaded(model))
            {
                await Delay(0);
            }

            SetPlayerModel(PlayerId(), model);
            SetModelAsNoLongerNeeded(model);

            SetPedDefaultComponentVariation(PlayerPedId());
        }

        public void OnStartMenu()
        {
            int ped = PlayerPedId();
            uint model = (uint)GetEntityModel(ped);

            uint male = (uint)GetHashKey("mp_m_freemode_01");
            uint female = (uint)GetHashKey("mp_f_freemode_01");

            string gender =
                model == male ? "male" :
                model == female ? "female" : "other";

            if (gender.ToUpper() == "OTHER") return;

            TriggerServerEvent("nEUPMenu:getEUPOutfitByGender", gender.ToUpper());
        }

        public void OnShowMenu(string json)
        {
            if (showMenu == false)
            {
                json = $"{{\"option\":\"showEupMenu\",\"eupData\":{json}}}";
                SendNuiMessage(json);
                SetNuiFocus(true, true);
                showMenu = true;
                ActivatePortraitCam();
            }
            else
            {
                json = $"{{\"option\":\"hideEupMenu\"}}";
                SendNuiMessage(json);
                SetNuiFocus(false, false);
                showMenu = false;
                ResetPortraitCam();
            }
        }

        private async Task OnTick()
        {
            if (showMenu == true)
            {
                InvalidateIdleCam();
                InvalidateVehicleIdleCam();
                DisableIdleCamera(true);
                DisableFirstPersonCamThisFrame();
            }
            await Task.FromResult(0);
        }

        private async void ActivatePortraitCam()
        {
            if (portraitActive) return;

            var ped = PlayerPedId();

            var pedPos = GetEntityCoords(ped, true);
            var pedHead = GetPedBoneCoords(ped, 0x796E, 0f, 0f, 0f);

            var camPos = GetOffsetFromEntityInWorldCoords(ped, 0f, 2.0f, 0.6f);

            if (!DoesCamExist(cam))
                cam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);

            SetCamCoord(cam, camPos.X, camPos.Y, camPos.Z);
            PointCamAtCoord(cam, pedHead.X, pedHead.Y, pedHead.Z);
            SetCamFov(cam, 75.0f);

            SetCamActive(cam, true);
            RenderScriptCams(true, true, 500, true, true);

            portraitActive = true;
            Tick += KeepCamStable;
        }

        private async Task KeepCamStable()
        {
            if (!portraitActive) return;

            var ped = PlayerPedId();

            var pedHead = GetPedBoneCoords(ped, 0x796E, 0f, 0f, 0f);
            var camPos = GetOffsetFromEntityInWorldCoords(ped, 0f, 2.0f, 0.6f);

            SetCamCoord(cam, camPos.X, camPos.Y, camPos.Z);
            PointCamAtCoord(cam, pedHead.X, pedHead.Y, pedHead.Z);

            InvalidateIdleCam();
            InvalidateVehicleIdleCam();

            await Task.FromResult(0);
        }

        private void ResetPortraitCam()
        {
            if (!portraitActive) return;

            RenderScriptCams(false, true, 500, true, true);

            if (DoesCamExist(cam))
            {
                DestroyCam(cam, false);
                cam = -1;
            }

            portraitActive = false;
            Tick -= KeepCamStable;

        }
    }
}
