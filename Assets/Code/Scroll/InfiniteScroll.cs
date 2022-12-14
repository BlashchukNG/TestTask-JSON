using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Scroll
{
    public class InfiniteScroll :
        MonoBehaviour,
        IDropHandler
    {
        private const int UPDATE_TIME_DIFF = 500;
        private const float SCROLL_SPEED = 50f;
        private const float SCROLL_DURATION = 0.25f;

        public enum Direction
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }

        public delegate int HeightItem(int index);

        public event HeightItem OnHeight;

        public delegate int WidthItem(int index);

        public event WidthItem OnWidth;

        public Action<int, GameObject> OnFill = delegate { };
        public Action<Direction> OnPull = delegate { };

        [Header("Item settings")]
        public GameObject Prefab;

        [Header("Padding")]
        public int TopPadding = 10;

        public int BottomPadding = 10;

        [Header("Padding")]
        public int LeftPadding = 10;

        public int RightPadding = 10;
        public int ItemSpacing = 2;

        [Header("Labels")]
        public TMP_FontAsset LabelsFont;

        public string TopPullLabel = "Pull to refresh";
        public string TopReleaseLabel = "Release to load";
        public string BottomPullLabel = "Pull to refresh";
        public string BottomReleaseLabel = "Release to load";
        public string LeftPullLabel = "Pull to refresh";
        public string LeftReleaseLabel = "Release to load";
        public string RightPullLabel = "Pull to refresh";
        public string RightReleaseLabel = "Release to load";

        [Header("Directions")]
        public bool IsPullTop = true;

        public bool IsPullBottom = true;

        [Header("Directions")]
        public bool IsPullLeft = true;

        public bool IsPullRight = true;

        [Header("Offsets")]
        public float PullValue = 1.5f;

        public float LabelOffset = 85f;

        [HideInInspector]
        public TextMeshProUGUI TopLabel;

        [HideInInspector]
        public TextMeshProUGUI BottomLabel;

        [HideInInspector]
        public TextMeshProUGUI LeftLabel;

        [HideInInspector]
        public TextMeshProUGUI RightLabel;

        [HideInInspector]
        public int Type;

        private ScrollRect _scroll;
        private RectTransform _content;
        private Rect _container;
        private RectTransform[] _rects;
        private GameObject[] _views;
        private bool _isCanLoadUp;
        private bool _isCanLoadDown;
        private bool _isCanLoadLeft;
        private bool _isCanLoadRight;
        private int _previousPosition = -1;
        private int _count;

        private Dictionary<int, int> _heights;
        private Dictionary<int, int> _widths;
        private Dictionary<int, float> _positions;

        private DateTime _lastMoveTime;
        private float _previousScrollPosition;
        private int _saveStepPosition = -1;

        void Awake()
        {
            _container = GetComponent<RectTransform>().rect;
            _scroll = GetComponent<ScrollRect>();
            _scroll.onValueChanged.AddListener(OnScrollChange);
            _content = _scroll.viewport.transform.GetChild(0).GetComponent<RectTransform>();
            _heights = new Dictionary<int, int>();
            _widths = new Dictionary<int, int>();
            _positions = new Dictionary<int, float>();
            CreateLabels();
        }

        void Update()
        {
            if (Type == 0)
            {
                UpdateVertical();
            }
            else
            {
                UpdateHorizontal();
            }
        }

        void UpdateVertical()
        {
            if (_count == 0)
            {
                return;
            }

            float _topPosition = _content.anchoredPosition.y - ItemSpacing;
            if (_topPosition <= 0f && _rects[0].anchoredPosition.y < -TopPadding - 10f)
            {
                InitData(_count);
                return;
            }

            if (_topPosition < 0f)
            {
                return;
            }

            if (!_positions.ContainsKey(_previousPosition) || !_heights.ContainsKey(_previousPosition))
            {
                return;
            }

            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
            int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            int border = (int)(_positions[0] + _heights[0]);
            int step = (int)((_topPosition + _topPosition / 1.25f) / border);
            if (step != _saveStepPosition)
            {
                _saveStepPosition = step;
            }
            else
            {
                return;
            }

            if (position < 0 || _previousPosition == position || _scroll.velocity.y == 0f)
            {
                return;
            }

            if (position > _previousPosition)
            {
                if (position - _previousPosition > 1)
                {
                    position = _previousPosition + 1;
                }

                int newPosition = position % _views.Length;
                newPosition--;
                if (newPosition < 0)
                {
                    newPosition = _views.Length - 1;
                }

                int index = position + _views.Length - 1;
                if (index < _count)
                {
                    Vector2 pos = _rects[newPosition].anchoredPosition;
                    pos.y = _positions[index];
                    _rects[newPosition].anchoredPosition = pos;
                    Vector2 size = _rects[newPosition].sizeDelta;
                    size.y = _heights[index];
                    _rects[newPosition].sizeDelta = size;
                    _views[newPosition].name = index.ToString();
                    OnFill(index, _views[newPosition]);
                }
            }
            else
            {
                if (_previousPosition - position > 1)
                {
                    position = _previousPosition - 1;
                }

                int newIndex = position % _views.Length;
                Vector2 pos = _rects[newIndex].anchoredPosition;
                pos.y = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.y = _heights[position];
                _rects[newIndex].sizeDelta = size;
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
            }

            _previousPosition = position;
        }

        void UpdateHorizontal()
        {
            if (_count == 0)
            {
                return;
            }

            float _leftPosition = _content.anchoredPosition.x * -1f - ItemSpacing;
            if (_leftPosition <= 0f && _rects[0].anchoredPosition.x < -LeftPadding - 10f)
            {
                InitData(_count);
                return;
            }

            if (_leftPosition < 0f)
            {
                return;
            }

            if (!_positions.ContainsKey(_previousPosition) || !_heights.ContainsKey(_previousPosition))
            {
                return;
            }

            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
            int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            int border = (int)(_positions[0] + _widths[0]);
            int step = (int)((_leftPosition + _leftPosition / 1.25f) / border);
            if (step != _saveStepPosition)
            {
                _saveStepPosition = step;
            }
            else
            {
                return;
            }

            if (position < 0 || _previousPosition == position || _scroll.velocity.x == 0f)
            {
                return;
            }

            if (position > _previousPosition)
            {
                if (position - _previousPosition > 1)
                {
                    position = _previousPosition + 1;
                }

                int newPosition = position % _views.Length;
                newPosition--;
                if (newPosition < 0)
                {
                    newPosition = _views.Length - 1;
                }

                int index = position + _views.Length - 1;
                if (index < _count)
                {
                    Vector2 pos = _rects[newPosition].anchoredPosition;
                    pos.x = _positions[index];
                    _rects[newPosition].anchoredPosition = pos;
                    Vector2 size = _rects[newPosition].sizeDelta;
                    size.x = _widths[index];
                    _rects[newPosition].sizeDelta = size;
                    _views[newPosition].name = index.ToString();
                    OnFill(index, _views[newPosition]);
                }
            }
            else
            {
                if (_previousPosition - position > 1)
                {
                    position = _previousPosition - 1;
                }

                int newIndex = position % _views.Length;
                Vector2 pos = _rects[newIndex].anchoredPosition;
                pos.x = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.x = _widths[position];
                _rects[newIndex].sizeDelta = size;
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
            }

            _previousPosition = position;
        }

        void OnScrollChange(Vector2 vector)
        {
            if (Type == 0)
            {
                ScrollChangeVertical(vector);
            }
            else
            {
                ScrollChangeHorizontal(vector);
            }
        }

        void ScrollChangeVertical(Vector2 vector)
        {
            _isCanLoadUp = false;
            _isCanLoadDown = false;
            if (_views == null)
            {
                return;
            }

            float y = 0f;
            float z = 0f;
            bool isScrollable = (_scroll.verticalNormalizedPosition != 1f && _scroll.verticalNormalizedPosition != 0f);
            y = _content.anchoredPosition.y;
            if (isScrollable)
            {
                if (_scroll.verticalNormalizedPosition < 0f)
                {
                    z = y - _previousScrollPosition;
                }
                else
                {
                    _previousScrollPosition = y;
                }
            }
            else
            {
                z = y;
            }

            if (y < -LabelOffset && IsPullTop)
            {
                TopLabel.gameObject.SetActive(true);
                TopLabel.text = TopPullLabel;
                if (y < -LabelOffset * PullValue)
                {
                    TopLabel.text = TopReleaseLabel;
                    _isCanLoadUp = true;
                }
            }
            else
            {
                TopLabel.gameObject.SetActive(false);
            }

            if (z > LabelOffset && IsPullBottom)
            {
                BottomLabel.gameObject.SetActive(true);
                BottomLabel.text = BottomPullLabel;
                if (z > LabelOffset * PullValue)
                {
                    BottomLabel.text = BottomReleaseLabel;
                    _isCanLoadDown = true;
                }
            }
            else
            {
                BottomLabel.gameObject.SetActive(false);
            }
        }

        void ScrollChangeHorizontal(Vector2 vector)
        {
            _isCanLoadLeft = false;
            _isCanLoadRight = false;
            if (_views == null)
            {
                return;
            }

            float x = 0f;
            float z = 0f;
            bool isScrollable = (_scroll.horizontalNormalizedPosition != 1f && _scroll.horizontalNormalizedPosition != 0f);
            x = _content.anchoredPosition.x;
            if (isScrollable)
            {
                if (_scroll.horizontalNormalizedPosition > 1f)
                {
                    z = x - _previousScrollPosition;
                }
                else
                {
                    _previousScrollPosition = x;
                }
            }
            else
            {
                z = x;
            }

            if (x > LabelOffset && IsPullLeft)
            {
                LeftLabel.gameObject.SetActive(true);
                LeftLabel.text = LeftPullLabel;
                if (x > LabelOffset * PullValue)
                {
                    LeftLabel.text = LeftReleaseLabel;
                    _isCanLoadLeft = true;
                }
            }
            else
            {
                LeftLabel.gameObject.SetActive(false);
            }

            if (z < -LabelOffset && IsPullRight)
            {
                RightLabel.gameObject.SetActive(true);
                RightLabel.text = RightPullLabel;
                if (z < -LabelOffset * PullValue)
                {
                    RightLabel.text = RightReleaseLabel;
                    _isCanLoadRight = true;
                }
            }
            else
            {
                RightLabel.gameObject.SetActive(false);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Type == 0)
            {
                DropVertical();
            }
            else
            {
                DropHorizontal();
            }
        }

        void DropVertical()
        {
            if (_isCanLoadUp)
            {
                OnPull(Direction.Top);
            }
            else if (_isCanLoadDown)
            {
                OnPull(Direction.Bottom);
            }

            _isCanLoadUp = false;
            _isCanLoadDown = false;
        }

        void DropHorizontal()
        {
            if (_isCanLoadLeft)
            {
                OnPull(Direction.Left);
            }
            else if (_isCanLoadRight)
            {
                OnPull(Direction.Right);
            }

            _isCanLoadLeft = false;
            _isCanLoadRight = false;
        }

        public void InitData(int count)
        {
            if (Type == 0)
            {
                InitVertical(count);
            }
            else
            {
                InitHorizontal(count);
            }
        }

        void InitVertical(int count)
        {
            float height = CalcSizesPositions(count);
            CreateViews();
            _previousPosition = 0;
            _count = count;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            Vector2 size = Vector2.zero;
            pos.y = 0f;
            _content.anchoredPosition = pos;
            int y = TopPadding;
            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > _count)
                {
                    continue;
                }

                pos = _rects[i].anchoredPosition;
                pos.y = _positions[i];
                pos.x = 0f;
                _rects[i].anchoredPosition = pos;
                size = _rects[i].sizeDelta;
                size.y = _heights[i];
                _rects[i].sizeDelta = size;
                y += ItemSpacing + _heights[i];
                _views[i].name = i.ToString();
                OnFill(i, _views[i]);
            }
        }

        void InitHorizontal(int count)
        {
            float width = CalcSizesPositions(count);
            CreateViews();
            _previousPosition = 0;
            _count = count;
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            Vector2 size = Vector2.zero;
            pos.x = 0f;
            _content.anchoredPosition = pos;
            int x = LeftPadding;
            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > _count)
                {
                    continue;
                }

                pos = _rects[i].anchoredPosition;
                pos.x = _positions[i];
                pos.y = 0f;
                _rects[i].anchoredPosition = pos;
                size = _rects[i].sizeDelta;
                size.x = _widths[i];
                _rects[i].sizeDelta = size;
                x += ItemSpacing + _widths[i];
                _views[i].name = i.ToString();
                OnFill(i, _views[i]);
            }
        }

        float CalcSizesPositions(int count)
        {
            return (Type == 0) ? CalcSizesPositionsVertical(count) : CalcSizesPositionsHorizontal(count);
        }

        float CalcSizesPositionsVertical(int count)
        {
            _heights.Clear();
            _positions.Clear();
            float result = 0f;
            for (int i = 0; i < count; i++)
            {
                _heights[i] = OnHeight(i);
                _positions[i] = -(TopPadding + i * ItemSpacing + result);
                result += _heights[i];
            }

            result += TopPadding + BottomPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        float CalcSizesPositionsHorizontal(int count)
        {
            _widths.Clear();
            _positions.Clear();
            float result = 0f;
            for (int i = 0; i < count; i++)
            {
                _widths[i] = OnWidth(i);
                _positions[i] = LeftPadding + i * ItemSpacing + result;
                result += _widths[i];
            }

            result += LeftPadding + RightPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        public void ApplyDataTo(int count, int newCount, Direction direction)
        {
            if (Type == 0)
            {
                ApplyDataToVertical(count, newCount, direction);
            }
            else
            {
                ApplyDataToHorizontal(count, newCount, direction);
            }
        }

        void ApplyDataToVertical(int count, int newCount, Direction direction)
        {
            _count = count;
            if (_count <= _views.Length)
            {
                InitData(count);
                return;
            }

            float height = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            if (direction == Direction.Top)
            {
                float y = 0f;
                for (int i = 0; i < newCount; i++)
                {
                    y += _heights[i] + ItemSpacing;
                }

                pos.y = y;
                _previousPosition = newCount;
            }
            else
            {
                float h = 0f;
                for (int i = _heights.Count - 1; i >= _heights.Count - newCount; i--)
                {
                    h += _heights[i] + ItemSpacing;
                }

                pos.y = height - h - _container.height;
            }

            _content.anchoredPosition = pos;
            float _topPosition = _content.anchoredPosition.y - ItemSpacing;
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
            int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0)
            {
                _previousPosition = 0;
                position = 1;
            }

            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = position % _views.Length;
                if (newIndex < 0)
                {
                    continue;
                }

                _views[newIndex].gameObject.SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
                pos = _rects[newIndex].anchoredPosition;
                pos.y = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.y = _heights[position];
                _rects[newIndex].sizeDelta = size;
                position++;
                if (position == _count)
                {
                    break;
                }
            }
        }

        void ApplyDataToHorizontal(int count, int newCount, Direction direction)
        {
            _count = count;
            if (_count <= _views.Length)
            {
                InitData(count);
                return;
            }

            float width = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            if (direction == Direction.Left)
            {
                float x = 0f;
                for (int i = 0; i < newCount; i++)
                {
                    x -= _widths[i] + ItemSpacing;
                }

                pos.x = x;
                _previousPosition = newCount;
            }
            else
            {
                float w = 0f;
                for (int i = _widths.Count - 1; i >= _widths.Count - newCount; i--)
                {
                    w += _widths[i] + ItemSpacing;
                }

                pos.x = -width + w + _container.width;
            }

            _content.anchoredPosition = pos;
            float _leftPosition = _content.anchoredPosition.x - ItemSpacing;
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
            int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0)
            {
                _previousPosition = 0;
                position = 1;
            }

            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = position % _views.Length;
                if (newIndex < 0)
                {
                    continue;
                }

                _views[newIndex].gameObject.SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
                pos = _rects[newIndex].anchoredPosition;
                pos.x = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.x = _widths[position];
                _rects[newIndex].sizeDelta = size;
                position++;
                if (position == _count)
                {
                    break;
                }
            }
        }

        void MoveDataTo(int index, float height)
        {
            if (Type == 0)
            {
                MoveDataToVertical(index, height);
            }
            else
            {
                MoveDataToHorizontal(index, height);
            }
        }

        void MoveDataToVertical(int index, float height)
        {
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = index % _views.Length;
                _views[newIndex].name = index.ToString();
                if (index >= _count)
                {
                    _views[newIndex].gameObject.SetActive(false);
                    continue;
                }
                else
                {
                    _views[newIndex].gameObject.SetActive(true);
                    OnFill(index, _views[newIndex]);
                }

                pos = _rects[newIndex].anchoredPosition;
                pos.y = _positions[index];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.y = _heights[index];
                _rects[newIndex].sizeDelta = size;
                index++;
            }
        }

        void MoveDataToHorizontal(int index, float width)
        {
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = index % _views.Length;
                _views[newIndex].name = index.ToString();
                if (index >= _count)
                {
                    _views[newIndex].gameObject.SetActive(false);
                    continue;
                }
                else
                {
                    _views[newIndex].gameObject.SetActive(true);
                    OnFill(index, _views[newIndex]);
                }

                pos = _rects[newIndex].anchoredPosition;
                pos.x = _positions[index];
                _rects[newIndex].anchoredPosition = pos;
                Vector2 size = _rects[newIndex].sizeDelta;
                size.x = _widths[index];
                _rects[newIndex].sizeDelta = size;
                index++;
            }
        }

        public void MoveToSide(Direction direction)
        {
            DateTime now = DateTime.Now;
            if ((now - _lastMoveTime).TotalMilliseconds < UPDATE_TIME_DIFF)
            {
                return;
            }

            _lastMoveTime = now;
            StartCoroutine(MoveTo(direction));
        }

        IEnumerator MoveTo(Direction direction)
        {
            float speed = SCROLL_SPEED;
            float start = 0f;
            float end = 0f;
            float timer = 0f;
            if (Type == 0)
            {
                start = _scroll.verticalNormalizedPosition;
                end = (direction == Direction.Bottom) ? 0f : 1f;
            }
            else
            {
                start = _scroll.horizontalNormalizedPosition;
                end = (direction == Direction.Left) ? 0f : 1f;
            }

            while (timer <= 1f)
            {
                speed = Mathf.Lerp(speed, 0f, timer);
                if (Type == 0)
                {
                    _scroll.verticalNormalizedPosition = Mathf.Lerp(start, end, timer);
                    _scroll.velocity = new Vector2(0f, (direction == Direction.Top) ? -speed : speed);
                }
                else
                {
                    _scroll.horizontalNormalizedPosition = Mathf.Lerp(start, end, timer);
                    _scroll.velocity = new Vector2((direction == Direction.Left) ? speed : -speed, 0f);
                }

                timer += Time.deltaTime / SCROLL_DURATION;
                yield return null;
            }

            if (Type == 0)
            {
                _scroll.velocity = new Vector2(0f, (direction == Direction.Top) ? -SCROLL_SPEED : SCROLL_SPEED);
            }
            else
            {
                _scroll.velocity = new Vector2((direction == Direction.Left) ? SCROLL_SPEED : -SCROLL_SPEED, 0f);
            }
        }

        public void RecycleAll()
        {
            _count = 0;
            if (_views == null)
            {
                return;
            }

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].gameObject.SetActive(false);
            }
        }

        public void Recycle(int index)
        {
            _count--;
            string name = index.ToString();
            float height = CalcSizesPositions(_count);
            for (int i = 0; i < _views.Length; i++)
            {
                if (string.CompareOrdinal(_views[i].name, name) == 0)
                {
                    _views[i].gameObject.SetActive(false);
                    MoveDataTo(i, height);
                    break;
                }
            }
        }

        public void UpdateVisible()
        {
            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < _count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > _count)
                {
                    continue;
                }

                int index = int.Parse(_views[i].name);
                OnFill(index, _views[i]);
            }
        }

        public void RefreshViews()
        {
            if (_views == null)
            {
                return;
            }

            for (int i = 0; i < _views.Length; i++)
            {
                Destroy(_views[i].gameObject);
            }

            _rects = null;
            _views = null;
            CreateViews();
        }

        void CreateViews()
        {
            if (Type == 0)
            {
                CreateViewsVertical();
            }
            else
            {
                CreateViewsHorizontal();
            }
        }

        void CreateViewsVertical()
        {
            if (_views != null)
            {
                return;
            }

            GameObject clone;
            RectTransform rect;
            int height = 0;
            foreach (int item in _heights.Values)
            {
                height += item;
            }

            height = height / _heights.Count;
            int fillCount = Mathf.RoundToInt(_container.height / height) + 4;
            _views = new GameObject[fillCount];
            for (int i = 0; i < fillCount; i++)
            {
                clone = (GameObject)Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                clone.transform.SetParent(_content);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;
                rect = clone.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = Vector2.one;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                _views[i] = clone;
            }

            _rects = new RectTransform[_views.Length];
            for (int i = 0; i < _views.Length; i++)
            {
                _rects[i] = _views[i].gameObject.GetComponent<RectTransform>();
            }
        }

        void CreateViewsHorizontal()
        {
            if (_views != null)
            {
                return;
            }

            GameObject clone;
            RectTransform rect;
            int width = 0;
            foreach (int item in _widths.Values)
            {
                width += item;
            }

            width = width / _widths.Count;
            int fillCount = Mathf.RoundToInt(_container.width / width) + 4;
            _views = new GameObject[fillCount];
            for (int i = 0; i < fillCount; i++)
            {
                clone = (GameObject)Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                clone.transform.SetParent(_content);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;
                rect = clone.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(0f, 1f);
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                _views[i] = clone;
            }

            _rects = new RectTransform[_views.Length];
            for (int i = 0; i < _views.Length; i++)
            {
                _rects[i] = _views[i].gameObject.GetComponent<RectTransform>();
            }
        }

        void CreateLabels()
        {
            if (Type == 0)
            {
                CreateLabelsVertical();
            }
            else
            {
                CreateLabelsHorizontal();
            }
        }

        void CreateLabelsVertical()
        {
            GameObject topText = new GameObject("TopLabel");
            topText.transform.SetParent(_scroll.viewport.transform);
            TopLabel = topText.AddComponent<TextMeshProUGUI>();
            TopLabel.font = LabelsFont;
            TopLabel.fontSize = 24;
            TopLabel.transform.localScale = Vector3.one;
            TopLabel.alignment = TextAlignmentOptions.Center;
            TopLabel.text = TopPullLabel;
            RectTransform rect = TopLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = new Vector2(0f, -LabelOffset);
            rect.anchoredPosition3D = Vector3.zero;
            topText.SetActive(false);
            GameObject bottomText = new GameObject("BottomLabel");
            bottomText.transform.SetParent(_scroll.viewport.transform);
            BottomLabel = bottomText.AddComponent<TextMeshProUGUI>();
            BottomLabel.font = LabelsFont;
            BottomLabel.fontSize = 24;
            BottomLabel.transform.localScale = Vector3.one;
            BottomLabel.alignment = TextAlignmentOptions.Center;
            BottomLabel.text = BottomPullLabel;
            BottomLabel.transform.position = Vector3.zero;
            rect = BottomLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.offsetMax = new Vector2(0f, LabelOffset);
            rect.offsetMin = Vector2.zero;
            rect.anchoredPosition3D = Vector3.zero;
            bottomText.SetActive(false);
        }

        void CreateLabelsHorizontal()
        {
            GameObject leftText = new GameObject("LeftLabel");
            leftText.transform.SetParent(_scroll.viewport.transform);
            LeftLabel = leftText.AddComponent<TextMeshProUGUI>();
            LeftLabel.font = LabelsFont;
            LeftLabel.fontSize = 24;
            LeftLabel.transform.localScale = Vector3.one;
            LeftLabel.alignment = TextAlignmentOptions.Center;
            LeftLabel.text = LeftPullLabel;
            RectTransform rect = LeftLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = new Vector2(-LabelOffset * 2, 0f);
            rect.anchoredPosition3D = Vector3.zero;
            leftText.SetActive(false);
            GameObject rightText = new GameObject("RightLabel");
            rightText.transform.SetParent(_scroll.viewport.transform);
            RightLabel = rightText.AddComponent<TextMeshProUGUI>();
            RightLabel.font = LabelsFont;
            RightLabel.fontSize = 24;
            RightLabel.transform.localScale = Vector3.one;
            RightLabel.alignment = TextAlignmentOptions.Center;
            RightLabel.text = RightPullLabel;
            RightLabel.transform.position = Vector3.zero;
            rect = RightLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = Vector3.one;
            rect.offsetMax = new Vector2(LabelOffset * 2, 0f);
            rect.offsetMin = Vector2.zero;
            rect.anchoredPosition3D = Vector3.zero;
            rightText.SetActive(false);
        }
    }
}