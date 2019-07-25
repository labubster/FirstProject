using System;
using Assets.SimpleLocalization;
using DG.Tweening;
using Nekoyume.BlockChain;
using Nekoyume.State;
using Nekoyume.Game.Controller;
using Nekoyume.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class LoginDetail : Widget
    {
        public GameObject btnLogin;
        public Text btnLoginText;
        public GameObject btnCreate;
        public Text btnCreateText;
        public InputField nameField;
        public Text namePlaceHolder;
        public Text textHp;
        public Text textExp;
        public Slider hpBar;
        public Slider expBar;
        public Text levelInfo;
        public Text nameInfo;
        public RectTransform content;
        public GameObject contentPivot;
        public GameObject profileImage;
        public GameObject statusGrid;
        public GameObject statusRow;
        public GameObject optionGrid;
        public GameObject optionRow;
        public GameObject palette;
        public Text paletteHairText;
        public Text paletteLensText;
        public Text paletteTopText;

        private int _selectedIndex;
        private bool _isCreateMode;

        private Color _namePlaceHolderOriginColor;
        private Color _namePlaceHolderFocusedColor;

        protected override void Awake()
        {
            base.Awake();

            btnCreateText.text = LocalizationManager.Localize("UI_CREATE_CHARACTER_CONFIRM");
            btnLoginText.text = LocalizationManager.Localize("UI_GAME_START");
            namePlaceHolder.text = LocalizationManager.Localize("UI_INPUT_NAME");
            paletteHairText.text = LocalizationManager.Localize("UI_HAIR");
            paletteLensText.text = LocalizationManager.Localize("UI_LENS");
            paletteTopText.text = LocalizationManager.Localize("UI_ETC");

            nameField.gameObject.SetActive(false);
            Game.Event.OnLoginDetail.AddListener(Init);
            
            _namePlaceHolderOriginColor = namePlaceHolder.color;
            _namePlaceHolderFocusedColor = namePlaceHolder.color;
            _namePlaceHolderFocusedColor.a = 0.3f;
        }

        private void Update()
        {
            if (nameField.isFocused)
            {
                namePlaceHolder.color = _namePlaceHolderFocusedColor;
            }
            else
            {
                namePlaceHolder.color = _namePlaceHolderOriginColor;
            }
        }

        public void CreateClick()
        {
            Find<GrayLoadingScreen>()?.Show();
            
            var nickName = nameField.text;

            ActionManager.instance
                .CreateAvatar(AvatarManager.GetOrCreateAvatarAddress(_selectedIndex), _selectedIndex, nickName)
                .Subscribe(eval =>
                {
                    var avatarState = AvatarManager.SetIndex(_selectedIndex);
                    OnDidAvatarStateLoaded(avatarState);
                    Find<GrayLoadingScreen>()?.Close();
                });
            AudioController.PlayClick();
        }

        public void LoginClick()
        {
            btnLogin.SetActive(false);
            nameField.gameObject.SetActive(false);
            var avatarState = AvatarManager.SetIndex(_selectedIndex);
            OnDidAvatarStateLoaded(avatarState);
            AudioController.PlayClick();
        }

        public void BackClick()
        {
            Close();
            Game.Event.OnNestEnter.Invoke();
            var login = Find<Login>();
            login.Show();
            AudioController.PlayClick();
        }

        private void Init(int index)
        {
            _selectedIndex = index;
            AvatarManager.GetOrCreateAvatarAddress(_selectedIndex);
            Player player;
            _isCreateMode = !States.Instance.avatarStates.ContainsKey(index);
            if (_isCreateMode)
            {
                player = new Player();
                nameField.text = "";
                nameInfo.text = "";
            }
            else
            {
                States.Instance.currentAvatarState.Value = States.Instance.avatarStates[_selectedIndex];
                player = new Player(States.Instance.currentAvatarState.Value);
                nameInfo.text = States.Instance.currentAvatarState.Value.name;
            }
            
            // create new or login
            nameField.gameObject.SetActive(_isCreateMode);
            btnCreate.SetActive(_isCreateMode);
            palette.SetActive(_isCreateMode);

            // 현재 프로필 사진의 용도가 불투명하기 때문에 가려둠.
            // profileImage.SetActive(!isCreateMode);
            btnLogin.SetActive(!_isCreateMode);
            optionGrid.SetActive(!_isCreateMode);
            
            SetInformation(player);

            Show();
        }

        private void SetInformation(Player player)
        {
            levelInfo.text = $"LV. {player.level}";
            
            var hp = player.currentHP;
            var hpMax = player.currentHP;
            var exp = player.exp;
            var expMax = player.expMax;

            //hp, exp
            textHp.text = $"{hp}/{hpMax}";
            textExp.text = $"{exp}/{expMax}";

            //percentage
            var hpPercentage = hp / (float) hpMax;
            var expPercentage = player.exp / (float) expMax;

            foreach (
                var tuple in new[]
                {
                    new Tuple<Slider, float>(hpBar, hpPercentage),
                    new Tuple<Slider, float>(expBar, expPercentage)
                }
            )
            {
                var slide = tuple.Item1;
                var percentage = tuple.Item2;
                slide.fillRect.gameObject.SetActive(percentage > 0.0f);
                percentage = Mathf.Min(Mathf.Max(percentage, 0.1f), 1.0f);
                slide.value = 0.0f;
                slide.DOValue(percentage, 2.0f).SetEase(Ease.OutCubic);
            }

            // status info
            var fields = player.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(InformationFieldAttribute), true))
                {
                    GameObject row = Instantiate(statusRow, statusGrid.transform);
                    var info = row.GetComponent<StatusInfo>();
                    info.Set(field.Name, field.GetValue(player), player.GetAdditionalStatus(field.Name));
                }
            }

            //option info
            foreach (var option in player.GetOptions())
            {
                GameObject row = Instantiate(optionRow, optionGrid.transform);
                var text = row.GetComponent<Text>();
                text.text = option;
                row.SetActive(true);
            }
        }

        public override void Show()
        {
            base.Show();
            contentPivot.SetActive(!_isCreateMode);
        }

        public override void Close()
        {
            Clear();
            base.Close();
        }

        private void Clear()
        {
            foreach (Transform child in statusGrid.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in optionGrid.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnDidAvatarStateLoaded(AvatarState avatarState)
        {
            if (_isCreateMode)
                Close();
            
            EnterRoom();
        }
        
        private void EnterRoom()
        {
            Game.Event.OnRoomEnter.Invoke();
            Find<Login>()?.Close();
            Close();
        }
    }
}
