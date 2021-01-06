using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;

namespace DataSerialization
{
    public enum FileType { JSON = default, Binary, Test }

    public class DataManager
    {
        private class FileInfoTypeContainer
        {
            public readonly FileType fileType;
            public readonly string fileExtension;
            public List<FileInfo> files = new List<FileInfo>();

            private string _jsonExtension = ".json";
            private string _binaryExtension = ".sav";

            public FileInfoTypeContainer(FileType fileType)
            {
                this.fileType = fileType;
                switch (fileType)
                {
                    case FileType.JSON:
                        fileExtension = _jsonExtension;
                        break;
                    case FileType.Binary:
                        fileExtension = _binaryExtension;
                        break;
                }
            }
        }
        //public static Action saving;
        //public static Action loading;

        public static string persistentDirectoryPath = Application.persistentDataPath;
        public static string subDirectory = "User Data";
        public static bool debugMode = false;

        private static List<FileInfoTypeContainer> _viableFilesContainers = new List<FileInfoTypeContainer>();
        private static SurrogateSelector surrogateSelector;
        private const string pathColor = "#80E6FF";

        public static string DefaultPath { get { string path = Path.Combine(persistentDirectoryPath, subDirectory); if (!Directory.Exists(path)) Directory.CreateDirectory(path); return path; } }
        public static string TargetDataPath { get => PlayerPrefs.GetString("TargetDataPath", DefaultPath); set => PlayerPrefs.SetString("TargetDataPath", value); }
        public static SurrogateSelector SurrogateSelector 
        {
            get 
            {
                if (surrogateSelector == null) 
                {
                    surrogateSelector = new SurrogateSelector();
                    ColorSurrogate colorSurrogate = new ColorSurrogate();
                    Vector2Surrogate vector2Surrogate = new Vector2Surrogate();
                    Vector3Surrogate vector3Surrogate = new Vector3Surrogate();
                    QuaternionSurrogate quaternionSurrogate = new QuaternionSurrogate();

                    surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), colorSurrogate);
                    surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);
                    surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
                    surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);
                }
                return surrogateSelector;
            }
        }

        public static void ResetTargetDataPath()
        {
            TargetDataPath = DefaultPath;
        }

        public static void LoadAllFileDatabases() 
        {
            LoadFileDatabases(FileType.JSON, FileType.Binary, FileType.Test);
        }
        public static void LoadFileDatabase(FileType fileType) 
        {
            LoadFileDatabases(fileType);
        }
        private static void LoadFileDatabases(params FileType[] fileTypes)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(TargetDataPath);
            List<string> directories = new List<string>() { directoryInfo.FullName };
            List<FileType> fileTypesFound = new List<FileType>();
            List<FileInfo> files = new List<FileInfo>();
            List<FileInfoTypeContainer> targetFileContainers = new List<FileInfoTypeContainer>();

            directories.AddRange(Directory.GetDirectories(directoryInfo.FullName, "*", SearchOption.AllDirectories));

            for (int i = 0; i < fileTypes.Length; i++) 
            {
                FileInfoTypeContainer targetContainer = null;
                for (int j = 0; j < _viableFilesContainers.Count; j++) 
                    if (_viableFilesContainers[j].fileType == fileTypes[i]) 
                    {
                        targetContainer = _viableFilesContainers[j];
                        break;
                    }
                if (targetContainer == null) 
                {
                    targetContainer = new FileInfoTypeContainer(fileTypes[i]);
                    _viableFilesContainers.Add(targetContainer);
                }
                targetContainer.files = new List<FileInfo>();
                targetFileContainers.Add(targetContainer);
            }

            for (int i = 0; i < directories.Count; i++)
                for (int j = 0; j < targetFileContainers.Count; j++) 
                {
                    if (targetFileContainers[j].fileExtension == null) 
                        continue;
                    FileInfo[] filesFound = new DirectoryInfo(directories[i]).GetFiles($"*{targetFileContainers[j].fileExtension}");
                    files.AddRange(filesFound);
                    targetFileContainers[j].files.AddRange(filesFound);
                }

            for (int i = 0; i < targetFileContainers.Count; i++)
                if (targetFileContainers[i].files.Count > 0) 
                    fileTypesFound.Add(targetFileContainers[i].fileType);

            if (debugMode) 
            {
                string fileTypesToString = "";
                for (int i = 0; i < fileTypesFound.Count; i++)
                {
                    fileTypesToString += $"<b>{Enum.GetName(typeof(FileType), fileTypesFound[i])}</b>";
                    if (fileTypesFound.Count > 1 && i + 1 < fileTypesFound.Count)
                        if (i + 1 == fileTypesFound.Count - 1)
                            fileTypesToString += " and ";
                        else fileTypesToString += ", ";
                }
                string filesFound = $"Files of type{(fileTypes.Length > 1 ? " — " : " ")}{fileTypesToString}{(fileTypes.Length > 1 ? " — " : " ")}were found within the directories under <b>TargetDataPath</b>.   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n";
                foreach (var file in files)
                    filesFound += $"   <color={pathColor}>*{Path.DirectorySeparatorChar}{directoryInfo.Name}{file.FullName.Remove(0, directoryInfo.FullName.Length)}</color>\n";
                Debug.Log(filesFound);
            }
        }

        public static bool CreateNewFileInDatabase<T>(string fileName, T file, FileType fileType) 
        {
            LoadFileDatabase(fileType);
            return CreateNewFile(fileName, file, fileType);
        }
        public static bool CreateNewFile<T>(string fileName, T file, FileType fileType)
        {
            FileInfoTypeContainer targetFileContainer = null;
            for (int i = 0; i < _viableFilesContainers.Count; i++) 
            {
                if (_viableFilesContainers[i].fileType == fileType)
                {
                    targetFileContainer = _viableFilesContainers[i];
                    break;
                }
            }

            if (targetFileContainer == null) 
            {
                if (debugMode)
                    Debug.Log($"Attempt to create file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Failed to locate pre-existing {Enum.GetName(typeof(FileType), fileType).ToLower()} file database!\n");
                return false;
            }

            string targetExtension = targetFileContainer.fileExtension;
            List<FileInfo> targetFiles = targetFileContainer.files;

            FileInfo fileInfo = new FileInfo(Path.Combine(TargetDataPath, fileName + targetExtension));
            if (Directory.Exists(fileInfo.Directory.FullName))
            {
                if (!File.Exists(fileInfo.FullName))
                {
                    switch (fileType) 
                    {
                        case FileType.JSON:
                            string dataAsJson = JsonUtility.ToJson(file, true);
                            File.WriteAllText(fileInfo.FullName, dataAsJson);
                            targetFiles.Add(fileInfo);
                            break;

                        case FileType.Binary:
                            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            binaryFormatter.SurrogateSelector = SurrogateSelector;
                            binaryFormatter.Serialize(fileStream, file);
                            fileStream.Close();
                            targetFiles.Add(fileInfo);
                            break;
                    }
                    if (debugMode)
                        Debug.Log($"Attempt to create file: <b>{fileName}</b>, <color=green><b><i>SUCCEEDED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Path: <color={pathColor}>{fileInfo.FullName}</color>\n");
                    return true;
                }
                else
                {
                    if (debugMode)
                        Debug.Log($"Attempt to create file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   A file already exists at path!\n   Path: <color={pathColor}>{fileInfo.FullName}</color>\n");
                    return false;
                }

            }
            else
            {
                if (debugMode)
                    Debug.Log($"Attempt to create file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Failed to traverse the directory given from <b>TargetDataPath</b>!\n   Path: <color={pathColor}>{fileInfo.Directory.FullName}</color>\n");
                return false;
            }
        }

        public static bool SaveFileInDatabase<T>(FileType fileType, string fileName, T file)
        {
            LoadFileDatabase(fileType);
            return SaveFile(fileName, file);
        }
        public static bool SaveFile<T>(string fileName, T file)
        {
            FileInfo fileInfo;
            FileType fileType;
            if (FindFileFromDatabase(fileName, out fileInfo, out fileType))
            {
                switch (fileType)
                {
                    case FileType.JSON:
                        string dataAsJson = JsonUtility.ToJson(file, true);
                        File.WriteAllText(fileInfo.FullName, dataAsJson);
                        break;

                    case FileType.Binary:
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
                        binaryFormatter.SurrogateSelector = SurrogateSelector;
                        binaryFormatter.Serialize(fileStream, file);
                        fileStream.Close();
                        break;
                }

                if (debugMode)
                    Debug.Log($"Attempt to save file: <b>{fileName}</b>, <color=green><b><i>SUCCEEDED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Path: <color={pathColor}>{fileInfo.FullName}</color>\n");
                return true;
            }

            if (debugMode)
                Debug.Log($"Attempt to save file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   File was not found within databases!\n");
            return false;
        }

        public static bool LoadFileInDatabase<T>(FileType fileType, string fileName, T file)
        {
            LoadFileDatabase(fileType);
            return LoadFile(fileName, file);
        }
        public static bool LoadFile<T>(string fileName, T file)
        {
            FileInfo fileInfo;
            FileType fileType;
            if (FindFileFromDatabase(fileName, out fileInfo, out fileType))
            {
                switch (fileType)
                {
                    case FileType.JSON:
                        string dataFromJson = File.ReadAllText(fileInfo.FullName);
                        JsonUtility.FromJsonOverwrite(dataFromJson, file);
                        break;

                    case FileType.Binary:
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open);
                        binaryFormatter.SurrogateSelector = SurrogateSelector;
                        FromTypeOverwrite((T)binaryFormatter.Deserialize(fileStream), file);
                        fileStream.Close();
                        break;
                }


                if (debugMode)
                    Debug.Log($"Attempt to load file: <b>{fileName}</b>, <color=green><b><i>SUCCEEDED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Path: <color={pathColor}>{fileInfo.FullName}</color>\n");
                return true;
            }

            if (debugMode)
                Debug.Log($"Attempt to load file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   File was not found within databases!\n");
            return false;
        }

        public static bool DeleteFile(string fileName) 
        {
            FileInfo fileInfo;
            if (FindFileFromDatabase(fileName, out fileInfo, out _))
            {
                File.Delete(fileInfo.FullName);
                if (debugMode)
                    Debug.Log($"Attempt to delete file: <b>{fileName}</b>, <color=green><b><i>SUCCEEDED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n   Path: <color={pathColor}>{fileInfo.FullName}</color>\n");
                return true;
            }

            if (debugMode)
                Debug.Log($"Attempt to delete file: <b>{fileName}</b>, <color=yellow><b><i>FAILED</i></b></color>   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n  File was not found within databases!\n");
            return false;
        }

        private static bool FindFileFromDatabase(string fileName, out FileInfo fileInfo, out FileType fileType)
        {
            for (int i = 0; i < _viableFilesContainers.Count; i++)
                for (int j = 0; j < _viableFilesContainers[i].files.Count; j++)
                    if (_viableFilesContainers[i].files[j].Name == $"{fileName}{_viableFilesContainers[i].fileExtension}")
                    {
                        fileInfo = _viableFilesContainers[i].files[j];
                        fileType = _viableFilesContainers[i].fileType;
                        return true;
                    }

            fileInfo = null;
            fileType = default;
            return false;
        }

        private static void FromTypeOverwrite<T>(T loadedObject, T objectToOverwrite)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField;
            FieldInfo[] loadedFields = loadedObject.GetType().GetFields(flags);
            FieldInfo[] overwriteFields = objectToOverwrite.GetType().GetFields(flags);
            for (int i = 0; i < loadedFields.Length; i++)
            {
                object value = loadedFields[i].GetValue(loadedObject);
                overwriteFields[i].SetValue(objectToOverwrite, value);
            }
        }
    }
}