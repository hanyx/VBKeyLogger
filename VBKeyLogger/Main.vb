Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Module Main
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
    End Function

    Private UCase As Boolean = False
    Private WithEvents Logger As New KeyboardHook(GetModuleHandle(Nothing))

    Sub Main()
        Process.GetCurrentProcess.WaitForExit()
    End Sub

    Private Sub Logger_KeyDown(Key As Keys) Handles Logger.KeyDown
        If key = Keys.LShiftKey Or key = Keys.RShiftKey Then UCase = True
        Console.WriteLine("DOWN: " + If(UCase, Key.ToString, LCase(Key.ToString)))
    End Sub

    Private Sub Logger_KeyUp(Key As Keys) Handles Logger.KeyUp
        If key = Keys.LShiftKey Or key = Keys.RShiftKey Then UCase = False
        Console.WriteLine("  UP: " + If(UCase, Key.ToString, LCase(Key.ToString)))
    End Sub
End Module

Public Class KeyboardHook

    <DllImport("User32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Overloads Shared Function SetWindowsHookEx(ByVal idHook As Integer, ByVal HookProc As KBDLLHookProc, ByVal hInstance As IntPtr, ByVal wParam As Integer) As Integer
    End Function
    <DllImport("User32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Private Overloads Shared Function CallNextHookEx(ByVal idHook As Integer, ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer
    End Function
    <DllImport("User32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Private Overloads Shared Function UnhookWindowsHookEx(ByVal idHook As Integer) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure KBDLLHOOKSTRUCT
        Public vkCode As UInt32
        Public scanCode As UInt32
        Public flags As KBDLLHOOKSTRUCTFlags
        Public time As UInt32
        Public dwExtraInfo As UIntPtr
    End Structure

    <Flags()>
    Private Enum KBDLLHOOKSTRUCTFlags As UInt32
        LLKHF_EXTENDED = &H1
        LLKHF_INJECTED = &H10
        LLKHF_ALTDOWN = &H20
        LLKHF_UP = &H80
    End Enum

    Public Shared Event KeyDown(ByVal Key As Keys)
    Public Shared Event KeyUp(ByVal Key As Keys)

    Private Const WH_KEYBOARD_LL As Integer = 13
    Private Const HC_ACTION As Integer = 0
    Private Const WM_KEYDOWN = &H100
    Private Const WM_KEYUP = &H101
    Private Const WM_SYSKEYDOWN = &H104
    Private Const WM_SYSKEYUP = &H105

    Private Delegate Function KBDLLHookProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer

    Private KBDLLHookProcDelegate As KBDLLHookProc = New KBDLLHookProc(AddressOf KeyboardProc)
    Private HHookID As IntPtr = IntPtr.Zero

    Private Function KeyboardProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer
        If (nCode = HC_ACTION) Then
            Dim struct As KBDLLHOOKSTRUCT
            Select Case wParam
                Case WM_KEYDOWN, WM_SYSKEYDOWN
                    RaiseEvent KeyDown(CType(CType(Marshal.PtrToStructure(lParam, struct.GetType()), KBDLLHOOKSTRUCT).vkCode, Keys))
                Case WM_KEYUP, WM_SYSKEYUP
                    RaiseEvent KeyUp(CType(CType(Marshal.PtrToStructure(lParam, struct.GetType()), KBDLLHOOKSTRUCT).vkCode, Keys))
            End Select
        End If
        Return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)
    End Function

    Public Sub New(ModuleHandle As IntPtr)
        HHookID = SetWindowsHookEx(WH_KEYBOARD_LL, KBDLLHookProcDelegate, ModuleHandle, 0)
        If HHookID = IntPtr.Zero Then
            Throw New Exception("Could not set keyboard hook - " + New System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error).Message)
        End If
    End Sub

    Protected Overrides Sub Finalize()
        If Not HHookID = IntPtr.Zero Then
            UnhookWindowsHookEx(HHookID)
        End If
        MyBase.Finalize()
    End Sub

End Class