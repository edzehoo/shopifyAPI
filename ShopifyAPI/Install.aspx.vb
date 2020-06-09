Imports ShopifySharp
Imports ShopifySharp.Enums
Public Class HomePage
    Inherits System.Web.UI.Page

    Dim ShopifyLib As ShopifyLib.ShopifyAPI
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Response.Redirect(ShopifyLib.BuildAuthorizationURL())

        'Dim store As String = Request.QueryString("shop")
        'Response.Redirect(ShopifyLib.BuildAuthorizationURL(store))
    End Sub

End Class