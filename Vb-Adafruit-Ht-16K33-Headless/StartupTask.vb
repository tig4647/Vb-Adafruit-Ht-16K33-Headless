
Imports Windows.ApplicationModel.Background
Imports Windows.Devices.Gpio
Imports Windows.Devices.I2C
Imports Windows.Devices.Enumeration

'Some code used from 

'https://github.com/bechynsky/AdafruitMini8x8LEDMatrixW10IoTCore/blob/master/GPIO101/MainPage.xaml.cs
' Modified to show as a background program as well as drawing objects

Public NotInheritable Class StartupTask
    Implements IBackgroundTask


    Dim deferral As BackgroundTaskDeferral
    Dim timer As Windows.System.Threading.ThreadPoolTimer

    Private Const MatrixSize As Integer = 8
    Private MatrixData As Byte(,) = New Byte(MatrixSize - 1, MatrixSize - 1) {}
    Private MatrixDevice As I2cDevice = Nothing

    Public Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        InitLedMatrixDevice()
        deferral = taskInstance.GetDeferral()


        While True

            For i = 0 To 7
                For c = 0 To 7
                    SetMatrixData(i, c, &H1)
                    System.Threading.Tasks.Task.Delay(200).Wait()
                Next
                System.Threading.Tasks.Task.Delay(200).Wait()
            Next
            System.Threading.Tasks.Task.Delay(500).Wait()
            For i = 7 To 0 Step -1
                For c = 7 To 0 Step -1
                    SetMatrixData(i, c, &H0)
                    System.Threading.Tasks.Task.Delay(200).Wait()
                Next
                System.Threading.Tasks.Task.Delay(200).Wait()
            Next

            ClearDisplay()
            System.Threading.Tasks.Task.Delay(500).Wait()
            WriteX()
            System.Threading.Tasks.Task.Delay(3000).Wait()
            ClearDisplay()
            System.Threading.Tasks.Task.Delay(700).Wait()
            WriteX()
            System.Threading.Tasks.Task.Delay(700).Wait()
            ClearDisplay()
            System.Threading.Tasks.Task.Delay(2000).Wait()


        End While



    End Sub




    Private Sub SetMatrixData(row As Integer, column As Integer, state As Byte)
        ' shift columns in grid to columns on device
        column = (column + 7) And 7
        ' write state to matrix
        MatrixData(row, column) = state

        Dim rowData As Byte = &H0

        ' calculate byte            
        For i As Integer = 0 To MatrixSize - 1
            rowData = rowData Or CByte(MatrixData(row, i) << CByte(i))
        Next

        If MatrixDevice Is Nothing Then
            Return
        End If

        ' write value to display
        MatrixDevice.Write(New Byte() {CByte(row * 2), rowData})
    End Sub
    Private Async Sub InitLedMatrixDevice()
        Try
            ' I2C bus settings
            ' Address of device is 0x70
            Dim settings As New I2cConnectionSettings(&H70)
            ' Using standard speed 100 kHZ
            settings.BusSpeed = I2cBusSpeed.StandardMode

            ' Get device on bus named I2C1
            Dim aqs As String = I2cDevice.GetDeviceSelector("I2C1")
            Dim dis = Await DeviceInformation.FindAllAsync(aqs)
            MatrixDevice = Await I2cDevice.FromIdAsync(dis(0).Id, settings)

            ' diplay initialization
            'this actually sets the oscillator on ( so its 0x20 + 0x01)

            MatrixDevice.Write(New Byte() {&H21})
            MatrixDevice.Write(New Byte() {&H81})


            ' switch all LEDs off
            ClearDisplay()

        Catch
            MatrixDevice = Nothing
        End Try
    End Sub

    Private Sub WriteX()
        SetMatrixData(0, 0, &H1)
        SetMatrixData(1, 1, &H1)
        SetMatrixData(2, 2, &H1)
        SetMatrixData(3, 3, &H1)
        SetMatrixData(4, 4, &H1)
        SetMatrixData(5, 5, &H1)
        SetMatrixData(6, 6, &H1)
        SetMatrixData(7, 7, &H1)

        SetMatrixData(7, 0, &H1)
        SetMatrixData(6, 1, &H1)
        SetMatrixData(5, 2, &H1)
        SetMatrixData(4, 3, &H1)
        SetMatrixData(2, 5, &H1)
        SetMatrixData(3, 4, &H1)
        SetMatrixData(1, 6, &H1)
        SetMatrixData(0, 7, &H1)



    End Sub
    Private Sub ClearDisplay()
        Dim i As Integer = 0
        While i < (MatrixSize * 2)
            MatrixDevice.Write(New Byte() {CByte(i), &H0})
            i = i + 2
        End While


    End Sub
End Class