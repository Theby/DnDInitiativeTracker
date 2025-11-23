using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using DnDInitiativeTracker.Controller;
using UnityEngine;
using UnityEngine.Networking;

namespace DnDInitiativeTracker.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DataManager dataManager;
        [SerializeField] CanvasManager canvasManager;

        IEnumerator Start()
        {
            yield return NativeGalleryController.RequestPermissions();

            //yield return dataManager.Initialize();

            //canvasManager.Initialize();

            //canvasManager.ShowPlayersScreen();
        }

        public void Awake()
        {
            //TODO solution: remove all coroutines and use async/await correctly
            Initialize();
        }

        async Task Initialize()
        {
            // var path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultBG.jpg");
            // yield return LoadImageAtPathAsync(path, false, texture2D =>
            // {
            //     if (texture2D != null)
            //     {
            //         texture2D.name = Path.GetFileNameWithoutExtension(path);
            //     }
            //
            //     canvasManager.background.texture = texture2D;
            // });

            await dataManager.Initialize();

            canvasManager.Initialize();

            canvasManager.ShowPlayersScreen();
        }

        static async Task<Texture2D> LoadImageAtPathAsync(string path, bool markTextureNonReadable = false, Action<Texture2D> onComplete = null)
        {
            Debug.LogError($"Loading image from Test path: {path}");

            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return null;
            }

            var uri = new Uri(path);
            Texture2D texture2D;

            try
            {
                using var www = UnityWebRequestTexture.GetTexture(uri, markTextureNonReadable);
                await www.SendWebRequest();

                texture2D = www.result is not UnityWebRequest.Result.Success
                    ? null
                    : DownloadHandlerTexture.GetContent(www);

                Debug.LogError($"success {www.result} - null? {texture2D == null}");
            }
            catch (Exception e)
            {
                texture2D = null;
                Debug.LogError(e.Message);
            }

            onComplete?.Invoke(texture2D);
            return texture2D;
        }

        // void Awake()
        // {
        //     StartCoroutine(Initialize());
        // }
        //
        // IEnumerator Initialize()
        // {
        //     yield return dataManager.Initialize();
        //
        //     canvasManager.Initialize();
        //
        //     canvasManager.ShowPlayersScreen();
        // }
    }
}