using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject[] menus; //The first element is the pause menu
    [SerializeField] private GameObject[] navigationGuides;

    [SerializeField] private TMP_InputField caveFillText;
    [SerializeField] private UnityEngine.UIElements.Slider caveFillSlider;
    [SerializeField] private TMP_Text goalText;

    private bool exocentric;
    [SerializeField] UnityEngine.UIElements.Toggle exocentricToggle;

    private bool egocentric;
    [SerializeField] UnityEngine.UIElements.Toggle egocentricToggle;

    [SerializeField] UserRotationTracker cageUserTracker;
    [SerializeField] UserRotationTracker iconUserTracker;

    private void Start()
    {
        ToggleAllMenus(false);
        ToggleCursor(true);
        EnableMenu(2);
    }
    public void TogglePause(bool input)
    {
        if (gameManager.caveData == null) return;

        gameManager.Paused = input;

        if (input)
        {
            EnableMenuExclusive(0);

            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ToggleAllMenus(false);

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }

        UnityEngine.Cursor.visible = input;
    }

    public void EnableMenu(int index)
    {
        if (index >= menus.Length || menus[index] == null) return;

        menus[index].SetActive(true);
    }

    public void EnableMenuExclusive(int index)
    {
        if (index >= menus.Length || menus[index] == null) return;

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
        if (index >= menus.Length || menus[index] == null) return;

        menus[index].SetActive(false);
    }

    public void ToggleAllMenus(bool input)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i] == null) continue;

            menus[i].SetActive(input);
        }
    }

    public void ToggleExocentricMinimap(bool input)
    {
        if(input || !egocentric)
        {
            navigationGuides[0].SetActive(input);
            navigationGuides[1].SetActive(input);
        }

        if(!input)
        {
            exocentric = false;
            return;
        }
        exocentric = true;
        egocentric = false;

        cageUserTracker.Disabled = true;
        iconUserTracker.Disabled = false;
        iconUserTracker.Inverted = false;

        cageUserTracker.transform.localRotation = Quaternion.identity;
    }

    public void ToggleEgocentricMinimap(bool input)
    {
        if (input || !exocentric)
        {
            navigationGuides[0].SetActive(input);
            navigationGuides[1].SetActive(input);
        }

        if (!input)
        {
            egocentric = false;
            return;
        }
        egocentric = true;
        exocentric = false;

        cageUserTracker.Disabled = false;
        cageUserTracker.Inverted = true;
        iconUserTracker.Disabled = true;

        iconUserTracker.transform.localRotation = Quaternion.identity;
    }

    public void ToggleCage(bool input)
    {
        navigationGuides[2].SetActive(input);
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
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    public TMP_InputField CaveFillText
    {
        get { return caveFillText; }
        set { caveFillText = value; }
    }

    public UnityEngine.UIElements.Slider CaveFillSlider
    {
        get { return caveFillSlider; }
        set { caveFillSlider = value; }
    }

    public TMP_Text GoalText
    {
        get { return goalText; }
        set { goalText = value; }
    }
}
