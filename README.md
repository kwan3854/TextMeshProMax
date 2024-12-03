# **TextMeshProMax**

**TextMeshProMax** is a utility library that extends `TextMesh Pro`, making it easier to perform advanced text-related tasks in Unity projects. It supports standard TextMesh Pro functionalities and optional RubyTextMeshPro integration.

------

## Table of Contents
<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
Details

- [**Features**](#features)
  - [**1. GetStringRects**](#1-getstringrects)
    - [**Parameters**](#parameters)
    - [**Returns**](#returns)
  - [**2. GetRubyStringRects *(Requires RubyTextMeshPro)***](#2-getrubystringrects-requires-rubytextmeshpro)
    - [**Parameters**](#parameters-1)
    - [**Returns**](#returns-1)
  - [**3. Multi-Line Support**](#3-multi-line-support)
    - [Example:](#example)
- [**Example Usage**](#example-usage)
  - [**Standard TMP_Text Example**](#standard-tmp_text-example)
  - [**RubyTextMeshPro Example**](#rubytextmeshpro-example)
- [**Contributing**](#contributing)
  - [**Requesting New Features**](#requesting-new-features)
- [**License**](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## How to Install

1. Open package manager from **Unity Editor**
   - `Widnow` -> `Package Manager` -> `+` button on top left -> `Add package from git URL` 
2. Copy paste this url
   - ```https://github.com/kwan3854/TextMeshProMax.git```
   - If you need specific version, you can specify like this ```https://github.com/kwan3854/TextMeshProMax.git#v0.2.0```


## Features

### 1. GetStringRects

Retrieve the **Rect information** for specific strings rendered by a `TMP_Text` object. This function supports multi-line text and overlapping cases, providing precise `Rect` data for all occurrences of the target string.

#### Parameters

- `text` (`TMP_Text` or `TextMeshProUGUI` or `TextMeshPro`): The target TextMesh Pro object.
- `targetString` (`string`): The string you want to locate in the text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the string.
  - `TextFindMode.All`: Returns all occurrences of the string.

#### Returns

- A list of`TextRectInfo` objects, each containing:
  - `Rects`: A list of `Rect` objects representing the coordinates of the string.
  - `TargetString`: The string corresponding to the `Rect`.

### 1.1 TryGetStringRects

Attempt to retrieve the **Rect information** for specific strings rendered by a `TMP_Text` object. Returns `true` if successful, `false` otherwise.

#### Parameters

- `text` (`TMP_Text` or `TextMeshProUGUI` or `TextMeshPro`): The target TextMesh Pro object.
- `targetString` (`string`): The string you want to locate in the text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the string.
  - `TextFindMode.All`: Returns all occurrences of the string.
- `out results` (`List<TextRectInfo>`): The list to store the resulting `TextRectInfo` objects.

#### Returns

- `bool`: `true` if successful, `false` otherwise.

------

### 2. GetRubyStringRects *(Requires RubyTextMeshPro)*

> [!WARNING]
> Requires [RubyTextMeshPro](https://github.com/jp-netsis/RubyTextMeshPro)

> [!NOTE]
> RubyText-specific features require the **RubyTextMeshProUGUI** package, while all other features of this library work independently of whether RubyTextMeshPro is installed.

Retrieve the **Rect information** for complex Ruby strings rendered by a `RubyTextMeshProUGUI` object. The Ruby string consists of multiple `RubyElement` entries, which can include Ruby text, body text, and plain text.

#### Parameters

- `rubyText` (`RubyTextMeshProUGUI` or `RubyTextMeshPro`): The target RubyTextMeshProUGUI object.
- `rubyString` (`RubyString`): A collection of `RubyElement` entries combining Ruby, body, and plain text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the Ruby string.
  - `TextFindMode.All`: Returns all occurrences of the Ruby string.

#### Returns

- A list of `TextRectInfo`objects, each containing:
  - `Rects`: A list of `Rect` objects for the string or line.
  - `TargetString`: The concatenated plain text of the Ruby string.

### 2.1 TryGetRubyStringRects *(Requires RubyTextMeshPro)*

Attempt to retrieve the **Rect information** for complex Ruby strings rendered by a `RubyTextMeshProUGUI` object. Returns `true` if successful, `false` otherwise.

#### Parameters

- `rubyText` (`RubyTextMeshProUGUI` or `RubyTextMeshPro`): The target RubyTextMeshProUGUI object.
- `rubyString` (`RubyString`): A collection of `RubyElement` entries combining Ruby, body, and plain text.
- `findMode`(`TextFindMode`):
  - `TextFindMode.First`: Returns the first occurrence of the Ruby string.
  - `TextFindMode.All`: Returns all occurrences of the Ruby string.
- `out results` (`List<TextRectInfo>`): The list to store the resulting `TextRectInfo` objects.

#### Returns

- `bool`: `true` if successful, `false` otherwise.
------

### 3. Multi-Line Support

The library can calculate `Rect` values for text that spans multiple lines. Whether the line breaks are due to manual newlines (`\n`) or automatic text wrapping applied by TextMesh Pro, the library handles them seamlessly.

> [!TIP]
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

## Example Usage

### Standard TMP_Text Example

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

```csharp
// Works on any tmp: TMP_Text, TextMeshPro, TextMeshProUGUI
TMP_Text text;
TextMeshPro text;
TextMeshProUGUI text;

text.text = "Hello World\nHello Universe";
var rects = text.GetStringRects(rubyString, TextFindMode.All);
```



------

### RubyTextMeshPro Example

> [!NOTE]
> Requires [RubyTextMeshPro](https://github.com/jp-netsis/RubyTextMeshPro)

> [!TIP]
> RubyText-specific features require the **RubyTextMeshPro** package, while all other features of this library work independently of whether RubyTextMeshPro is installed.

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

```csharp
// Works on both RubyTextMeshPro(3D text) and RubyTextMeshProUGUI(2D UI text)
RubyTextMeshProUGUI text;
RubyTextMeshPro text;

text.uneditedText = "<r=domo>Hello</r> <r=sekai>World</r> This is normal text!";
var rects = text.GetRubyStringRects(rubyString, TextFindMode.All);
```

------

## Contributing

We welcome contributions to this project. If you have ideas for new features or improvements, feel free to open an issue or submit a pull request.

### Requesting New Features

If you want to request a new feature or report a bug, please visit the [Issues](https://github.com/kwan3854/TextMeshProMax/issues) page of the repository and create a new issue. Make sure to provide a detailed description of your request or problem to help us address it effectively.

------

## License

This project is licensed under the MIT License. See the LICENSE file for details.
