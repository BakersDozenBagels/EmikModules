﻿using KeepCoding;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Supplies an enumerator ButtonType, and ModSettings.
/// </summary>
namespace PhosphorescenceModule
{
    /// <summary>
    /// The mod settings that can be adjusted by a user, usually from the ModSelector.
    /// </summary>
    public class ModSettingsJSON
    {
        /// <summary>
        /// When disabled, there will be no indicators/letters for colors.
        /// </summary>
        [JsonProperty("Phosphorescence -> CruelMode")]
        public bool CruelMode { get; set; }

        /// <summary>
        /// How much additional time needs to be given due to stream delay?
        /// </summary>
        [JsonProperty("Phosphorescence -> StreamDelay")]
        public int StreamDelay { get; set; }

        /// <summary>
        /// Gets the values from ModSettings.
        /// </summary>
        /// <param name="vrMode">Used for initalization only.</param>
        /// <param name="streamDelay">Determines the timer.</param>
        public static void Get(PhosphorescenceScript pho, out bool cruelMode, out int streamDelay)
        {
            // Default values.
            cruelMode = false;
            streamDelay = 0;

            try
            {
                // Try loading settings.
                var settings = JsonConvert.DeserializeObject<ModSettingsJSON>(pho.ModSettings.Settings);

                // Do settings exist?
                if (settings != null)
                {
                    cruelMode = settings.CruelMode;
                    streamDelay = 15 * settings.StreamDelay;
                }
            }
            catch (JsonReaderException e)
            {
                // In the case of catastrophic failure and devastation.
                Debug.LogFormat("[Phosphorescence #{0}]: JSON error: \"{1}\", resorting to default values.", pho.moduleId, e.Message);
            }
        }

        public static bool LoadMission(PhosphorescenceScript pho, ref bool cruelMode, ref int streamDelay)
        {
            string description = Application.isEditor ? "" : Game.Mission.Description;

            if (description == null)
                return true;

            Regex regex = new Regex(@"\[Phosphorescence\] \d+,-*\d+");

            var match = regex.Match(description);

            if (!match.Success)
                return true;

            int[] values = match.Value.Replace("[Phosphorescence] ", "").Split(',').ToNumbers(minLength: 2, maxLength: 2);

            if (values == null || !values[0].IsBetween(0, 1))
                return true;

            cruelMode = values[0] == 1;
            streamDelay = values[1] * 15;

            Debug.LogFormat("[Phosphorescence #{0}]: Mission-specific data found.", pho.moduleId);

            return false;
        }
    }
}
