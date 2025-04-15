using System;
using System.Collections;
using BaseClass;
using Battle;
using DataClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using War;

namespace Solo
{
    public class UISolo : MonoBehaviour
    {
        [SerializeField] private RawImage head0;
        [SerializeField] private RawImage head1;
        [SerializeField] private TextMeshProUGUI name0;
        [SerializeField] private TextMeshProUGUI name1;
        [SerializeField] private Text hp0;
        [SerializeField] private Text hp1;
        [SerializeField] private Slider hpSlider0;
        [SerializeField] private Slider hpSlider1;
        [SerializeField] private TextMeshProUGUI weapon0;
        [SerializeField] private TextMeshProUGUI armor0;
        [SerializeField] private TextMeshProUGUI weapon1;
        [SerializeField] private TextMeshProUGUI armor1;
        
        [SerializeField] private Animator hmAnimator;
        [SerializeField] private Animator aiAnimator;
        [SerializeField] private GameObject buttonPanel;
        [SerializeField] private GameObject soloInfoPanel;
        [SerializeField] private TextMeshProUGUI soloInfo;
        [SerializeField] private Button pinButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button whackButton;
        [SerializeField] private Button persuadeButton;
        [SerializeField] private Button retreatButton;
        [SerializeField] private Button surrenderButton;
        [SerializeField] private UITips uiTips;

        private SoloState _soloState
        {
            get => BattleManager.Instance.soloState;
            set => BattleManager.Instance.soloState = value;
        }

        private General _hmGeneral{get => BattleManager.Instance.hmGeneral;set => BattleManager.Instance.hmGeneral = value;}
        private General _aiGeneral{get => BattleManager.Instance.aiGeneral;set => BattleManager.Instance.aiGeneral = value;}
        private SoloManager _soloManager;// 单挑方法
        
        // 单挑开始设定
        void Start()
        {
            pinButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Pin, true));
            });
            attackButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Attack, true));
            });
            whackButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Whack, true));
            });
            persuadeButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Persuade, true));
            });
            retreatButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Retreat, true));
            });
            surrenderButton.onClick.AddListener(delegate
            {
                StartCoroutine(AnimationPlay(SoloAction.Surrender, true));
            });
            Time.timeScale = 1f;
            Debug.Log($"{Time.timeScale}");
            DataManagement.Instance.LoadAndInitializeData();
            _hmGeneral = GeneralListCache.GetGeneral(61);
            _aiGeneral = GeneralListCache.GetGeneral(4);
            _soloManager = new SoloManager(_hmGeneral, _aiGeneral);
            _soloManager.CheckWeaponDrop();
            ShowSoloPanel();
            _soloState = SoloState.AITurn;
            StartSoloTurn();
        }

        
        void ShowSoloPanel()
        {
            head0.texture = Resources.Load<Texture2D>($"HeadImage/{_hmGeneral.generalId}");
            head1.texture = Resources.Load<Texture2D>($"HeadImage/{_aiGeneral.generalId}");
            name0.text = _hmGeneral.generalName;
            name1.text = _aiGeneral.generalName;
            hp0.text = _hmGeneral.curPhysical.ToString();
            hp1.text = _aiGeneral.curPhysical.ToString();
            hpSlider0.value = (float)(_hmGeneral.curPhysical / 100f);
            hpSlider1.value = (float)(_aiGeneral.curPhysical / 100f);
            weapon0.text = WeaponListCache.GetWeapon(_hmGeneral.weapon).weaponName;
            weapon1.text = WeaponListCache.GetWeapon(_aiGeneral.weapon).weaponName;
            armor0.text = WeaponListCache.GetWeapon(_hmGeneral.armor).weaponName;
            armor1.text = WeaponListCache.GetWeapon(_aiGeneral.armor).weaponName;
        }

        void ShowSoloInfo(string info)
        {
            buttonPanel.SetActive(false);
            soloInfoPanel.SetActive(true);
            soloInfo.text = info;
        }
        
        // 单挑动画控制流程
        IEnumerator AnimationPlay(SoloAction soloAction, bool isPlayer)
        {
            Animator animator = isPlayer ? hmAnimator : aiAnimator;
            Animator enemyAni = isPlayer ? aiAnimator : hmAnimator;
            General atkGeneral = isPlayer ? _hmGeneral : _aiGeneral;
            General defGeneral = isPlayer ? _aiGeneral : _hmGeneral;
            string info = String.Empty;
            short atk = atkGeneral.GetAttackPower();
            short def = defGeneral.GetDefendPower();
            switch (soloAction)
            {
                case SoloAction.Pin:
                    info = isPlayer? TextLibrary.SoloInfo[0] : TextLibrary.SoloInfo[7];
                    ShowSoloInfo(info);
                    animator.SetTrigger("Attack");
                    yield return new WaitForSeconds(2f);
                    IsDead(_soloManager.SinglePin(atkGeneral,atk, def), !isPlayer);
                    yield return new WaitForSeconds(1f);
                    break;
                case SoloAction.Attack:
                    info = isPlayer? TextLibrary.SoloInfo[0] : TextLibrary.SoloInfo[7];
                    ShowSoloInfo(info);
                    animator.SetTrigger("Attack");
                    yield return new WaitForSeconds(2f);
                    IsDead(_soloManager.SingleAtk(atkGeneral, defGeneral, atk, def), !isPlayer);
                    yield return new WaitForSeconds(1f);
                    break;
                case SoloAction.Whack:
                    info = isPlayer? TextLibrary.SoloInfo[0] : TextLibrary.SoloInfo[7];
                    ShowSoloInfo(info);
                    animator.SetTrigger("Whack");
                    yield return new WaitForSeconds(2f);
                    short hurt = _soloManager.SingleAllAtk(atkGeneral, defGeneral, atk, def);
                    if (hurt > 0)// 全力一击成功
                    {
                        IsDead(hurt, !isPlayer);
                        yield return new WaitForSeconds(1f);
                    }
                    else// 全力一击失败损血
                    {
                        hurt = Math.Abs(hurt);
                        if (!IsDead(hurt, isPlayer))
                        {
                            animator.SetTrigger("Back");
                        }
                        yield return new WaitForSeconds(1f);
                    }
                    break;
                case SoloAction.Persuade:
                    if (isPlayer)// 玩家劝降
                    {
                        if (defGeneral.IsKing())// 君主不可投降
                        {
                            info = TextLibrary.SoloInfo[14];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                            info = TextLibrary.SoloInfo[16];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                            break;
                        }
                        if (_soloManager.Persuade(atkGeneral, defGeneral))// 玩家劝降成功
                        {
                            info = TextLibrary.SoloInfo[14];
                            ShowSoloInfo(info);
                            animator.SetTrigger("Accept");
                            enemyAni.SetBool("Surrender", true);
                            yield return new WaitForSeconds(1f);
                            info = TextLibrary.SoloInfo[12];
                            ShowSoloInfo(info);
                            _soloState = SoloState.AISurrender;
                            yield return new WaitForSeconds(1f);
                            yield return EndSoloTurn();
                            yield break;
                        }
                        else
                        {
                            info = TextLibrary.SoloInfo[14];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                            info = TextLibrary.SoloInfo[15];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                        }
                    }
                    else
                    {
                        if (_soloManager.AIPersuade(atkGeneral, defGeneral))
                        {
                            info = TextLibrary.SoloInfo[14];
                            ShowSoloInfo(info);
                            animator.SetTrigger("Accept");
                            enemyAni.SetBool("Surrender", true);
                            yield return new WaitForSeconds(1f);
                            info = TextLibrary.SoloInfo[8];
                            ShowSoloInfo(info);
                            _soloState = SoloState.PlayerSurrender;
                            yield return new WaitForSeconds(1f);
                            yield return EndSoloTurn();
                            yield break;
                        }
                        else
                        {
                            info = TextLibrary.SoloInfo[14];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                            info = TextLibrary.SoloInfo[15];
                            ShowSoloInfo(info);
                            yield return new WaitForSeconds(1f);
                        }
                    }
                    break;
                case SoloAction.Retreat:
                    if (isPlayer)
                    {
                        if (_soloManager.Escape(atkGeneral, defGeneral))
                        {
                            info = TextLibrary.SoloInfo[3];
                            ShowSoloInfo(info);
                            animator.SetBool("Retreat", true);
                            _soloState = SoloState.PlayerRetreat;
                            yield return new WaitForSeconds(1f);
                            yield return EndSoloTurn();
                            yield break;
                        }
                        else
                        {
                            info = TextLibrary.SoloInfo[4];
                            ShowSoloInfo(info);
                        }
                    }
                    else
                    {
                        if (_soloManager.AIEscape(atkGeneral, defGeneral))
                        {
                            info = TextLibrary.SoloInfo[10];
                            ShowSoloInfo(info);
                            animator.SetBool("Retreat", true);
                            _soloState = SoloState.AIRetreat;
                            yield return new WaitForSeconds(1f);
                            yield return EndSoloTurn();
                            yield break;
                        }
                        else
                        {
                            info = TextLibrary.SoloInfo[11];
                            ShowSoloInfo(info);
                        }
                    }
                    yield return new WaitForSeconds(1f);
                    break;
                case SoloAction.Surrender://只有玩家可以投降
                    info = TextLibrary.SoloInfo[5];
                    ShowSoloInfo(info);
                    animator.SetBool("Surrender", true);
                    enemyAni.SetTrigger("Accept");
                    _soloState = SoloState.PlayerSurrender;
                    yield return new WaitForSeconds(1f);
                    yield return EndSoloTurn();
                    yield break;
                    
            }

            if (_soloManager.ComboTrigger(atkGeneral) && soloAction != SoloAction.Persuade && soloAction != SoloAction.Retreat)// 连击触发
            {
                StopCoroutine(ExecuteSoloTurn());
                StartSoloTurn();
            }
            else
            {
                _soloState = isPlayer ? SoloState.AITurn : SoloState.PlayerTurn;
            }
        }

        public void OnWhackAnimationEnd()
        {
            Debug.Log("Whack 动画播放完毕，开始判定命中与伤害");
        }
        
        

        // 单挑判定是否死亡
        bool IsDead(short hurt, bool isPlayer)
        {
            Animator animator = isPlayer ? hmAnimator : aiAnimator;
            Slider hpSlider = isPlayer ? hpSlider0 : hpSlider1;
            Text hpText = isPlayer ? hp0 : hp1;
            General hurtGeneral = isPlayer ? _hmGeneral : _aiGeneral;
            byte health = hurtGeneral.GetCurPhysical();
            if (health - hurt <= 0)
            {
                hpSlider.value = 0;
                hurtGeneral.SubHp(hurtGeneral.GetCurPhysical());
                hpText.text = "0";
                _soloState = isPlayer? SoloState.PlayerDie : SoloState.AIDie;
                animator.SetBool("Dead", true);
                StopAllCoroutines();
                StartCoroutine(EndSoloTurn());
                return true;
            }
            else
            {
                hpSlider.value -= hurt/100f;
                hurtGeneral.SubHp((byte)hurt);
                hpText.text = hurtGeneral.GetCurPhysical().ToString();
                return false;
            }
        }
        
        
        
        private void StartSoloTurn()
        {
            if (_soloState == SoloState.PlayerTurn)
            {
                Debug.Log("玩家单挑回合开始");
                soloInfoPanel.SetActive(false);
                buttonPanel.SetActive(true);
                StartCoroutine(ExecuteSoloTurn());
            }
            else if (_soloState == SoloState.AITurn)
            {
                Debug.Log("敌方单挑回合开始");
                ShowSoloInfo($"{TextLibrary.SoloInfo[7]}");
                StartCoroutine(ExecuteSoloTurn());
            }
        }

        private IEnumerator ExecuteSoloTurn()
        {
            if (_soloState == SoloState.PlayerTurn)
            {
                Debug.Log("玩家执行单挑操作");
                yield return new WaitUntil(() => _soloState == SoloState.AITurn);
                yield return null;
            }
            else if (_soloState == SoloState.AITurn)
            {
                Debug.Log("敌方执行单挑操作");
                SoloAction soloAction = _soloManager.GetAISoloAction();
                yield return AnimationPlay(soloAction,false);
                yield return null;
            }
            yield return EndSoloTurn();
        }
        
        /// <summary>
        /// 处理单挑时武将撤退、投降、死亡情况
        /// </summary>
        /// <returns>单挑时武将撤退、投降、死亡</returns>
        private IEnumerator EndSoloTurn()
        {
            switch (_soloState)
            {
                // 检查战斗是否结束
                case SoloState.PlayerRetreat:
                    _soloManager.HandleSoloExp();
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[3]}");
                    SceneManager.LoadSceneAsync("BattleScene");
                    yield break;
                case SoloState.PlayerSurrender:
                    _soloManager.HandleSoloExp();
                    BattleManager.Instance.battleState = BattleState.HMCaptured;
                    BattleManager.Instance.AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(_hmGeneral.generalId);
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[5]}");
                    BattleManager.Instance.BattleOver();
                    yield break;
                case SoloState.PlayerDie:
                    _soloManager.HandleSoloExp();
                    BattleManager.Instance.battleState = BattleState.HMDie;
                    BattleManager.Instance.AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(_hmGeneral.generalId);
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[6]}");
                    BattleManager.Instance.BattleOver();
                    yield break;
                case SoloState.AIRetreat:
                    _soloManager.HandleSoloExp();
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[10]}");
                    SceneManager.LoadSceneAsync("BattleScene");
                    yield break;
                case SoloState.AISurrender:
                    _soloManager.HandleSoloExp();
                    BattleManager.Instance.battleState = BattleState.AICaptured;
                    BattleManager.Instance.AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(_aiGeneral.generalId);
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[12]}");
                    BattleManager.Instance.BattleOver();
                    yield break;
                case SoloState.AIDie:
                    _soloManager.HandleSoloExp();
                    BattleManager.Instance.battleState = BattleState.AIDie;
                    BattleManager.Instance.AfterBattleSettlement();
                    if (_soloManager.seizedWeapon)
                    {
                        string weaponName = _soloManager.ObtainWeaponOrArmor();
                        yield return  uiTips.ShowNoticeTips($"恭喜获得‘{weaponName}’");
                    }
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(_aiGeneral.generalId);
                    yield return  uiTips.ShowNoticeTips($"{TextLibrary.SoloInfo[13]}");
                    BattleManager.Instance.BattleOver();
                    yield break;
                default://正常继续单挑回合切换
                    Debug.Log("回合结束");
                    StartSoloTurn();
                    break;
            }
        }
        
        
    }
}


