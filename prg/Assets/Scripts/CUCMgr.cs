using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public class CUCMgr
{
    const int RAND_MAX = 10000;
    const int RAND_MIN = 0;
    const int WM_USER = 0x0400;
    const int WM_CLOSE = 0x0010;
    const int WM_KEYDOWN = 0x0100;
    const int WM_KEYUP = 0x0101;
    const int VK_UP = 0x26;
    const int VK_DOWN = 0x28;
    const int VK_RETURN = 0x0d;
    const int VK_ESCAPE = 0x1b;
    const int VK_SPACE = 0x20;
    const int VK_RIGHT = 0x27;
    const int VK_LEFT = 0x25;
    const int VK_F1 = 0x70;
    const int VK_F2 = 0x71;
    const int VK_F3 = 0x72;
    const int VK_F4 = 0x73;
    const int VK_F5 = 0x74;
    const int VK_F6 = 0x75;
    const int VK_F7 = 0x76;
    const int FRAME_INTERVAL = 60;
    const int SW_MINIMIZE = 6;
    const int GWL_STYLE = -16;
    const uint WS_VISIBLE = 0x10000000;
    int frameCnt = 0;
    int pushColor = 0;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern System.IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    public static CUCMgr instance;

    private HandleRef hMainWindow;
    private IntPtr oldWndProcPtr;
    private IntPtr newWndProcPtr;
    private WndProcDelegate newWndProc;

    private static readonly string CUC_FolderPath = "C:\\minpachi\\MinPachi_Data\\StreamingAssets\\tool\\";
    private static readonly string CUC_FilePath = CUC_FolderPath + "CUC.exe";
    private PachiSlot pachiSlot;
    private IntPtr hWnd_CUC;
    private Process proc;

    public static CUCMgr GetInstance()
    {
        if (instance == null)
        {
            instance = new CUCMgr();
        }
        return instance;
    }
    CUCMgr()
    {
        InitWindowProc();
        StartExe(CUC_FilePath);
    }
    ~CUCMgr()
    {
        TermWindowProc();
    }
    public void Update()
    {
        if (pachiSlot == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.PUSHBUTTON, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.PUSHBUTTON, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_RIGHT, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_RIGHT, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_LEFT, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_LEFT, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_UP, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_UP, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_DOWN, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_DOWN, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.MAXBET, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F1))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.MAXBET, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEVER, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F2))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEVER, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEFTBUTTON, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F3))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEFTBUTTON, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.CENTERBUTTON, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F4))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.CENTERBUTTON, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.RIGHTBUTTON, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F5))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.RIGHTBUTTON, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.OUT, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F6))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.OUT, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ONEBET, (int)CONTROLLER_SWITCH.ON);
        }
        else if (Input.GetKeyUp(KeyCode.F7))
        {
            if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ONEBET, (int)CONTROLLER_SWITCH.OFF);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnApplicationQuit();
        }
#if !UNITY_EDITOR
        if (hWnd_CUC == IntPtr.Zero && ++frameCnt >= FRAME_INTERVAL)
        {
            frameCnt = 0;
            hWnd_CUC = ProcessControl("CUC", PROCESS_CONTROL.GETHANDLE);
        }
#endif

    }
    public IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
        {
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }
    }
    private IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_USER)
        {
            if (pachiSlot != null)pachiSlot.InputCommand(wParam.ToInt32(), lParam.ToInt32());
        }
        else if (msg == WM_KEYDOWN)
        {
            if (wParam.ToInt32() == VK_SPACE)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.PUSHBUTTON, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_RIGHT)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_RIGHT, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_LEFT)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_LEFT, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_UP)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_UP, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_DOWN)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_DOWN, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F1)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.MAXBET, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F2)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEVER, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F3)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEFTBUTTON, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F4)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.CENTERBUTTON, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F5)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.RIGHTBUTTON, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F6)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.OUT, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_F7)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ONEBET, (int)CONTROLLER_SWITCH.ON);
            }
            else if (wParam.ToInt32() == VK_ESCAPE)
            {
                OnApplicationQuit();
            }
        }
        else if (msg == WM_KEYUP)
        {
            if (wParam.ToInt32() == VK_SPACE)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.PUSHBUTTON, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_RIGHT)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_RIGHT, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_LEFT)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_LEFT, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_UP)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_UP, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_DOWN)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ARROW_DOWN, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F1)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.MAXBET, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F2)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEVER, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F3)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.LEFTBUTTON, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F4)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.CENTERBUTTON, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F5)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.RIGHTBUTTON, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F6)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.OUT, (int)CONTROLLER_SWITCH.OFF);
            }
            else if (wParam.ToInt32() == VK_F7)
            {
                if (pachiSlot != null) pachiSlot.InputCommand((int)CONTROLLER.ONEBET, (int)CONTROLLER_SWITCH.OFF);
            }
        }
        else if (msg == WM_CLOSE)
        {
            OnApplicationQuit();
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void InitWindowProc()
    {
        // ウインドウプロシージャをフックする
        hMainWindow = new HandleRef(null, GetActiveWindow());
        newWndProc = new WndProcDelegate(wndProc);
        newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
        oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);
    }

    public void TermWindowProc()
    {
        SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
        hMainWindow = new HandleRef(null, IntPtr.Zero);
        oldWndProcPtr = IntPtr.Zero;
        newWndProcPtr = IntPtr.Zero;
        newWndProc = null;
    }
    public void OnApplicationQuit()
    {
        ProcessControl("CUC", PROCESS_CONTROL.KILL);
        TermWindowProc();
        Application.Quit();
    }

    public void SetObject(object obj)
    {
        if (obj is PachiSlot pachiObj)
        {
            pachiSlot = pachiObj;
        }
    }
    public void ControllerRequest(CONTROLLER_TYPE type, CONTROLLER_DEVICE dev, CONTROLLER_SWITCH sw)
    {
        IntPtr wParam;
        IntPtr lParam;

        if (hWnd_CUC == IntPtr.Zero)
        {
            hWnd_CUC = ProcessControl("CUC", PROCESS_CONTROL.GETHANDLE);
        }

        if (hWnd_CUC != IntPtr.Zero)
        {
            switch (type)
            {

                case CONTROLLER_TYPE.VIBE_ONE:
                    wParam = new IntPtr(2);
                    lParam = new IntPtr(0);
                    break;
                case CONTROLLER_TYPE.VIBE_REN:
                    wParam = new IntPtr(3);
                    lParam = new IntPtr(1);
                    break;
                case CONTROLLER_TYPE.COLOR:
                    int mask = (int)dev;
                    if (sw == CONTROLLER_SWITCH.ON)
                    {
                        pushColor |= mask;
                    }
                    else
                    {
                        pushColor &= ~mask;
                    }
                    wParam = new IntPtr(1);
                    lParam = new IntPtr(pushColor);
                    break;
                default:
                    wParam = new IntPtr(0);
                    lParam = new IntPtr(0);
                    break;

            }
            SendMessage(hWnd_CUC, WM_USER, wParam, lParam);
        }
    }
    public IntPtr ProcessControl(string processName, PROCESS_CONTROL control)
    {
        IntPtr windowHandle = IntPtr.Zero;
        Process[] processes = Process.GetProcessesByName(processName);

        EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            foreach (Process proc in processes)
            {
                if (proc.Id == processId)
                {
                    if (control == PROCESS_CONTROL.GETHANDLE)
                    {
                        windowHandle = hWnd;
                        return false; // ウィンドウが見つかったら列挙を停止
                    }
                    else if (control == PROCESS_CONTROL.KILL)
                    {
                        proc.Kill();
                        return false;
                    }
                    else if (control == PROCESS_CONTROL.MINIMIZE)
                    {
                        long style = GetWindowLong(hWnd, GWL_STYLE);
                        if ((style & WS_VISIBLE) != 0)
                        {
                            ShowWindow(hWnd, SW_MINIMIZE);
                        }
                    }
                }
            }

            return true; // 列挙を続ける
        }, IntPtr.Zero);

        return windowHandle;
    }
    private void StartExe(string name)
    {
#if !UNITY_EDITOR
        ProcessControl("CUC", PROCESS_CONTROL.KILL);
        proc = new Process();
        proc.StartInfo.FileName = name;
        proc.Start();
#endif
    }
}