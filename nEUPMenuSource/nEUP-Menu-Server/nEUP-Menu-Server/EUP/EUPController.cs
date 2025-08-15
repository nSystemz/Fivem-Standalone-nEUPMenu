using CitizenFX.Core;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static CitizenFX.Core.Native.API;

namespace nEUP_Menu_Server.EUP
{
    public class EUPController : BaseScript
    {
        private static List<EUPData> eupDataList = new List<EUPData>();
        public const string ColorRed = "\u001b[31m";
        public const string ColorGreen = "\u001b[32m";

        public EUPController()
        {
            LoadEUPData();

            // Server-Events: Ersten Parameter als Player aus der Quelle binden
            EventHandlers["nEUPMenu:getEUPOutfitByGender"] += new Action<Player, string>(GetEUPJsonByGender);
            EventHandlers["nEUPMenu:setEUPOutfit"] += new Action<Player, int>(OnSetEUPOutfit);
        }

        public static void LoadEUPData()
        {
            try
            {
                Debug.WriteLine($"{ColorGreen}[nEUP-Menu] By Nemesus.de Loaded!");
                using (var conn = Database.GetConnection())
                using (var cmd = new MySqlCommand("SELECT * FROM outfits LIMIT 1000", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    int idOrd = reader.GetOrdinal("id");
                    int nameOrd = reader.GetOrdinal("name");
                    int categoryOrd = reader.GetOrdinal("category");
                    int genderOrd = reader.GetOrdinal("gender");
                    int json1Ord = reader.GetOrdinal("json1");
                    int json2Ord = reader.GetOrdinal("json2");

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(idOrd);
                        string name = reader.GetString(nameOrd);
                        string category = reader.GetString(categoryOrd);
                        string gender = reader.GetString(genderOrd);

                        string json1 = reader.IsDBNull(json1Ord) ? "[]" : reader.GetString(json1Ord);
                        string json2 = reader.IsDBNull(json2Ord) ? "[]" : reader.GetString(json2Ord);

                        var props = JsonConvert.DeserializeObject<List<EUPProp>>(json1) ?? new List<EUPProp>();
                        var comps = JsonConvert.DeserializeObject<List<EUPComponent>>(json2) ?? new List<EUPComponent>();

                        eupDataList.Add(new EUPData(id, name, props, comps, category, gender));
                    }
                }

                Debug.WriteLine($"{ColorGreen}[nEUP-Menu] Loaded {eupDataList.Count} EUP Outfits.");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{ColorRed}[Error-LoadEUPData] {e.Message}");
            }
        }

        public static EUPData GetEUPOutfitById(int id)
        {
            foreach (var eupData in eupDataList)
            {
                if (eupData.id == id)
                {
                    return eupData;
                }
            }
            return null;
        }

        public static void OnSetEUPOutfit([FromSource] Player player, int outfitId)
        {
            try
            {
                var outfit = GetEUPOutfitById(outfitId);
                EUPController.PreLoadOutfit(player, outfit.name);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{ColorRed}[Error-OnSetEUPOutfit] {e}");
            }
        }

        public static void PreLoadOutfit(Player player, string outfitName)
        {
            try
            {
                foreach (var eupOutfit in eupDataList)
                {
                    if (eupOutfit.name.ToUpper() == outfitName.ToUpper())
                    {
                        SetEupOutfit(player, eupOutfit);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{ColorRed}[Error-PreLoadOutfit] {e.Message}");
            }
        }

        public static void SetEupOutfit(Player player, EUPData outfit)
        {
            if (outfit == null) return;

            int ped = GetPlayerPed(player.Handle);

            foreach (var comp in outfit.components ?? new List<EUPComponent>())
            {
                int compId = Math.Max(0, Math.Min(comp.component, 11));
                int drawable = Math.Max(comp.drawable - 1, 0);
                int texture = Math.Max(comp.texture - 1, 0);

                SetPedComponentVariation(ped, compId, drawable, texture, 0);
            }

            foreach (var prop in outfit.props ?? new List<EUPProp>())
            {
                int propId = Math.Max(0, Math.Min(prop.prop, 9));

                if (prop.drawable == 0)
                {
                    ClearPedProp(ped, propId);
                }
                else
                {
                    int drawable = Math.Max(prop.drawable - 1, 0);
                    int texture = Math.Max(prop.texture - 1, 0);
                    SetPedPropIndex(ped, propId, drawable, texture, true);
                }
            }
        }

        public static void GetEUPJsonByGender([FromSource]Player player, string gender = "MALE")
        {
            if (eupDataList == null || eupDataList.Count == 0) return;

            List<EUPData> eupDataTempList = new List<EUPData>();
            eupDataTempList.Clear();
            foreach (var eupOutfit in eupDataList)
            {
                if (Regex.IsMatch(eupOutfit.name.ToUpper(), $@"\b{Regex.Escape(gender)}\b", RegexOptions.IgnoreCase))
                {
                    var newEupOutfit = JsonConvert.DeserializeObject<EUPData>(
                        JsonConvert.SerializeObject(eupOutfit)
                    );
                    newEupOutfit.components = null;
                    newEupOutfit.props = null;
                    eupDataTempList.Add(newEupOutfit);
                }
            }
            TriggerClientEvent("nEUPMenu:showMenu", JsonConvert.SerializeObject(eupDataTempList, Formatting.None));
        }
    }
}
