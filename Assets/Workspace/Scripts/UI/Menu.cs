using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Menu : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public string name = null;

        public UnityEvent onSubmit = new UnityEvent();
    }

    public static Menu Instance = null;

    public Data[] data = null;

    public CanvasGroup canvasGroup = null;

    public RectTransform imageText = null;

    public Transform parentMenu = null;

    public MenuHud prefab = null;

    public TextMeshProUGUI text = null;

    private int currentIndex = -1;

    private bool isMouseMoved = false;

    private bool isOpened = false;

    private Vector3 screenCenter = new Vector2();

    private Vector3 startMousePosition = new Vector2();

    private Tweener tweener = null;

    private List<MenuHud> hudList = new List<MenuHud>();


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        int count = data.Length;
        for (int a = 0; a < count; ++a)
        {
            MenuHud hud = Instantiate(prefab, parentMenu);
            hud.Initialize(a, count);
            hudList.Add(hud);
        }

        screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
    }


    private void Update()
    {
        if (!isOpened)
        {
            return;
        }

        Vector3 direction = Vector3.zero;
        bool isValid = false;
        if (!isMouseMoved && Vector3.Distance(Input.mousePosition, startMousePosition) > 3f)
        {
            isMouseMoved = true;
        }

        if (InputManager.IsLookValid)
        {
            Vector3 lookValue = InputManager.LookValue;
            lookValue.y = lookValue.z;
            lookValue.z = 0f;
            direction = lookValue;
            isValid = true;
        }
        else if (isMouseMoved)
        {
            direction = Input.mousePosition - screenCenter;
            isValid = true;
        }

        if (!isValid)
        {
            ChangeIndex(-1);
        }
        else if (direction != Vector3.zero)
        {
            float angle = 90f - Mathf.Atan2(direction.y, direction.x) * 180f / Mathf.PI;
            if (angle < 0f)
            {
                angle += 360f;
            }

            print(angle);
            int count = data.Length;
            int index = Mathf.Clamp(Mathf.RoundToInt(angle * count / 360f), 0, count);
            if (index == count)
            {
                index = 0;
            }

            ChangeIndex(index);
        }
    }


    private void ChangeIndex(int newIndex)
    {
        if (currentIndex == newIndex)
        {
            return;
        }

        if (currentIndex >= 0 && currentIndex < data.Length)
        {
            hudList[currentIndex].TweenOut();
        }

        currentIndex = newIndex;
        if (newIndex >= 0 && newIndex < data.Length)
        {
            AudioManager.Instance.Play("菜单切换", AudioManager.Instance.audioListener.transform);
            hudList[newIndex].TweenIn();
            text.text = data[newIndex].name;
            imageText.sizeDelta = text.GetPreferredValues() + new Vector2(20f, 10f);
        }
        else
        {
            text.text = null;
            imageText.sizeDelta = Vector2.zero;
        }
    }


    public void Close()
    {
        AudioManager.Instance.Play("菜单关闭", AudioManager.Instance.audioListener.transform);
        isOpened = false;
        tweener?.Kill();
        tweener = DOTween.To(value => { canvasGroup.alpha = value; }, 1f, 0f, 0.15f).SetEase(Ease.OutExpo);
        Submit();
        ChangeIndex(-1);
    }


    public void Open()
    {
        AudioManager.Instance.Play("菜单开启", AudioManager.Instance.audioListener.transform);
        isMouseMoved = false;
        isOpened = true;
        startMousePosition = Input.mousePosition;
        tweener?.Kill();
        tweener = DOTween.To(value => { canvasGroup.alpha = value; }, 0f, 1f, 0.15f).SetEase(Ease.OutExpo);
    }


    private void Submit()
    {
        if (currentIndex >= 0 && currentIndex < data.Length)
        {
            data[currentIndex].onSubmit.Invoke();
        }
    }
}