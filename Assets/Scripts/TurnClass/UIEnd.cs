using System;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEnd : MonoBehaviour
{
    [SerializeField] Image eventImage;
    [SerializeField] Image resultImage;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Button confirmButton;
    
    private float _time = 0;
    private int _index = 0;
    private string _resultText;

    private int _score = 0;
    private int _unify = 0;
    private byte[] _interior = new byte[7];
    private Dictionary<byte, City> _cities = new Dictionary<byte, City>(); // 存储字典引用，避免重复访问
    private int _cityCount; // 存储计数，避免重复调用
    private List<short> _generals = new List<short>();
    private int _generalCount; // 存储计数，避免重复调用

    private void Start()
    {
        if (GameInfo.PlayingState == GameState.GameOver)
        {
            GameFail();
        }
        else if (GameInfo.PlayingState == GameState.GameSuccess)
        {
            GameWin();
        }
    }

    public void GameFail()
    {
        eventImage.sprite = Resources.Load<Sprite>("Event/GameOver");
        resultImage.sprite = Resources.Load<Sprite>("UI/Fail");
        _resultText = TextLibrary.gameFail;
        confirmButton.gameObject.SetActive(true);
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => { SceneManager.LoadScene("StartScene"); });
    }
    public void GameWin()
    {
        _cities = CityListCache.cityDictionary;
        _cityCount = _cities.Count; // 存储计数，避免重复调用
        foreach (var city in _cities)
        {
            short[] generals = city.Value.GetOfficerIds();
            _generalCount += generals.Length;
            _generals.AddRange(generals);
        }
        CalculateTotalScore(); 
    }

    
    void Update()
    {
        _time += Time.deltaTime;//r随帧增加
        if (_time > 0.1)//如果到了0.1秒就接受字符
        {
            if (_index < _resultText.Length)//前提条件下标不越界
            {
                text.text += _resultText[_index];//+=一直接受并保存
                _index++;//继续读取
            }
            else
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => { SceneManager.LoadScene("StartScene"); });
            }
            _time = 0;//重置计时器
        }
    }

    void CalculateArgoTrade(int i, int j)//计算开垦和商业得分
    {
        if(i >= 700)
        {
            _interior[j] = 6;
            _score += 100;
        } else
        if(i >= 600)
        {
            _interior[j] = 5;
            _score += 80;
        } else
        if(i >= 500)
        {
            _interior[j] = 4;
            _score += 70;
        } else
        if(i >= 400)
        {
            _interior[j] = 3;
            _score += 60;
        } else
        if(i >= 300)
        {
            _interior[j] = 2;
            _score += 40;
        } else
        if(i >= 200)
        {
            _interior[j] = 1;
            _score += 20;
        } else
        {
            _interior[j] = 0;
        }
        _unify = 1 + i/100;
    }

    void AverageAgro()//开垦
    {
        int agro = 0;
        if (_cityCount > 0) // 避免除以零
        {
            foreach (var pair in _cities)
            {
                agro += pair.Value.agro;
            }

            agro /= _cityCount; // 计算平均值
        }
        CalculateArgoTrade(agro, 0);
    }

    void AverageTrade()//商业
    {
        int trade = 0;
        if (_cityCount > 0) // 避免除以零
        {
            foreach (var pair in _cities)
            {
                trade += pair.Value.trade;
            }

            trade /= _cityCount; // 计算平均值
        }
        CalculateArgoTrade(trade, 1);
    }

    void AverageFloodControl()//治水
    {
        int floodControl = 0;
        if (_cityCount > 0) // 避免除以零
        {
            foreach (var pair in _cities)
            {
                floodControl += pair.Value.floodControl;
            }

            floodControl /= _cityCount; // 计算平均值
        }
        if(floodControl >= 99)
        {
            _interior[2] = 4;
            _score += 100;
        } else
        if(floodControl >= 80)
        {
            _interior[2] = 3;
            _score += 80;
        } else
        if(floodControl >= 70)
        {
            _interior[2] = 2;
            _score += 70;
        } else
        if(floodControl >= 50)
        {
            _interior[2] = 1;
            _score += 50;
        } else
        {
            _interior[2] = 0;
        }
        _unify += 1 + floodControl/10;
    }

    void AveragePopulation()//人口
    {
        int population = 0;
        if (_cityCount > 0) // 避免除以零
        {
            foreach (var pair in _cities)
            {
                population += pair.Value.population;
            }

            population /= _cityCount; // 计算平均值
        }
        if(population >= 700000)
        {
            _interior[3] = 5;
            _score += 100;
        } else
        if(population >= 600000)
        {
            _interior[3] = 4;
            _score += 80;
        } else
        if(population >= 400000)
        {
            _interior[3] = 3;
            _score += 70;
        } else
        if(population >= 200000)
        {
            _interior[3] = 2;
            _score += 50;
        } else
        if(population >= 100000)
        {
            _interior[3] = 1;
            _score += 40;
        } else
        {
            _interior[3] = 0;
        }
        _unify += (population/100000)+1;
    }

    void AverageRule()//统治
    {
        int rule = 0;
        if (_cityCount > 0) // 避免除以零
        {
            foreach (var pair in _cities)
            {
                rule += pair.Value.rule;
            }

            rule /= _cityCount; // 计算平均值
        }
        if(rule >= 99)
        {
            _interior[5] = 5;
            _score += 100;
        } else
        if(rule >= 90)
        {
            _interior[5] = 4;
            _score += 90;
        } else
        if(rule >= 80)
        {
            _interior[5] = 3;
            _score += 80;
        } else
        if(rule >= 60)
        {
            _interior[5] = 2;
            _score += 60;
        } else
        if(rule >= 30)
        {
            _interior[5] = 1;
            _score += 20;
        } else
        {
            _interior[5] = 0;
        }
        _unify += rule/10 +1;
    }

    void AverageSoldier()//平均武将兵力
    {
        int soldier = 0;
        if (_generalCount > 0) // 避免除以零
        {
            foreach (var id in _generals)
            {
                General general = GeneralListCache.GetGeneral(id);
                soldier += general.generalSoldier;
            }
            soldier /= _generalCount;
        }
        if(soldier >= 1000)
        {
            _interior[6] = 5;
            _score += 100;
        } else
        if(soldier >= 900)
        {
            _interior[6] = 4;
            _score += 90;
        } else
        if(soldier >= 800)
        {
            _interior[6] = 3;
            _score += 80;
        } else
        if(soldier >= 700)
        {
            _interior[6] = 2;
            _score += 70;
        } else
        if(soldier >= 500)
        {
            _interior[6] = 1;
            _score += 50;
        } else
        {
            _interior[6] = 0;
            _score += 30;
        }
        if (soldier >= 1000)
		{
        	_unify += 10;
		}else {
			_unify += soldier/100;
		}
    }

    void AverageLoyalty()//平均武将忠诚
    {
        int loyalty = 0;
        if (_generalCount > 0) // 避免除以零
        {
            foreach (var id in _generals)
            {
                General general = GeneralListCache.GetGeneral(id);
                loyalty += general.loyalty;
            }
            loyalty /= _generalCount;
        }
        if(loyalty >= 99)
        {
            _interior[4] = 6;
            _score += 100;
        } else
        if(loyalty >= 95)
        {
            _interior[4] = 5;
            _score += 90;
        } else
        if(loyalty >= 90)
        {
            _interior[4] = 4;
            _score += 80;
        } else
        if(loyalty >= 80)
        {
            _interior[4] = 3;
            _score += 70;
        } else
        if(loyalty >= 60)
        {
            _interior[4] = 2;
            _score += 50;
        } else
        if(loyalty >= 36)
        {
            _interior[4] = 1;
            _score += 20;
        } else
        {
            _interior[4] = 0;
            _score += 10;
        }
        _unify += loyalty/10+1;
    }

    void AverageLevel()//每个武将等级
    {
        int level = 0;
        if (_generalCount > 0) // 避免除以零
        {
            foreach (var id in _generals)
            {
                General general = GeneralListCache.GetGeneral(id);
                level += general.level;
            }
            level /= _generalCount;
        }
        if(level >= 8)
            _score += 100;
        else
        if(level >= 7)
            _score += 90;
        else
        if(level >= 6)
            _score += 80;
        else
        if(level >= 5)
            _score += 70;
        else
        if(level >= 4)
            _score += 60;
        else
        if(level >= 3)
            _score += 30;
        if (level == 8)
		{
			_unify += 10;
		}else {
			_unify += level;
		}
    }

    void AverageIq()//每个个武将智力
    {
        int iq = 0;
        if (_generalCount > 0) // 避免除以零
        {
            foreach (var id in _generals)
            {
                General general = GeneralListCache.GetGeneral(id);
                iq += general.IQ;
            }
            iq /= _generalCount;
        }
        if(iq >= 90)
            _score += 100;
        else
        if(iq == 80)
            _score += 80;
        else
        if(iq == 70)
            _score += 60;
        else
        if(iq >= 60)
            _score += 50;
        else
        if(iq >= 40)
            _score += 40;
        
        if (_generalCount >= 150)
		{
			_unify += 10;
		}else {
			_unify += _generalCount/15;
		}
    }

    void AverageWeapon()//每个武将武器
    {
        int weapon = 0;
        if (_generalCount > 0) // 避免除以零
        {
            foreach (var id in _generals)
            {
                General general = GeneralListCache.GetGeneral(id);
                switch(general.weapon)
                {
                    case 15: // '\017'
                    case 23: // '\027'
                        weapon += 2;
                        break;

                    case 22: // '\026'
                        weapon += 1;
                        break;

                    case 5: // '\005'
                    case 6: // '\006'
                    case 7: // '\007'
                        weapon += 3;
                        break;

                    case 14: // '\016'                    
                        weapon += 2;
                        break;
                }
                switch(general.armor)
                {
                    case 30: // '\036'
                        weapon += 2;
                        break;

                    case 31: // '\037'
                        weapon += 4;
                        break;
                }
            }
            weapon /= _generalCount;
        }

        _score += weapon * 50;
        
        if (weapon > 10)
		{
			_unify += 10;
		}else {
			_unify += weapon;
		}
    }

    void CalculateTotalScore()//计算总得分和统一度
    {
        AverageAgro();
        AverageTrade();
        AverageFloodControl();
        AveragePopulation();
        AverageRule();
        AverageSoldier();
        AverageLoyalty();
        AverageLevel();
        AverageIq();
        AverageWeapon();
        
        _resultText = $"君主:{CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).KingName()}\n";
        _resultText += "统一度：" + _unify + "\n";
        _resultText += "总得分：" + _score;
        for (int i = 0; i < 7; i++)
        {
            _resultText += "\n" +TextLibrary.gameScore[i][_interior[i]];
        }
        
    }
}
