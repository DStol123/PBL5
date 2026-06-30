using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject[] menus; //The first element is the pause menu
    [SerializeField] private GameObject artificialHorizon;

    [SerializeField] private TMP_InputField caveFillText;
    [SerializeField] private Slider caveFillSlider;
    [SerializeField] private TMP_Text goalText;

    private void Start()
    {
        ToggleAllMenus(false);
        ToggleCursor(true);
        EnableMenu(2);
        EnableMenu(4);
    }
    public void TogglePause(bool input)
    {
        gameManager.Paused = input;

        if (input)
        {
            EnableMenuExclusive(0);
            EnableMenu(4);

            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ToggleAllMenus(false);
            EnableMenu(4);
            EnableMenu(5);

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }

        UnityEngine.Cursor.visible = input;
    }

    public void EnableMenu(int index)
    {
        menus[index].SetActive(true);
    }

    public void EnableMenuExclusive(int index)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (i == index)
            {
                menus[i].SetActive(true);
            }
            else
            {
                menus[i].SetActive(false);
            }
        }
    }

    public void DisableMenu(int index)
    {
        menus[index].SetActive(false);
    }

    public void ToggleAllMenus(bool input)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].SetActive(input);
        }
    }
     
    public void ToggleArtificialHorizon()
    {
        artificialHorizon.SetActive(!artificialHorizon.activeSelf);
    }

    public void ChangeCaveFillText(float input)
    {
        caveFillText.text = input.ToString();
    }

    public void ChangeCaveFillSlider(string input)
    {
        float num = float.Parse(input);
        caveFillSlider.value = Mathf.Clamp01(num);
    }

    public void ToggleCursor(bool input)
    {
        if(input)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public TMP_InputField CaveFillText
    {
        get { return caveFillText; }
        set { caveFillText = value; }
    }

    public Slider CaveFillSlider
    {
        get { return caveFillSlider; }
        set { caveFillSlider = value; }
    }

    public TMP_Text GoalText
    {
        get { return goalText; }
        set { goalText = value; }
    }

    public GameObject ArtificialHorizon
    {
        get { return  artificialHorizon; }
    }
}
