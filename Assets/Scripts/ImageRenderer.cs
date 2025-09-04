using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageRenderer : MonoBehaviour
{
    [Header("UI элементы")]
    public RawImage previewImage;       // превью пользователю
    public InputField inputField1;      // первое поле ввода
    public InputField inputField2;      // второе поле ввода

    [Header("Скрытый Canvas")]
    public Canvas renderCanvas;         // Canvas для рендера
    public RawImage backgroundImage;    // фон
    public RectTransform digitsParent1; // контейнер для текста 1
    public RectTransform digitsParent2; // контейнер для текста 2
    public GameObject digitPrefab;      // префаб символа (Image)
    public Camera renderCamera;
    public RenderTexture renderTexture;

    [Header("Наборы символов")]
    public Sprite[] numbers1; // [0–9], [10]=":", [11]="."
    public Sprite[] numbers2; // [0–9], [10]=":", [11]="."
    public Sprite[] numbers3; // [0–9], [10]=":", [11]="."
    public Sprite space;      // пробел

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
                    // превью пользователю
                    previewImage.texture = texture;
                    previewImage.SetNativeSize();

                    // фон скрытого Canvas
                    backgroundImage.texture = texture;
                    backgroundImage.SetNativeSize();

                    // растягиваем под Canvas
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

        // генерируем текст в двух местах
        GenerateImage(inputField1.text, digitsParent1);
        GenerateImage(inputField2.text, digitsParent2);

        SaveFinalImage();

        renderCanvas.gameObject.SetActive(false);
    }

    // Создание текста в указанном контейнере
    private void GenerateImage(string text, RectTransform parent)
    {
        // очистка старого текста
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        float startX = -200f;

        foreach (char c in text)
        {
            Sprite charSprite = null;

            if (char.IsDigit(c))
            {
                int digit = c - '0';
                int randomSet = UnityEngine.Random.Range(1, 4);

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
                int randomSet = UnityEngine.Random.Range(1, 4);
                switch (randomSet)
                {
                    case 1: charSprite = numbers1[10]; break;
                    case 2: charSprite = numbers2[10]; break;
                    case 3: charSprite = numbers3[10]; break;
                }
            }
            else if (c == '.')
            {
                int randomSet = UnityEngine.Random.Range(1, 4);
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
