using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.Manager;
using UnityEngine;
using UnityEngine.Networking;

namespace DnDInitiativeTracker.Controller
{
    public static class NativeBrowserController
    {
        const string AlbumName = "DnDIT";

        static readonly Dictionary<string, AudioType> AudioTypeMap = new()
        {
            {".mp3", AudioType.MPEG},
            {".wav", AudioType.WAV},
            {".ogg", AudioType.OGGVORBIS},
            {".aiff", AudioType.AIFF},
            {".aif", AudioType.AIFF},
            {".mod", AudioType.MOD},
            {".it", AudioType.IT},
            {".s3m", AudioType.S3M},
            {".xm", AudioType.XM},
            {".xma", AudioType.XMA},
            {".vag", AudioType.VAG},
            {".mp2", AudioType.MPEG},
            {".acc", AudioType.ACC},
        };

        public static async Task RequestPermissionsAsync()
        {
            try
            {
                bool hasImageReadPermission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read,
                    NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
                if(!hasImageReadPermission)
                {
                    await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read,
                    NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
                }

                bool hasImageWritePermissions = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write,
                    NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
                if (!hasImageWritePermissions)
                {
                    await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write,
                        NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
                }

                bool hasFileReadPermission = NativeFilePicker.CheckPermission(true);
                if (!hasFileReadPermission)
                {
                    await NativeFilePicker.RequestPermissionAsync(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static void GetImagePathFromGallery(Action<string> onComplete)
        {
            try
            {
                NativeGallery.GetImageFromGallery(onComplete.Invoke);
            }
            catch (Exception e)
            {
                onComplete?.Invoke(string.Empty);
                Debug.LogError(e.Message);
            }
        }

        public static async Task<Texture2D> GetImageFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Texture2D texture;

            try
            {
                texture = IsStreamingAssetsPath(path)
                    ? await LoadImageFromStreamingAssetsAsync(path)
                    : await NativeGallery.LoadImageAtPathAsync(path);
            }
            catch (Exception e)
            {
                texture = null;
                Debug.LogError(e.Message);
            }

            if (texture == null)
                return null;

            var fileName = Path.GetFileNameWithoutExtension(path);
            texture.name = fileName;

            return texture;
        }

        static bool IsStreamingAssetsPath(string path)
        {
            return path.StartsWith(DataManager.StreamingAssetTag);
        }

        static async Task<Texture2D> LoadImageFromStreamingAssetsAsync(string path, bool markTextureNonReadable = false)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Texture2D texture2D;

            try
            {
                path = path.Replace(DataManager.StreamingAssetTag, "");
                var fullPath = Path.Combine(Application.streamingAssetsPath, path);
                var uri = new Uri(fullPath);

                using var www = UnityWebRequestTexture.GetTexture(uri, markTextureNonReadable);
                await www.SendWebRequest();

                texture2D = www.result is not UnityWebRequest.Result.Success
                    ? null
                    : DownloadHandlerTexture.GetContent(www);
            }
            catch (Exception e)
            {
                texture2D = null;
                Debug.LogError(e.Message);
            }

            return texture2D;
        }

        public static void SaveImageToGallery(string path, Action<string, string> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(string.Empty, string.Empty);
                return;
            }

            try
            {
                var fullPath = path;
                var currentFileName = Path.GetFileNameWithoutExtension(path);

                NativeGallery.SaveImageToGallery(fullPath, AlbumName, currentFileName, (success, savePath) =>
                {
#if UNITY_EDITOR
                    onComplete?.Invoke(fullPath, currentFileName);
#else
                    var newFileName = Path.GetFileNameWithoutExtension(savePath);
                    onComplete?.Invoke(savePath, newFileName);
#endif
                });
            }
            catch (Exception e)
            {
                onComplete?.Invoke(string.Empty, string.Empty);
                Debug.LogError(e.Message);
            }
        }

        public static void GetAudioPathFromGallery(Action<string> onComplete)
        {
            try
            {
                NativeGallery.GetAudioFromGallery(onComplete.Invoke);
            }
            catch (Exception e)
            {
                onComplete?.Invoke(string.Empty);
                Debug.LogError(e.Message);
            }
        }

        public static async Task<AudioClip> GetAudioClipFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            AudioClip audioClip;

            try
            {
                path = path.Replace(DataManager.StreamingAssetTag, "");
                var fullPath = Path.Combine(Application.streamingAssetsPath, path);
                var uri = new Uri(fullPath);
                var audioType = GetAudioType(path);

                using var www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
                await www.SendWebRequest();

                audioClip = www.result is not UnityWebRequest.Result.Success
                    ? null
                    : DownloadHandlerAudioClip.GetContent(www);

                if (audioClip != null)
                {
                    audioClip.name = Path.GetFileNameWithoutExtension(path);
                }
            }
            catch (Exception e)
            {
                audioClip = null;
                Debug.LogError(e.Message);
            }

            return audioClip;
        }

        static AudioType GetAudioType(string path)
        {
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            return string.IsNullOrEmpty(extension)
                ? AudioType.UNKNOWN
                : AudioTypeMap.GetValueOrDefault(extension, AudioType.UNKNOWN);
        }

        public static void SaveAudioToGallery(string path, Action<string, string> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(string.Empty, string.Empty);
                return;
            }

            try
            {
                var fullPath = path;
                var currentFileName = Path.GetFileNameWithoutExtension(path);
                
                NativeGallery.SaveAudioToGallery(fullPath, AlbumName, currentFileName, (success, savePath) =>
                {
#if UNITY_EDITOR
                    onComplete?.Invoke(fullPath, currentFileName);
#else
                    var newFileName = Path.GetFileNameWithoutExtension(savePath);
                    onComplete?.Invoke(savePath, newFileName);
#endif
                });
            }
            catch (Exception e)
            {
                onComplete?.Invoke(string.Empty, string.Empty);
                Debug.LogError(e.Message);
            }
        }

        public static async Task<string> PickFileAsync(string fileExtension)
        {
            string path;

            try
            {
                string fileType = NativeFilePicker.ConvertExtensionToFileType(fileExtension);
                path = await TaskAsyncExtensions.MakeAsync<string>(tsc =>
                {
                    NativeFilePicker.PickFile(tsc.Invoke, fileType);
                });
            }
            catch (Exception e)
            {
                path = string.Empty;
                Debug.LogError(e.Message);
            }

            return path;
        }

        public static async Task<string> ReadFileAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            string fileContent;

            try
            {
                using var www = UnityWebRequest.Get(path);
                await www.SendWebRequest();

                fileContent = www.result is not UnityWebRequest.Result.Success
                    ? string.Empty
                    : www.downloadHandler.text;
            }
            catch (Exception e)
            {
                fileContent = null;
                Debug.LogError(e.Message);
            }

            return fileContent;
        }
    }
}