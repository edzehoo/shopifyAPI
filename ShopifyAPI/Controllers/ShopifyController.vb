Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Timers
Imports System.Web.Http

<RoutePrefix("api/Shopify")>
Public Class ShopifyController
    Inherits ApiController
    Dim ShopifyLib As New ShopifyLib.ShopifyAPI
    ''' <summary>
    ''' Get list of products based on exact mataching description
    ''' </summary>
    ''' <param name="sku"></param>
    ''' <returns>List of Locations</returns>
    <ActionName("GetStockSKU")>
    <HttpGet>
    Public Function GetStockAvailable(ByVal sku As String) As IHttpActionResult

        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of ProductSearchResults)
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(sku)) Then
                Return BadRequest("sku parameter is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim results = ShopifyLib.CheckStockAvailability(sku, access_token)
                    If results.Count > 0 Then
                        For Each res In results
                            searchResults.Add(New ProductSearchResults With {
                                       .title = res.title,
                                       .size = res.size
                            })
                        Next
                    Else
                        searchResults.Add(New ProductSearchResults With {
                                       .title = "Product",
                                       .size = "No products found"
                            })
                    End If

                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function


    <ActionName("GetStockWords")>
    <HttpGet>
    Public Function GetStockKeyWords(ByVal description As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of ProductSearchDescResults)
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(description)) Then
                Return BadRequest("description is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim shopifyProductSearch As New ShopifyProductsSearch()
                    shopifyProductSearch.description = description
                    Dim results = ShopifyLib.CheckStockKeyWords(shopifyProductSearch, access_token)
                    If results.Count > 0 Then
                        For Each res In results
                            searchResults.Add(New ProductSearchDescResults With {
                                .title = res.title,
                                .size = res.size,
                                .imgURL = res.imgURL
                            })
                        Next
                    Else
                        searchResults.Add(New ProductSearchDescResults With {
                                .title = "Product",
                                .size = "No products found",
                                .imgURL = "https://cdn.shopify.com/s/files/1/0533/2089/files/placeholder-images-product-4_large.png?format=jpg&quality=90&v=1530129360"
                            })
                    End If

                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function


    <ActionName("GetStockSize")>
    <HttpGet>
    Public Function GetStockSizes(ByVal sku As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of KeyValuePair(Of String, String))
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(sku)) Then
                Return BadRequest("sku is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim results = ShopifyLib.CheckStockSize(sku, access_token)
                    For Each res In results
                        searchResults.Add(New KeyValuePair(Of String, String)(res.title, res.size))
                    Next
                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function

    <ActionName("GetStockColor")>
    <HttpGet>
    Public Function GetStockColors(ByVal sku As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of KeyValuePair(Of String, String))
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(sku)) Then
                Return BadRequest("sku is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim results = ShopifyLib.CheckStockColor(sku, access_token)
                    For Each res In results
                        searchResults.Add(New KeyValuePair(Of String, String)(res.title, res.color))
                    Next
                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function

    <ActionName("GetOrderStatus")>
    <HttpGet>
    Public Function GetShipmentStatus(ByVal orderNos As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of OrderSearchResults)
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(orderNos)) Then
                Return BadRequest("Order number is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim results = ShopifyLib.CheckShipmentStatus(orderNos, access_token)
                    If results.Count > 0 Then
                        For Each res In results
                            searchResults.Add(New OrderSearchResults With {
                                .orderId = res.orderId,
                                .status = res.status,
                                .tracking_number = res.tracking_number
                            })
                        Next
                    Else
                        searchResults.Add(New OrderSearchResults With {
                                .orderId = "Not found",
                                .status = Nothing,
                                .tracking_number = Nothing
                            })
                    End If

                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function

    <ActionName("GetUnshippedOrders")>
    <HttpGet>
    Public Function GetUnfulfilledOrders(ByVal orderNos As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of OrderUnfulfilledResults)
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(orderNos)) Then
                Return BadRequest("Order number is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else
                    Dim results = ShopifyLib.CheckShipmentStatusUnfulfilled(orderNos, access_token)
                    If results.Count > 0 Then
                        For Each res In results
                            searchResults.Add(New OrderUnfulfilledResults With {
                                .orderId = res.orderId,
                                .status = res.status,
                                .lineItem = res.lineItem
                            })
                        Next
                    Else searchResults.Add(New OrderUnfulfilledResults With {
                             .orderId = "Not found",
                             .status = Nothing,
                             .lineItem = Nothing
                         })

                    End If

                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function


    <ActionName("GetUnshippedOrdersByCustId")>
    <HttpGet>
    Public Function GetUnfulfilledOrdersByCustomerId(ByVal customerId As String) As IHttpActionResult
        Dim httpReponse As New HttpResponseMessage()
        Dim searchResults As New List(Of OrderUnfulfilledResults)
        Dim access_token As String = String.Empty
        Try
            Dim Headers As Headers.HttpRequestHeaders = Me.Request.Headers

            If (String.IsNullOrEmpty(customerId)) Then
                Return BadRequest("Customer Id is empty")
            End If

            If Headers.Contains("X-Shopify-Access-Token") Then

                access_token = Headers.GetValues("X-Shopify-Access-Token").FirstOrDefault()

                If String.IsNullOrEmpty(access_token) Then
                    Return BadRequest("Invalid token information")
                Else

                    Dim Results = ShopifyLib.CheckAmendableOrderByCustomerId(customerId, access_token)
                    For Each res In Results
                        searchResults.Add(New OrderUnfulfilledResults With {
                            .orderId = res.orderId,
                            .status = res.status,
                            .lineItem = res.lineItem
                        })
                    Next
                End If
            Else
                Return BadRequest("Invalid credentials")
            End If


        Catch ex As Exception
            Return BadRequest("Invalid parameters entered")
        End Try

        Return Json(searchResults)
    End Function
End Class
