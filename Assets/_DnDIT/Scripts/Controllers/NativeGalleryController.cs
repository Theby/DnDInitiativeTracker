using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DnDInitiativeTracker.Controller
{
    public static class NativeGalleryController
    {
        const string AlbumName = "DnDIT";

        public static async Task RequestPermissions()
        {
            Task readTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
            Task writeTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);

            await readTask;
            await writeTask;
        }

        public static void GetImageFromGallery(Action<Texture2D> onComplete = null, bool saveToGallery = false, Action<string, string> onSaveComplete = null)
        {
            NativeGallery.GetImageFromGallery(path =>
            {
                if (path == null)
                    return;

                var texture = GetImageFromPath(path);

                if (saveToGallery)
                {
                    SaveImageToGallery(path, AlbumName, texture.name, onSaveComplete);
                }

                onComplete?.Invoke(texture);
            });
        }

        public static Texture2D GetImageFromPath(string path)
        {
            var texture = NativeGallery.LoadImageAtPath(path);
            if (texture == null)
                return null;

            var fileName = Path.GetFileNameWithoutExtension(path);
            texture.name = fileName;

            return texture;
        }

        public static void SaveImageToGallery(string path, string albumName, string fileName, Action<string, string> onComplete = null)
        {
            string fullPath = path;
            string currentFileName = fileName;
            NativeGallery.SaveImageToGallery(fullPath, albumName, currentFileName, (success, savePath) =>
            {
                #if UNITY_EDITOR
                    onComplete?.Invoke(fullPath, currentFileName);
                #else
                    onComplete?.Invoke(savePath, currentFileName);
                #endif
            });
        }

        public static void GetAudioFromGallery(Action<AudioClip> onComplete = null)
        {
            NativeGallery.GetAudioFromGallery(path =>
            {
                if (path == null)
                    return;

                GetAudioClipFromPath(path, onComplete);
            });
        }

        public static async Task GetAudioClipFromPath(string path, Action<AudioClip> onComplete = null)
        {
            var uri = new Uri(path);

            var audioType = AudioType.UNKNOWN;
            if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.MPEG;
            }
            else if (path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.WAV;
            }
            else if (path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.OGGVORBIS;
            }

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            await www.SendWebRequest();

            AudioClip audioClip;
            if (www.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error loading audio: {www.error} - Uri path: {uri.AbsolutePath} - AudioType: {audioType}");
                audioClip = null;
            }
            else
            {
                audioClip = DownloadHandlerAudioClip.GetContent(www);
            }

            onComplete?.Invoke(audioClip);
        }

        public static void SaveAudioToGallery(string path, string albumName, string fileName, Action OnComplete = null)
        {
            NativeGallery.SaveAudioToGallery(path, albumName, fileName, (success, s) =>
            {
                OnComplete?.Invoke();
            });
        }
    }
}