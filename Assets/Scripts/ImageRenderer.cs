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

    public UnityEngine.UI.Toggle myToggle;
    public UnityEngine.UI.Toggle useSystemDateToggle;

    [Header("Месяц")]
    public Dropdown monthDropdown;
    public Sprite[] months1; // 12 спрайтов
    public Sprite[] months2; // 12 спрайтов
    public Sprite[] months3; // 12 спрайтов
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
            }
        }, "Выберите изображение", "image/*");
    }

    private void GenerateToggleImage()
    {
        // очищаем оба места
        foreach (Transform child in toggleParentOn)
            Destroy(child.gameObject);
        foreach (Transform child in toggleParentOff)
            Destroy(child.gameObject);

        if (myToggle.isOn)
        {
            // если включен → вставляем картинку в toggleParentOn
            GameObject go = Instantiate(digitPrefab, toggleParentOn);
            UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
            img.sprite = toggleSpriteOn;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(toggleSpriteOn.rect.width, toggleSpriteOn.rect.height);
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            // если выключен → вставляем картинку в toggleParentOff
            GameObject go = Instantiate(digitPrefab, toggleParentOff);
            UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
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

        // генерируем текст в 6 местах
        GenerateImage(dataGoTimeInputField.text, digitsParentTimeGoAway);
        GenerateImage(dataGoDataInputField.text, digitsParentDataGoAway);
        GenerateImage(dataComeTimeInputField.text, digitsParentTimeComeback);
        GenerateImage(dataComeDataInputField.text, digitsParentDataComeback);
        GenerateImage(dataCreateTimeInputField.text, digitsParentCreateDay);
        GenerateImage(dataCreateDataInputField.text, digitsParentYearCreate);

        // генерируем месяц
        if (useSystemDateToggle.isOn)
        {
            // Берём системную дату телефона
            System.DateTime now = System.DateTime.Now;
            string day = now.Day.ToString();   
            string year = now.Year.ToString();       // 2025

            GenerateImage(day, digitsParentCreateDay);
            GenerateImage(year[^1].ToString(), digitsParentYearCreate);
        }
        else
        {
            // Старый вариант: как вводил пользователь
            GenerateImage(dataCreateTimeInputField.text, digitsParentCreateDay);
            GenerateImage(dataCreateDataInputField.text, digitsParentYearCreate);
        }
        GenerateMonth(digitsParentMonth);

        GenerateToggleImage();

        SaveFinalImage();

        renderCanvas.gameObject.SetActive(false);
    }

    // Создание текста (цифры, точки, пробелы, двоеточие)
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
                switch (randomSet)
                {
                    case 1: charSprite = numbers1[digit]; break;
                    case 2: charSprite = numbers2[digit]; break;
                    case 3: charSprite = numbers3[digit]; break;
                    case 4: charSprite = numbers4[digit]; break;
                    case 5: charSprite = numbers5[digit]; break;
                    case 6: charSprite = numbers6[digit]; break;
                    case 7: charSprite = numbers7[digit]; break;
                    case 8: charSprite = numbers8[digit]; break;
                    case 9: charSprite = numbers9[digit]; break;
                    case 10: charSprite = numbers10[digit]; break;
                }
            }
            else if (c == ' ')
            {
                charSprite = space;
            }
            else if (c == ':')
            {
                int randomSet = UnityEngine.Random.Range(1, 11);
                switch (randomSet)
                {
                    case 1: charSprite = numbers1[10]; break;
                    case 2: charSprite = numbers2[10]; break;
                    case 3: charSprite = numbers3[10]; break;
                    case 4: charSprite = numbers4[10]; break;
                    case 5: charSprite = numbers5[10]; break;
                    case 6: charSprite = numbers6[10]; break;
                    case 7: charSprite = numbers7[10]; break;
                    case 8: charSprite = numbers8[10]; break;
                    case 9: charSprite = numbers9[10]; break;
                    case 10: charSprite = numbers10[10]; break;
                }
            }
            else if (c == '.')
            {
                int randomSet = UnityEngine.Random.Range(1, 11);
                switch (randomSet)
                {
                    case 1: charSprite = numbers1[11]; break;
                    case 2: charSprite = numbers2[11]; break;
                    case 3: charSprite = numbers3[11]; break;
                    case 4: charSprite = numbers4[11]; break;
                    case 5: charSprite = numbers5[11]; break;
                    case 6: charSprite = numbers6[11]; break;
                    case 7: charSprite = numbers7[11]; break;
                    case 8: charSprite = numbers8[11]; break;
                    case 9: charSprite = numbers9[11]; break;
                    case 10: charSprite = numbers10[11]; break;
                }
            }

            if (charSprite != null)
            {
                GameObject go = Instantiate(digitPrefab, parent);
                UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
                img.sprite = charSprite;

                // Автоматически ставим материал с шейдером
                if (digitMaterial != null)
                {
                    img.material = digitMaterial;
                }

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(charSprite.rect.width, charSprite.rect.height); // используем исходный размер
                rt.anchoredPosition = new Vector2(startX, 0);

                startX += charSprite.rect.width + digitSpacing; // учитываем реальную ширину спрайта
            }
        }
    }

    // Генерация месяца
    private void GenerateMonth(RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        int monthIndex = 1;

        if (useSystemDateToggle.isOn)
        {
            System.DateTime now = System.DateTime.Now;
            monthIndex = now.Month - 1;
        }
        else
        {
            monthIndex = monthDropdown.value;
        }

        if (monthIndex < 0 || monthIndex >= 12) return;

        int randomSet = UnityEngine.Random.Range(1, 4);
        Sprite monthSprite = null;

        switch (randomSet)
        {
            case 1: monthSprite = months1[monthIndex]; break;
            case 2: monthSprite = months2[monthIndex]; break;
            case 3: monthSprite = months3[monthIndex]; break;
        }

        if (monthSprite == null) return;

        GameObject go = Instantiate(digitPrefab, parent);
        UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
        img.sprite = monthSprite;

        if (digitMaterial != null)
        {
            img.material = digitMaterial;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(monthSprite.rect.width, monthSprite.rect.height); // исходный размер
        rt.anchoredPosition = Vector2.zero;
    }

    // Сохраняем итоговую картинку
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
}
