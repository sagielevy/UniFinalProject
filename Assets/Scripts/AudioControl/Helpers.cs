using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.AudioControl
{
    static class Helpers
    {
        public static string volFileName = "volume";
        public static string pitchFileName = "pitch";
        private static string filenameFormat = "{0}_{1}.dat";

        /// <summary>
        /// Convert via the following popular formula:
        /// m = 2595 * Log10(1 + f/700)
        /// </summary>
        /// <param name="hertz"></param>
        /// <returns></returns>
        public static float HertzToMel(float hertz)
        {
            return 2595 * Mathf.Log10(1 + hertz / 700);
        }

        public static void SaveFile(string playerName, Dictionary<string, OffsetsProfile> playerProfiles)
        {
            string destinationVol = Application.persistentDataPath + Path.DirectorySeparatorChar + string.Format(filenameFormat, playerName, volFileName);
            string destinationPitch = Application.persistentDataPath + Path.DirectorySeparatorChar + string.Format(filenameFormat, playerName, pitchFileName);
            FileStream fileVolume, filePitch;

            if (File.Exists(destinationVol))
            {
                fileVolume = File.OpenWrite(destinationVol);
            }
            else
            {
                fileVolume = File.Create(destinationVol);
            }

            if (File.Exists(destinationPitch))
            {
                filePitch = File.OpenWrite(destinationPitch);
            }
            else
            {
                filePitch = File.Create(destinationPitch);
            }
            
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fileVolume, playerProfiles[volFileName]);
            bf.Serialize(filePitch, playerProfiles[pitchFileName]);
            fileVolume.Close();
        }

        public static Dictionary<string, OffsetsProfile> LoadFile(string playerName)
        {
            var playerProfiles = new Dictionary<string, OffsetsProfile>();

            string destinationVol = Application.persistentDataPath + Path.DirectorySeparatorChar + string.Format(filenameFormat, playerName, volFileName);
            string destinationPitch = Application.persistentDataPath + Path.DirectorySeparatorChar + string.Format(filenameFormat, playerName, pitchFileName);

            FileStream fileVolume, filePitch;

            if (File.Exists(destinationVol))
            {
                fileVolume = File.OpenRead(destinationVol);
            }
            else
            {
                UnityEngine.Debug.LogError("File not found");
                return null;
            }

            if (File.Exists(destinationPitch))
            {
                filePitch = File.OpenRead(destinationPitch);
            }
            else
            {
                UnityEngine.Debug.LogError("File not found");
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            //GameData data = (GameData)bf.Deserialize(file);
            playerProfiles[volFileName] = (OffsetsProfile)bf.Deserialize(fileVolume);
            playerProfiles[pitchFileName] = (OffsetsProfile)bf.Deserialize(filePitch);
            fileVolume.Close();
            filePitch.Close();
            
            //UnityEngine.Debug.Log(data.name);
            //UnityEngine.Debug.Log(data.score);
            //UnityEngine.Debug.Log(data.timePlayed);

            return playerProfiles;
        }
    }
}
