Imports Newtonsoft.Json.Linq
Imports RestSharp
Imports ShopifyLibrary
Imports ShopifySharp
Imports ShopifyLib = ShopifyLibrary.ShopifyLibrary.ShopifyAPI

Public Class Auth
    Inherits System.Web.UI.Page
    Dim ShopifyLib As ShopifyLib.ShopifyAPI
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim code As String = Request.QueryString("code")
        Dim shopifyURL As String = Request.QueryString("shop")
        Dim hmac As String = Request.QueryString("hmac")
        Dim qs = Request.QueryString.ToString()

        If AuthorizationService.IsAuthenticRequest(qs, ShopfiyAPIConfig.ShopifyAPI_Secret_Key) Then

            Dim restClient = New RestClient(ShopfiyAPIConfig.ShopifyAPI_Store_URL & "/admin/oauth/access_token")

            Dim webRequest = New RestRequest() _
                                .AddParameter("client_id", ShopfiyAPIConfig.ShopifyAPI_API_Key) _
                                .AddParameter("client_secret", ShopfiyAPIConfig.ShopifyAPI_Secret_Key) _
                                .AddParameter("code", code)

            Dim webResponse = restClient.Post(webRequest)

            If webResponse.IsSuccessful Then
                Dim responseObj = JObject.Parse(webResponse.Content)
                Dim shopifyAuthResponse As New ShopifyAuthResponse()

                shopifyAuthResponse.access_token = responseObj("access_token")
                shopifyAuthResponse.scopes = responseObj("scope")
                ShopifyLib.InsertAccessToken(shopifyAuthResponse)
                Response.Redirect("StoreData.aspx")
            End If


        End If
    End Sub

End Class