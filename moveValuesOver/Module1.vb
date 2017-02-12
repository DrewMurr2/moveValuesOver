
Imports System.Data.SqlClient
Imports System.Threading

Module Module1
    Private ConnectionString As String = "Server=tcp:roilfirstsqlserver.database.windows.net,1433;Initial Catalog=RoilOperations;Persist Security Info=False;User ID=roilservices;Password=Roil111111;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=90;"
    'Dim SourceName = "Example_Well_1"
    'Dim DestinationName = "Demo_Well_1"
    'Dim OffsetDays As Integer = -180
    Dim Sims As DataTable
    ''Example_Well_1 started on 2016-12-30 11:55:50 and lasted 4 days
    ''Example_Well_2 started on 2017-01-11 11:34:14 and lasted 12 days
    Sub Main()
        Dim Start = Now()
        Dim Endt = Now()

        While 1 = 1
            Dim span = (Endt - Start).TotalSeconds
            Console.WriteLine(Endt.ToString)
            If span > 1 Then
                Console.WriteLine("iteration time in seconds: " = span.ToString)
            End If


            Start = Now()

            Do Until retrieveSims()
            Loop

            CreateTables()


            updateTables()
            Endt = Now()
        End While


    End Sub
    Public Function retrieveSims() As Boolean
        Sims = Nothing
        Dim query = "Select * from SimulationWells"
        Try
            Sims = QueryToDS(query).Tables(0)
            If Sims IsNot Nothing AndAlso Sims.Rows.Count > 0 Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Console.WriteLine("retrieveSims " & vbCrLf & ex.Message)
            Return False
        End Try
    End Function
    Public Sub CreateTables()

        For Each Row In Sims.Rows
            If Not Row("AllTablesCreated") Then
                createTable(Row)
                createTable(Row, "_ii")
                createTable(Row, "_ii_10s")
                createTable(Row, "_ii_1min")
                If createTable(Row, "_ii_1hour") Then
                    Sql("Update SimulationWells Set AllTablesCreated = 1 Where DestinationName = '" & Row("DestinationName") & "'")
                End If
            End If
        Next

    End Sub
    Public Sub updateTables()
        For Each Row In Sims.Rows
            If Row("Status") = "Active" Then
                updateTable(Row)
                updateTable(Row, "_ii")
                updateTable(Row, "_ii_10s")
                updateTable(Row, "_ii_1min")
                updateTable(Row, "_ii_1hour")
            End If
        Next
    End Sub

    Public Function Sql(query As String) As Boolean
        Try
            Using connection As New SqlConnection(ConnectionString)
                Dim command As New SqlCommand(query, connection)
                command.Connection.Open()
                command.ExecuteNonQuery()
                command.Connection.Close()
            End Using
            Return True
        Catch ex As Exception
            Console.WriteLine("Sub: CreateCommand " & vbCrLf & ex.Message)
            Return False
        End Try
    End Function
    Private Function createTable(Row As DataRow, Optional suffix As String = "") As Boolean

        Dim query = ""
        If suffix <> Nothing Then
            query += DeclareMyInterpolatedTable
        Else
            query += DeclareMyRawTable
        End If


        query +=
    "Insert into @MyTempTable
	Select Top 1 *
	From " & Row("SourceName") & suffix & "
	Order By DateTime Asc

	Update @MyTempTable 
Set DateTime = DATEADD(day," & Row("OffsetDays") & ", DateTime)

Select * into " & Row("DestinationName") & suffix & "
From @MyTempTable;

CREATE INDEX index_name_" & Row("DestinationName") & suffix & "
ON " & Row("DestinationName") & suffix & " (DateTime);
"
        Try
            Sql(query)
            Return True
        Catch

        End Try
    End Function

    Private Sub updateTable(Row As DataRow, Optional suffix As String = "")
        Dim query = ""
        If suffix <> Nothing Then
            query += DeclareMyInterpolatedTable
        Else
            query += DeclareMyRawTable
        End If



        query += "

Declare @StartT DateTIme2 
Set @StartT = (
Select top 1 DateTime
From " & Row("DestinationName") & suffix & vbCrLf &
"Order By DateTime Desc
)
Set @StartT =  DateAdd(day," & (Row("OffsetDays") * -1) & ",@StartT)

Declare @nowVar DateTime 
Set @nowVar = DateAdd(day," & (Row("OffsetDays") * -1) & ",GETUTCDATE())

Insert into @MyTempTable
Select top 1000 * from " & Row("SourceName") & suffix & vbCrLf &
"Where DateTime > @StartT
AND DateTime < @nowVar
order by DateTime asc

Update @MyTempTable 
Set DateTime = DATEADD(day," & Row("OffsetDays") & ", DateTime)


Insert into " & Row("DestinationName") & suffix & "
Select "

        If suffix <> Nothing Then

            query += "[DateTime],
	[Hole_Depth_0110],
	[Bit_Depth_0108],
	[Block_Height_0112] ,
	[ROP_0113] ,
	[Hook_Load_0115],
	[WOB_0117],
	[Torque_0119] ,
	[RPM_0120],
	[SPP_0121],
	[Flow_0128],
	[Pump_Rate_0130],
[ID],
	[Interpolated]"
        Else
            query += " * "

        End If

        query += "
from @MyTempTable"


        Sql(query)

    End Sub
    Public Function QueryToDS(query As String) As DataSet
        Try
            Dim SqlDataSet As New DataSet
            Using SQLCon As New SqlConnection With {.ConnectionString = ConnectionString}
                Using sqlCmd = New SqlCommand(query, SQLCon)
                    Using SqlDa As New SqlDataAdapter(sqlCmd)
                        SQLCon.Open()
                        SqlDa.Fill(SqlDataSet)
                        Return SqlDataSet
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("Function: QueryToDS() " & vbCrLf & ex.Message)
        End Try
    End Function


    Dim DeclareMyRawTable = "Declare @MyTempTable Table(
	[DateTime] [datetime2](7) NULL,
	[1984] [varchar](250) NULL,
	[1108] [varchar](250) NULL,
	[1111] [varchar](250) NULL,
	[1115] [varchar](250) NULL,
	[1116] [varchar](250) NULL,
	[1117] [varchar](250) NULL,
	[1118] [varchar](250) NULL,
	[1119] [varchar](250) NULL,
	[1120] [varchar](250) NULL,
	[1121] [varchar](250) NULL,
	[1122] [varchar](250) NULL,
	[1129] [varchar](250) NULL,
	[1212] [varchar](250) NULL,
	[1213] [varchar](250) NULL,
	[1214] [varchar](250) NULL,
	[1215] [varchar](250) NULL,
	[1815] [varchar](250) NULL,
	[1818] [varchar](250) NULL,
	[1819] [varchar](250) NULL,
	[1820] [varchar](250) NULL,
	[1821] [varchar](250) NULL,
	[1827] [varchar](250) NULL,
	[1828] [varchar](250) NULL,
	[1829] [varchar](250) NULL,
	[1830] [varchar](250) NULL,
	[1831] [varchar](250) NULL,
	[6310] [varchar](250) NULL,
	[6311] [varchar](250) NULL,
	[6339] [varchar](250) NULL,
	[6340] [varchar](250) NULL,
	[0105] [varchar](250) NULL,
	[0106] [varchar](250) NULL,
	[0108] [varchar](250) NULL,
	[0110] [varchar](250) NULL,
	[0112] [varchar](250) NULL,
	[0113] [varchar](250) NULL,
	[0115] [varchar](250) NULL,
	[0117] [varchar](250) NULL,
	[0119] [varchar](250) NULL,
	[0120] [varchar](250) NULL,
	[0121] [varchar](250) NULL,
	[0122] [varchar](250) NULL,
	[0123] [varchar](250) NULL,
	[0124] [varchar](250) NULL,
	[0125] [varchar](250) NULL,
	[0126] [varchar](250) NULL,
	[0127] [varchar](250) NULL,
	[0128] [varchar](250) NULL,
	[0130] [varchar](250) NULL,
	[0137] [varchar](250) NULL,
	[0139] [varchar](250) NULL,
	[0140] [varchar](250) NULL,
	[0141] [varchar](250) NULL,
	[0142] [varchar](250) NULL,
	[0143] [varchar](250) NULL,
	[0144] [varchar](250) NULL,
	[0145] [varchar](250) NULL,
	[0169] [varchar](250) NULL,
	[0170] [varchar](250) NULL,
	[0171] [varchar](250) NULL,
	[0172] [varchar](250) NULL,
	[0173] [varchar](250) NULL,
	[0813] [varchar](250) NULL,
	[0816] [varchar](250) NULL,
	[0817] [varchar](250) NULL,
	[0820] [varchar](250) NULL,
	[0821] [varchar](250) NULL,
	[0824] [varchar](250) NULL,
	[0829] [varchar](250) NULL,
	[0831] [varchar](250) NULL,
	[0832] [varchar](250) NULL,
	[0834] [varchar](250) NULL,
	[0839] [varchar](250) NULL,
	[0841] [varchar](250) NULL,
	[0913] [varchar](250) NULL,
	[0208] [varchar](250) NULL,
	[0210] [varchar](250) NULL,
	[0211] [varchar](250) NULL,
	[0212] [varchar](250) NULL,
	[0213] [varchar](250) NULL,
	[0214] [varchar](250) NULL,
	[0215] [varchar](250) NULL,
	[0219] [varchar](250) NULL,
	[0221] [varchar](250) NULL,
	[0222] [varchar](250) NULL,
	[0272] [varchar](250) NULL,
	[0273] [varchar](250) NULL,
	[0708] [varchar](250) NULL,
	[0713] [varchar](250) NULL,
	[0715] [varchar](250) NULL,
	[0716] [varchar](250) NULL,
	[0717] [varchar](250) NULL,
	[0722] [varchar](250) NULL,
	[ID] [int] NOT NULL)

"

    Dim DeclareMyInterpolatedTable = "Declare @MyTempTable Table(
[DateTime] [datetime2](7) NOT NULL,
	[Hole_Depth_0110] [float] NULL,
	[Bit_Depth_0108] [float] NULL,
	[Block_Height_0112] [float] NULL,
	[ROP_0113] [float] NULL,
	[Hook_Load_0115] [float] NULL,
	[WOB_0117] [float] NULL,
	[Torque_0119] [float] NULL,
	[RPM_0120] [float] NULL,
	[SPP_0121] [float] NULL,
	[Flow_0128] [float] NULL,
	[Pump_Rate_0130] [float] NULL,
	[ID] [bigint] NOT NULL,
	[Interpolated] [bit] NOT NULL)"

End Module
