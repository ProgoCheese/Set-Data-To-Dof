using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    public GameObject digitPrefab;
    public Camera renderCamera;
    public RenderTexture renderTexture;

    [Header("Наборы символов")]
    public Sprite[] numbers1; // [0–9], [10]=":", [11]="."
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

    [Header("Настройки символов")]
    public Vector2 digitSize = new Vector2(15, 30);
    public float digitSpacing = 17f;

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
        GenerateMonth(digitsParentMonth);

        SaveFinalImage();

        renderCanvas.gameObject.SetActive(false);
    }

    // Создание текста (цифры, точки, пробелы, двоеточие)
    private void GenerateImage(string text, RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        float startX = -200f;

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
                }
            }

            if (charSprite != null)
            {
                GameObject go = Instantiate(digitPrefab, parent);
                Image img = go.GetComponent<Image>();
                img.sprite = charSprite;

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = digitSize;
                rt.anchoredPosition = new Vector2(startX, 0);

                startX += digitSpacing;
            }
        }
    }

    // Генерация месяца
    private void GenerateMonth(RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        int monthIndex = monthDropdown.value;
        if (monthIndex < 0 || monthIndex >= 12) return;

        // выбираем случайный набор
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
        Image img = go.GetComponent<Image>();
        img.sprite = monthSprite;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 40);  // подогнать под размер месяца
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
        string path = Path.Combine(Application.persistentDataPath, "final_image.png");
        File.WriteAllBytes(path, pngData);

        NativeGallery.SaveImageToGallery(path, "MyApp", "final_image.png");
        Debug.Log("Изображение сохранено: " + path);
    }
}
