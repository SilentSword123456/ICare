using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ICare;

public class Keyboard : IDisposable
{
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int HOTKEY_ID = 1;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_CONTROL = 0x0002;
    private const uint VK_Q = 0x51;

    private IntPtr _hookId = IntPtr.Zero;
    private IntPtr _windowHandle;
    private LowLevelKeyboardProc _proc;
    private Action _onSkip;
    private bool _blockInput = false;

    public Keyboard()
    {
        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        _hookId = SetWindowsHookEx(13, _proc, GetModuleHandle(curModule.ModuleName!), 0);
    }

    public void StartBlocking() => _blockInput = true;
    public void StopBlocking() => _blockInput = false;

    public void RegisterSkipHotkey(IntPtr windowHandle, Action onSkip)
    {
        _windowHandle = windowHandle;
        _onSkip = onSkip;
        RegisterHotKey(windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_Q);
    }

    public void StartListening(IntPtr handle)
    {
        var source = HwndSource.FromHwnd(handle);
        source.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            _onSkip();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _blockInput)
            return (IntPtr)1;

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookId);
        UnregisterHotKey(_windowHandle, HOTKEY_ID);
    }
}