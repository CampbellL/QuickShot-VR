using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Game
{
    public class SaveGameManager<T>
    {
        private SaveGameManager<T> _saveGameManager;
        private static readonly string SavePath = Application.persistentDataPath + "/saveGame.PewPew";
        
        public SaveGameManager<T> GetInstance()
        {
            return _saveGameManager ?? (this._saveGameManager = new SaveGameManager<T>());
        }

        private SaveGameManager() { }

        public static void Save(T saveData)
        {
            FileStream file = File.Exists(SavePath) ? File.OpenWrite(SavePath) : File.Create(SavePath);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, saveData);
            file.Close();
        }
        
        public static T Load()
        {
            if(!File.Exists(SavePath)) throw new FileNotFoundException();
            FileStream file = File.OpenRead(SavePath);
            BinaryFormatter formatter = new BinaryFormatter();
            T data = (T) formatter.Deserialize(file);
            file.Close();
            return data;
        }
    }
}