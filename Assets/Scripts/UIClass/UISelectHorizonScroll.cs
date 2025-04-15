using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIClass
{
    public class SelectHorizontalScroll : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable] private struct ItemInfo
        {
            public string name;
            public Sprite sprite;
            public string description;
            public ItemInfo(string name, Sprite sprite, string description)
            {
                this.name = name;
                this.sprite = sprite;
                this.description = description;
            }
        }

        [Tooltip("选项预制体")][SerializeField] private GameObject itemPrefab;
        [Tooltip("选项父物体")][SerializeField] private RectTransform itemParent;
        [Tooltip("描述文字")][SerializeField] private TextMeshProUGUI descriptionText;
        [Tooltip("选项信息")][SerializeField] private ItemInfo[] itemInfos;
        [Tooltip("显示数量(尽量填奇数) ")][SerializeField] private int displayNumber;
        [Tooltip("选项间隔")][SerializeField] private float itemSpace;
        [Tooltip("移动插值")][SerializeField] private float moveSmooth;
        [Tooltip("拖动速度")][SerializeField] private float dragSpeed;
        [Tooltip("缩放倍率")][SerializeField] private float scaleMultiplying;
        [Tooltip("透明度倍率")][SerializeField] private float alphaMultiplying;
        public event Action<int> SelectAction;
        private SelectHorizontalScrollItem[] items;
        private float displayWidth;
        private int offsetTimes;
        private bool isDrag;
        private int currentItemIndex;
        private float[] distances;
        private float selectItemX;
        private bool isSelectMove;
        public bool isSelected;


        private void Start()
        {
            Init(); // 初始化选项列表
            MoveItems(0); // 移动列表到初始位置
        }

        ///<summary>初始化
        private void Init()
        {
            displayWidth = (displayNumber - 1) * itemSpace;
            items = new SelectHorizontalScrollItem[displayNumber];
            for (int i = 0; i < displayNumber; i++)
            {
                SelectHorizontalScrollItem item = Instantiate(itemPrefab, itemParent).GetComponent<SelectHorizontalScrollItem>();
                item.itemIndex = i;
                items[i] = item;
            }
        }

        ///<summary>
        ///设置选项信息
        ///</summary>
        ///<param name="names"> 选项名称</param >
        ///<param name="sprites">选项图片</param>
        ///<param name="descriptions"> 选项描述</param >
        public void SetItemsInfo(string[] names, Sprite[] sprites, string[] descriptions)
        {
            if (names.Length != sprites.Length || sprites.Length != descriptions.Length || names.Length != descriptions.Length)
            {
                Debug.Log("选择数据不完整，确保名称、图片和描述数组长度相同！");
                return;
            }
            Debug.Log("初始化计策面板"+ name.Length);
            itemInfos = new ItemInfo[names.Length];
            for (int i = 0; i < itemInfos.Length; i++)
            {
                itemInfos[i] = new ItemInfo(names[i], sprites[i], descriptions[i]);
            }
            SelectAction = null;
            isSelected = false;
        }

        ///<summary>
        ///点击选择
        ///</summary>
        ///<param name="itemIndex">点击的选项索引</param>
        ///<param name="infoIndex">点击的选择信息索引</param> 
        public void Select(int itemIndex, int infoIndex, RectTransform itemRectTransform)
        {   //根据选择的选项来判断是否为居中的选项
            if (!isSelected && itemIndex == currentItemIndex)
            {
                SelectAction?.Invoke(infoIndex);
                isSelected = true;
                Debug.Log("选项选择序号:" + infoIndex);
            }
            else//不是则移动选项
            {
                isSelectMove = true;
                selectItemX = itemRectTransform.localPosition.x;
            }
        }


        ///<summary>
        ///移动列表
        ///<summary>
        ///<param name ="offsetTimes"> 偏移次数</param>
        private void MoveItems(int offsetTimes)
        {
            for (int i = 0; i < displayNumber; i++)//这个方法先把所有选项移动到正确的位置
            {
                float x = itemSpace * (i - offsetTimes) - displayWidth / 2;
                items[i].rectTransform.localPosition = new Vector2(x, items[i].rectTransform.localPosition.y);
            }
            //要循环显示选项可以先找到中间的选项所对应的选项信息
            int middle;

            if (offsetTimes > 0)
            {
                middle = itemInfos.Length - offsetTimes % itemInfos.Length;
            }
            else
            {
                middle = -offsetTimes % itemInfos.Length;
            }
            //从中间正向循环赋值
            int infoIndex = middle;
            for (int i = Mathf.FloorToInt(displayNumber / 2f); i < displayNumber; i++)
            {
                if (infoIndex >= itemInfos.Length)
                {
                    infoIndex = 0;
                }
                items[i].SetInfo(itemInfos[infoIndex].sprite, itemInfos[infoIndex].name, itemInfos[infoIndex].description, infoIndex, this);
                infoIndex++;
            }
            //从中间的上一个反向循环赋值
            infoIndex = middle - 1;
            for (int i = Mathf.FloorToInt(displayNumber / 2f) - 1; i >= 0; i--)
            {
                if (infoIndex <= -1)
                {
                    infoIndex = itemInfos.Length - 1;
                }
                items[i].SetInfo(itemInfos[infoIndex].sprite, itemInfos[infoIndex].name, itemInfos[infoIndex].description, infoIndex, this);
                infoIndex--;
            }
        }

        private void Update()
        {
            //根据是否拖拽进行自动吸附
            if (!isDrag)
            {
                Adsorption();
            }
            int currentOffsetTimes = Mathf.FloorToInt(itemParent.localPosition.x / itemSpace);
            if (currentOffsetTimes != offsetTimes)//并根据拖拽距离判断是否需要移动列表
            {
                offsetTimes = currentOffsetTimes;
                MoveItems(offsetTimes);
            }
            ItemsControl();
        }

        ///<summary>控制选项的透明度缩放获取中间的选项
        private void ItemsControl()
        {
            distances = new float[displayNumber];//最后根据每个选项到中心的距离控制它的缩放和透明度
            for (int i = 0; i < displayNumber; i++)
            {
                float distance = Mathf.Abs(items[i].rectTransform.position.x - transform.position.x);
                distances[i] = distance;
                float scale = 1 - distance * scaleMultiplying;
                items[i].rectTransform.localScale = new Vector3(scale, scale, 1);
                items[i].SetAlpha(1 - distance * alphaMultiplying);
            }
            //比较距离并得出居中的选项
            float minDistance = itemSpace * displayNumber; 
            int minIndex = 0;
            for (int i = 0; i < displayNumber; i++)
            {
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    minIndex = i;
                }
            }
            descriptionText.text = items[minIndex].description;
            currentItemIndex = items[minIndex].itemIndex;
        }


        ///<summary〉自动吸附
        private void Adsorption()
        {
            float targetX;
            if (!isSelectMove)//先判断有没有选择选项的移动
            {
                float distance = itemParent.localPosition.x % itemSpace;
                int times = Mathf.FloorToInt(itemParent.localPosition.x / itemSpace);
                if (distance > 0)
                {
                    if (distance < itemSpace / 2)
                    {
                        targetX = times * itemSpace;
                    }
                    else
                    {
                        targetX = (times + 1) * itemSpace;
                    }
                }
                else
                {
                    if (distance < -itemSpace / 2)
                    {
                        targetX = times * itemSpace;
                    }
                    else
                    {
                        targetX = (times + 1) * itemSpace;
                    }
                }
            }
            else//有就是选择的选项
            {
                targetX = -selectItemX;
            }
            itemParent.localPosition = new Vector2(Mathf.Lerp(itemParent.localPosition.x, targetX, moveSmooth / 10), itemParent.localPosition.y);
        }

        // 拖拽列表
        public void OnDrag(PointerEventData eventData)
        {
            isSelectMove = false;
            itemParent.localPosition = new Vector2(itemParent.localPosition.x + eventData.delta.x * dragSpeed, itemParent.localPosition.y);
        }
        public void OnPointerDown(PointerEventData eventData)
        { 
            isDrag = true;
        } 
        public void OnPointerUp(PointerEventData eventData)
        { 
            isDrag = false; 
        }
    }
}

