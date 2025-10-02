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
        public static async Task RequestPermissions()
        {
            Task readTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
            Task writeTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write,
                NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);

            await readTask;
            await writeTask;
        }

        public static void GetImageFromGallery(Action<Texture2D> onComplete)
        {
            NativeGallery.GetImageFromGallery(path =>
            {
                if (path == null)
                    return;

                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture == null)
                    return;

                var fileName = Path.GetFileNameWithoutExtension(path);
                texture.name = fileName;

                onComplete?.Invoke(texture);

                // NativeGallery.SaveImageToGallery(path, "DnDIT", fileName, (success, s) =>
                // {
                //     onComplete?.Invoke(texture);
                // });
            });
        }

        public static IEnumerator LoadAudioClip(string path)
        {
            var uri = new System.Uri(path);

            AudioType audioType = AudioType.UNKNOWN;
            if (path.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.MPEG;
            }
            else if (path.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.WAV;
            }
            else if (path.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase))
            {
                audioType = AudioType.OGGVORBIS;
            }

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            yield return www.SendWebRequest();

            if (www.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Error loading audio: {www.error} - Uri path: {uri.AbsolutePath} - AudioType: {audioType}");
                yield break;
            }

            var _audioClip = DownloadHandlerAudioClip.GetContent(www);
            if (_audioClip != null)
            {
                // NativeGallery.SaveAudioToGallery(path, "DnDIT", "loadedAudioClip",
                //     (success, s) => { imagePath.text = $"Saving to {s} - {success}"; });
            }
        }
    }
}