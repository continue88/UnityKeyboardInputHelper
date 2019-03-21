using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 键盘输入支持
/// 在所能够点击的控件上绘制显示框。
/// </summary>
public class KeyboardInputHelper : MonoBehaviour
{
    public static KeyboardInputHelper Instance { private set; get; }
    public event UnityAction OnCancel;

    private Selectable mActiveControl;

    void Start()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    /// <summary>
    /// 更新检查输入
    /// </summary>
    void Update()
    {
        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return)) OnSubmit();
        if (Input.GetButtonDown("Cancel")) OnCancel?.Invoke();
        if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") > 0) ForcusControl(mActiveControl?.FindSelectableOnUp());
        if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") < 0) ForcusControl(mActiveControl?.FindSelectableOnDown());
        if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0) ForcusControl(mActiveControl?.FindSelectableOnRight());
        if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0) ForcusControl(mActiveControl?.FindSelectableOnLeft());
    }

    /// <summary>
    /// GUI绘制
    /// </summary>
    void OnGUI()
    {
        if (mActiveControl && mActiveControl.isActiveAndEnabled)
        {
            var rect = GetSelectableRect(mActiveControl.GetComponent<RectTransform>(), Camera.main);
            GUI.Box(rect, "");
        }
    }

    /// <summary>
    /// 获取目标控件是否能够点击（没有呗其他控件遮挡）
    /// </summary>
    /// <param name="selectable"></param>
    /// <param name="eventSystem"></param>
    /// <returns></returns>
    static bool IsSelectableValid(Selectable selectable, EventSystem eventSystem)
    {
        var raycaster = selectable.GetComponentInParent<GraphicRaycaster>();
        if (!raycaster || !eventSystem) return false;

        var rect = GetSelectableRect(selectable.GetComponent<RectTransform>(), Camera.main);
        var pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = rect.center;

        var results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
            if (result.gameObject == selectable.gameObject) return true;
        return false;
    }

    /// <summary>
    /// 获取目标控件的矩形框
    /// </summary>
    /// <param name="rectTrans"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    static Rect GetSelectableRect(RectTransform rectTrans, Camera camera)
    {
        var worldCorners = new Vector3[4];
        rectTrans.GetWorldCorners(worldCorners);

        var screenPos = new Vector2[worldCorners.Length];
        for (var i = 0; i < worldCorners.Length; i++)
            screenPos[i] = RectTransformUtility.WorldToScreenPoint(camera, worldCorners[i]);

        var bounds = new Bounds(screenPos[0], Vector3.one);
        for (var i = 1; i < screenPos.Length; i++)
            bounds.Encapsulate(screenPos[i]);

        var position = new Vector2(bounds.min.x, Screen.height - bounds.max.y);
        return new Rect(position, bounds.size);
    }

    /// <summary>
    /// 获取可以选择的控件。
    /// </summary>
    /// <param name="selectable"></param>
    /// <returns></returns>
    static Selectable GetValidSelectable(Selectable selectable)
    {
        var eventSystem = Object.FindObjectOfType<EventSystem>();
        if (!eventSystem) return selectable;

        // 这个控件可以点击得到。
        if (IsSelectableValid(selectable, eventSystem)) return selectable;

        // 避免重复添加。
        var ignoreList = new HashSet<Selectable>();
        ignoreList.Add(selectable);

        // 用来遍历处理了。
        var checkStack = new Stack<Selectable>();
        checkStack.Push(selectable);

        while (checkStack.Count > 0)
        {
            var checkOne = checkStack.Pop();
            var nextList = new Selectable[]
            {
                checkOne.FindSelectableOnUp(),
                checkOne.FindSelectableOnDown(),
                checkOne.FindSelectableOnRight(),
                checkOne.FindSelectableOnLeft(),
            };
            for (var i = 0; i < nextList.Length; i++)
            {
                var next = nextList[i];
                if (!next || !ignoreList.Contains(next)) continue;
                if (IsSelectableValid(next, eventSystem)) return next;
                ignoreList.Add(next);
                checkStack.Push(next);
            }
        }

        return selectable;
    }

    /// <summary>
    /// 焦点移动到对应的控件上去。
    /// </summary>
    /// <param name="selectable"></param>
    public void ForcusControl(Selectable selectable)
    {
        if (selectable) mActiveControl = GetValidSelectable(selectable);
    }

    public void OnSubmit()
    {
        if (!mActiveControl) return;

        if (mActiveControl is Button) (mActiveControl as Button).onClick.Invoke();
        if (mActiveControl is InputField) (mActiveControl as InputField).ActivateInputField();
    }
}
