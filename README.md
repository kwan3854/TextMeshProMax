# **TextMeshProMax**

**TextMeshProMax** is a utility library that extends `TextMesh Pro`, making it easier to perform advanced text-related tasks in Unity projects. It supports standard TextMesh Pro functionalities and optional RubyTextMeshPro integration.

------

## Table of Contents
<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
Details

- [How to Install](#how-to-install)
- [Features](#features)
  - [1. GetStringRects](#1-getstringrects)
    - [Parameters](#parameters)
    - [Returns](#returns)
  - [1.1 TryGetStringRects](#11-trygetstringrects)
    - [Parameters](#parameters-1)
    - [Returns](#returns-1)
  - [2. GetRubyStringRects *(Requires RubyTextMeshPro)*](#2-getrubystringrects-requires-rubytextmeshpro)
    - [Parameters](#parameters-2)
    - [Returns](#returns-2)
  - [2.1 TryGetRubyStringRects *(Requires RubyTextMeshPro)*](#21-trygetrubystringrects-requires-rubytextmeshpro)
    - [Parameters](#parameters-3)
    - [Returns](#returns-3)
  - [3. Multi-Line Support](#3-multi-line-support)
    - [Example:](#example)
- [Example Usage](#example-usage)
  - [Standard TMP_Text Example](#standard-tmp_text-example)
  - [RubyTextMeshPro Example](#rubytextmeshpro-example)
- [Contributing](#contributing)
  - [Requesting New Features](#requesting-new-features)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## How to Install

1. Open package manager from **Unity Editor**
  - `Widnow` -> `Package Manager` -> `+` button on top left -> `Add package from git URL`
2. Copy paste this url
  - ```https://github.com/kwan3854/TextMeshProMax.git```
  - If you need specific version, you can specify like this ```https://github.com/kwan3854/TextMeshProMax.git#v0.2.0```

> [!TIP]
> You can see inline comments in the code editor by enabling this option in the Unity Editor: `Edit` -> `Preferences` -> `External Tools` -> `Generate .csproj files`
> <img src="Documentation~/Images/xml_settings.png" alt="How to enable xml comments" width="700" />

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

## CJK TextBakerProMax

The **CJK TextBakerProMax** is a utility for creating customized TextMesh Pro font atlases optimized for CJK (Chinese, Japanese, Korean) characters. It generates **Dynamic Atlases**, ensuring text rendering flexibility without pre-baking static font assets.

`Tools` -> `CJK Font Atlas Generator`
<img src="Documentation~/Images/CJK_baker.png" alt="CJK Baking Tool" width="700" />
### Why Use This Tool?

By default, dynamic font atlases in TextMesh Pro allow you to use fonts without worrying about missing characters. Characters are added to the atlas dynamically as needed. However, there are specific cases where you might want to predefine a set of characters in the atlas to avoid runtime performance issues:

1. **High-density text rendering**: For example, chat applications, subtitles, or text-rich interfaces where a large number of characters are displayed at once.
2. **Unpredictable character usage**: For example, when text is dynamically loaded from servers or large datasets, and the exact characters are unknown at build time.

For most projects, especially where the text is predictable and limited in quantity, you don’t need to predefine a large set of characters. Instead, rely on the default dynamic behavior to optimize performance.

---

### Atlas Settings

- **Font Size**: Characters are set to 100 points.
- **Padding**: Padding is fixed at 10% of the character size.
- **Atlas Size**: Atlases are always 2048x2048 pixels.

#### Why 100 Points and 10% Padding?
> CJK characters are more complex than Latin characters. Through testing, it was found that font sizes below 100 points often result in visual artifacts, such as blurriness or inadequate detail for complex shapes like Chinese ideographs or Korean Hangul syllables. The 10% padding ensures proper spacing between characters and prevents artifacts at the edges.

---

### Options

#### Language Options
You can select from **Chinese**, **Japanese**, and **Korean**. These options determine which specific ranges and subsets of characters will be included in the generated font atlas.

#### CJK Options
These options are shared across all three languages. They cover additional character sets commonly used in CJK writing systems. The following sections provide detailed descriptions of each range, their purpose, and when to enable them.

---

### Detailed Options and Usage

#### **Korean**
1. **KS-1001 (B0A0-C8FF)**
- **Description**:  
  KS X 1001 is the South Korean character encoding standard. It includes 2,350 commonly used Hangul syllables, Hanja, and symbols.  
  [Learn more about KS X 1001](https://en.wikipedia.org/wiki/KS_X_1001)
- **When to Enable**:  
  Use this option if your project targets South Korean audiences and focuses on frequently used characters (e.g., educational apps, simple interfaces).

2. **Full Hangul (AC00-D7A3)**
- **Description**:  
  This range includes all 11,172 possible Hangul syllables, covering every valid combination of Korean Jamo.
- **When to Enable**:  
  Use for applications that involve extensive Korean text or unpredictable user input, such as chat apps or text-heavy games.

3. **Hangul Jamo (1100-11FF)**
- **Description**:  
  Includes the individual components (consonants and vowels) used to form Hangul syllables.
- **When to Enable**:  
  Choose this if your project involves advanced Korean typography or phonetic analysis, where Jamo might be displayed independently.

4. **Hangul Compatibility Jamo (3130-318F)**
- **Description**:  
  Contains pre-composed Hangul Jamo blocks for backward compatibility.
- **When to Enable**:  
  Use only if your application needs to support legacy Korean text encoding.

5. **Hangul Jamo Extended-A (A960-A97F)**
- **Description**:  
  Additional Jamo blocks introduced in Unicode 5.2.
- **When to Enable**:  
  Enable for specialized linguistic tools or projects requiring modern Korean typographic features.

6. **Hangul Jamo Extended-B (D7B0-D7FF)**
- **Description**:  
  A further extension of Hangul Jamo for rare or historic Korean text.
- **When to Enable**:  
  Enable only if you are working on historic Korean text or rare linguistic projects.

---

#### **Japanese**
1. **Hiragana (3040-309F)**
- **Description**:  
  The basic phonetic script used in Japanese. Includes 83 characters.  
  [Learn more about Hiragana](https://en.wikipedia.org/wiki/Hiragana)
- **When to Enable**:  
  Use for any Japanese text rendering, as Hiragana is fundamental to the language.

2. **Katakana (30A0-30FF)**
- **Description**:  
  The second phonetic script in Japanese, used for foreign words and emphasis. Includes 96 characters.  
  [Learn more about Katakana](https://en.wikipedia.org/wiki/Katakana)
- **When to Enable**:  
  Essential for Japanese text, especially in games or apps with foreign word integration or stylized text.

3. **Katakana Phonetic Extensions (31F0-31FF)**
- **Description**:  
  Extensions to Katakana, mainly for Ainu language support.
- **When to Enable**:  
  Use for projects involving Ainu language support or specialized Japanese typographic needs.

---

#### **CJK Options**
1. **CJK Unified Ideographs (4E00-9FBF)**
- **Description**:  
  The core block of 20,976 Chinese, Japanese, and Korean ideographs.  
  [Learn more about CJK Unified Ideographs](https://en.wikipedia.org/wiki/CJK_Unified_Ideographs)
- **When to Enable**:  
  Use this for general-purpose support of Chinese, Japanese, or Korean text. Essential for any CJK-focused application.

2. **CJK Unified Ideographs Extension B (20000-2A6DF)**
- **Description**:  
  Includes 42,711 additional ideographs.  
  [Learn more about Extension B](https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_B)
- **When to Enable**:  
  Use for scholarly or historic texts requiring rare or ancient characters.

3. **CJK Compatibility Ideographs (F900-FAFF)**
- **Description**:  
  A set of characters for compatibility with legacy encodings.  
  [Learn more about CJK Compatibility Ideographs](https://en.wikipedia.org/wiki/CJK_Compatibility_Ideographs)
- **When to Enable**:  
  Use only for backward compatibility or applications handling legacy text files.

4. **CJK Compatibility Ideographs Supplement (2F800-2FA1F)**
- **Description**:  
  Supplements the compatibility ideographs for specialized needs.  
  [Learn more about the Supplement](https://en.wikipedia.org/wiki/CJK_Compatibility_Ideographs_Supplement)
- **When to Enable**:  
  Enable for rare cases involving historic or specialized text processing.

5. **CJK Radicals Supplement (2E80-2EFF)**
- **Description**:  
  Contains radicals used in East Asian scripts for dictionaries or indexing.  
  [Learn more about CJK Radicals Supplement](https://en.wikipedia.org/wiki/CJK_Radicals_Supplement)
- **When to Enable**:  
  Use for dictionary applications or tools involving radical-based text analysis.

---

## Contributing

We welcome contributions to this project. If you have ideas for new features or improvements, feel free to open an issue or submit a pull request.

### Requesting New Features

If you want to request a new feature or report a bug, please visit the [Issues](https://github.com/kwan3854/TextMeshProMax/issues) page of the repository and create a new issue. Make sure to provide a detailed description of your request or problem to help us address it effectively.

------

## License

This project is licensed under the MIT License. See the LICENSE file for details.
