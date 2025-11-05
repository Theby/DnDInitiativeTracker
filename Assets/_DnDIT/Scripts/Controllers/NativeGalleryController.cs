using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DnDInitiativeTracker.Controller
{
    public static class NativeGalleryController
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

        public static async Task RequestPermissions()
        {
            Task readTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
            Task writeTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);

            await readTask;
            await writeTask;
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

        public static Texture2D GetImageFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Texture2D texture;

            try
            {
                texture = NativeGallery.LoadImageAtPath(path);
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

        public static void SaveImageToGallery(string path, Action<string, string> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(string.Empty, string.Empty);
                return;
            }

            var fullPath = path;
            var currentFileName = Path.GetFileNameWithoutExtension(path);

            try
            {
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

        public static async Task<AudioClip> GetAudioClipFromPathAsync(string path, Action<AudioClip> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return null;
            }

            var uri = new Uri(path);
            var audioType = GetAudioType(path);
            AudioClip audioClip;

            try
            {
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

            onComplete?.Invoke(audioClip);
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

            var fullPath = path;
            var currentFileName = Path.GetFileNameWithoutExtension(path);

            try
            {
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
    }
}