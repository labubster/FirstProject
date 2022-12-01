using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using Nekoyume.Game.Controller;
using Nekoyume.UI.Model;
using Nekoyume.Game.Notice;

namespace Nekoyume.UI
{
    public class NoticePopup : PopupWidget
    {
        [SerializeField]
        private Image contentImage;

        [SerializeField]
        private Button detailButton;

        [SerializeField]
        private Button closeButton;

        private const string LastNoticeDayKeyFormat = "LAST_NOTICE_DAY_{0}";

        private Notice _data;

        private bool CanShowNoticePopup()
        {
            if (_data == null)
            {
                return false;
            }

            if (!Game.Game.instance.Stage.TutorialController.IsCompleted)
            {
                return false;
            }

            if (!DateTime.UtcNow.IsInTime(_data.BeginDateTime, _data.EndDateTime))
            {
                return false;
            }

            var lastNoticeDayKey = string.Format(LastNoticeDayKeyFormat, _data.ImageName);
            var lastNoticeDay = DateTime.ParseExact(
                PlayerPrefs.GetString(lastNoticeDayKey, "2022/03/01 00:00:00"),
                "yyyy/MM/dd HH:mm:ss",
                null);
            var now = DateTime.UtcNow;
            var isNewDay = now.Year != lastNoticeDay.Year ||
                           now.Month != lastNoticeDay.Month ||
                           now.Day != lastNoticeDay.Day;
            if (isNewDay)
            {
                PlayerPrefs.SetString(lastNoticeDayKey, now.ToString("yyyy/MM/dd HH:mm:ss"));
            }

            return isNewDay;
        }

        protected override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() =>
            {
                Close();
                AudioController.PlayClick();
            });
        }

        public override void Show(bool ignoreStartAnimation = false)
        {
            base.Show(ignoreStartAnimation);
            var firstPopup =
                NoticeManager.instance.Data.FirstOrDefault(data => data.PopupImage != null);
            if (firstPopup is not null)
            {
                detailButton.onClick.RemoveAllListeners();
                detailButton.onClick.AddListener(() =>
                {
                    Application.OpenURL(firstPopup.Url);
                    AudioController.PlayClick();
                });
                contentImage.sprite = firstPopup.PopupImage;
                Debug.LogError($"desc: {firstPopup.Description}");
            }
        }
    }
}
