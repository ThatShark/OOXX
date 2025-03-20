using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour {
    public enum Player {
        O,
        X,
        Neither,
        Checking
    }
    public Player currentTurn, winner;
    // private ScriptForCursor cursorManager;
    private ScriptForButton buttonManager;
    void Start() {
        currentTurn = Player.O;
        // cursorManager = FindFirstObjectByType<ScriptForCursor>();
        buttonManager = FindFirstObjectByType<ScriptForButton>();
        ResetScene();
    }

    #region ResetScene()系列
    public GameObject EmptyCube;
    private int[,,] countBoard;
    private GameObject[,,] cubeBoard = new GameObject[3, 4, 3]; // 座標[7*(i-1), 7*(j-1), 7*(k-1)]
    public void ResetScene() {
        XWinText.SetActive(true);
        OWinText.SetActive(true);
        endScene.SetActive(false);
        OCanvas.SetActive(true);
        OTriangleButton.SetActive(true);
        OSpinButton.SetActive(true);
        OFlipButton.SetActive(true);
        XTriangleButton.SetActive(true);
        XSpinButton.SetActive(true);
        XFlipButton.SetActive(true);
        XCanvas.SetActive(false);
        triangleHoldingCanvas.SetActive(false);
        continueCanvas.SetActive(false);

        currentTurn = Player.O;
        winner = Player.Neither;
        clickedCube = null;
        isOTriangleUsed = false;
        isXTriangleUsed = false;
        isOSpinUsed = false;
        isXSpinUsed = false;
        isOFlipUsed = false;
        isXFlipUsed = false;

        for (int x = 0; x < 3; x++) { // 清除舊方塊
            for (int y = 0; y < 4; y++) {
                for (int z = 0; z < 3; z++) {
                    if (cubeBoard[x, y, z] != null) {
                        Destroy(cubeBoard[x, y, z]);
                        cubeBoard[x, y, z] = null;
                    }

                    if (cubeSelectBoard[x, y, z] != null) {
                        Destroy(cubeSelectBoard[x, y, z]);
                        cubeSelectBoard[x, y, z] = null;
                    }
                }
            }
        }
        countBoard = new int[3, 4, 3];
        for (int i = 0; i < 3; i++) {
            for (int k = 0; k < 3; k++) {
                countBoard[i, 3, k] = 0;
            }
        }

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                for (int z = 0; z < 3; z++) {
                    GameObject cube = Instantiate(EmptyCube, new Vector3(7 * (x - 1), 7 * (y - 1), 7 * (z - 1)), Quaternion.identity);
                    cube.SetActive(true);// 先把3x3建出來
                    cubeBoard[x, y, z] = cube; // 存入 cubeBoard 陣列
                    cube.name = $"({x}, {y}, {z})EmptyCube";
                }
            }
        }
    }
    #endregion

    public GameObject OCanvas, XCanvas, endScene, OWinText, XWinText, checkBox;
    public GameObject OTriangleButton, XTriangleButton, OSpinButton, XSpinButton, OFlipButton, XFlipButton;
    public GameObject OCube, XCube, TriangleCube, SelectEmptyCube, SelectOCube, SelectXCube, SelectTriangleCube;
    void Update() {
        switch (currentTurn) {
            case Player.Neither:
                // cursorManager.cursorState = ScriptForCursor.CursorState.Default;
                checkBox.SetActive(false);
                OCanvas.SetActive(false);
                XCanvas.SetActive(false);
                triangleHoldingCanvas.SetActive(false);
                continueCanvas.SetActive(false);
                endScene.SetActive(true);

                ((winner == Player.O) ? XWinText : OWinText).SetActive(false);
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    buttonManager.SwitchSceneTo(0);
                } else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                    ResetScene();
                }
                break;
            case Player.Checking:
                // cursorManager.cursorState = ScriptForCursor.CursorState.Default;
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    CancelSurrender();
                } else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                    SurrenderSkill();
                }
                break;
            default:
                if (currentTurn == Player.O) {
                    // cursorManager.cursorState = ScriptForCursor.CursorState.O;
                } else {
                    // cursorManager.cursorState = ScriptForCursor.CursorState.X;
                }
                IsNumberPress();
                CheckIfNextTurn();
                break;
        }
    }

    #region IsNumberPress()系列
    private int currentNumber = 0;
    void IsNumberPress() {
        foreach (KeyCode key in new KeyCode[] {
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,

            KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5,
            KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Keypad0,}) {

            if (Input.GetKeyDown(key)) {
                //切換數字時重置狀態
                DestroyAllSelect();
                currentNumber = 0;
                AllCubeActive();
                switch (key) {
                    case KeyCode.Alpha1:
                    case KeyCode.Keypad1:
                        currentNumber = 1;
                        SetRowActive(0);
                        break;
                    case KeyCode.Alpha2:
                    case KeyCode.Keypad2:
                        currentNumber = 2;
                        SetRowActive(1);
                        break;
                    case KeyCode.Alpha3:
                    case KeyCode.Keypad3:
                        currentNumber = 3;
                        SetRowActive(2);
                        break;
                    case KeyCode.Alpha4:
                    case KeyCode.Keypad4:
                        currentNumber = 4;
                        SetColumnActive(0);
                        break;
                    case KeyCode.Alpha5:
                    case KeyCode.Keypad5:
                        currentNumber = 5;
                        SetColumnActive(1);
                        break;
                    case KeyCode.Alpha6:
                    case KeyCode.Keypad6:
                        currentNumber = 6;
                        SetColumnActive(2);
                        break;
                    case KeyCode.Alpha7:
                    case KeyCode.Keypad7:
                        currentNumber = 7;
                        SetLayerActive(0);
                        break;
                    case KeyCode.Alpha8:
                    case KeyCode.Keypad8:
                        currentNumber = 8;
                        SetLayerActive(1);
                        break;
                    case KeyCode.Alpha9:
                    case KeyCode.Keypad9:
                        currentNumber = 9;
                        SetLayerActive(2);
                        break;
                }
                SelectCubeInstantiate(clickedCube?.name);
            }
        }
    }

    //設定只激活一行
    void SetRowActive(int x) {
        for (int z = 0; z < 3; z++) {
            for (int y = 0; y < 3; y++) {
                cubeBoard[(x + 1) % 3, y, z].SetActive(false);
                cubeBoard[(x + 2) % 3, y, z].SetActive(false);
            }
            if (cubeBoard[(x + 1) % 3, 3, z] != null) {
                cubeBoard[(x + 1) % 3, 3, z].SetActive(false);
            }
            if (cubeBoard[(x + 2) % 3, 3, z] != null) {
                cubeBoard[(x + 2) % 3, 3, z].SetActive(false);
            }
        }
    }

    //設定只激活一列
    void SetColumnActive(int z) {
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                cubeBoard[x, y, (z + 1) % 3].SetActive(false);
                cubeBoard[x, y, (z + 2) % 3].SetActive(false);
            }
            if (cubeBoard[x, 3, (z + 1) % 3] != null) {
                cubeBoard[x, 3, (z + 1) % 3].SetActive(false);
            }
            if (cubeBoard[x, 3, (z + 2) % 3] != null) {
                cubeBoard[x, 3, (z + 2) % 3].SetActive(false);
            }
        }
    }

    //設定只激活一層
    void SetLayerActive(int y) {
        for (int x = 0; x < 3; x++) {
            for (int z = 0; z < 3; z++) {
                cubeBoard[x, (y + 1) % 3, z].SetActive(false);
                cubeBoard[x, (y + 2) % 3, z].SetActive(false);
                if (cubeBoard[x, 3, z] != null) {
                    cubeBoard[x, 3, z].SetActive(false);
                }
            }
        }
    }

    //激活所有方塊
    void AllCubeActive() {
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                for (int z = 0; z < 3; z++) {
                    cubeBoard[x, y, z].SetActive(true);

                }
            }
        }
        for (int x = 0; x < 3; x++) {
            for (int z = 0; z < 3; z++) {
                if (cubeBoard[x, 3, z] != null) {
                    cubeBoard[x, 3, z].SetActive(true);
                }
            }

        }
    }
    #endregion

    void DestroyAllSelect() {
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 4; y++) {
                for (int z = 0; z < 3; z++) {
                    if (cubeSelectBoard[x, y, z] != null) {
                        Destroy(cubeSelectBoard[x, y, z]);
                        cubeSelectBoard[x, y, z] = null;
                        cubeBoard[x, y, z].SetActive(true);
                    }
                }
            }
        }
    }

    void CheckIfNextTurn() {
        if (SomeoneHasWin()) {
            currentTurn = Player.Neither;
        } else if (IsMoveComplete()) {
            clickedCube = null;
            currentNumber = 0;
            DestroyAllSelect();
            AllCubeActive();
            if (currentTurn == Player.O) {
                currentTurn = Player.X;
                OCanvas.SetActive(false);
                XCanvas.SetActive(true);
            } else {
                currentTurn = Player.O;
                OCanvas.SetActive(true);
                XCanvas.SetActive(false);
            }
        }
    }

    #region SomeonesHasWin()系列
    public bool SomeoneHasWin() {
        int vs = 0;
        // 檢查每層、列、行以及對角線
        for (int i = 0; i < 3; i++) {
            vs += CheckLayer(i); // 檢查每一層
            vs += CheckColumn(i); // 檢查每一列
            vs += CheckRow(i); // 檢查每一行
        }

        // 檢查 3D 對角線
        vs += Check3DDiagonals();

        if (vs != 0) {
            winner = (vs > 0) ? Player.O : Player.X;
            return true;
        } else {
            return false;
        }
    }

    // 檢查某一層是否有玩家獲勝
    int CheckLayer(int layer) {
        int vs = 0;
        // 檢查每一層的行和列
        for (int i = 0; i < 3; i++) {
            // 檢查行
            if (Mathf.Abs(countBoard[i, layer, 0] + countBoard[i, layer, 1] + countBoard[i, layer, 2]) == 3) {
                vs += countBoard[i, layer, 0];
            }

            // 檢查列
            if (Mathf.Abs(countBoard[0, layer, i] + countBoard[1, layer, i] + countBoard[2, layer, i]) == 3) {
                vs += countBoard[0, layer, i];
            }
        }

        // 檢查對角線
        if (Mathf.Abs(countBoard[0, layer, 0] + countBoard[1, layer, 1] + countBoard[2, layer, 2]) == 3) {
            vs += countBoard[0, layer, 0];
        }

        if (Mathf.Abs(countBoard[0, layer, 2] + countBoard[1, layer, 1] + countBoard[2, layer, 0]) == 3) {
            vs += countBoard[0, layer, 2];
        }
        return vs;
    }

    // 檢查縱向的列是否有玩家獲勝
    int CheckColumn(int col) {
        int vs = 0;
        // 檢查每一列的行和列
        for (int i = 0; i < 3; i++) {
            // 檢查行
            if (Mathf.Abs(countBoard[i, 0, col] + countBoard[i, 1, col] + countBoard[i, 2, col]) == 3) {
                vs += countBoard[i, 0, col];
            }

            // 檢查列
            if (Mathf.Abs(countBoard[0, i, col] + countBoard[1, i, col] + countBoard[2, i, col]) == 3) {
                vs += countBoard[0, i, col];
            }

            // 特殊第四層
            if (Mathf.Abs(countBoard[i, 1, col] + countBoard[i, 2, col] + countBoard[i, 3, col]) == 3) {
                vs += countBoard[1, i, col];
            }
        }

        // 檢查對角線
        if (Mathf.Abs(countBoard[0, 0, col] + countBoard[1, 1, col] + countBoard[2, 2, col]) == 3) {
            vs += countBoard[0, 0, col];
        }

        if (Mathf.Abs(countBoard[0, 1, col] + countBoard[1, 2, col] + countBoard[2, 3, col]) == 3) {
            vs += countBoard[0, 0, col];
        }

        if (Mathf.Abs(countBoard[0, 2, col] + countBoard[1, 1, col] + countBoard[2, 0, col]) == 3) {
            vs += countBoard[0, 2, col];
        }


        if (Mathf.Abs(countBoard[2, 1, col] + countBoard[1, 2, col] + countBoard[0, 3, col]) == 3) {
            vs += countBoard[1, 2, col];
        }

        return vs;
    }

    // 檢查橫向的行是否有玩家獲勝
    int CheckRow(int row) {
        int vs = 0;
        // 檢查每一行的行和列
        for (int i = 0; i < 3; i++) {
            // 檢查行
            if (Mathf.Abs(countBoard[row, i, 0] + countBoard[row, i, 1] + countBoard[row, i, 2]) == 3) {
                vs += countBoard[row, i, 0];
            }

            // 檢查列
            if (Mathf.Abs(countBoard[row, 0, i] + countBoard[row, 1, i] + countBoard[row, 2, i]) == 3) {
                vs += countBoard[row, 0, i];
            }
            if (Mathf.Abs(countBoard[row, 1, i] + countBoard[row, 2, i] + countBoard[row, 3, i]) == 3) {
                vs += countBoard[row, 1, i];
            }
        }

        // 檢查對角線
        if (Mathf.Abs(countBoard[row, 0, 0] + countBoard[row, 1, 1] + countBoard[row, 2, 2]) == 3) {
            vs += countBoard[row, 0, 0];
        }
        if (Mathf.Abs(countBoard[row, 1, 0] + countBoard[row, 2, 1] + countBoard[row, 3, 2]) == 3) {
            vs += countBoard[row, 0, 0];
        }

        if (Mathf.Abs(countBoard[row, 0, 2] + countBoard[row, 1, 1] + countBoard[row, 2, 0]) == 3) {
            vs += countBoard[row, 0, 2];
        }
        if (Mathf.Abs(countBoard[row, 1, 2] + countBoard[row, 2, 1] + countBoard[row, 3, 0]) == 3) {
            vs += countBoard[row, 1, 2];
        }

        return vs;
    }

    // 檢查 3D 對角線是否有玩家獲勝
    int Check3DDiagonals() {
        int vs = 0;
        if (Mathf.Abs(countBoard[0, 0, 0] + countBoard[1, 1, 1] + countBoard[2, 2, 2]) == 3) {
            vs += countBoard[0, 0, 0];
        }
        if (Mathf.Abs(countBoard[0, 0, 2] + countBoard[1, 1, 1] + countBoard[2, 2, 0]) == 3) {
            vs += countBoard[0, 0, 2];
        }
        if (Mathf.Abs(countBoard[0, 2, 0] + countBoard[1, 1, 1] + countBoard[2, 0, 2]) == 3) {
            vs += countBoard[0, 2, 0];
        }
        if (Mathf.Abs(countBoard[2, 0, 0] + countBoard[1, 1, 1] + countBoard[0, 2, 2]) == 3) {
            vs += countBoard[2, 0, 0];
        }
        if (Mathf.Abs(countBoard[0, 1, 0] + countBoard[1, 2, 1] + countBoard[2, 3, 2]) == 3) {
            vs += countBoard[0, 1, 0];
        }
        if (Mathf.Abs(countBoard[0, 1, 2] + countBoard[1, 2, 1] + countBoard[2, 3, 0]) == 3) {
            vs += countBoard[0, 1, 2];
        }
        if (Mathf.Abs(countBoard[2, 1, 0] + countBoard[1, 2, 1] + countBoard[0, 3, 2]) == 3) {
            vs += countBoard[2, 1, 0];
        }
        if (Mathf.Abs(countBoard[0, 3, 0] + countBoard[1, 2, 1] + countBoard[2, 1, 2]) == 3) {
            vs += countBoard[0, 3, 0];
        }

        return vs;
    }

    #endregion

    #region IsMoveComplete()系列
    private GameObject[,,] cubeSelectBoard = new GameObject[5, 5, 5];
    private GameObject clickedCube = null;
    public GameObject errorSkillCanvas, triangleHoldingCanvas, continueCanvas;
    private bool isHoldingTriangle = false, isSpinning = false, isFlipping = false;
    private bool isOTriangleUsed, isXTriangleUsed, isOSpinUsed = false, isXSpinUsed = false, isOFlipUsed, isXFlipUsed;
    bool IsMoveComplete() {
        // 判斷滑鼠點擊Cube
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                GameObject clickedObject = hit.collider.gameObject;
                //點擊時讓原本被點過的SelectCube消失
                if (!clickedObject.name.Contains("Select") && clickedObject.name.Contains("Cube")) {
                    clickedCube = clickedObject;
                }
                DestroyAllSelect();
                SelectCubeInstantiate(clickedCube?.name);

            }
        }
        foreach (KeyCode key in new KeyCode[] {
            KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.Q,

            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow,

            KeyCode.Escape, KeyCode.Return, KeyCode.KeypadEnter
        }) {
            if (Input.GetKeyDown(key)) {
                switch (key) {
                    case KeyCode.W:
                        TrianglePress();
                        isFlipping = false;
                        return false;
                    case KeyCode.A:
                        SpinPress();
                        isFlipping = false;
                        if (isHoldingTriangle) {
                            isHoldingTriangle = false;
                            triangleHoldingCanvas.SetActive(false);
                        }
                        if ((currentTurn == Player.O && isOFlipUsed == true) || (currentTurn == Player.X && isXFlipUsed == false)) {
                            if (errorInputCanvas.activeSelf) {
                                errorInputCanvas.SetActive(false);
                                errorSkillCanvas.SetActive(true);
                                StartCoroutine(DelayedSetNotActive(errorSkillCanvas, 1.5f));
                            }
                            return false;
                        }

                        return false;
                    case KeyCode.S:
                        FlipPress();
                        return false;
                    case KeyCode.Q:
                        CheckIfSurrender();
                        return false;

                    case KeyCode.UpArrow:
                        if (isSpinning && (isOSpinUsed == false || isXSpinUsed == false)) {
                            Spin("UpArrow");
                            return true;
                        }
                        return InputOX(true, clickedCube?.name);

                    case KeyCode.DownArrow:
                        if (isSpinning && (isOSpinUsed == false || isXSpinUsed == false)) {
                            Spin("DownArrow");
                            return true;
                        }
                        return InputOX(false, clickedCube?.name);
                    case KeyCode.RightArrow:
                        if (isSpinning && (isOSpinUsed == false || isXSpinUsed == false)) {
                            Spin("RightArrow");
                            return true;
                        }
                        return false;
                    case KeyCode.LeftArrow:
                        if (isSpinning && (isOSpinUsed == false || isXSpinUsed == false)) {
                            Spin("LeftArrow");
                            return true;
                        }
                        return false;
                    case KeyCode.Escape:
                        if (isHoldingTriangle) {
                            isHoldingTriangle = false;
                            triangleHoldingCanvas.SetActive(false);
                        }
                        if (isSpinning) {
                            isSpinning = false;
                            // Canvas.SetActive(false);
                        }
                        if (isFlipping) {
                            isFlipping = false;
                            continueCanvas.SetActive(false);
                        }
                        clickedCube = null;
                        DestroyAllSelect();
                        return false;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (isFlipping) {
                            Flip();
                            isFlipping = false;
                            continueCanvas.SetActive(false);
                            return true;
                        }
                        return false;
                }
            }
        }
        return false;
    }

    void Spin(string spinDirection) {
        int[,,] tempCountBoard = new int[3, 3, 3];
        GameObject[,,] tempCubeBoard = new GameObject[3, 3, 3];

        if (spinDirection == "LeftArrow") {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    for (int k = 0; k < 3; k++) {
                        if (i == 0) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i, j + 2, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j + 2, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i + 1, j + 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 1, j + 1, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i + 2, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 2, j, k];
                            }
                        } else if (i == 1) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i - 1, j + 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 1, j + 1, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i + 1, j - 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 1, j - 1, k];
                            }
                        } else if (i == 2) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i - 2, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 2, j, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i - 1, j - 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 1, j - 1, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i, j - 2, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j - 2, k];
                            }
                        }
                    }
                }
            }
            if (currentTurn == Player.O) {
                isOSpinUsed = true;
                OSpinButton.SetActive(false);
            } else {
                isXSpinUsed = true;
                XSpinButton.SetActive(false);
            }
        } else if (spinDirection == "RightArrow") {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    for (int k = 0; k < 3; k++) {
                        if (i == 0) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i + 2, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 2, j, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i + 1, j - 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 1, j - 1, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i, j - 2, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j - 2, k];
                            }
                        } else if (i == 1) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i + 1, j + 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i + 1, j + 1, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i - 1, j - 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 1, j - 1, k];
                            }
                        } else if (i == 2) {
                            if (j == 0) {
                                tempCountBoard[i, j, k] = countBoard[i, j + 2, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i, j + 2, k];
                            } else if (j == 1) {
                                tempCountBoard[i, j, k] = countBoard[i - 1, j + 1, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 1, j + 1, k];
                            } else if (j == 2) {
                                tempCountBoard[i, j, k] = countBoard[i - 2, j, k];
                                tempCubeBoard[i, j, k] = cubeBoard[i - 2, j, k];
                            }
                        }
                    }
                }
            }
            if (currentTurn == Player.O) {
                isOSpinUsed = true;
                OSpinButton.SetActive(false);
            } else {
                isXSpinUsed = true;
                XSpinButton.SetActive(false);
            }
        } else if (spinDirection == "UpArrow") {
            for (int i = 0; i < 3; i++) {

                tempCountBoard[i, 0, 0] = countBoard[i, 2, 0];
                tempCubeBoard[i, 0, 0] = cubeBoard[i, 2, 0];

                tempCountBoard[i, 0, 1] = countBoard[i, 1, 0];
                tempCubeBoard[i, 0, 1] = cubeBoard[i, 1, 0];

                tempCountBoard[i, 0, 2] = countBoard[i, 0, 0];
                tempCubeBoard[i, 0, 2] = cubeBoard[i, 0, 0];

                tempCountBoard[i, 1, 0] = countBoard[i, 2, 1];
                tempCubeBoard[i, 1, 0] = cubeBoard[i, 2, 1];

                tempCountBoard[i, 1, 1] = countBoard[i, 1, 1];
                tempCubeBoard[i, 1, 1] = cubeBoard[i, 1, 1];

                tempCountBoard[i, 1, 2] = countBoard[i, 0, 1];
                tempCubeBoard[i, 1, 2] = cubeBoard[i, 0, 1];

                tempCountBoard[i, 2, 0] = countBoard[i, 2, 2];
                tempCubeBoard[i, 2, 0] = cubeBoard[i, 2, 2];

                tempCountBoard[i, 2, 1] = countBoard[i, 1, 2];
                tempCubeBoard[i, 2, 1] = cubeBoard[i, 1, 2];

                tempCountBoard[i, 2, 2] = countBoard[i, 0, 2];
                tempCubeBoard[i, 2, 2] = cubeBoard[i, 0, 2];
            }



            if (currentTurn == Player.O) {
                isOSpinUsed = true;
                OSpinButton.SetActive(false);
            } else {
                isXSpinUsed = true;
                XSpinButton.SetActive(false);
            }
        } else if (spinDirection == "DownArrow") {
            for (int i = 0; i < 3; i++) {
                tempCountBoard[i, 0, 0] = countBoard[i, 0, 2];
                tempCubeBoard[i, 0, 0] = cubeBoard[i, 0, 2];

                tempCountBoard[i, 0, 1] = countBoard[i, 1, 2];
                tempCubeBoard[i, 0, 1] = cubeBoard[i, 1, 2];
                tempCountBoard[i, 0, 2] = countBoard[i, 2, 2];
                tempCubeBoard[i, 0, 2] = cubeBoard[i, 2, 2];

                tempCountBoard[i, 1, 0] = countBoard[i, 0, 1];
                tempCubeBoard[i, 1, 0] = cubeBoard[i, 0, 1];

                tempCountBoard[i, 1, 1] = countBoard[i, 1, 1];
                tempCubeBoard[i, 1, 1] = cubeBoard[i, 1, 1];

                tempCountBoard[i, 1, 2] = countBoard[i, 2, 1];
                tempCubeBoard[i, 1, 2] = cubeBoard[i, 2, 1];

                tempCountBoard[i, 2, 0] = countBoard[i, 0, 0];
                tempCubeBoard[i, 2, 0] = cubeBoard[i, 0, 0];

                tempCountBoard[i, 2, 1] = countBoard[i, 1, 0];
                tempCubeBoard[i, 2, 1] = cubeBoard[i, 1, 0];

                tempCountBoard[i, 2, 2] = countBoard[i, 2, 0];
                tempCubeBoard[i, 2, 2] = cubeBoard[i, 2, 0];
            }

            if (currentTurn == Player.O) {
                isOSpinUsed = true;
                OSpinButton.SetActive(false);
            } else {
                isXSpinUsed = true;
                XSpinButton.SetActive(false);
            }
        }
        int[,,] temptempCountBoard = new int[3, 3, 3];
        int count = 0;
        GameObject[,,] temptempCubeBoard = new GameObject[3, 3, 3];

        for (int i = 0; i < 3; i++) {
            for (int k = 0; k < 3; k++) {
                for (int j = 0; j < 3; j++) {
                    if (tempCountBoard[i, j, k] != 0) {
                        temptempCountBoard[i, count, k] = tempCountBoard[i, j, k];
                        tempCountBoard[i, j, k] = 0;
                        temptempCubeBoard[i, count, k] = tempCubeBoard[i, j, k];
                        tempCubeBoard[i, j, k] = null;
                        count++;
                    }
                }
                count = 0;
            }
        }

        //for (int i = 0; i < 3; i++) {
        //    for (int k = 0; k < 3; k++) {
        //        if (tempCountBoard[i, 0, k] == 0 && tempCountBoard[i, 1, k] != 0) {
        //            tempCountBoard[i, 0, k] = tempCountBoard[i, 1, k];
        //            tempCountBoard[i, 1, k] = 0;
        //            tempCubeBoard[i, 0, k] = tempCubeBoard[i, 1, k];
        //            tempCubeBoard[i, 1, k] = null;
        //        }
        //        if (tempCountBoard[i, 1, k] == 0 && tempCountBoard[i, 2, k] != 0) {
        //            tempCountBoard[i, 1, k] = tempCountBoard[i, 2, k];
        //            tempCountBoard[i, 2, k] = 0;
        //            tempCubeBoard[i, 1, k] = tempCubeBoard[i, 2, k];
        //            tempCubeBoard[i, 2, k] = null;
        //        }
        //    }
        //}
        DestroyAllSelect();

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    Destroy(cubeBoard[i, j, k]);
                    cubeBoard[i, j, k] = temptempCubeBoard[i, j, k];
                    temptempCubeBoard[i, j, k] = null;
                    countBoard[i, j, k] = temptempCountBoard[i, j, k];
                    temptempCountBoard[i, j, k] = 0;
                    InstantiateCube(i, j, k, countBoard[i, j, k]);
                }
            }
        }

        for (int i = 0; i < 3; i++) {
            for (int j = 2; j >= 0; j--) {
                for (int k = 0; k < 3; k++) {
                    if (countBoard[i, j, k] == 0 && countBoard[i, j + 1, k] != 0) {
                        tempCountBoard[i, j, k] = tempCountBoard[i, j + 1, k];
                        tempCubeBoard[i, j, k] = tempCubeBoard[i, j + 1, k];
                    }
                }
            }
        }

        AllCubeActive();
        isSpinning = false;
    }

    void Flip() {
        DestroyAllSelect();
        for (int i = 0; i < 3; i++) {
            for (int k = 0; k < 3; k++) {
                int[] tempCountBoard = new int[3] { 0, 0, 0 };
                int index = 0;
                if (countBoard[i, 0, k] == 10) {
                    if (countBoard[i, 3, k] != 0) {
                        Destroy(cubeBoard[i, 3, k]);
                        countBoard[i, 3, k] = 0;
                    }
                    countBoard[i, 2, k] = 0;
                    if (countBoard[i, 1, k] != 10) {
                        countBoard[i, 1, k] = 0;
                    }
                    for (int j = 1; j >= 0; j--) {
                        if (countBoard[i, j, k] == 10) {
                            tempCountBoard[index] = countBoard[i, j, k];
                            index++;
                        }
                    }
                } else {
                    for (int j = 3; j >= 0 && index < 3; j--) {
                        if (countBoard[i, j, k] != 0) {
                            tempCountBoard[index] = countBoard[i, j, k];
                            index++;
                        }
                    }
                    if (countBoard[i, 3, k] != 0) {
                        Destroy(cubeBoard[i, 3, k]);
                        countBoard[i, 3, k] = 0;
                    }
                }
                Destroy(cubeBoard[i, 2, k]);
                InstantiateCube(i, 2, k, tempCountBoard[2]);
                Destroy(cubeBoard[i, 1, k]);
                InstantiateCube(i, 1, k, tempCountBoard[1]);
                Destroy(cubeBoard[i, 0, k]);
                InstantiateCube(i, 0, k, tempCountBoard[0]);
            }
        }
        AllCubeActive();
    }

    void InstantiateCube(int i, int j, int k, int cubeType) {
        Vector3 position = new Vector3(7 * (i - 1), 7 * (j - 1), 7 * (k - 1));
        countBoard[i, j, k] = cubeType;
        if (cubeType == 1) {
            cubeBoard[i, j, k] = Instantiate(OCube, position, Quaternion.identity);
            cubeBoard[i, j, k].name = $"({i}, {j}, {k})OCube";
        } else if (cubeType == -1) {
            cubeBoard[i, j, k] = Instantiate(XCube, position, Quaternion.identity);
            cubeBoard[i, j, k].name = $"({i}, {j}, {k})XCube";
        } else if (cubeType == 10) {
            cubeBoard[i, j, k] = Instantiate(TriangleCube, position, Quaternion.identity);
            cubeBoard[i, j, k].name = $"({i}, {j}, {k})TriangleCube";
        } else {
            cubeBoard[i, j, k] = Instantiate(EmptyCube, position, Quaternion.identity);
            cubeBoard[i, j, k].name = $"({i}, {j}, {k})EmptyCube";
        }
    }
    #endregion

    #region Button系列
    public void TrianglePress() {
        if (isFlipping) {
            isFlipping = false;
            continueCanvas.SetActive(false);
        }
        if (isSpinning) {
            isSpinning = false;
            // Canvas.SetActive(false);
        }
        if ((currentTurn == Player.O && isOTriangleUsed == false) || (currentTurn == Player.X && isXTriangleUsed == false)) {
            isHoldingTriangle = true;
            triangleHoldingCanvas.SetActive(true);
        } else {
            if (errorInputCanvas.activeSelf) {
                errorInputCanvas.SetActive(false);
            }
            errorSkillCanvas.SetActive(true);
            StartCoroutine(DelayedSetNotActive(errorSkillCanvas, 1.5f));
        }
    }

    private void SpinPress() {
        if (isHoldingTriangle) {
            isHoldingTriangle = false;
            triangleHoldingCanvas.SetActive(false);
        }
        if (isFlipping) {
            isFlipping = false;
            continueCanvas.SetActive(false);
        }
        if ((currentTurn == Player.O && isOSpinUsed == false) || (currentTurn == Player.X && isXSpinUsed == false)) {
            isSpinning = true;
            // Canvas.SetActive(true);
        } else {
            if (errorInputCanvas.activeSelf) {
                errorInputCanvas.SetActive(false);
            }
            errorSkillCanvas.SetActive(true);
            StartCoroutine(DelayedSetNotActive(errorSkillCanvas, 1.5f));
        }
    }

    public void FlipPress() {
        if (isHoldingTriangle) {
            isHoldingTriangle = false;
            triangleHoldingCanvas.SetActive(false);
        }
        if (isSpinning) {
            isSpinning = false;
            // Canvas.SetActive(false);
        }
        if ((currentTurn == Player.O && isOFlipUsed == false) || (currentTurn == Player.X && isXFlipUsed == false)) {
            isFlipping = true;
            continueCanvas.SetActive(true);
        } else {
            if (errorInputCanvas.activeSelf) {
                errorInputCanvas.SetActive(false);
            }
            errorSkillCanvas.SetActive(true);
            StartCoroutine(DelayedSetNotActive(errorSkillCanvas, 1.5f));
        }
    }

    Player nowTurn;
    public void SurrenderSkill() {
        if (nowTurn == Player.O) {
            winner = Player.X;
        } else {
            winner = Player.O;
        }
        currentTurn = Player.Neither;
    }

    public void CheckIfSurrender() {
        if (isHoldingTriangle) {
            isHoldingTriangle = false;
            triangleHoldingCanvas.SetActive(false);
        }
        if (isSpinning) {
            isSpinning = false;
            // Canvas.SetActive(false);
        }
        if (isFlipping) {
            isFlipping = false;
            continueCanvas.SetActive(false);
        }
        nowTurn = currentTurn;
        currentTurn = Player.Checking;
        checkBox.SetActive(true);
    }
    public void CancelSurrender() {
        currentTurn = nowTurn;
        checkBox.SetActive(false);
    }
    #endregion

    #region inputOX()系列
    public GameObject errorInputCanvas;
    // 輸入O或X
    bool InputOX(bool pressUpArrow, string clickedCubeName) { // true: up, false: down
        //非點選方塊時無效
        if (clickedCubeName == null || !clickedCubeName.Contains("Cube")) {
            return false;
        }
        int emptyCubeCount = 0;
        int i = clickedCubeName[1] - '0', k = clickedCubeName[7] - '0';
        for (int y = 0; y < 3; y++) {
            if (countBoard[i, y, k] == 0) {
                emptyCubeCount++;
            }
        }
        if (emptyCubeCount == 0 && !isHoldingTriangle) {
            if (errorSkillCanvas.activeSelf) {
                errorSkillCanvas.SetActive(false);
            }
            errorInputCanvas.SetActive(true);
            StartCoroutine(DelayedSetNotActive(errorInputCanvas, 1.5f));
            return false;
        }
        if (pressUpArrow) {
            if (isHoldingTriangle) {
                if (countBoard[i, 3, k] != 0) {
                    Destroy(cubeBoard[i, 3, k]);
                    countBoard[i, 3, k] = 0;
                }

                Destroy(cubeBoard[i, 2, k]);
                cubeBoard[i, 2, k] = Instantiate(EmptyCube, new Vector3(7 * (i - 1), 7, 7 * (k - 1)), Quaternion.identity);
                cubeBoard[i, 2, k].name = $"({i}, 2, {k})EmptyCube";
                countBoard[i, 2, k] = 0;


                Destroy(cubeBoard[i, 1, k]);
                if (countBoard[i, 0, k] == 10) {
                    cubeBoard[i, 1, k] = Instantiate(TriangleCube, new Vector3(7 * (i - 1), 0, 7 * (k - 1)), Quaternion.identity);
                    cubeBoard[i, 1, k].name = $"({i}, 1, {k})TriangleCube";
                    countBoard[i, 1, k] = 10;
                } else {
                    cubeBoard[i, 1, k] = Instantiate(EmptyCube, new Vector3(7 * (i - 1), 0, 7 * (k - 1)), Quaternion.identity);
                    cubeBoard[i, 1, k].name = $"({i}, 1, {k})EmptyCube";
                    countBoard[i, 1, k] = 0;

                    Destroy(cubeBoard[i, 0, k]);
                    cubeBoard[i, 0, k] = Instantiate(TriangleCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                    cubeBoard[i, 0, k].name = $"({i}, 0, {k})TriangleCube";
                    countBoard[i, 0, k] = 10;
                }

            } else {
                if (countBoard[i, 1, k] != 0) {
                    Destroy(cubeBoard[i, 2, k]);
                    if (currentTurn == Player.O) {
                        cubeBoard[i, 2, k] = Instantiate(OCube, new Vector3(7 * (i - 1), 7, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 2, k].name = $"({i}, 2, {k})OCube";
                        countBoard[i, 2, k] = 1;
                    } else if (currentTurn == Player.X) {
                        cubeBoard[i, 2, k] = Instantiate(XCube, new Vector3(7 * (i - 1), 7, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 2, k].name = $"({i}, 2, {k})XCube";
                        countBoard[i, 2, k] = -1;
                    }
                } else if (countBoard[i, 0, k] != 0) {
                    Destroy(cubeBoard[i, 1, k]);
                    if (currentTurn == Player.O) {
                        cubeBoard[i, 1, k] = Instantiate(OCube, new Vector3(7 * (i - 1), 0, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 1, k].name = $"({i}, 1, {k})OCube";
                        countBoard[i, 1, k] = 1;
                    } else if (currentTurn == Player.X) {
                        cubeBoard[i, 1, k] = Instantiate(XCube, new Vector3(7 * (i - 1), 0, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 1, k].name = $"({i}, 1, {k})XCube";
                        countBoard[i, 1, k] = -1;
                    }
                } else {
                    Destroy(cubeBoard[i, 0, k]);
                    if (currentTurn == Player.O) {
                        cubeBoard[i, 0, k] = Instantiate(OCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 0, k].name = $"({i}, 0, {k})OCube";
                        countBoard[i, 0, k] = 1;
                    } else if (currentTurn == Player.X) {
                        cubeBoard[i, 0, k] = Instantiate(XCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                        cubeBoard[i, 0, k].name = $"({i}, 0, {k})XCube";
                        countBoard[i, 0, k] = -1;
                    }
                }
            }


        } else {
            if (cubeBoard[i, 0, k].name.Contains("Triangle")) {
                if (errorSkillCanvas.activeSelf) {
                    errorSkillCanvas.SetActive(false);
                }
                errorInputCanvas.SetActive(true);
                StartCoroutine(DelayedSetNotActive(errorInputCanvas, 1.5f));
                return false;
            }

            if (isHoldingTriangle && countBoard[i, 2, k] != 0) {
                cubeBoard[i, 3, k] = Instantiate(cubeBoard[i, 2, k], new Vector3(7 * (i - 1), 14, 7 * (k - 1)), Quaternion.identity);
                cubeBoard[i, 3, k].name = NameCube(i, 3, k, countBoard[i, 2, k]);
                countBoard[i, 3, k] = countBoard[i, 2, k];
            }

            Destroy(cubeBoard[i, 2, k]);
            cubeBoard[i, 2, k] = Instantiate(cubeBoard[i, 1, k], new Vector3(7 * (i - 1), 7, 7 * (k - 1)), Quaternion.identity);
            cubeBoard[i, 2, k].name = NameCube(i, 2, k, countBoard[i, 1, k]);
            countBoard[i, 2, k] = countBoard[i, 1, k];

            Destroy(cubeBoard[i, 1, k]);
            cubeBoard[i, 1, k] = Instantiate(cubeBoard[i, 0, k], new Vector3(7 * (i - 1), 0, 7 * (k - 1)), Quaternion.identity);
            cubeBoard[i, 1, k].name = NameCube(i, 1, k, countBoard[i, 0, k]);
            countBoard[i, 1, k] = countBoard[i, 0, k];

            Destroy(cubeBoard[i, 0, k]);
            if (isHoldingTriangle) {
                cubeBoard[i, 0, k] = Instantiate(TriangleCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                cubeBoard[i, 0, k].name = $"({i}, 0, {k})TriangleCube";
                countBoard[i, 0, k] = 10;
            } else {
                if (currentTurn == Player.O) {
                    cubeBoard[i, 0, k] = Instantiate(OCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                    cubeBoard[i, 0, k].name = $"({i}, 0, {k})OCube";
                    countBoard[i, 0, k] = 1;
                } else if (currentTurn == Player.X) {
                    cubeBoard[i, 0, k] = Instantiate(XCube, new Vector3(7 * (i - 1), -7, 7 * (k - 1)), Quaternion.identity);
                    cubeBoard[i, 0, k].name = $"({i}, 0, {k})XCube";
                    countBoard[i, 0, k] = -1;
                }
            }
        }
        if (isHoldingTriangle) {
            isHoldingTriangle = false;
            triangleHoldingCanvas.SetActive(false);
            if (currentTurn == Player.O) {
                isOTriangleUsed = true;
                OTriangleButton.SetActive(false);
            } else {
                isXTriangleUsed = true;
                XTriangleButton.SetActive(false);
            }
        }
        return true;
    }

    bool stop = false;
    IEnumerator DelayedSetNotActive(GameObject canvas, float delayTime) {
        if (delayTime < 0) {
            stop = true;
            while (stop) {
                yield return new WaitForSeconds(1f);
            }
        }
        yield return new WaitForSeconds(delayTime);  // 延遲指定的時間
        canvas.SetActive(false);
    }
    string NameCube(int i, int j, int k, int cubeType) {
        if (cubeType == 1) {
            return $"({i}, {j}, {k})OCube";
        } else if (cubeType == -1) {
            return $"({i}, {j}, {k})XCube";
        } else {
            return $"({i}, {j}, {k})EmptyCube";
        }

    }

    #endregion

    #region SelectCubeInstantiate()系列
    //設定cube根據點擊激活變為selectCube
    void SelectCubeInstantiate(string clickedCubeName) {
        if (clickedCubeName == null) {
            return;
        }
        int i = clickedCubeName[1] - '0', k = clickedCubeName[7] - '0';
        int tempNumber;
        //int emptyCubeCount = 0;
        // for (int y = 0; y < 4; y++) {
        //     if (countBoard[i, y, k] == 0) {
        //         emptyCubeCount++;
        //     }
        // }
        // if (emptyCubeCount == 0) {
        //     return;
        // }
        switch (currentNumber) {
            case 1:
            case 2:
            case 3:
                tempNumber = currentNumber - 1;
                if (clickedCubeName.Contains($"({tempNumber}") && clickedCubeName.Contains($"{k})")) {
                    for (int j = 0; j < 3; j++) {
                        InstantiateSelectCube(tempNumber, j, k, countBoard[tempNumber, j, k]);
                    }
                    if (cubeBoard[tempNumber, 3, k] != null) {
                        InstantiateSelectCube(tempNumber, 3, k, countBoard[tempNumber, 3, k]);
                    }
                }
                break;

            case 4:
            case 5:
            case 6:
                tempNumber = currentNumber - 4;
                if (clickedCubeName.Contains($"({i}") && clickedCubeName.Contains($"{tempNumber})")) {
                    for (int j = 0; j < 3; j++) {
                        InstantiateSelectCube(i, j, tempNumber, countBoard[i, j, tempNumber]);
                    }
                    if (cubeBoard[i, 3, tempNumber] != null) {
                        InstantiateSelectCube(i, 3, tempNumber, countBoard[i, 3, tempNumber]);
                    }
                }
                break;

            case 7:
            case 8:
            case 9:
                tempNumber = currentNumber - 7;
                InstantiateSelectCube(i, tempNumber, k, countBoard[i, tempNumber, k]);
                break;

            case 0:
                for (int j = 0; j < 3; j++) {
                    InstantiateSelectCube(i, j, k, countBoard[i, j, k]);

                }
                if (cubeBoard[i, 3, k] != null) {
                    InstantiateSelectCube(i, 3, k, countBoard[i, 3, k]);
                }
                break;
        }
    }

    void InstantiateSelectCube(int i, int j, int k, int cubeType) {
        GameObject tempSelectCube;
        Vector3 position = new Vector3(7 * (i - 1), 7 * (j - 1), 7 * (k - 1));
        if (cubeType == 1) {
            tempSelectCube = Instantiate(SelectOCube, position, Quaternion.identity);
        } else if (cubeType == -1) {
            tempSelectCube = Instantiate(SelectXCube, position, Quaternion.identity);
        } else if (cubeType == 10) {
            tempSelectCube = Instantiate(SelectTriangleCube, position, Quaternion.identity);
        } else {
            tempSelectCube = Instantiate(SelectEmptyCube, position, Quaternion.identity);
        }

        tempSelectCube.SetActive(true);
        cubeBoard[i, j, k].SetActive(false);
        cubeSelectBoard[i, j, k] = tempSelectCube;

        if (cubeType == 1) {
            cubeSelectBoard[i, j, k].name = $"({i}, {j}, {k})SelectOCube";
        } else if (cubeType == -1) {
            cubeSelectBoard[i, j, k].name = $"({i}, {j}, {k})SelectXCube";
        } else if (cubeType == 10) {
            cubeSelectBoard[i, j, k].name = $"({i}, {j}, {k})SelectTriangleCube";
        } else {
            cubeSelectBoard[i, j, k].name = $"({i}, {j}, {k})SelectEmptyCube";
        }
    }
    #endregion
}