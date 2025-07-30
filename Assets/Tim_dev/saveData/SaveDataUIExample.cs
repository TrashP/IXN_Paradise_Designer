using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档UI设置示例
/// 这个脚本展示了如何设置存档系统的UI布局
/// </summary>
public class SaveDataUIExample : MonoBehaviour
{
    [Header("UI设置说明")]
    [TextArea(5, 10)]
    public string setupInstructions = @"
存档UI设置步骤：

1. 创建Canvas和存档UI面板
2. 添加3个存档槽位按钮（对应3个存档槽位）
3. 添加3个文本组件显示槽位信息
4. 添加保存、加载、删除按钮
5. 添加当前槽位显示文本
6. 将SaveDataUI脚本添加到UI面板上
7. 在Inspector中设置所有UI引用

推荐UI布局：
- 顶部：当前槽位显示
- 中间：3个存档槽位按钮（水平排列）
- 底部：保存、加载、删除按钮（水平排列）
";

    [Header("UI组件引用示例")]
    public Button[] slotButtons = new Button[3]; // 3个槽位按钮
    public TextMeshProUGUI[] slotTexts = new TextMeshProUGUI[3]; // 3个槽位信息文本
    public Button saveButton;
    public Button loadButton;
    public Button deleteButton;
    public TextMeshProUGUI currentSlotText;

    [Header("UI设置提示")]
    [TextArea(3, 5)]
    public string buttonSetupHint = @"
按钮设置提示：
- 槽位按钮：点击切换当前选中的槽位
- 保存按钮：保存到当前选中的槽位
- 加载按钮：从当前选中的槽位加载
- 删除按钮：删除当前选中的槽位存档
";

    private void Start()
    {
        // 检查UI组件是否正确设置
        CheckUIComponents();
    }

    private void CheckUIComponents()
    {
        bool hasError = false;

        // 检查槽位按钮
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
            {
                Debug.LogError($"槽位按钮 {i + 1} 未设置！");
                hasError = true;
            }
        }

        // 检查槽位文本
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] == null)
            {
                Debug.LogError($"槽位文本 {i + 1} 未设置！");
                hasError = true;
            }
        }

        // 检查功能按钮
        if (saveButton == null) { Debug.LogError("保存按钮未设置！"); hasError = true; }
        if (loadButton == null) { Debug.LogError("加载按钮未设置！"); hasError = true; }
        if (deleteButton == null) { Debug.LogError("删除按钮未设置！"); hasError = true; }
        if (currentSlotText == null) { Debug.LogError("当前槽位文本未设置！"); hasError = true; }

        if (!hasError)
        {
            Debug.Log("所有UI组件已正确设置！");
        }
    }

    // 示例：如何通过代码设置按钮文本
    public void SetupButtonTexts()
    {
        if (saveButton != null)
        {
            TextMeshProUGUI saveText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (saveText != null) saveText.text = "保存";
        }

        if (loadButton != null)
        {
            TextMeshProUGUI loadText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
            if (loadText != null) loadText.text = "加载";
        }

        if (deleteButton != null)
        {
            TextMeshProUGUI deleteText = deleteButton.GetComponentInChildren<TextMeshProUGUI>();
            if (deleteText != null) deleteText.text = "删除";
        }
    }

    // 示例：如何设置按钮颜色
    public void SetupButtonColors()
    {
        if (saveButton != null)
        {
            ColorBlock colors = saveButton.colors;
            colors.normalColor = Color.green;
            colors.pressedColor = Color.green * 0.8f;
            saveButton.colors = colors;
        }

        if (loadButton != null)
        {
            ColorBlock colors = loadButton.colors;
            colors.normalColor = Color.blue;
            colors.pressedColor = Color.blue * 0.8f;
            loadButton.colors = colors;
        }

        if (deleteButton != null)
        {
            ColorBlock colors = deleteButton.colors;
            colors.normalColor = Color.red;
            colors.pressedColor = Color.red * 0.8f;
            deleteButton.colors = colors;
        }
    }
} 