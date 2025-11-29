using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DnDInitiativeTracker.Test
{
    public class NativeGalleryTest : MonoBehaviour
    {
        [SerializeField] RawImage image;
        [SerializeField] TextMeshProUGUI imagePath;
        [SerializeField] AudioSource audioSource;
        [SerializeField] TextMeshProUGUI audioPath;

        AudioClip _audioClip;

        void Awake()
        {
            RequestPermissions();
        }

        async void RequestPermissions()
        {
            try
            {
                await Task.Delay(2000);

                Task readTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read,
                    NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);
                Task writeTask = NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write,
                    NativeGallery.MediaType.Image | NativeGallery.MediaType.Audio);

                await readTask;
                await writeTask;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void OnImagePressed()
        {
            imagePath.text = string.Empty;

            NativeGallery.GetImageFromGallery(path =>
            {
                if (path == null)
                    return;

                imagePath.text = path;

                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture == null)
                    return;

                image.texture = texture;

                NativeGallery.SaveImageToGallery(path, "DnDIT", "loadedImage",
                    (success, s) => { imagePath.text = $"Saving to {s} - {success}"; });
            });
        }

        public void OnAudioPressed()
        {
            audioPath.text = string.Empty;

            NativeGallery.GetAudioFromGallery(path =>
            {
                if (path == null)
                    return;

                StartCoroutine(LoadAudioClip(path));
            });
        }

        IEnumerator LoadAudioClip(string path)
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

            imagePath.text = $"pre result {www.result}";

            if (www.result is not UnityWebRequest.Result.Success)
            {
                audioPath.text =
                    $"Error loading audio: {www.error} - Uri path: {uri.AbsolutePath} - AudioType: {audioType}";
            }
            else
            {
                _audioClip = DownloadHandlerAudioClip.GetContent(www);
                if (_audioClip != null)
                {
                    _audioClip.name = "loadedAudioClip";

                    imagePath.text = "Success";
                    audioPath.text = path;

                    NativeGallery.SaveAudioToGallery(path, "DnDIT", "loadedAudioClip",
                        (success, s) => { imagePath.text = $"Saving to {s} - {success}"; });
                }
                else
                {
                    imagePath.text = "Failed";
                    audioPath.text =
                        $"Failed to get AudioClip from downloaded content. - path {path} - Uri path: {uri.AbsoluteUri} - www {www.result} - AudioType: {audioType}";
                }
            }
        }

        public void OnPlayAudioPressed()
        {
            if (_audioClip == null)
            {
                imagePath.text = "Audio Clip null";
                return;
            }

            imagePath.text = "One Shot";
            audioSource.PlayOneShot(_audioClip);
        }
    }
}