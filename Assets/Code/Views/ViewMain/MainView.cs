using Code.Infrastructure.Datas;
using Code.Views.ViewUserData;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Views.ViewMain
{
    public sealed class MainView :
        MonoBehaviour,
        IMainView
    {
        public Transform RootUserData => _rootUserData;

        [SerializeField] private UserDataView _prefabView;
        [SerializeField] private Transform _rootUserData;
        [SerializeField] private RectTransform _scrollContent;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

        [SerializeField] private float _spacing = 15;
        [SerializeField] private int _startItemCount = 20;
        [SerializeField] private int _checkIndex = 10;

        private UserDataView[] _items;
        private UserData[] _data;

        private float[] _itemPositions;
        private float _prevPositions;
        private int _upIndex;
        private int _downIndex;

        public void Awake()
        {
            _scrollRect.onValueChanged.AddListener(ScrollValueChanged);
        }

        public void Initialize(UserData[] data)
        {
            _data = data;

            _itemPositions = new float[_data.Length];

            var height = _prefabView.GetComponent<RectTransform>().rect.height;

            for (int i = 0; i < _data.Length; i++) _itemPositions[i] = -(height + _spacing) * i;

            _scrollContent.sizeDelta = new Vector2(0, Mathf.Abs(_itemPositions[^1]));

            _items = new UserDataView[_startItemCount];

            for (var i = 0; i < _startItemCount; i++)
            {
                var userData = _data[i];
                var view = Instantiate(_prefabView, _rootUserData);
                view.transform.localPosition = new Vector2(0, _itemPositions[i]);
                view.Initialize(userData);

                _items[i] = view;
            }

            _upIndex = 0;
            _downIndex = _items.Length;
        }

        private void ScrollValueChanged(Vector2 position)
        {
            var normalizedPosition = _scrollRect.verticalNormalizedPosition;
            if (normalizedPosition is 0 or 1) return;

            var currentPosition = Mathf.Abs(_itemPositions[^1]) - Mathf.Abs(_itemPositions[^1]) * normalizedPosition;

            if (currentPosition > _prevPositions)
            {
                if (_downIndex >= _data.Length) return;

                if (-currentPosition < _items[_checkIndex].RectTransform.anchoredPosition.y)
                {
                    var item = _items[0];
                    for (var i = 0; i < _items.Length - 1; i++) _items[i] = _items[i + 1];
                    _items[^1] = item;

                    UpdateItem(item, _downIndex);

                    _upIndex++;
                    _downIndex++;
                }
            }
            else
            {
                if (_upIndex < 0) return;

                if (-currentPosition > _items[_checkIndex].RectTransform.anchoredPosition.y)
                {
                    var item = _items[^1];
                    for (var i = _items.Length - 1; i > 0; i--) _items[i] = _items[i - 1];
                    _items[0] = item;

                    UpdateItem(item, _upIndex);

                    _upIndex--;
                    _downIndex--;
                }
            }

            _prevPositions = currentPosition;
        }

        private void UpdateItem(UserDataView item, int index)
        {
            item.transform.localPosition = new Vector2(0, _itemPositions[index]);
            item.Initialize(_data[index]);
        }
    }
}