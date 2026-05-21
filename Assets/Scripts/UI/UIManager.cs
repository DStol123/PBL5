using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject[] menus; //The first element is the pause menu
    [SerializeField] private GameObject artificialHorizon;

    private void Start()
    {
        ToggleAllMenus(false);
    }
    public void TogglePause(bool input)
    {
        gameManager.Paused = input;

        if (input)
        {
            EnableMenuExclusive(0);

            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ToggleAllMenus(false);

            Cursor.lockState = CursorLockMode.Locked;
        }

        Cursor.visible = input;
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
}
