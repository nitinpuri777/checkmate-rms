Module Module1
    'RMS Needs to provide you with ClientNo, ClientPassword and authorize 'AgentId: 44'.
    Public Const nRMSClientNo As Integer = 1 
    Public Const sClientPassword As String = "" 
    Public Const nAgentId As Integer = 44 'This is Checkmate's Agent ID
    Public Const sAgentPwd As String = "" 'Enter in Checkmate's Agent Password from Integrations Document
    'Api Token is the Admin Token ...@mail.checkmate.io
    Public Const aApiToken As String = "**ENTER CHECKMATE API TOKEN**"
    'Login to security.google.com/settings/security/apppasswords and create a new single use App password and name it for the hotel
    Public Const sEmailAppPassword As String = ""
    'Replace the Hotel Name in the quotes
    Public Const emailFrom = """NAME_OF_HOTEL_GOES_HERE"" <dataworker@checkmate.io>"

    Sub Main()
        Dim nListOfPropertyIds As List(Of Integer) = New List(Of Integer)(1)


        'Filepath for Arrivals Export
        Dim oArrivalsFilePath As String = "Arrivals.txt"
        Dim oInventoryFilePath As String = "Inventory.txt"

        'Get Token
        Dim sToken As String = getToken(nRMSClientNo, sClientPassword, nAgentId, sAgentPwd)
        'Get List of Categories -- for understanding hotel's configuration
        'getCategories(sToken)
        'Get Arrivals
        Dim oArrivalsList As String = getReservationsList(sToken, Now.AddDays(0).Date & " 00:00:00", Now.AddDays(2).Date & " 23:59:59")
        'Write Arrivals
        My.Computer.FileSystem.WriteAllText(oArrivalsFilePath, oArrivalsList, False)
        'Get Inventory
        Dim oInventoryList As String = getInventoryString(sToken, nListOfPropertyIds)
        'Write Inventory
        My.Computer.FileSystem.WriteAllText(oInventoryFilePath, oInventoryList, False)
        'Mail Arrivals
        sendFiles(oArrivalsFilePath, oInventoryFilePath, aApiToken)

    End Sub

    Sub getCategories(oToken As String) ' This is a method just for discovering hotel's settings, should be commented in Main
        Dim oRMSPublic As New RMSPublic.PublicServiceClient
        Dim oListOfCategories As New List(Of RMSPublic.CategoryBasic)

        oListOfCategories = oRMSPublic.GetListOfCategories(oToken, Nothing)

        Dim oListofAreas = oRMSPublic.GetListOfAreas(oToken, Nothing, Nothing)

    End Sub


    Function getToken(oRMSClientNo As Integer, oClientPassword As String, oAgentId As Integer, oAgentPwd As String)
        Dim sToken As String = ""
        Dim oRMSPublic As New RMSPublic.PublicServiceClient
        Dim bTrainingDatabase As Boolean = False
        Try
            'Get Connection String
            sToken = oRMSPublic.GetToken(oRMSClientNo, oClientPassword, oAgentId, oAgentPwd, bTrainingDatabase)
            'Test Property Name
            'MsgBox(oRMSPublic.GetPropertyName(sToken))

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return sToken


    End Function

    Function getReservationsList(sToken As String, oFromArrivalDate As Date, oToArrivalDate As Date)
        Dim oResReq As New RMSPublic.ResRequest
        Dim oRMSPublic As New RMSPublic.PublicServiceClient
        Dim oCsvString As String = ""
        Dim oListOfStatus As List(Of RMSPublic.ResStatus) = New List(Of RMSPublic.ResStatus)(RMSPublic.ResStatus.Confirmed)
        With oResReq
            'Res Optional fields
            oResReq.ResOptionalFieldList = New RMSPublic.OptionalFieldsRes()
            With .ResOptionalFieldList
                .CategoryName = True
                .PropertyId = True
                .PropertyName = True
                .Company = True
                .BookingSource = True
                .DiscountCode = True
                .RMSOnlineConfirmationNo = True
                .CategoryId = True
                .RateCode = True


            End With
            'Client optional fields
            oResReq.ClientOptionalFieldList = New RMSPublic.OptionalFieldsClient()
            With .ClientOptionalFieldList
                .Suburb = True
                .Country = True
                .Company = True
                .BookingSource = True
                .Email = True
            End With
            'Include all the clients, not just the primary
            .IncludeSecondaryClients = True
            'Set search parameters.
            .ArriveFrom = oFromArrivalDate 'Start of Today
            .ArriveTo = oToArrivalDate 'End of Tomorrow
            .ListOfStatus = oListOfStatus

        End With
        Dim oResResult As RMSPublic.ResResult = oRMSPublic.GetListOfReservations(sToken, oResReq)
        If Not oResResult Is Nothing Then
            For Each oRes As RMSPublic.ResRecord In oResResult.ListOfRes
                If (oRes.AreaName.ToLower.Contains("with garden") And nRMSClientNo = 8325) Then 'This is specific to St. Jerome's Setup.  Splits Category 2 into 2 and 3 based on the name.
                    oRes.CategoryId = 3
                End If
                If oRes.Status = RMSPublic.ResStatus.Confirmed Then
                    oCsvString = (oCsvString & oRes.ResId & "," & oRes.PrimaryClient.Surname & "," & oRes.PrimaryClient.Given & "," & oRes.Arrive.Date & "," & oRes.Depart.Date & "," & oRes.PrimaryClient.Email & "," & oRes.CategoryId & "," & oRes.RateCode & vbCrLf)
                End If
            Next
        End If
        Return oCsvString
    End Function

    Function getInventoryString(sToken As String, oListOfPropertyIds As List(Of Integer))
        Dim oAvailReq As New RMSPublic.AvailabilityRequest
        'Dim oAvailReqCat2 As New RMSPublic.AvailabilityRequest
        ' Dim oAvailReqCat3 As New RMSPublic.AvailabilityRequest
        Dim oAvailResponse As List(Of RMSPublic.AvailabilityResponse)
        'Dim oAvailResponseCat2 As List(Of RMSPublic.AvailabilityResponse)
        'Dim oAvailResponseCat3 As List(Of RMSPublic.AvailabilityResponse)
        Dim oRMSPublic As New RMSPublic.PublicServiceClient
        Dim Cat1 = New List(Of Integer)
        Cat1.Add(1)
        Dim Cat2 = New List(Of Integer)
        Cat2.Add(2)

        Dim oCsvString As String = ""
        Dim i As Integer = 0
        Dim oLuxeGardenCounter As Integer = 0
        While i < 30
            'Category 1
            With oAvailReq
                .StartingPeriod = Now.AddDays(i).Date & " 00:00:00"
                .DateOfNow = Now.Date
                .EndingPeriod = Now.AddDays(i).Date & " 23:59:59"
                .OnlyCleanAreas = False
                .ListOfCategoryIds = Cat1

            End With

            oAvailResponse = oRMSPublic.GetAvailability(sToken, oAvailReq)
            If Not oAvailResponse Is Nothing Then
                For Each oAvailRec As RMSPublic.AvailabilityResponse In oAvailResponse
                    oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & oAvailRec.CategoryID & ",NULL," & oAvailRec.NoOfAvailableAreas & vbCrLf)
                Next
            End If
            If oAvailResponse Is Nothing Then
                oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & oAvailReq.ListOfCategoryIds.Item(0).ToString & ",NULL," & "0" & vbCrLf)
            End If
            oAvailResponse = Nothing

            'Category 2 -- Category 2 contains both Luxe Plus and Luxe Plus with Garden and needs to be split out.
            With oAvailReq
                .StartingPeriod = Now.AddDays(i).Date & " 00:00:00"
                .DateOfNow = Now.Date
                .EndingPeriod = Now.AddDays(i).Date & " 23:59:59"
                .OnlyCleanAreas = False
                .ListOfCategoryIds = Cat2

            End With

            oAvailResponse = oRMSPublic.GetAvailability(sToken, oAvailReq)
            oLuxeGardenCounter = 0 'Reset this to 0 for each day
            If Not oAvailResponse Is Nothing Then
                For Each oAvailRec As RMSPublic.AvailabilityResponse In oAvailResponse
                    For Each oAvailArea In oAvailRec.ListOfAvailableAreas
                        If (oAvailArea.Area.ToLower.Contains("with garden") And nRMSClientNo = 8325) Then 'Code specific for St. Jerome's (ClientNo 8325)
                            oLuxeGardenCounter = oLuxeGardenCounter + 1
                        End If
                    Next
                    oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & oAvailRec.CategoryID & ",NULL," & oAvailRec.NoOfAvailableAreas - oLuxeGardenCounter & vbCrLf)
                    oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & "3" & ",NULL," & oLuxeGardenCounter & vbCrLf)
                Next
            End If
            If oAvailResponse Is Nothing Then
                oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & oAvailReq.ListOfCategoryIds.Item(0).ToString & ",NULL," & "0" & vbCrLf)
                If (nRMSClientNo = 8325) Then 'St. Jerome's specific: make sure to include 0 rooms available for category 3 as well.
                    oCsvString = (oCsvString & "NULL," & oAvailReq.StartingPeriod & ",NULL," & "3" & ",NULL," & "0" & vbCrLf)
                End If
            End If
            oAvailResponse = Nothing
            i += 1
        End While
        Return oCsvString

    End Function


    Function sendFiles(iArrivalsFilePath As String, iInventoryFilePath As String, iApiToken As String)
        ' Change these for each property as appropriate
        ' MAKE SURE TO INCLUDE THE TRAILING \ IN THE arrivalsFolder
        Const cdoSendUsingPickup = 1 'Send message using the local SMTP service pickup directory.
        Const cdoSendUsingPort = 2 'Send the message using the network (SMTP over the network).
        Const cdoAnonymous = 0 'Do not authenticate
        Const cdoBasic = 1 'basic (clear-text) authentication
        Const cdoNTLM = 2 'NTLM
        Const arrivalsEmailSubject = "Arrivals"
        Const inventoryEmailSubject = "Inventory"

        Dim arrivalsEmailTo = iApiToken & "@mail.checkmate.io"
        Dim inventoryEmailTo = iApiToken & "-inv@mail.checkmate.io"
        Dim objMessage

        'Send Arrivals
        objMessage = CreateObject("CDO.Message")
        objMessage.Subject = arrivalsEmailSubject
        objMessage.From = emailFrom
        objMessage.To = arrivalsEmailTo
        objMessage.TextBody = "Arrivals"

        objMessage.AddAttachment(My.Computer.FileSystem.CurrentDirectory & "\" & iArrivalsFilePath)
        '==This section provides the configuration information for the remote SMTP server.
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendusing") = cdoSendUsingPort
        'Name or IP of Remote SMTP Server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpserver") = "smtp.gmail.com"
        'Type of authentication, NONE, Basic (Base64 encoded), NTLM
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate") = cdoBasic
        'Your UserID on the SMTP server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendusername") = "dataworker@checkmate.io"
        'Your password on the SMTP server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendpassword") = sEmailAppPassword
        'Server port (typically 25)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpserverport") = 465
        'Use SSL for the connection (False or True)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpusessl") = True
        'Connection Timeout in seconds (the maximum time CDO will try to establish a connection to the SMTP server)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout") = 60
        objMessage.Configuration.Fields.Update()
        '==End remote SMTP server configuration section==
        objMessage.Send()


        'Send Inventory
        objMessage = CreateObject("CDO.Message")
        objMessage.Subject = inventoryEmailSubject
        objMessage.From = emailFrom
        objMessage.To = inventoryEmailTo
        objMessage.TextBody = "Inventory"

        objMessage.AddAttachment(My.Computer.FileSystem.CurrentDirectory & "\" & iInventoryFilePath)
        '==This section provides the configuration information for the remote SMTP server.
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendusing") = cdoSendUsingPort
        'Name or IP of Remote SMTP Server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpserver") = "smtp.gmail.com"
        'Type of authentication, NONE, Basic (Base64 encoded), NTLM
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate") = cdoBasic
        'Your UserID on the SMTP server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendusername") = "dataworker@checkmate.io"
        'Your password on the SMTP server
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/sendpassword") = sEmailAppPassword
        'Server port (typically 25)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpserverport") = 465
        'Use SSL for the connection (False or True)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpusessl") = True
        'Connection Timeout in seconds (the maximum time CDO will try to establish a connection to the SMTP server)
        objMessage.Configuration.Fields.Item _
        ("http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout") = 60
        objMessage.Configuration.Fields.Update()
        '==End remote SMTP server configuration section==
        objMessage.Send()
    End Function



End Module
