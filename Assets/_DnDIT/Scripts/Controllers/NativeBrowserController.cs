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

        public static async Task<string> GetImagePathFromGallery()
        {
            var path = await GetMediaAssetPathFromGallery(NativeGallery.GetImageFromGallery, "", "image/*");
            return path;
        }

        public static async Task<Texture2D> GetTexture2DFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Texture2D texture;

            try
            {
                texture = path.StartsWith(DataManager.StreamingAssetTag)
                    ? await GetMediaAssetFromPathAsync(path,
                        uri => UnityWebRequestTexture.GetTexture(uri, false),
                        DownloadHandlerTexture.GetContent)
                    : await NativeGallery.LoadImageAtPathAsync(path);
            }
            catch (Exception e)
            {
                texture = null;
                Debug.LogError(e.Message);
            }

            return texture;
        }

        public static async Task<(string, string)> SaveImageToGallery(string path)
        {
            var pathFileNameTuple = await SaveMediaAssetToGallery(path, NativeGallery.SaveImageToGallery);
            return pathFileNameTuple;
        }

        public static async Task<string> GetAudioPathFromGallery()
        {
            var path = await GetMediaAssetPathFromGallery(NativeGallery.GetAudioFromGallery, "", "audio/*");
            return path;
        }

        public static async Task<AudioClip> GetAudioClipFromPathAsync(string path)
        {
            var audioType = GetAudioType(path);
            var audioClip = await GetMediaAssetFromPathAsync(path,
                uri => UnityWebRequestMultimedia.GetAudioClip(uri, audioType),
                DownloadHandlerAudioClip.GetContent);
            return audioClip;
        }

        static AudioType GetAudioType(string path)
        {
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            return string.IsNullOrEmpty(extension)
                ? AudioType.UNKNOWN
                : AudioTypeMap.GetValueOrDefault(extension, AudioType.UNKNOWN);
        }

        public static async Task<(string, string)> SaveAudioToGallery(string path)
        {
            var pathFileNameTuple = await SaveMediaAssetToGallery(path, NativeGallery.SaveAudioToGallery);
            return pathFileNameTuple;
        }

        static async Task<T> GetMediaAssetFromPathAsync<T>(string path, Func<Uri, UnityWebRequest> webRequestHandler, Func<UnityWebRequest, T> getContentHandler)
        {
            if (string.IsNullOrEmpty(path))
                return default(T);

            T mediaAsset;

            try
            {
                path = path.Replace(DataManager.StreamingAssetTag, "");
                var fullPath = Path.Combine(Application.streamingAssetsPath, path);
                var uri = new Uri(fullPath);

                using var www = webRequestHandler.Invoke(uri);
                await www.SendWebRequest();

                mediaAsset = www.result is not UnityWebRequest.Result.Success
                    ? default(T)
                    : getContentHandler.Invoke(www);
            }
            catch (Exception e)
            {
                mediaAsset = default(T);
                Debug.LogError(e.Message);
            }

            return mediaAsset;
        }

        static async Task<string> GetMediaAssetPathFromGallery(Action<NativeGallery.MediaPickCallback, string, string> getPathHandler, string title, string mime)
        {
            string path;

            try
            {
                path = await TaskAsyncExtensions.MakeAsync<string>(tsc =>
                {
                    getPathHandler.Invoke(tsc.Invoke, title, mime);
                });
            }
            catch (Exception e)
            {
                path = string.Empty;
                Debug.LogError(e.Message);
            }

            return path;
        }

        static async Task<(string, string)> SaveMediaAssetToGallery(string path, Action<string, string, string, NativeGallery.MediaSaveCallback> saveToGalleryHandler)
        {
            if (string.IsNullOrEmpty(path))
                return (string.Empty, string.Empty);

            string fullPath;
            string currentFileName;
            try
            {
                fullPath = path;
                currentFileName = Path.GetFileNameWithoutExtension(path);
                (bool success, string savePath) = await TaskAsyncExtensions.MakeAsync<(bool, string)>(tsc =>
                {
                    saveToGalleryHandler.Invoke(fullPath, AlbumName, currentFileName,
                        (success, savePath) => tsc.Invoke((success, savePath)));
                });

#if UNITY_EDITOR
                fullPath = success ? fullPath : string.Empty;
                currentFileName = success ? currentFileName : string.Empty;
#else
                fullPath = success ? savePath : string.Empty;
                currentFileName = success ? Path.GetFileNameWithoutExtension(savePath) : string.Empty;
#endif
            }
            catch (Exception e)
            {
                fullPath = string.Empty;
                currentFileName = string.Empty;
                Debug.LogError(e.Message);
            }

            return (fullPath, currentFileName);
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