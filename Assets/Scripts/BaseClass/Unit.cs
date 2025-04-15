using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using War;

namespace BaseClass
{
    public enum UnitState
    {
        None,
        Retreat,
        Idle,
        Retire,
        Captive,
        Dead,
        Trapped
    }
    
    [System.Serializable]
    public class UnitData
    {
        public short genID;         // 武将ID
        public bool isPlayer;       // 是否属于玩家
        public bool isCommander;    // 是否为主将
        public byte moveBonus;      // 机动力
        public byte trappedDay;     // 被困天数
        public bool isMoved;        // 标记是否已经移动过
        public UnitState unitState; // 单位状态
        public Vector2Int arrayPos; // 单位当前的数组坐标
        public byte Terrain => (byte)(WarManager.Instance.warMap[arrayPos.y, arrayPos.x] & 0xf);

        public UnitData(short genID, bool isPlayer, bool isCommander, byte moveBonus, byte trappedDay, bool isMoved, UnitState unitState,Vector2Int arrayPos)
        {
            this.genID = genID;
            this.isPlayer = isPlayer;
            this.isCommander = isCommander;
            this.moveBonus = moveBonus;
            this.trappedDay = trappedDay;
            this.isMoved = isMoved;
            this.unitState = unitState;
            this.arrayPos = arrayPos;
        }
        
        public string UnitName => GeneralListCache.GetGeneral(genID).generalName;
        public void SetIsPlayer(bool newIsPlayer)
        {
            isPlayer = newIsPlayer;
        }

        /// <summary>
        /// 设置单位的移动力
        /// </summary>
        public void SetMoveBonus(byte newMoveBonus)
        {
            if (newMoveBonus <= 30)
            {
                moveBonus = newMoveBonus;
            }
            else
            {
                Debug.LogWarning("移动力设定不能大于30");
            }
        }

        /// <summary>
        /// 减少单位的移动力
        /// </summary>
        /// <param name="loseMoveBonus"></param>
        /// <returns>返回是否可减少移动力</returns>
        public bool SubMoveBonus(byte loseMoveBonus)
        {
            if (moveBonus >= loseMoveBonus)
            {
                moveBonus -= loseMoveBonus;
                return true;
            }
            else
            {
                Debug.LogWarning($"移动力仅有{moveBonus}点,不足扣减{loseMoveBonus}点");
                return false;
            }
        }

        /// <summary>
        /// 设置单位的被困天数
        /// </summary>
        public void SetTrappedDay(byte newTrappedDay)
        {
            trappedDay = newTrappedDay;
        }

        /// <summary>
        /// 减少单位的被困天数
        /// </summary>
        /// <param name="loseTrappedDay"></param>
        /// <returns>是否可以减少单位的被困天数</returns>
        public bool SubTrappedDay(byte loseTrappedDay)
        {
            if (trappedDay >= loseTrappedDay)
            {
                trappedDay -= loseTrappedDay;
                return true;
            }
            else
            {
                Debug.LogWarning("被困天数不足");
                return false;
            }
        }
        
        /// <summary>
        /// 设置单位是否已经移动过
        /// </summary>
        public void SetIsMoved(bool newIsMoved)
        {
            isMoved = newIsMoved;
        }

        /// <summary>
        /// 设置单位状态
        /// </summary>
        public void SetUnitState(UnitState state)
        {
            unitState = state;
            Debug.Log($"单位{GeneralListCache.GetGeneral(genID).generalName}状态改变为{state}");
        }
    }
    
    public class UnitObj : MonoBehaviour
    {
        public UnitData data;
        public short genID => data.genID;
        public bool isPlayer => data.isPlayer;
        public bool IsCommander => data.isCommander;
        public byte moveBonus => data.moveBonus;
        public byte trappedDay => data.trappedDay;
        public bool isMoved => data.isMoved;
        public UnitState unitState => data.unitState;
        public Vector2Int arrayPos => data.arrayPos;
        public byte Terrain => (byte)(WarManager.Instance.warMap[arrayPos.y, arrayPos.x] & 0xf);

        public string UnitName => GeneralListCache.GetGeneral(data.genID).generalName;
        
        public void Init(UnitData unitData)
        {
            data = unitData;
        }

        public void SetIsPlayer(bool newIsPlayer)
        {
            data.isPlayer = newIsPlayer;
        }

        /// <summary>
        /// 设置单位的移动力
        /// </summary>
        public void SetMoveBonus(byte newMoveBonus)
        {
            if (newMoveBonus <= 30)
            {
                data.moveBonus = newMoveBonus;
            }
            else
            {
                Debug.LogWarning("移动力设定不能大于30");
            }
        }

        /// <summary>
        /// 减少单位的移动力
        /// </summary>
        /// <param name="loseMoveBonus"></param>
        /// <returns>返回是否可减少移动力</returns>
        public bool SubMoveBonus(byte loseMoveBonus)
        {
            if (moveBonus >= loseMoveBonus)
            {
                data.moveBonus -= loseMoveBonus;
                return true;
            }
            else
            {
                Debug.LogWarning($"移动力仅有{moveBonus}点,不足扣减{loseMoveBonus}点");
                return false;
            }
        }

        /// <summary>
        /// 设置单位的被困天数
        /// </summary>
        public void SetTrappedDay(byte newTrappedDay)
        {
            data.trappedDay = newTrappedDay;
        }

        /// <summary>
        /// 减少单位的被困天数
        /// </summary>
        /// <param name="loseTrappedDay"></param>
        /// <returns>是否可以减少单位的被困天数</returns>
        public bool SubTrappedDay(byte loseTrappedDay)
        {
            if (trappedDay >= loseTrappedDay)
            {
                data.trappedDay -= loseTrappedDay;
                return true;
            }
            else
            {
                Debug.LogWarning("被困天数不足");
                return false;
            }
        }
        
        /// <summary>
        /// 设置单位是否已经移动过
        /// </summary>
        public void SetIsMoved(bool newIsMoved)
        {
            data.isMoved = newIsMoved;
        }

        /// <summary>
        /// 设置单位状态
        /// </summary>
        public void SetUnitState(UnitState state)
        {
            data.unitState = state;
            Debug.Log($"单位{UnitName}状态改变为{state}");
        }
        
        // 开始时添加Toggle组件监听和加入ToggleGroup
        private void Start()
        {
            Toggle toggle = gameObject.GetComponent<Toggle>();
            // 在场景中查找ToggleGroup
            ToggleGroup toggleGroup = GameObject.Find("UnitCanvas").GetComponent<ToggleGroup>();
            toggle.group = toggleGroup;
            /*if (!isPlayer)
            {
                toggle.interactable = false;
            }*/
            toggle.onValueChanged.AddListener(delegate { OnUnitToggleChanged(toggle.isOn); });
        }



        public void OnUnitToggleChanged(bool isOn)
        {
            MapManager.Instance.ClearAllMarkers();
            // AI的回合禁用
            if (!isPlayer || WarManager.Instance.warState == WarState.AITurn) return;
            // 如果不是初次选择一个单位
            if (WarManager.Instance.hmUnitObj != null)
            {
                switch (WarManager.Instance.hmUnitObj.unitState)
                {
                    case UnitState.Retreat:
                        WarManager.Instance.hmUnitObj = this;
                        break;
                    case UnitState.Idle:
                    case UnitState.None:// 如果不是初次选择一个单位
                        if (isPlayer && WarManager.Instance.hmUnitObj != this)
                        {   //切换到该玩家单位
                            WarManager.Instance.hmUnitObj = this;
                            //ChangeUnitColor(Color.green);
                            Debug.Log($"选择单位{genID}");
                        }
                        else
                        {
                            //WarInfo.Instance.selectedUnit.ChangeUnitColor(Color.white);
                            WarManager.Instance.hmUnitObj = null;
                            Debug.Log("未选择单位");
                        }
                        UIWar.Instance.DisplayWarMenu();
                        break;
                }
            }
            else
            {   //如果是初次选择第一个单位
                WarManager.Instance.hmUnitObj = this;
                //ChangeUnitColor(Color.green);
                Debug.Log($"选择单位{genID}");
                UIWar.Instance.DisplayWarMenu();
            }
        }

       
        
        public void GetArmySprite()
        {
            Image image = gameObject.GetComponent<Image>();
            General general = GeneralListCache.GetGeneral(genID);
            int soldier = general.generalSoldier / 300;
            SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("War/Unit/UnitAtlas"); // 部队的精灵
            switch (Terrain)
            {
                case 1: //平原
                case 2: //平原
                case 3: //平原
                case 4: //草原
                case 5:
                case 6:
                case 7:
                case 19:
                case 22:
                    if (!isPlayer)
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{33 + soldier}");
                    }
                    else
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{soldier}");
                    }

                    break;
                case 10: //树林
                case 11: //森林
                case 12: //山地
                    if (!isPlayer)
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{55 + soldier}");
                    }
                    else
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{22 + soldier}");
                    }

                    break;
                case 9: //水域
                case 15: //雪域
                    if (!isPlayer)
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{44 + soldier}");
                    }
                    else
                    {
                        image.sprite = spriteAtlas.GetSprite($"Army_{11 + soldier}");
                    }

                    break;

                case 8: //城楼
                    image.sprite = spriteAtlas.GetSprite($"Terrain_7");
                    break;
            }
        }

        // 移动将军到目标位置
        public void MoveUnitToCell((Vector2Int, byte) targetCell)
        {
            // 获取目标瓦片的中心世界坐标
            Vector3 worldPosition = MapManager.TurnToWorldPos(targetCell.Item1);
            Vector2Int newArrayPos = targetCell.Item1;

            // 清除将军之前位置的占用状态
            WarManager.Instance.warMap[arrayPos.y, arrayPos.x] &= 0x3f; // 移除之前位置的占用标记
            
            MapManager.ChangeXYInDictionary(newArrayPos, arrayPos);
            // 更新 warMap 数组，标记新位置的占用状态
            WarManager.Instance.warMap[newArrayPos.y, newArrayPos.x] |= 0x40; // 新位置标记为已占用
            

            // 更新将军的数组坐标
            data.arrayPos = newArrayPos;

            // 将将军移动到目标位置
            transform.position = worldPosition;

            // 清除所有可移动单元格的标记
            MapManager.Instance.ClearAllMarkers(); // 清除所有标记

            // 标记为已经移动
            //SetIsMoved(true);

            // 更新将军贴图
            GetArmySprite();
            Debug.Log("将军已移动，清除了所有可移动区域的标记。");

            if (isPlayer && InVillage())
            {
                UIWar.Instance.WhetherGoVillage(Terrain);
            }
        }

        public bool InVillage() //判断是否是村庄
        {
            return Terrain is 5 or 6 or 7;
        }
        
        public void Remove(UnitState state)
        {
            SetUnitState(state);
            if (isPlayer)
            {
                switch (state)
                {
                    case UnitState.Dead:
                    case UnitState.Retire:
                    case UnitState.Retreat:
                        WarManager.Instance.hmUnits.Remove(this);
                        Destroy(gameObject);
                        break;
                    case UnitState.Captive:
                        SetIsPlayer(false);
                        WarManager.Instance.aiUnits.Add(this);
                        WarManager.Instance.hmUnits.Remove(this);
                        gameObject.SetActive(false);
                        break;
                }
                WarManager.Instance.hmUnitObj = null;
            }
            else
            {
                switch (state)
                {
                    case UnitState.Dead:
                    case UnitState.Retire:
                    case UnitState.Retreat:
                        WarManager.Instance.aiUnits.Remove(this);
                        Destroy(gameObject);
                        break;
                    case UnitState.Captive:
                        SetIsPlayer(true);
                        WarManager.Instance.hmUnits.Add(this);
                        WarManager.Instance.aiUnits.Remove(this);
                        gameObject.SetActive(false);
                        break;
                }
                WarManager.Instance.aiUnitObj = null;
            }
            
            WarManager.Instance.warMap[arrayPos.y, arrayPos.x] &= 0x3f; // 移除之前位置的占用标记
            MapManager.RemoveUnit(arrayPos);
            Debug.Log($"移除单位:{genID},坐标:{arrayPos.x},{arrayPos.y},新地图值:{WarManager.Instance.warMap[arrayPos.y, arrayPos.x]}");
        }
        
        
        // 战斗后判断将军是否死亡，并进行相应处理
        public bool HandleGeneralDie()
        {
            if (isPlayer)  // 如果是玩家的将军死亡
            {
                if (IsCommander)  // 判断玩家的主将是否死亡
                {  
                    // 如果死亡的是玩家主将
                    GeneralListCache.GeneralDie(genID);  // 处理将军死亡逻辑
                    Remove(UnitState.Dead);// 先将玩家主将移除
                    if (WarManager.Instance.hmUnits.Count <= 0)  // 如果还有其他的将军则先执行强制撤退
                    {
                        WarManager.Instance.AfterWarSettlement(false, true);  // 调用战斗结束逻辑
                    }
                    return false;
                }

                // 如果阵亡的是玩家非主将将军
                GeneralListCache.GeneralDie(genID);  // 处理将军死亡逻辑
                Remove(UnitState.Dead);
            }
            else  // 如果是AI将军死亡
            {
                if (IsCommander)  // 判断AI主将是否死亡
                {
                    GeneralListCache.GeneralDie(genID);  // 处理将军死亡逻辑
                    Remove(UnitState.Dead);// AI主将先移除
                    AIWar.AIFollowRetreat();  //AI全军撤退方法
                    WarManager.Instance.AfterWarSettlement(true, true);  // 调用战斗结束逻辑
                    return false;
                }
                
                // 检查AI非主将将军是否阵亡
                GeneralListCache.GeneralDie(genID);  // 处理将军死亡逻辑
                Remove(UnitState.Dead);  //AI将军阵亡移除
            }
            return true;  // 默认返回true表示处理结束继续战争
        }


        // 战斗后武将被擒时做以下判断
        public bool HandleGeneralCaptured()
        {
            if (isPlayer)  // 如果是玩家
            {
                if (IsCommander)  // 如果是玩家的主要将军
                {
                    Remove(UnitState.Captive);
                    if (WarManager.Instance.hmUnits.Count <= 0)  // 如果还有其他的将军则先执行强制撤退
                    {
                        WarManager.Instance.AfterWarSettlement(false, true);  // 调用战斗结束逻辑
                    }
                    return false;
                }

                // 如果被俘的是玩家非主将军
                Remove(UnitState.Captive);
            }
            else  // 如果是AI将军
            {
                if (IsCommander)  // 检查AI主要将军是否被俘
                {
                    Remove(UnitState.Captive);// AI主将先移除
                    AIWar.AIFollowRetreat();  //AI全军撤退方法
                    WarManager.Instance.AfterWarSettlement(true, true);  // 调用战斗结束逻辑
                    return false;
                }

                Remove(UnitState.Captive);
                
            }
            return true;
        }
        
        //撤退按钮后处理武将撤退
        public bool HandleGeneralRetreat(byte cityId)
        {
            if (isPlayer)
            {
                if (IsCommander)
                {
                    WarManager.RetreatGeneralToCity(this, cityId, WarManager.Instance.hmKingId);
                    Remove(UnitState.Retreat);
                    if (WarManager.Instance.hmUnits.Count <= 0)  // 如果还有其他的将军则先执行强制撤退
                    {
                        WarManager.Instance.AfterWarSettlement(false, false);  // 调用战斗结束逻辑
                    }
                    return false;
                }
                // 如果撤退的是玩家非主将
                WarManager.RetreatGeneralToCity(this, cityId, WarManager.Instance.hmKingId);
                Remove(UnitState.Retreat);
            }
            else
            {
                /*if (IsCommander)  // 检查AI主要将军是否撤退
                {
                    //AI全军撤退方法
                    AIWar.SingletonRetreat(data);
                    AIWar.AIFollowRetreat();
                    WarManager.Instance.AfterWarSettlement(true, true);  // 调用战斗结束逻辑
                    return false;
                }

                AIWar.SingletonRetreat(data);*/
            }
            return true;
        }
        
        //处理武将下野
        public bool HandleGeneralRetire()
        {
            City city = CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId);
            if (isPlayer)
            {
                if (IsCommander)
                {
                    // 如果下野的是玩家主将将军
                    city.AddNotFoundGeneralId(genID);
                    Remove(UnitState.Retire);
                    return false;  // 返回false表示战斗结束撤退其他武将
                }
                // 如果下野的是玩家非主将将军
                city.AddNotFoundGeneralId(genID);
                Remove(UnitState.Retire);
            }
            else
            {
                if (IsCommander)
                {
                    AIWar.AIFollowRetreat();
                    WarManager.Instance.AfterWarSettlement(true, true);  // 调用战斗结束逻辑
                    return false;
                }
                city.AddNotFoundGeneralId(genID);
                Remove(UnitState.Retire);
            }

            return true;
        }


        private void OnEnable()
        {
            UIWar.OnInfoPanelShow += OnInfoPanelShow;
        }
        
        private void OnDisable()
        {
            UIWar.OnInfoPanelShow -= OnInfoPanelShow;
        }

        private void OnInfoPanelShow(bool isShow)
        {
            Toggle toggle = gameObject.GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            if (isShow)
            {
                // if (!isPlayer)
                // {
                //     toggle.interactable = true;
                // }
                toggle.onValueChanged.AddListener(delegate
                {
                    UIWar.Instance.uiIntelligencePanel.ShowIntelligencePanel(genID);
                });
            }
            else
            {
                /*if (!isPlayer)
                {
                    toggle.interactable = false;
                }*/
                toggle.onValueChanged.AddListener(delegate
                {
                    OnUnitToggleChanged(toggle.isOn);
                });
            }
        }
    }
}