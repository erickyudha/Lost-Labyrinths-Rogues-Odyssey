using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Slider slider;
    public Player player;
    private Stats stats;
    private Transform bgFill;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        stats = player.GetComponentInChildren<Stats>();
        bgFill = transform.Find("BG Fill");
    }

    // Update is called once per frame
    void Update()
    {
        if (slider.TryGetComponent<RectTransform>(out var sliderTransform))
        {
            sliderTransform.sizeDelta = new Vector2(stats.maxHealth, sliderTransform.sizeDelta.y);
            bgFill.GetComponent<RectTransform>().sizeDelta = new Vector2(stats.maxHealth, bgFill.GetComponent<RectTransform>().sizeDelta.y);
        }

        slider.value = stats.currentHealth / stats.maxHealth;
    }
}
