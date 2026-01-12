using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageRenderer : MonoBehaviour
{
    [Header("UI элементы")]
    public RawImage previewImage;

    // 6 полей ввода
    public InputField dataGoTimeInputField;
    public InputField dataGoDataInputField;
    public InputField dataComeTimeInputField;
    public InputField dataComeDataInputField;
    public InputField dataCreateTimeInputField;
    public InputField dataCreateDataInputField;

    public Toggle myToggle;
    public Toggle useSystemDateToggle;

    [Header("Выбор даты через календарь")]
    public InputField targetDateInputField1; // первое поле даты
    public InputField targetDateInputField2; // второе поле даты

    [Header("Месяц")]
    public Dropdown monthDropdown;
    public Sprite[] months1;
    public Sprite[] months2;
    public Sprite[] months3;
    public RectTransform digitsParentMonth;

    [Header("Скрытый Canvas")]
    public Canvas renderCanvas;
    public RawImage backgroundImage;
    public RectTransform digitsParentTimeGoAway;
    public RectTransform digitsParentDataGoAway;
    public RectTransform digitsParentTimeComeback;
    public RectTransform digitsParentDataComeback;
    public RectTransform digitsParentCreateDay;
    public RectTransform digitsParentYearCreate;
    public RectTransform toggleParentOn;
    public RectTransform toggleParentOff;
    public GameObject digitPrefab;
    public Camera renderCamera;
    public RenderTexture renderTexture;

    [Header("Наборы символов (10 массивов)")]
    public Sprite[] numbers1;
    public Sprite[] numbers2;
    public Sprite[] numbers3;
    public Sprite[] numbers4;
    public Sprite[] numbers5;
    public Sprite[] numbers6;
    public Sprite[] numbers7;
    public Sprite[] numbers8;
    public Sprite[] numbers9;
    public Sprite[] numbers10;
    public Sprite space;
    public Sprite toggleSpriteOn;
    public Sprite toggleSpriteOff;

    [Header("Расстояние между символами")]
    public float digitSpacing = 17f;

    [Header("Материал для цифр (шейдер WhiteToTransparent)")]
    public Material digitMaterial;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject unityActivity;
#endif

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
    }

    // Кнопка "Выбрать картинку"
    public void PickImage()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 2048);
                if (texture != null)
                {
                    previewImage.texture = texture;
                    previewImage.SetNativeSize();

                    backgroundImage.texture = texture;
                    backgroundImage.SetNativeSize();

                    RectTransform rt = backgroundImage.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                PlayerPrefs.SetString("SavedImagePath", path);
                PlayerPrefs.Save();
            }

        }, "Выберите изображение", "image/*");
    }

    public void Start()
    {
        // Проверяем, был ли ранее сохранён путь к изображению
        if (PlayerPrefs.HasKey("SavedImagePath"))
        {
            string savedPath = PlayerPrefs.GetString("SavedImagePath");
            if (File.Exists(savedPath))
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(savedPath, 2048);
                if (texture != null)
                {
                    previewImage.texture = texture;
                    previewImage.SetNativeSize();

                    backgroundImage.texture = texture;
                    backgroundImage.SetNativeSize();

                    RectTransform rt = backgroundImage.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    Debug.Log("Загружено сохранённое изображение: " + savedPath);
                }
            }
        }
    }

    private void GenerateToggleImage()
    {
        foreach (Transform child in toggleParentOn)
            Destroy(child.gameObject);
        foreach (Transform child in toggleParentOff)
            Destroy(child.gameObject);

        if (myToggle.isOn)
        {
            GameObject go = Instantiate(digitPrefab, toggleParentOn);
            Image img = go.GetComponent<Image>();
            img.sprite = toggleSpriteOn;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(toggleSpriteOn.rect.width, toggleSpriteOn.rect.height);
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            GameObject go = Instantiate(digitPrefab, toggleParentOff);
            Image img = go.GetComponent<Image>();
            img.sprite = toggleSpriteOff;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(toggleSpriteOff.rect.width, toggleSpriteOff.rect.height);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    // Кнопка "Сохранить"
    public void SaveImage()
    {
        renderCanvas.gameObject.SetActive(true);

        GenerateImage(dataGoTimeInputField.text, digitsParentTimeGoAway);
        GenerateImage(dataGoDataInputField.text, digitsParentDataGoAway);
        GenerateImage(dataComeTimeInputField.text, digitsParentTimeComeback);
        GenerateImage(dataComeDataInputField.text, digitsParentDataComeback);
        GenerateImage(dataCreateTimeInputField.text, digitsParentCreateDay);
        GenerateImage(dataCreateDataInputField.text, digitsParentYearCreate);

        if (useSystemDateToggle.isOn)
        {
            DateTime now = DateTime.Now;
            string day = now.Day.ToString();
            string year = now.Year.ToString();

            GenerateImage(day, digitsParentCreateDay);
            GenerateImage(year[^1].ToString(), digitsParentYearCreate);
        }
        else
        {
            GenerateImage(dataCreateTimeInputField.text, digitsParentCreateDay);
            GenerateImage(dataCreateDataInputField.text, digitsParentYearCreate);
        }

        GenerateMonth(digitsParentMonth);
        GenerateToggleImage();
        SaveFinalImage();

        renderCanvas.gameObject.SetActive(false);
    }

    private void GenerateImage(string text, RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        float startX = 0f;

        foreach (char c in text)
        {
            Sprite charSprite = null;

            if (char.IsDigit(c))
            {
                int digit = c - '0';
                int randomSet = UnityEngine.Random.Range(1, 11);
                Sprite[] set = GetDigitArray(randomSet);
                if (set != null && set.Length > digit)
                    charSprite = set[digit];
            }
            else if (c == ' ')
            {
                charSprite = space;
            }
            else if (c == ':' || c == '.')
            {
                int index = c == ':' ? 10 : 11;
                int randomSet = UnityEngine.Random.Range(1, 11);
                Sprite[] arr = GetDigitArray(randomSet);
                charSprite = arr.Length > index ? arr[index] : null;
            }

            if (charSprite != null)
            {
                GameObject go = Instantiate(digitPrefab, parent);
                Image img = go.GetComponent<Image>();
                img.sprite = charSprite;
                if (digitMaterial != null) img.material = digitMaterial;

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(charSprite.rect.width, charSprite.rect.height);
                rt.anchoredPosition = new Vector2(startX, 0);
                startX += charSprite.rect.width + digitSpacing;
            }
        }
    }

    private Sprite[] GetDigitArray(int randomSet)
    {
        switch (randomSet)
        {
            case 1: return numbers1;
            case 2: return numbers2;
            case 3: return numbers3;
            case 4: return numbers4;
            case 5: return numbers5;
            case 6: return numbers6;
            case 7: return numbers7;
            case 8: return numbers8;
            case 9: return numbers9;
            case 10: return numbers10;
            default: return numbers1;
        }
    }

    private void GenerateMonth(RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        int monthIndex = useSystemDateToggle.isOn ? DateTime.Now.Month - 1 : monthDropdown.value;
        if (monthIndex < 0 || monthIndex >= 12) return;

        int randomSet = UnityEngine.Random.Range(1, 4);
        Sprite monthSprite = randomSet switch
        {
            1 => months1[monthIndex],
            2 => months2[monthIndex],
            3 => months3[monthIndex],
            _ => null
        };
        if (monthSprite == null) return;

        GameObject go = Instantiate(digitPrefab, parent);
        Image img = go.GetComponent<Image>();
        img.sprite = monthSprite;
        if (digitMaterial != null) img.material = digitMaterial;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(monthSprite.rect.width, monthSprite.rect.height);
        rt.anchoredPosition = Vector2.zero;
    }

    private void SaveFinalImage()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();

        Texture2D result = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        result.Apply();

        renderCamera.targetTexture = null;
        RenderTexture.active = currentRT;

        byte[] pngData = result.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, "Заявление_уезд.png");
        File.WriteAllBytes(path, pngData);

        NativeGallery.SaveImageToGallery(path, "MyApp", "Заявление_уезд.png");
        Debug.Log("Изображение сохранено: " + path);
    }

    // 🔹 Реализация вызова Android DatePicker
    public void OnPickDateButton(int fieldIndex)
    {
        InputField targetField = fieldIndex == 1 ? targetDateInputField1 : targetDateInputField2;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (unityActivity == null)
        {
            Debug.LogWarning("Не инициализирован UnityActivity!");
            return;
        }

        unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            var listener = new DateSetListener((year, month, day) =>
            {
                string date = $"{day:D2}.{month + 1:D2}.{year}";
                targetField.text = date;
            });

            AndroidJavaObject dialog = new AndroidJavaObject(
                "android.app.DatePickerDialog",
                unityActivity,
                listener,
                DateTime.Now.Year,
                DateTime.Now.Month - 1,
                DateTime.Now.Day
            );

            dialog.Call("show");
        }));
#else
        targetField.text = DateTime.Now.ToString("dd.MM.yyyy");
#endif
    }

    // 🔸 Вспомогательный класс для DatePickerDialog
    private class DateSetListener : AndroidJavaProxy
    {
        private readonly Action<int, int, int> onDateSetCallback;

        public DateSetListener(Action<int, int, int> onDateSet)
            : base("android.app.DatePickerDialog$OnDateSetListener")
        {
            this.onDateSetCallback = onDateSet;
        }

        // Этот метод должен строго называться onDateSet — но чтобы не конфликтовало с именем поля, мы просто поменяли имя поля
        public void onDateSet(AndroidJavaObject view, int year, int monthOfYear, int dayOfMonth)
        {
            onDateSetCallback?.Invoke(year, monthOfYear, dayOfMonth);
        }
    }
}
