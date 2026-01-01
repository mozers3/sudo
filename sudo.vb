Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Security.Principal
Imports System.Reflection

<Assembly: AssemblyTitle("SUDO for Windows")>
<Assembly: AssemblyCompany("mozers ™")>
<Assembly: AssemblyProduct("SUDO for Windows")>
<Assembly: AssemblyCopyright("https://github.com/mozers3/sudo")>
<Assembly: AssemblyFileVersion("1.0.1")>
<Assembly: AssemblyVersion("1.0.1.0")>

Module Sudo
	Sub Main(args() As String)
		If args.Length = 0 Then
			Console.Error.WriteLine(vbCrLf & "SUDO for Windows v1.0.1 <https://github.com/mozers3/sudo>" & vbCrLf & vbCrLf & _ 
			"Usage: sudo [-n] <command> [args...]" & vbCrLf & _ 
			" -n, --nowait       Starts the command and does not wait for it to complete. Use to launch GUI applications." & vbCrLf & vbCrLf & _ 
			"Examples:" & vbCrLf & _ 
			" sudo whoami /groups" & vbCrLf & _ 
			" sudo -n notepad %windir%\system32\drivers\etc\hosts" & vbCrLf & _ 
			" sudo bcdedit" & vbCrLf & _ 
			" sudo net session" & vbCrLf & _ 
			" sudo net user hacker P@ss123 /add" & vbCrLf & _ 
			" sudo cmd /c ""exit /b %random%"" (and then, ""echo %errorlevel%"")")
			Environment.Exit(1)
		End If

		Dim waitForExit As Boolean = True
		Dim realArgs As New System.Collections.ArrayList()
		Dim realArgsArray() As String = DirectCast(realArgs.ToArray(GetType(String)), String())

		If Not ParseArgs(args, waitForExit, realArgsArray) Then
			Console.Error.WriteLine("Error: no command specified")
			Environment.Exit(1)
		End If

		Dim wi As WindowsIdentity = WindowsIdentity.GetCurrent()
		Dim wp As New WindowsPrincipal(wi)
		Dim isAdmin As Boolean = wp.IsInRole(WindowsBuiltInRole.Administrator)
		Dim tempPath As String = Environment.ExpandEnvironmentVariables("%TEMP%\$udo_output.tmp")

		If Not isAdmin Then
			' Неповышенный запуск: создаём маркер-файл, затем elevate
			' Создаём пустой маркер, чтобы elevated-копия знала: "меня вызвали"
			File.WriteAllText(tempPath, "")

			Dim psi As New ProcessStartInfo()
			psi.FileName = Process.GetCurrentProcess().MainModule.FileName
			' Передаём ИСХОДНЫЕ args, чтобы elevated-копия тоже могла распарсить --nowait
			psi.Arguments = QuoteAll(args)  ' ← оставляем args, НО elevated должен фильтровать сам
			psi.UseShellExecute = True
			' psi.CreateNoWindow = True ' Игнорируется
			psi.WindowStyle = ProcessWindowStyle.Minimized ' Хотя бы - так работает (не гарантированно)
			psi.Verb = "runas"

			Dim exitCode As Integer = 1
			Try
				Dim proc As Process = Process.Start(psi)
				proc.WaitForExit()
				exitCode = proc.ExitCode
			Catch
				Environment.Exit(1)
			End Try

			If File.Exists(tempPath) Then
				ConsoleWriteColor(File.ReadAllText(tempPath))
				File.Delete(tempPath)
			End If

			Environment.Exit(exitCode)
		Else
			' Уже elevated — нужно повторно распарсить args, чтобы получить waitForExit и realArgs
			Dim exitCode As Integer = 0
			Dim output As String = RunCommand(realArgsArray, exitCode, waitForExit)

			' Если маркер-файл существует — значит, нас вызвали автоматически
			If File.Exists(tempPath) Then
				File.WriteAllText(tempPath, output)
			Else
				' Маркер отсутствует — запущены вручную > вывод в консоль
				ConsoleWriteColor(output)
			End If

			Environment.Exit(exitCode)
		End If
	End Sub

	Sub ConsoleWriteColor(output As String)
		Dim index As Integer = output.IndexOf(Chr(7))
		If index = -1 Then
			Console.Write(output)
		Else
			Console.Write(output.Substring(0, index))
			Dim errors As String = output.Substring(index + 1)
			If errors <> "" Then
				Console.ForegroundColor = ConsoleColor.Red
				Console.Write(errors)
				Console.ResetColor()
			End If
		End If
	End Sub

	Function RunCommand(args() As String, ByRef exitCode As Integer, waitForExit As Boolean) As String
		Dim psi As New ProcessStartInfo()
		psi.FileName = args(0)
		If args.Length > 1 Then
			psi.Arguments = String.Join(" ", args, 1, args.Length - 1)
		End If
		psi.UseShellExecute = False
		psi.RedirectStandardOutput = True
		psi.RedirectStandardError = True
		psi.CreateNoWindow = True

		Dim proc As Process = Process.Start(psi)
		If waitForExit Then
			proc.WaitForExit()
			Dim output As String = proc.StandardOutput.ReadToEnd()
			Dim errors As String = proc.StandardError.ReadToEnd()
			exitCode = proc.ExitCode
			Return output & IIf(errors <> "", Chr(7) & errors, "")
		Else
			' Не ждём, не читаем — иначе зависнем
			proc.Dispose()
			exitCode = 0 ' или оставить как есть — но обычно не важно
			Return ""
		End If
	End Function

	Function QuoteAll(args() As String) As String
		Dim result As String = ""
		For i As Integer = 0 To args.Length - 1
			If i > 0 Then result &= " "
			Dim a As String = args(i)
			If a.IndexOf(" ") >= 0 OrElse a.IndexOf("""") >= 0 OrElse a = "" Then
				a = """" & a.Replace("""", "\""""") & """"
			End If
			result &= a
		Next
		Return result
	End Function

	Private Function ParseArgs(args() As String, ByRef waitForExit As Boolean, ByRef realArgs As String()) As Boolean
		waitForExit = True
		Dim list As New System.Collections.ArrayList()

		For Each arg As String In args
			Dim key As String = arg.TrimStart("-"c, "/"c).ToLowerInvariant()
			If key = "nowait" OrElse key = "n" Then
				waitForExit = False
			Else
				list.Add(arg)
			End If
		Next

		If list.Count = 0 Then
			Return False ' ключ указан, а команду - забыли написать
		End If

		realArgs = DirectCast(list.ToArray(GetType(String)), String())
		Return True
	End Function

End Module