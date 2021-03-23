Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim _origen As String
        Dim _filePath As String
        Dim _fileName As String
        Dim _destPath As String
        Dim _fileReader As System.IO.StreamReader
        Dim _fileReaderB As System.IO.StreamReader
        Dim _lenPath As Integer
        Dim _headerPath As String
        Dim _contentPath As String
        Dim i As Integer
        Dim _headerLen(75) As String
        Dim _textLen(330000) As String
        Dim Piv As Boolean
        Dim _documentoFinal(100000) As String
        Dim _contAnimacion As Integer

        _origen = My.Computer.FileSystem.CurrentDirectory
        TextBox2.Text = _origen
        OpenFileDialog1.Filter = "HST Files | *.hst"
        ProgressBar1.Value = 0

        If (OpenFileDialog1.ShowDialog() = DialogResult.OK) Then
            _filePath = OpenFileDialog1.FileName                'obtener  el path nombre del archivo seleccionado 
            _fileName = System.IO.Path.GetFileName(_filePath)   'obtener la carpeta el nombre del archivo seleccionado
            _fileName = "\" + _fileName
            _destPath = _origen + _fileName                     'Path completo del destino con nombre 
            Button1.Enabled = False

            Dim d As Boolean = copiar(_filePath, _destPath, True)
            'My.Computer.FileSystem.CopyFile(_filePath, _destPath, True)
            TextBox1.Text = _filePath

            Dim retval As Long = Shell("cmd.exe /c hst2txt " + _destPath, AppWinStyle.NormalFocus, True) 'Ejecucion de shell sincrono

            _lenPath = Len(_destPath)
            _headerPath = Mid(_destPath, 1, _lenPath - 3) + "hdr"
            _contentPath = Mid(_destPath, 1, _lenPath - 3) + "txt"

            'For _contAnimacion = 0 To 30 Step 1
            'System.Threading.Thread.Sleep(50)
            'ProgressBar1.Value = _contAnimacion
            'ProgressBar1.Refresh()
            'System.Threading.Thread.Sleep(50)
            'Next
            actualizarBarra(0, 50)
            System.Threading.Thread.Sleep(500)

            _fileReader = My.Computer.FileSystem.OpenTextFileReader(_headerPath)
            _fileReaderB = My.Computer.FileSystem.OpenTextFileReader(_contentPath)
            Dim _stringReader As String
            Dim _stringReaderB As String

            Piv = True
            i = 0

            'Lectura del archivo header
            While Piv
                _stringReader = _fileReader.ReadLine()
                If _stringReader = Nothing Then
                    Piv = False
                End If

                _headerLen(i) = _stringReader
                i = i + 1

            End While
            i = 0
            Piv = True
            _fileReader.Close()

            'Lectura del archivo de texto
            While Piv
                _stringReaderB = _fileReaderB.ReadLine()
                If _stringReaderB = Nothing Then
                    Piv = False

                End If
                _textLen(i) = _stringReaderB
                i = i + 1

            End While

            _fileReaderB.Close()

            'Creacion de una matriz con los datos del archivo final
            _documentoFinal = CrearArchivo(_headerLen, _textLen)
            actualizarBarra(50, 80)

            Dim archivo As System.IO.StreamWriter
            archivo = My.Computer.FileSystem.OpenTextFileWriter(Mid(_destPath, 1, _lenPath - 3) + "csv", True)

            i = 0
            Piv = True

            'Escritura del archivo final
            While Piv
                If _documentoFinal(i) Is Nothing Then
                    Piv = False
                Else
                    archivo.WriteLine(_documentoFinal(i))
                    i = i + 1
                End If
            End While

            archivo.Close()
            actualizarBarra(80, 100)
            System.IO.File.Delete(Mid(_destPath, 1, _lenPath - 3) + "txt")
            System.IO.File.Delete(Mid(_destPath, 1, _lenPath - 3) + "hdr")
            System.IO.File.Delete(Mid(_destPath, 1, _lenPath - 3) + "hst")
            Beep()

            Button1.Enabled = True
            System.Threading.Thread.Sleep(500)
            ProgressBar1.Value = 0
        End If

    End Sub
    Public Function CrearArchivo(ByRef header() As String, ByRef body() As String) As String()
        Dim Encabezado As String = Nothing
        Dim piv As Boolean = True
        Dim cont As Integer = 1
        Dim cont2 As Integer = 0
        Dim i As Integer = 1
        Dim pivDocumento(330000) As String
        Dim Documento(100000) As String
        Dim selector As Char

        'Creacion de matriz con los datos del archivo final 

        'Transposicion de la columna del archivo header 

        While piv
            If header(cont) = Nothing Then
                piv = False
            Else
                Encabezado = Encabezado + header(cont) + vbTab
                cont = cont + 1
            End If
        End While
        ' Seleccion del tiempo de muestreo 
        If RadioButton1.Checked Then
            selector = "H"
        ElseIf RadioButton2.Checked Then
            selector = "M"
        ElseIf RadioButton3.Checked Then
            selector = "S"
        End If

        pivDocumento(0) = Encabezado

        'Se copia el total de los registros del archivo de texto a la matriz pivote de salida
        piv = True
        cont = 0
        While piv

            If body(cont) = Nothing Then
                piv = False
            Else

                pivDocumento(cont + 1) = body(cont)
                cont = cont + 1
            End If
        End While

        piv = True

        cont = 1
        cont2 = 1

        'Agrega titulos de columna faltantes 

        Documento(0) = "Fecha" + vbTab + "Hora" + vbTab + pivDocumento(0)
        ' Si el registro de hora es el mismo  que el registro anterioren segundos, minutos u horas se desecha
        'Si es diferente se guarda en la matriz pivote
        While piv = True
            If pivDocumento(cont2) = Nothing Then
                piv = False
            Else
                If ObtenerHora(Documento(cont - 1), selector) = ObtenerHora(pivDocumento(cont2), selector) Then
                    cont2 = cont2 + 1
                Else
                    Documento(cont) = pivDocumento(cont2)
                    cont = cont + 1
                    cont2 = cont2 + 1


                End If
            End If

        End While
        Return Documento

    End Function

    Public Function ObtenerHora(ByVal a As String, sel As Char) As String

        'Retorna la hora del registro como un string 
        Dim hora As String

        Select Case sel
            Case "H"
                hora = Mid(a, 12, 2)
            Case "M"
                hora = Mid(a, 12, 5)
            Case "S"
                hora = Mid(a, 12, 8)
            Case Else
                hora = "Error"

        End Select

        Return hora

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RadioButton3.Checked = True
    End Sub
    Private Function copiar(ByVal origen As String, destino As String, sobreE As Boolean) As Boolean
        System.IO.File.Copy(origen, destino, sobreE)

        Return True
    End Function

    Public Sub actualizarBarra(inicial As Integer, final As Integer)
        ProgressBar1.Step = 1
        For _contAnimacion = inicial To final Step 1
            System.Threading.Thread.Sleep(50)
            ProgressBar1.PerformStep()

            ProgressBar1.Refresh()
            System.Threading.Thread.Sleep(50)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Application.Exit()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

        Dim _pathCarpeta As String

        _pathCarpeta = TextBox2.Text

        If _pathCarpeta <> Nothing Then

            Try
                Shell("explorer.exe " + _pathCarpeta, vbNormalFocus)

            Catch ex As Exception
                MsgBox(e)

            End Try
        Else
            MsgBox("No hay directorio seleccionado")
        End If

    End Sub
End Class
