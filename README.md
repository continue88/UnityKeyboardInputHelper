# UnityKeyboardInputHelper
Keyboard input helper (TV.) made for unity.

## 解决目标：
很多安卓的应用，都是基于触摸屏的设计，一旦放到没有触摸/鼠标的设备上，就会导致无法操作（比如很多的电视/机顶盒）。UnityKeyboardInputHelper用来帮助app来实现通过键盘（遥控器）来操作基本的控件（输入/按钮）。

## 使用方法：
将KeyboardInputHelper.cs拖放到一个节点上即可用

## 实现思路
主要通过和【Selectable】来进行交互处理

## 限制
 * 仅限于Unity开发（Made in Unity 2018）
 * 只支持UGUI（Make for UnityEngine.UI.Selectable）
 * 交互目前只支持按钮/输入框(InputField/Button)
