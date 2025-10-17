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
            NativeGallery.GetImageFromGallery(onComplete.Invoke);
        }

        public static Texture2D GetImageFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var texture = NativeGallery.LoadImageAtPath(path);
            if (texture == null)
                return null;

            var fileName = Path.GetFileNameWithoutExtension(path);
            texture.name = fileName;

            return texture;
        }

        public static void SaveImageToGallery(string path, Action<string, string> onComplete = null)
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

        public static void GetAudioPathFromGallery(Action<string> onComplete)
        {
            NativeGallery.GetAudioFromGallery(onComplete.Invoke);
        }

        public static void GetAudioClipFromPath(string path, Action<AudioClip> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return;
            }

            GetAudioClipFromPathAsync(path, onComplete);
        }

        public static async Task<AudioClip> GetAudioClipFromPathAsync(string path, Action<AudioClip> onComplete = null)
        {
            var uri = new Uri(path);
            var audioType = GetAudioType(path);

            using var www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            await www.SendWebRequest();

            var audioClip = www.result is not UnityWebRequest.Result.Success
                ? null
                : DownloadHandlerAudioClip.GetContent(www);

            if (audioClip != null)
            {
                audioClip.name = Path.GetFileNameWithoutExtension(path);
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
    }
}