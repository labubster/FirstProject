using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nekoyume.EnumType;
using Nekoyume.Game.Character;
using Nekoyume.Game.Controller;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class ItemView<TViewModel> : VanillaItemView
        where TViewModel : Model.Item
    {
        public TouchHandler touchHandler;
        public Button itemButton;
        public Image backgroundImage;
        public TextMeshProUGUI enhancementText;
        public Image selectionImage;

        private readonly List<IDisposable> _disposablesAtSetData = new List<IDisposable>();

        public RectTransform RectTransform { get; private set; }

        public Vector3 CenterOffsetAsPosition
        {
            get
            {
                var pivotPosition = RectTransform.GetPivotPositionFromAnchor(PivotPresetType.MiddleCenter);
                var position = new Vector3(pivotPosition.x, pivotPosition.y);
                return RectTransform.localToWorldMatrix * position;
            }
        }

        [CanBeNull] public TViewModel Model { get; private set; }
        public bool IsEmpty => Model?.ItemBase.Value is null;

        public readonly Subject<ItemView<TViewModel>> OnClick = new Subject<ItemView<TViewModel>>();
        public readonly Subject<ItemView<TViewModel>> OnDoubleClick = new Subject<ItemView<TViewModel>>();

        #region Mono

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            touchHandler.OnClick.Subscribe(_ =>
            {
                AudioController.PlayClick();
                OnClick.OnNext(this);
                Model?.OnClick.OnNext(Model);
            }).AddTo(gameObject);
            touchHandler.OnDoubleClick.Subscribe(_ =>
            {
                AudioController.PlayClick();
                OnDoubleClick.OnNext(this);
                Model?.OnDoubleClick.OnNext(Model);
            }).AddTo(gameObject);
        }

        protected virtual void OnDestroy()
        {
            Model?.Dispose();
            OnClick.Dispose();
            OnDoubleClick.Dispose();
            Clear();
        }

        #endregion
        
        public virtual void SetData(TViewModel model)
        {
            if (model is null)
            {
                Clear();
                return;
            }
            
            base.SetData(model.ItemBase.Value.Data);
            _disposablesAtSetData.DisposeAllAndClear();
            Model = model;
            Model.GradeEnabled.SubscribeTo(gradeImage).AddTo(_disposablesAtSetData);
            Model.Enhancement.SubscribeTo(enhancementText).AddTo(_disposablesAtSetData);
            Model.EnhancementEnabled.SubscribeTo(enhancementText).AddTo(_disposablesAtSetData);
            Model.Dimmed.Subscribe(SetDim).AddTo(_disposablesAtSetData);
            Model.Selected.SubscribeTo(selectionImage).AddTo(_disposablesAtSetData);

            UpdateView();
        }

        public virtual void SetToUnknown()
        {
            Clear();
            iconImage.enabled = true;
            iconImage.overrideSprite = Resources.Load<Sprite>("UI/Textures/UI_icon_item_question");
            iconImage.SetNativeSize();
        }

        public override void Clear()
        {
            Model = null;
            _disposablesAtSetData.DisposeAllAndClear();

            UpdateView();
            base.Clear();
        }

        protected override void SetDim(bool isDim)
        {
            base.SetDim(isDim);
            
            enhancementText.color = isDim ? DimmedColor : OriginColor;
            selectionImage.color = isDim ? DimmedColor : OriginColor;
        }

        private void UpdateView()
        {
            if (Model is null ||
                Model.ItemBase.Value is null)
            {
                enhancementText.enabled = false;
                selectionImage.enabled = false;
            }
        }
    }
}
