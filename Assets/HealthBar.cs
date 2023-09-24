using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public enum BarType {Hp,Stemina,MonsterHp};
    public BarType type;
    public PlayerController player;
    public AIMonsterController monster;
    public Image fillImg;
    public Color[] threeColors;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case BarType.Hp:
                UpdateHealthToUI();
                break;
            case BarType.Stemina:
                UpdateSteminaToUI();
                break;
            case BarType.MonsterHp:
                UpdateMonsterHealthToUI();
                    break;
        }


        
    }

    public void UpdateSteminaToUI()
    {

        fillImg.fillAmount = player.GetCurrentSteminaRate();
    }

    public void UpdateHealthToUI()
    {
        fillImg.fillAmount = player.GetCurrentHp();

        if (player.GetCurrentHp() < 0.3f) { fillImg.color = threeColors[1]; }
        else
        {
            fillImg.color = threeColors[0];
        }
    }

    public void UpdateMonsterHealthToUI()
    {
        fillImg.fillAmount = monster.GetCurrentHp();
    }
}
