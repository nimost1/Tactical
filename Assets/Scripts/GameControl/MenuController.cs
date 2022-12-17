using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class MenuController : MonoBehaviour
{
    [SerializeField] private Canvas _menuCanvas;
    [SerializeField] private GameObject[] _buttons;
    [SerializeField] private int _activeButtons;

    public delegate void DelegateFunction();

    public void DisplayMenu()
    {
        _menuCanvas.enabled = true;
        
        GameController.CurrentGameController.EventSystem.SetSelectedGameObject(_buttons[0]);
    }

    public void HideMenu()
    {
        _menuCanvas.enabled = false;
    }

    public void ResetMenu()
    {
        for (int i = _activeButtons - 1; i >= 0; i--)
        {
            _buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            _buttons[i].SetActive(false);
            _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        _activeButtons = 0;
    }

    public void AddButton(string buttonText, DelegateFunction callbackFunction)
    {
        //Creates buttons with the given text that call the given function when pressed
        _buttons[_activeButtons].SetActive(true);

        Button currentButton = _buttons[_activeButtons].GetComponent<Button>();

        currentButton.onClick.RemoveAllListeners();
        currentButton.onClick.AddListener(callbackFunction.Invoke);
        currentButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

        _activeButtons++;
    }
}
