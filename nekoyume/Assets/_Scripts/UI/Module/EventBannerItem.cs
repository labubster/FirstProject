using Nekoyume.Game.Notice;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class EventBannerItem : MonoBehaviour
    {
        [SerializeField]
        private RawImage image;

        [SerializeField]
        private Button button;

        public void Set(NoticeData data, System.Action<NoticeData> onClick = null)
        {
            image.texture = data.BannerImage.texture;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                var action = onClick ?? (noticeData =>
                    Widget.Find<EventReleaseNotePopup>().Show(noticeData));
                action.Invoke(data);
            });
        }
    }
}
