// Forrest Hunter Lowe 2019-2021

using System; // For "Exception"
using System.IO; // For File and Directory accessors
using System.Collections.Generic; // For "List"
using System.Runtime.Serialization; // For standard serialization
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq; // For "last or default"
using UnityEngine; // For JsonUtility

namespace FileSystem
{
    /// <summary>
    /// Saves and loads items for the user.
    /// </summary>
    public static class FileSaveManager
    {
        /// <summary>
        /// The current application directory.
        /// NOTE: this is set to the dataPath while in editor and will not be in the same location for the user.
        /// </summary>
        public static string AppDir
        {
            get
            {
#if UNITY_EDITOR
                Debug.Log($"Returning data path from {nameof(Application.dataPath)}");
                return Application.dataPath + "\\user_data";
#endif
#if !UNITY_EDITOR
            return  Application.persistentDataPath + "\\user_data";
#endif
            }
        }

        public enum Directories
        {
            global,
            saves,
            settings,
            // Add values here for more directories
        }

        public enum Extensions
        {
            /// <summary>
            /// Saves the item as a JSON object.
            /// </summary>
            json,

            /// <summary>
            /// Uses binary serialization on the object.
            /// </summary>
            dat,

            /// <summary>
            /// Saves the item as a JSON object.
            /// </summary>
            txt,

            /// <summary>
            /// Uses binary serialization on the object.
            /// </summary>
            setting,

            // Add extensions here for more file types
        }

        /// <summary>
        /// Ensures a directory exists, creating one if it does not.
        /// </summary>
        /// <param name="directory">Directory to validate.</param>
        private static void ValidateDirectory(Directories directory)
        {
            string dir = $"{AppDir}/{directory}";

            // The directory does not exist and we need to create one
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Creates a string directory for the user.
        /// </summary>
        /// <param name="fileName">The file name to use.</param>
        /// <param name="directory">The directory to access.</param>
        /// <param name="extension">The extention to append to the file name.</param>
        /// <returns>A valid directory with all parameters applied.</returns>
        public static string GetFileDirectory(string fileName, Directories directory, Extensions extension)
        {
            // Validate the directory before trying to return the string
            ValidateDirectory(directory);

            // Return the string accessor to the directory item
            return $"{AppDir}/{directory}/{fileName}.{extension}";
        }

        /// <summary>
        /// Creates a string directory for the user.
        /// </summary>
        /// <param name="directory">The directory to access.</param>
        /// <returns>A valid file directory string.</returns>
        public static string GetDirectory(Directories directory)
        {
            // Validate the directory before trying to return the string
            ValidateDirectory(directory);

            // Return the string accessor to the directory
            return $"{AppDir}/{directory}/";
        }

        /// <summary>
        /// Goes through every file in a directory looking for a given extension.
        /// </summary>
        /// <param name="directory">Directory to search through.</param>
        /// <param name="extension">Extension to look for.</param>
        /// <returns>An array of string item names within the directory.</returns>
        public static string[] GetFilesFromDirectory(Directories directory, Extensions extension = Extensions.dat)
        {
            var files = new List<string>();
            var dir = GetDirectory(directory);

            // Go through each item in the directory
            foreach (var fileDir in Directory.EnumerateFiles(dir))
            {
                // Split the file directory into just the file name and extension
                var file = fileDir.Split('\\').LastOrDefault();

                // Grab the file name and extension
                var fileName = file.Split('.').FirstOrDefault();
                var fileExtension = file.Split('.').LastOrDefault();

                // If the extension is equal to the search extension then we should add the file name to the list.
                // Alternatively to this we could check the file string object if it contains the search extension.
                // The reason we aren't going with the alternative there is it could lead to errors where the user
                // has given a name similar enough to the extension that it gets incorrectly returned.
                if (fileExtension == extension.ToString())
                {
                    files.Add(fileName);
                }
            }

            return files.ToArray();
        }

        /// <summary>
        /// Will save an abstract item to a specified directory.
        /// NOTE: Remember to mark your object as [System.Serializable]
        /// </summary>
        /// <param name="dataToSave">Object to serialize and save. Remember to mark your object as [System.Serializable]</param>
        /// <param name="fileName">File name for the object to save.</param>
        /// <param name="directory">Directory to put the object after serialization.</param>
        /// <param name="extension">Extension to use to save the object.</param>
        public static void Save(object dataToSave, string fileName, Directories directory = Directories.global, Extensions extension = Extensions.dat)
        {
            string dir = GetFileDirectory(fileName, directory, extension);

            if (extension == Extensions.json || extension == Extensions.txt)
            {
                // Create a json file and save all our data to it
                File.WriteAllText($"{dir}", JsonUtility.ToJson(dataToSave));
            }
            else
            {
                // Create a binary file and save all our date to it
                var formatter = GetBinaryFormatter();
                var file = File.Create($"{dir}");

                formatter.Serialize(file, dataToSave);

                // Don't forget to close your files!
                file.Close();
            }
        }

        /// <summary>
        /// Will load an item from a given directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">Name of the file to search for.</param>
        /// <param name="directory">Directory to extra the file from.</param>
        /// <param name="extension">Extension type to look for.</param>
        /// <returns></returns>
        public static T Load<T>(string fileName, Directories directory = Directories.global, Extensions extension = Extensions.dat)
        {
            string dir = GetFileDirectory(fileName, directory, extension);

            // If no file exists at the input directory return default null.
            if (File.Exists(dir) == false)
            {
                Debug.LogError($"No file at: {dir}");
                return default;
            }

            // If we are searching for a json file just go ahead and try to read that now
            if (extension == Extensions.json || extension == Extensions.txt) return JsonUtility.FromJson<T>(File.ReadAllText(dir));

            // Deserialize using a binary formatter
            var formatter = GetBinaryFormatter();
            var file = File.Open(dir, FileMode.Open);
            T save = default;

            try
            {
                save = (T)formatter.Deserialize(file);
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to deserialize file \"{dir}\"\n{e}");
            }

            // Don't forget to close your files!
            file.Close();

            return save;
        }

        public static BinaryFormatter GetBinaryFormatter()
        {
            var formatter = new BinaryFormatter();
            var selector = new SurrogateSelector();

            var quaternionSurrogate = new QuaternionSerializationSurrogate();
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);

            var vector2Surrogate = new Vector3SerializationSurrogate();
            selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);

            var vector3Surrogate = new Vector3SerializationSurrogate();
            selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);

            formatter.SurrogateSelector = selector;

            return formatter;
        }
    }

    // FIXME: extend this to allow abstraction of types instead of having to write a surrogate class
    // foreach individual type. I would rather have one complicated class.

    public class QuaternionSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var quaternion = (Quaternion)obj;
            info.AddValue("x", quaternion.x);
            info.AddValue("y", quaternion.y);
            info.AddValue("z", quaternion.z);
            info.AddValue("w", quaternion.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var quaternion = (Quaternion)obj;
            quaternion.x = (float)info.GetValue("x", typeof(float));
            quaternion.y = (float)info.GetValue("y", typeof(float));
            quaternion.z = (float)info.GetValue("z", typeof(float));
            quaternion.w = (float)info.GetValue("w", typeof(float));

            return quaternion;
        }
    }

    public class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var vector3 = (Vector3)obj;
            info.AddValue("x", vector3.x);
            info.AddValue("y", vector3.y);
            info.AddValue("z", vector3.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var vector3 = (Vector3)obj;
            vector3.x = (float)info.GetValue("x", typeof(float));
            vector3.y = (float)info.GetValue("y", typeof(float));
            vector3.z = (float)info.GetValue("z", typeof(float));

            return vector3;
        }
    }

    public class Vector2SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var vector2 = (Vector2)obj;
            info.AddValue("x", vector2.x);
            info.AddValue("y", vector2.y);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var vector2 = (Vector2)obj;
            vector2.x = (float)info.GetValue("x", typeof(float));
            vector2.y = (float)info.GetValue("y", typeof(float));

            return vector2;
        }
    }
}