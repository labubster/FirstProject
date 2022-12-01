using System;
using System.Collections.Generic;
using System.Text.Json;
using Cysharp.Threading.Tasks;
using Nekoyume.Pattern;
using Nekoyume.UI;
using Nekoyume.UI.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace Nekoyume.Game.Notice
{
    [Serializable]
    public class NoticeData
    {
        public int Priority { get; set; }
        public Sprite BannerImage { get; set; }
        public Sprite PopupImage { get; set; }
        public bool UseDateTime { get; set; }
        public string BeginDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string Url { get; set; }
        public bool UseAgentAddress { get; set; }
        public string Description { get; set; }
    }

    public class NoticeManager : MonoSingleton<NoticeManager>
    {
        private const string JsonUrl =
            "https://raw.githubusercontent.com/planetarium/NineChronicles.LiveAssets/feature/renew-notice/Assets/Json/Banner.json";

        private const string ImageUrl =
            "https://raw.githubusercontent.com/planetarium/NineChronicles.LiveAssets/feature/renew-notice/Assets/Images";
        private static readonly Vector2 Pivot = new(0.5f, 0.5f);
        private List<NoticeData> _data = new();

        public IReadOnlyList<NoticeData> Data => _data;
        public bool IsInitialized { get; private set; }

        public void InitializeData()
        {
            StartCoroutine(RequestManager.instance.GetJson(JsonUrl, Set));
        }

        private void Set(string response)
        {
            var responseData = JsonSerializer.Deserialize<EventBanners>(response);
            MakeNoticeData(responseData.Banners).Forget();
        }

        private async UniTaskVoid MakeNoticeData(IEnumerable<EventBannerData> bannerData)
        {
            var tasks = new List<UniTask>();
            foreach (var banner in bannerData)
            {
                var newData = new NoticeData
                {
                    Priority = banner.Priority,
                    BannerImage = null,
                    PopupImage = null,
                    UseDateTime = banner.UseDateTime,
                    BeginDateTime = banner.BeginDateTime,
                    EndDateTime = banner.EndDateTime,
                    Url = banner.Url,
                    UseAgentAddress = banner.UseAgentAddress,
                    Description = banner.Description
                };
                _data.Add(newData);

                if (newData.UseDateTime && !Helper.Util.IsInTime(newData.BeginDateTime, newData.EndDateTime))
                {
                    continue;
                }

                var bannerTask = GetTexture("Banner", banner.BannerImageName)
                    .ContinueWith(sprite => newData.BannerImage = sprite);
                var popupTask = GetTexture("Notice", banner.PopupImageName)
                    .ContinueWith(sprite => newData.PopupImage = sprite);
                tasks.Add(bannerTask);
                tasks.Add(popupTask);
            }

            await UniTask.WaitUntil(() =>
                tasks.TrueForAll(task => task.Status == UniTaskStatus.Succeeded));
            IsInitialized = true;
        }

        private async UniTask<Sprite> GetTexture(string textureType, string imageName)
        {
            var www = UnityWebRequestTexture.GetTexture($"{ImageUrl}/{textureType}/{imageName}.png");
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                return null;
            }

            var myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            return Sprite.Create(
                myTexture,
                new Rect(0, 0, myTexture.width, myTexture.height),
                Pivot);
        }
    }
}
