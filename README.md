# **TextMeshProMax**

**TextMeshProMax** is a utility library that extends `TextMesh Pro`, making it easier to perform advanced text-related tasks in Unity projects. It supports standard TextMesh Pro functionalities and optional RubyTextMeshPro integration.

------

## **Features**

### **1. GetStringRects**

Retrieve the **Rect information** for specific strings rendered by a `TMP_Text` object. This function supports multi-line text and overlapping cases, providing precise `Rect` data for all occurrences of the target string.

#### **Parameters**

- `text` (`TMP_Text`): The target TextMesh Pro object.
- `targetString` (`string`): The string you want to locate in the text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the string.
  - `TextFindMode.All`: Returns all occurrences of the string.

#### **Returns**

- A list of`TextRectInfo` objects, each containing:
  - `Rects`: A list of `Rect` objects representing the coordinates of the string.
  - `TargetString`: The string corresponding to the `Rect`.

------

### **2. GetRubyStringRects *(Requires RubyTextMeshPro)***

[!WARNING]

>  Requires [RubyTextMeshProUGUI](https://github.com/jp-netsis/RubyTextMeshPro)

[!NOTE]

> RubyText-specific features require the **RubyTextMeshProUGUI** package, while all other features of this library work independently of whether RubyTextMeshPro is installed.

Retrieve the **Rect information** for complex Ruby strings rendered by a `RubyTextMeshProUGUI` object. The Ruby string consists of multiple `RubyElement` entries, which can include Ruby text, body text, and plain text.

#### **Parameters**

- `rubyText` (`RubyTextMeshProUGUI`): The target RubyTextMeshProUGUI object.
- `rubyString` (`RubyString`): A collection of `RubyElement` entries combining Ruby, body, and plain text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the Ruby string.
  - `TextFindMode.All`: Returns all occurrences of the Ruby string.

#### **Returns**

- A list of `TextRectInfo`objects, each containing:
  - `Rects`: A list of `Rect` objects for the string or line.
  - `TargetString`: The concatenated plain text of the Ruby string.

------

### **3. Multi-Line Support**

The library can calculate `Rect` values for text that spans multiple lines. Whether the line breaks are due to manual newlines (`\n`) or automatic text wrapping applied by TextMesh Pro, the library handles them seamlessly.

[!TIP]

> If the target string crosses line boundaries, the library automatically splits the result into one `Rect` per line, regardless of whether the line break was introduced manually or by automatic word wrapping.

#### Example:

For the text:

```
Hello
World!
```

If the target string is `"Hello\nWorld!"`, the result will include two `Rect` objects:

- One for `"Hello"`
- One for `"World!"`

For the text:

```
Hello very long
text that wraps!
```

If the target string is `"Hello very long text that wraps!"`, the result will include two `Rect` objects:

- One for `"Hello very long"`
- One for `"text that wraps!"`

------

## **Example Usage**

### **Standard TMP_Text Example**

```csharp
using Runtime.Helper;
using TMPro;
using UnityEngine;

public class TMPExample : MonoBehaviour
{
    private TMP_Text _text;

    void Start()
    {
        // Initialize TMP_Text
        _text = GetComponent<TMP_Text>();
        _text.text = "Hello World\nHello Universe";

        // Retrieve Rect information for all occurrences of "Hello"
        var rects = _text.GetStringRects("Hello", TextFindMode.All);

        foreach (var rectInfo in rects)
        {
            foreach (var rect in rectInfo.Rects)
            {
                Debug.Log($"BottomLeft: {rect.min}, TopRight: {rect.max}, String: {rectInfo.TargetString}");
            }
        }
    }
}
```

------

### **RubyTextMeshPro Example**

[!NOTE]

>  Requires [RubyTextMeshProUGUI](https://github.com/jp-netsis/RubyTextMeshPro)

[!TIP]

> RubyText-specific features require the **RubyTextMeshProUGUI** package, while all other features of this library work independently of whether RubyTextMeshPro is installed.

```csharp
using Runtime.Helper;
using TMPro;
using UnityEngine;

public class RubyTMPExample : MonoBehaviour
{
    private RubyTextMeshProUGUI _text;

    void Start()
    {
        // Initialize RubyTextMeshProUGUI
        _text = GetComponent<RubyTextMeshProUGUI>();
        _text.uneditedText = "<r=domo>Hello</r> <r=sekai>World</r> This is normal text!";

        // Parse RubyTextMeshPro's plain text
        Debug.Log($"PlainText: {_text.GetParsedText()}");

        // Create RubyString
        var rubyString = new RubyString(new List<RubyElement>
        {
            new RubyElement("Hello", "domo"),
            new RubyElement(" "), // Space
            new RubyElement("World", "sekai"),
            new RubyElement(" This is normal text!") // Plain text
        });

        // Retrieve Rect information
        var rects = _text.GetRubyStringRects(rubyString, TextFindMode.All);

        foreach (var rectInfo in rects)
        {
            foreach (var rect in rectInfo.Rects)
            {
                Debug.Log($"BottomLeft: {rect.min}, TopRight: {rect.max}, String: {rectInfo.TargetString}");
            }
        }
    }
}
```



------

## **Contributing**

We welcome contributions to this project. If you have ideas for new features or improvements, feel free to open an issue or submit a pull request.

### **Requesting New Features**

If you want to request a new feature or report a bug, please visit the [Issues](https://github.com/kwan3854/TextMeshProMax/issues) page of the repository and create a new issue. Make sure to provide a detailed description of your request or problem to help us address it effectively.

------

## **License**

This project is licensed under the MIT License. See the LICENSE file for details.
