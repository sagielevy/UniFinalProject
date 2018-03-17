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
        public readonly static string volFileName = "volume";
        public readonly static string pitchFileName = "pitch";
        public readonly static string playerPrefsKey = "CurrPlayerName";
        public readonly static string playersListFile = "Players.dat";
        private readonly static string filenameFormat = "{0}_{1}.dat";

        // TODO Save current level and difficulty data for each player

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

        private static void AddPlayerToList(string playerName)
        {
            PlayersList playersList = new PlayersList();
            BinaryFormatter bf = new BinaryFormatter();
            string destination = Application.persistentDataPath + Path.DirectorySeparatorChar + playersListFile;
            FileStream file;

            if (File.Exists(destination))
            {
                file = File.OpenRead(destination);

                try
                {
                    playersList = (PlayersList)bf.Deserialize(file);
                }
                catch (Exception)
                {
                    playersList.players = new string[] { };
                }
            }
            else
            {
                file = File.Create(destination);
                playersList.players = new string[] { };
            }

            file.Close();

            // Concat all players and save
            playersList.players = playersList.players.Concat(new string[] { playerName }).ToArray();

            if (File.Exists(destination))
            {
                // Overwrite file
                file = File.Create(destination);
            }

            bf.Serialize(file, playersList);
            file.Close();
        }

        public static string[] LoadPlayers()
        {
            PlayersList playersList = new PlayersList();
            string destination = Application.persistentDataPath + Path.DirectorySeparatorChar + playersListFile;
            FileStream file;

            if (File.Exists(destination))
            {
                file = File.OpenRead(destination);

                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    playersList = (PlayersList)bf.Deserialize(file);
                }
                catch (Exception)
                {
                    playersList.players = new string[] { };
                }
            }
            else
            {
                file = File.Create(destination);
                playersList.players = new string[] { };
            }

            file.Close();

            return playersList.players;
        }

        public static void SavePlayerProfile(string playerName, Dictionary<string, OffsetsProfile> playerProfiles)
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

            // Add player to list
            AddPlayerToList(playerName);

            fileVolume.Close();
        }

        public static Dictionary<string, OffsetsProfile> LoadPlayerProfile(string playerName)
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
            playerProfiles[volFileName] = (OffsetsProfile)bf.Deserialize(fileVolume);
            playerProfiles[pitchFileName] = (OffsetsProfile)bf.Deserialize(filePitch);
            fileVolume.Close();
            filePitch.Close();
            
            return playerProfiles;
        }
    }

    [Serializable]
    class PlayersList
    {
        public string[] players;
    }
}