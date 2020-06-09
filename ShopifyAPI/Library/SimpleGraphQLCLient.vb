Imports System
Imports RestSharp
Imports System.Net
Imports System.Dynamic
Imports Newtonsoft.Json.Linq
Imports System.Collections.Generic

Namespace SimpleGraphQLClient
    Public Class SimpleGraphQLClient
        Private _client As RestClient

        Public Sub New(ByVal GraphQLApiUrl As String)
            _client = New RestClient(GraphQLApiUrl)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls
        End Sub

        Public Function Execute(ByVal query As String, ByVal Optional variables As Object = Nothing, ByVal Optional additionalHeaders As Dictionary(Of String, String) = Nothing, ByVal Optional timeout As Integer = 0) As Object
            Dim request = New RestRequest("/", Method.POST)
            request.Timeout = timeout

            If additionalHeaders IsNot Nothing AndAlso additionalHeaders.Count > 0 Then

                For Each additionalHeader In additionalHeaders
                    request.AddHeader(additionalHeader.Key, additionalHeader.Value)
                Next
            End If

            request.AddJsonBody(New With {
                .query = query,
                .variables = variables
            })
            Return JObject.Parse(_client.Execute(request).Content)
        End Function
    End Class
End Namespace
