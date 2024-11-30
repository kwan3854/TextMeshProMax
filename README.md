# TextMeshProExtensions

TextMeshProExtensions is a utility library that provides extended functionalities for [TextMesh Pro](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126), aimed at simplifying and enhancing text-related tasks in Unity projects. This library is continuously updated with new features to assist developers in working with TextMesh Pro more effectively.

---

## Features

### 1. GetStringRects

This feature allows you to retrieve the **Rect information** of a specific string rendered by a `TMP_Text` object. The returned data includes the bottom-left and top-right coordinates for each occurrence of the string within the text, supporting multi-line and overlapping cases.

#### **Parameters**

- `text` (`TMP_Text`): The target TextMesh Pro object.
- `targetString` (`string`): The string you want to find in the text.
- `findMode` (`TextFindMode`): Specifies whether to return the first occurrence or all occurrences of the string:
  - `TextFindMode.First`: Returns the first occurrence.
  - `TextFindMode.All`: Returns all occurrences.

#### **Returns**

- A list of `TextRectInfo` objects, each containing:
  - `Rects`: A list of `Rect` objects representing the coordinates of the string.
  - `TargetString`: The string that the `Rect` refers to.

---

### Example Usage

```csharp
using Runtime.Helper;
using TMPro;
using UnityEngine;

public class TMPExample : MonoBehaviour
{
    private TMP_Text text;

    void Start()
    {
        // Initialize TMP_Text
        text = GetComponent<TMP_Text>();
        text.text = "Hello World\nHello Universe";

        // Retrieve Rect information for all occurrences of "Hello"
        var rects = text.GetStringRects("Hello", TextFindMode.All);

        foreach (var rectInfo in rects)
        {
            foreach (var rect in rectInfo.Rects)
            {
                Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
            }
        }
    }
}
```

---

------

## Contributing

We welcome contributions to this project. If you have ideas for new features or improvements, feel free to open an issue or submit a pull request.

### Requesting New Features

If you want to request a new feature or report a bug, please visit the [Issues](https://github.com/kwan3854/TextMeshProMax/issues) page of the repository and create a new issue. Make sure to provide a detailed description of your request or problem to help us address it effectively.

------

## License

This project is licensed under the MIT License. See the LICENSE file for details.
