using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Game.Notice;
using Nekoyume.State;
using Nekoyume.UI.Module;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class EventReleaseNotePopup : PopupWidget
    {
        [SerializeField]
        private Image eventImage;

        [SerializeField]
        private Button eventDetailButton;

        [SerializeField]
        private EventBannerItem originEventButtonObject;

        [SerializeField]
        private Transform eventStrollViewport;

        private List<NoticeData> _noticeData;
        private NoticeData _selectedNotice;

        public override void Initialize()
        {
            eventDetailButton.onClick.AddListener(() =>
            {
                var u = _selectedNotice.Url;
                if (_selectedNotice.UseAgentAddress)
                {
                    var address = States.Instance.AgentState.address;
                    u = string.Format(u, address);
                }

                Application.OpenURL(u);
            });

            base.Initialize();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => NoticeManager.instance.IsInitialized);

            _noticeData = (List<NoticeData>) NoticeManager.instance.Data;
            foreach (var notice in _noticeData)
            {
                var button = Instantiate(originEventButtonObject, eventStrollViewport);
                button.Set(notice, SelectNotice);
            }
        }

        public void Show(NoticeData data, bool ignoreStartAnimation = false)
        {
            base.Show(ignoreStartAnimation);
            SelectNotice(data);
        }

        private void SelectNotice(NoticeData data)
        {
            _selectedNotice = data;
            eventImage.overrideSprite = _selectedNotice.PopupImage;
        }
    }
}
