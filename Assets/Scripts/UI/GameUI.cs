using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Canvases")]
    public GameObject alwaysActive;
    public GameObject casting;
    public GameObject notCasting;
    [Header("Sliders")]
    public Slider manaOver;
    public Slider manaUnder;
    public Slider healthSlider;
    public Image healthImage;
    [Header("Parameters")]
    public Gradient healthGradient;
    public float manaCatchSpeed;
    private float health;
    private float mana;
    public enum UIState
    {
        Casting,
        NotCasting,
        Viewing,
        Paused
    }

    public UIState currentState;
    void Awake()
    {
        Instance = this;
        healthImage.color = healthGradient.Evaluate(1f);
    }

    void Update()
    {
        manaOver.maxValue = PlayerStats.Instance.MaxMana;
        manaUnder.maxValue = PlayerStats.Instance.MaxMana;
        healthSlider.maxValue = PlayerStats.Instance.MaxHealth;

        health = PlayerStats.Instance.Health;
        mana = PlayerStats.Instance.Mana;

        switch (currentState)
        {
            case UIState.Casting:
                casting.SetActive(true);
                notCasting.SetActive(false);
                break;
            case UIState.NotCasting:
                casting.SetActive(false);
                notCasting.SetActive(true);
                break;
        }

        EvaluateSliders();
    }

    void EvaluateSliders()
    {
        manaOver.value = mana;

        if (manaUnder.value > mana && currentState == UIState.NotCasting)
            manaUnder.value = Mathf.Lerp(manaUnder.value, mana, manaCatchSpeed * Time.deltaTime);
        else if (manaUnder.value < mana)
            manaUnder.value = mana;

        healthSlider.value = health;
        healthImage.color = healthGradient.Evaluate(health / PlayerStats.Instance.MaxHealth);
    }
}
