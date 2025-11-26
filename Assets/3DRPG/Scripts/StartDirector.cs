using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // LoadSceneを使うために必要
using UnityEngine.InputSystem; // Input Systemを使うために必要
using UnityEngine.InputSystem.Controls;

public class StartDirector : MonoBehaviour
{
    void Update()
    {
        bool gamepadInput = false;

        // ゲームパッドが接続されているかチェック
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    gamepadInput = true;
                    break; // どれか押されたらループを抜ける
                }
            }
        }

        // マウス左クリック または ゲームパッドボタン押下でシーン遷移
        if (Input.GetMouseButtonDown(0) || gamepadInput)
        {
            SceneManager.LoadScene("Main");
        }
    }
}
