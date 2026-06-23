Imports System.IO
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Text
Imports System.Configuration
Imports System.Security.Cryptography
Public Class Form1
    Dim cn As Integer
    Dim StartPath As String = Application.StartupPath
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Label9.Text = "In Progress"
        Chart1.Series.Clear()
        BackgroundWorker1.WorkerSupportsCancellation = True
        BackgroundWorker1.WorkerReportsProgress = True
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim Dt, Dx, MaxT, temp1, temp10, temp2, temp20, temp3, dgausx, dgausy, temp4, Prob, cageDx, cagesize As Single
        Dim x As Single()
        'Dim deltaGauss()
        'Dim rand0()
        'Dim rand1()
        Dim xsc As Single()
        Dim y As Single()
        Dim xcage As Single()
        Dim MaxI, nfiles As Integer
        Dim i As Integer = 0
        Dim m As Integer = 0
        Dim Cagepos(1) As Single
        'Randomize()
        Dim rng As New Xoshiro256StarStarRandom()
        Dt = TextBox1.Text
        Dx = TextBox2.Text
        cageDx = TextBox7.Text
        MaxT = TextBox3.Text
        MaxI = CInt(MaxT / Dt)
        Prob = TextBox5.Text
        nfiles = CInt(TextBox6.Text)
        ReDim Preserve x(MaxI)
        ReDim Preserve y(MaxI)
        ReDim Preserve xsc(MaxI)
        ReDim Preserve xcage(MaxI)
        ' ReDim Preserve deltaGauss(MaxI)
        'ReDim Preserve rand0(MaxI)
        'ReDim Preserve rand1(MaxI)
        cagesize = 2 * TextBox4.Text
        cn = 0
        For cn = 0 To nfiles - 1
            x(0) = 0
            xsc(0) = 0
            y(0) = 0
            i = 1
            m = 1
            Cagepos(0) = -TextBox4.Text
            Cagepos(1) = TextBox4.Text
            xcage(0) = Cagepos(1)
            Do While i < MaxI - 1
                'temp1 = Rnd()
                temp1 = rng.NextDouble()
                'temp10 = Rnd()
                temp10 = rng.NextDouble()
                If temp1 = 0 Then
                    temp1 = rng.NextDouble()
                End If
                If temp10 = 0 Then
                    temp10 = rng.NextDouble()
                End If
                dgausx = Math.Sqrt(-2 * Math.Log10(temp1)) * Math.Cos(2 * Math.PI * temp10)
                ' deltaGauss(i) = dgausx
                ' rand0(i) = temp1
                'rand1(i) = temp10
                'temp2 = Rnd()
                temp2 = rng.NextDouble()
                'temp20 = Rnd()
                temp20 = rng.NextDouble()
                If temp2 = 0 Then
                    temp2 = rng.NextDouble()
                End If
                If temp20 = 0 Then
                    temp20 = rng.NextDouble()
                End If
                dgausy = Math.Sqrt(-2 * Math.Log10(temp2)) * Math.Cos(2 * Math.PI * temp20)
                temp4 = 2 * Rnd() - 1
                Cagepos(0) = Cagepos(0) + temp4 * cageDx
                Cagepos(1) = Cagepos(1) + temp4 * cageDx
                xcage(i) = Cagepos(1)
                If xsc(i - 1) >= Cagepos(1) Then
                    x(i - 1) = x(i - 1) - (xsc(i - 1) - Cagepos(1))
                    xsc(i - 1) = Cagepos(1)

                End If
                If xsc(i - 1) <= Cagepos(0) Then
                    x(i - 1) = x(i - 1) + (Cagepos(0) - xsc(i - 1))
                    xsc(i - 1) = Cagepos(0)

                End If
                x(i) = x(i - 1) + dgausx * Dx
                xsc(i) = xsc(i - 1) + dgausx * Dx
                If xsc(i) >= Cagepos(1) Then
                    temp3 = Rnd()
                    If temp3 >= Prob Then
                        x(i) = x(i - 1) - dgausx * Dx
                        xsc(i) = xsc(i - 1) - dgausx * Dx
                    End If
                    If temp3 < Prob Then
                        xsc(i) = xsc(i) - cagesize
                    End If
                End If
                If xsc(i) <= Cagepos(0) Then
                    temp3 = Rnd()
                    If temp3 >= Prob Then
                        x(i) = x(i - 1) - dgausx * Dx
                        xsc(i) = xsc(i - 1) - dgausx * Dx
                    End If
                    If temp3 < Prob Then
                        xsc(i) = xsc(i) + cagesize
                    End If
                End If
                'If x(i) > Cagepos(1) Or xsc(i) < Cagepos(0) Then
                'Label9.Text = "Error"
                'End If
                y(i) = y(i - 1) + dgausy * Dx
                i = i + 1
                m = m + 1
            Loop
            Dim filen As Integer = Label7.Text
            Dim writer1 As New StreamWriter(StartPath & "\tray\tray" & cn & ".csv")
            writer1.WriteLine("Frame" & "," & "x" & "," & "y" & "," & "xsinglecell" & "," & "xcage")
            For j = 0 To MaxI - 2
                'writer1.WriteLine(j & "," & x(j) & "," & y(j) & "," & xsc(j) & "," & xcage(j))
                writer1.WriteLine(j & "," & x(j) & "," & y(j) & "," & xsc(j) & "," & xcage(j)) '& "," & deltaGauss(j) & "," & rand0(j) & "," & rand1(j))
            Next
            writer1.Close()
            'Label7.Text = filen + 1
        Next
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Label7.Text = cn
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If e.Error IsNot Nothing Then
            '' if BackgroundWorker terminated due to error
            Label9.Text = "Error occurred!"
        ElseIf e.Cancelled Then
            '' otherwise if it was cancelled
            Label9.Text = "Cancelled!"
        Else
            '' otherwise it completed normally
            Label9.Text = "Completed!"
        End If
        Chart1.Series.Add("x")
        'Chart1.Series.Add("y")
        Chart1.Series.Add("xcage")
        Chart1.Series("x").ChartType = SeriesChartType.Point
        Chart1.Series("xcage").ChartType = SeriesChartType.Point
        'Chart1.Series("y").ChartType = SeriesChartType.Point
        Dim line As String
        Dim temp As String()
        Dim reader As New StreamReader(StartPath & "\tray\tray" & CInt(TextBox6.Text) - 1 & ".csv")
        line = reader.ReadLine()
        Dim j As Integer = 0
        Do While reader.Peek() <> -1
            line = reader.ReadLine()
            temp = line.Split(",")
            Chart1.Series("x").Points.AddXY(j * TextBox1.Text, temp(1))
            'Chart1.Series("y").Points.AddXY(j * TextBox1.Text, temp(2))
            Chart1.Series("xcage").Points.AddXY(j * TextBox1.Text, temp(4))
            j = j + 1
        Loop
        Label7.Text = 0
        reader.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        BackgroundWorker1.CancelAsync()
    End Sub
End Class
