Imports System.Data.SqlClient
Imports RestSharp
Imports ShopifySharp
Imports ShopifySharp.Enums
Imports ShopifySharp.Filters
Imports ShopifySharp.Lists
Imports System.Threading
Imports RestSharp.Authenticators
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Namespace ShopifyLibrary
    Public Class ShopifyAPI
        Dim APIKey As String = My.Settings.Shopify_API_Key
        Dim SecretKey As String = My.Settings.Shopify_Secret_Key
        Dim StoreURL As String = My.Settings.Shopify_Store_URL
        Dim AppURL As String = My.Settings.Shopify_App_URL
        Dim DB_Conn As String = My.Settings.Shopify_App_URL
        Dim API_Call_Limit_Header As String = My.Settings.Shopify_API_Rate_Limit_Header
        Dim API_Retry_Header As String = My.Settings.Shopify_API_Rate_Limit_Retry_Header
        Dim API_Call_Limit As String = My.Settings.Shopify_API_Call_Limit
        Dim Dummy_CustomerID As String = My.Settings.Shopify_API_Dummy_CustomerID

        Const producstListJSON As String = "admin/api/2020-04/products.json"
        Const ordersListJSON As String = "admin/api/2020-04/orders.json"
        'Const singleOrdersListJSON As String = "admin/api/2020-04/orders/#{0}.json"
        Const singleOrdersListJSON As String = "admin/api/2020-04/orders/{0}.json"
        Const customerOrdersJSON As String = "admin/api/2020-04/customers/{0}/orders.json"
        Public Function BuildAuthorizationURL() As String
            Dim scopes = New List(Of AuthorizationScope) From {
                AuthorizationScope.ReadProducts,
                AuthorizationScope.WriteOrders,
                AuthorizationScope.ReadOrders
            }
            Dim redirecURL As String = AppURL & "/Auth.aspx"

            Dim authUrl = AuthorizationService.BuildAuthorizationUrl(scopes, StoreURL, APIKey, redirecURL)

            Return authUrl.ToString()
        End Function

        Public Async Function GetAccessTokenFromAuhtURL(ByVal authorizationCode As String, ByVal storeURL As String, ByVal secretKey As String) As Threading.Tasks.Task(Of String)
            Return Await AuthorizationService.Authorize(authorizationCode, storeURL, APIKey, secretKey)
        End Function

        Public Sub InsertAccessToken(ByVal shopifyReponse As ShopifyAuthResponse)
            Dim sqlConnection As New SqlConnection With {
                .ConnectionString = DB_Conn
            }

            Dim insertCmd = String.Format("
            INSERT INTO [dbo].[shopifyAppToken]
           ([scopes]
           ,[access_token]
           ,[refresh_token]
           ,[createdDate])
             VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'  
           ,GETUTCDATE())",
           shopifyReponse.scopes, shopifyReponse.access_token, Nothing)

            'Dim logger As ILog = Nothing)
            Using cn As New SqlConnection(sqlConnection.ConnectionString)
                Try
                    Using cmd As New SqlCommand
                        With cmd
                            .Connection = cn
                            .Connection.Open()
                            .CommandText = insertCmd
                            .CommandType = CommandType.Text
                        End With

                        cmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    'logger.Error("InsertTokenInfo , error occurred on " & ex.Message)
                End Try
            End Using
        End Sub

        Public Function CheckStockAvailability(ByVal SKU_ItemName As String, ByVal access_token As String) As List(Of ProductSearchResults) 'Check stock availability (Option 1) / Locate item, return available sizes
            Dim ProductSearchResults As List(Of ProductSearchResults) = New List(Of ProductSearchResults)
            Dim restClient = New RestClient(StoreURL)
            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

RetryAPI:
            Dim filter = "?title=" & SKU_ItemName
            Dim webRequest = New RestRequest(producstListJSON & filter, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)


            Try
                'Dim callLimit = webResponse.Headers.Any( = "X-Shopify-Api-Call-Limit").ToString()
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())
                        Dim sizePos = -1
                        Dim colorPos = -1
                        For Each obj In responseObj("products")(0)("options")
                            If obj("name").Value.ToString().ToLower() = "size" Then
                                sizePos = Integer.Parse(obj("position"))
                            ElseIf obj("name").Value.ToString().ToLower() = "color" Then
                                colorPos = Integer.Parse(obj("position"))
                            End If
                        Next

                        If sizePos = -1 Then
                            ProductSearchResults.Add(New ProductSearchResults With {
                                .title = responseObj("products")(0)("title"),
                                .size = "No size available"
                            })
                        Else
                            For Each item In responseObj("products")
                                For Each vars In item("variants")
                                    ProductSearchResults.Add(New ProductSearchResults With {
                                            .title = item("title").Value & "-" & vars("option" & colorPos).Value,
                                            .size = vars("option" & sizePos).Value
                                        })
                                Next
                            Next
                        End If
                    End If
                End If

            Catch ex As Exception

            End Try
            Return ProductSearchResults
        End Function

        Public Function CheckStockAvailability(ByVal ShopifyProductsSearch As ShopifyProductsSearch, ByVal access_token As String) As List(Of ProductSearchDescResults)    'Check stock availability (Option 2) / Locate items with those words, return item names in list
            Dim ProductSearchResults As List(Of ProductSearchDescResults) = New List(Of ProductSearchDescResults)
            Dim restClient = New RestClient(StoreURL)
            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

RetryAPI:
            Dim webRequest = New RestRequest(producstListJSON, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)


            Try
                'Dim callLimit = webResponse.Headers.Any( = "X-Shopify-Api-Call-Limit").ToString()
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())
                        Dim sizePos = -1
                        Dim colorPos = -1
                        For Each item In responseObj("products")

                            If InStr(item("title").Value.ToString(), ShopifyProductsSearch.description, CompareMethod.Text) > 0 Or
                               InStr(item("body_html").Value.ToString(), ShopifyProductsSearch.description, CompareMethod.Text) > 0 Or
                               InStr(item("product_type").Value.ToString(), ShopifyProductsSearch.description, CompareMethod.Text) > 0 Then
                                ProductSearchResults.Add(New ProductSearchDescResults With {
                                        .title = item("title").Value,
                                        .size = Nothing,
                                        .imgURL = If(item("images")(0)("src").Value.ToString() Is Nothing, "https://cdn.shopify.com/s/files/1/0533/2089/files/placeholder-images-product-4_large.png?format=jpg&quality=90&v=1530129360", item("images")(0)("src").Value.ToString())
                                    })
                            End If


                            'objCtr += 1
                        Next
                    End If
                End If

            Catch ex As Exception

            End Try
            Return ProductSearchResults
        End Function


        Public Function CheckStockAvailabilityColor(ByVal SKU_ItemName As String, ByVal access_token As String) As List(Of ProductSearchColorResults) 'Check stock availability (Option 2) / Locate item, return available colors
            Dim ProductSearchResults As List(Of ProductSearchColorResults) = New List(Of ProductSearchColorResults)
            Dim restClient = New RestClient(StoreURL)
            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

RetryAPI:
            Dim filter = "?title=" & SKU_ItemName
            Dim webRequest = New RestRequest(producstListJSON & filter, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)


            Try
                'Dim callLimit = webResponse.Headers.Any( = "X-Shopify-Api-Call-Limit").ToString()
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())
                        Dim sizePos = -1
                        Dim colorPos = -1
                        For Each obj In responseObj("products")(0)("options")
                            If obj("name").Value.ToString().ToLower() = "size" Then
                                sizePos = Integer.Parse(obj("position"))
                            ElseIf obj("name").Value.ToString().ToLower() = "color" Then
                                colorPos = Integer.Parse(obj("position"))
                            End If
                        Next

                        If sizePos = -1 Then
                            ProductSearchResults.Add(New ProductSearchColorResults With {
                               .productId = Nothing,
                               .title = responseObj("products")(0)("title"),
                               .color = "No size available"
                           })
                        Else
                            Dim objCtr = 0
                            For Each item In responseObj("products")
                                ProductSearchResults.Add(New ProductSearchColorResults With {
                                    .productId = item("id").Value,
                                    .title = item("title").Value,
                                    .color = If(item("variants")(0)("option" & colorPos).Value Is Nothing, "No Color", item("variants")(0)("option" & colorPos).Value)
                                })
                            Next
                        End If
                    End If
                End If

            Catch ex As Exception

            End Try
            Return ProductSearchResults
        End Function

        Public Function CheckStockAvailabilitySize(ByVal productId As String, ByVal access_token As String) As List(Of ProductSearchSizeResults) 'Check stock availability (Option 1) / Locate item, return available sizes
            Dim ProductSearchResults As List(Of ProductSearchSizeResults) = New List(Of ProductSearchSizeResults)
            Dim restClient = New RestClient(StoreURL)
            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

RetryAPI:
            Dim filter = "?ids=" & productId
            Dim webRequest = New RestRequest(producstListJSON & filter, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)


            Try
                'Dim callLimit = webResponse.Headers.Any( = "X-Shopify-Api-Call-Limit").ToString()
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())
                        Dim sizePos = -1
                        Dim colorPos = -1
                        For Each obj In responseObj("products")(0)("options")
                            If obj("name").Value.ToString().ToLower() = "size" Then
                                sizePos = Integer.Parse(obj("position"))
                            ElseIf obj("name").Value.ToString().ToLower() = "color" Then
                                colorPos = Integer.Parse(obj("position"))
                            End If
                        Next

                        If sizePos = -1 Then
                            ProductSearchResults.Add(New ProductSearchSizeResults With {
                                .title = responseObj("products")(0)("title"),
                                .size = "No size available",
                                .productURL = Nothing
                            })
                        Else
                            For Each item In responseObj("products")
                                For Each vars In item("variants")
                                    ProductSearchResults.Add(New ProductSearchSizeResults With {
                                            .title = item("title").Value & "-" & vars("option" & colorPos).Value,
                                            .size = vars("option" & sizePos).Value,
                                            .productURL = StoreURL & "products/" & item("handle")
                                        })
                                Next
                            Next
                        End If
                    End If
                End If

            Catch ex As Exception

            End Try
            Return ProductSearchResults
        End Function
        Public Function CheckShipmentStatus(ByVal orderNo As String, ByVal access_token As String) As List(Of OrderSearchResults)

            Dim orderSearchResults As List(Of OrderSearchResults) = New List(Of OrderSearchResults)
            Dim restClient = New RestClient(StoreURL)

            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)
            Dim orderNumbers As String()
            If orderNo.Contains(",") Then
                orderNumbers = orderNo.Split(",")
            Else
                orderNumbers = {orderNo}
            End If

RetryAPI:
            Dim filter = String.Empty

            Dim webRequest = New RestRequest(ordersListJSON & "?status=any&limit=250&fields=id,name,fulfillment_status,fulfillments", Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)
            Dim orderNoFromResult As String = String.Empty
            Dim orderStatus As String = String.Empty
            Dim trackingNo As String = String.Empty
            Try
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())

                        If orderNumbers.Length > 0 Then
                            For Each elem As String In orderNumbers
                                For Each item In responseObj("orders")
                                    If item("name").Value = "#" & elem Then
                                        orderNoFromResult = item("name").Value
                                        orderStatus = If(item("fulfillment_status").Value Is Nothing, "unfulfilled", item("fulfillment_status"))
                                        If item("fulfillments").Count = 0 Then
                                            trackingNo = Nothing
                                        Else
                                            trackingNo = If(item("fulfillments")(0)("tracking_number").Value Is Nothing, Nothing, item("fulfillments")(0)("tracking_number"))
                                        End If

                                        orderSearchResults.Add(New OrderSearchResults With {
                                            .orderId = orderNoFromResult,
                                            .status = orderStatus,
                                            .tracking_number = trackingNo
                                            }
                                        )
                                    End If
                                Next
                            Next
                        End If
                    End If
                End If

            Catch ex As Exception

            End Try
            Return orderSearchResults
        End Function

        Public Function CheckShipmentStatusUnfulfilled(ByVal orderNo As String, ByVal access_token As String) As List(Of OrderUnfulfilledResults)

            Dim orderSearchResultsList As List(Of OrderUnfulfilledResults) = New List(Of OrderUnfulfilledResults)
            Dim restClient = New RestClient(StoreURL)

            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)
            Dim orderNumbers As String() = Nothing
            If orderNo.Contains(",") Then
                orderNumbers = orderNo.Split(",")
            Else
                If orderNo.Length > 0 Then
                    orderNumbers = {orderNo}
                End If
            End If

RetryAPI:
            Dim filter = String.Empty

            Dim webRequest = New RestRequest(ordersListJSON & "?status=unshipped&limit=250&fields=id,name,fulfillment_status,line_items", Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)
            Dim orderId As String = String.Empty
            Dim orderNoFromResult As String = String.Empty
            Dim orderLineItemList As List(Of OrderSearchLineItem) = New List(Of OrderSearchLineItem)
            Dim orderLineItem As OrderSearchLineItem = New OrderSearchLineItem()
            Dim orderUnfufilled As New OrderUnfulfilledResults
            Dim orderStatus As String = String.Empty
            Dim trackingNo As String = String.Empty
            Dim proceed As Boolean = False
            Dim objCtr As Integer = 0


            Try
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())

                        If orderNumbers.Count > 0 Then
                            For Each elem As String In orderNumbers

                                For Each item In responseObj("orders")
                                    If item("name").Value = "#" & elem Then
                                        orderNoFromResult = item("name").Value
                                        orderStatus = If(item("fulfillment_status").Value Is Nothing, "unfulfilled", item("fulfillment_status"))
                                        orderUnfufilled = New OrderUnfulfilledResults()
                                        If orderStatus = "unfulfilled" Then
                                            'Or orderStatus = "partial" Then 'temporary disabled partial fulfilment filter
                                            orderUnfufilled.orderId = orderNoFromResult
                                            orderUnfufilled.status = orderStatus
                                            orderLineItemList = New List(Of OrderSearchLineItem)
                                            Dim lineItems = JsonConvert.DeserializeObject(Of Object)(item("line_items").ToString())
                                            For Each lineItem In lineItems
                                                If lineItem("fulfillment_status").Value = Nothing Then
                                                    'Or lineItem("fulfillment_status").Value = "partial" then 'temporary disabled partial fulfilment filter

                                                    orderLineItem = New OrderSearchLineItem()
                                                    orderLineItem.id = lineItem("id")
                                                    orderLineItem.name = lineItem("name")
                                                    orderLineItem.title = lineItem("title")
                                                    orderLineItem.quantity = lineItem("quantity")

                                                    orderLineItemList.Add(orderLineItem)
                                                End If
                                            Next
                                            orderUnfufilled.lineItem = orderLineItemList
                                            orderSearchResultsList.Add(orderUnfufilled)


                                        End If
                                    End If
                                Next
                            Next
                        End If
                    End If
                End If

            Catch ex As Exception

            End Try
            Return orderSearchResultsList
        End Function

        Public Function CheckShipmentAmendableOrder(ByVal customerId As String, ByVal access_token As String) As List(Of OrderUnfulfilledResults)

            Dim orderSearchResultsList As List(Of OrderUnfulfilledResults) = New List(Of OrderUnfulfilledResults)
            Dim restClient = New RestClient(StoreURL)

            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

            customerId = Dummy_CustomerID
RetryAPI:
            Dim filter = String.Empty
            'filter = "?status=unshipped&limit=250&fields=id,name,fulfillment_status,line_items"        
            Dim webRequest = New RestRequest(String.Format(customerOrdersJSON, customerId) & filter, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)
            Dim orderId As String = String.Empty
            Dim orderNoFromResult As String = String.Empty
            Dim orderLineItemList As List(Of OrderSearchLineItem) = New List(Of OrderSearchLineItem)
            Dim orderLineItem As OrderSearchLineItem = New OrderSearchLineItem()
            Dim orderUnfufilled As New OrderUnfulfilledResults
            Dim orderStatus As String = String.Empty
            Dim trackingNo As String = String.Empty
            Dim proceed As Boolean = False
            Dim objCtr As Integer = 0


            Try
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())


                        For Each item In responseObj("orders")
                            orderNoFromResult = item("name").Value
                            orderStatus = If(item("fulfillment_status").Value Is Nothing, "unfulfilled", item("fulfillment_status"))
                            orderUnfufilled = New OrderUnfulfilledResults()
                            If orderStatus = "unfulfilled" Or orderStatus = "partial" Then
                                orderUnfufilled.orderId = orderNoFromResult
                                orderUnfufilled.status = orderStatus
                                If Convert.ToDateTime(item("created_at").Value) >= DateTime.UtcNow.AddDays(-40) Then
                                    orderUnfufilled.createdDate = Convert.ToDateTime(item("created_at").Value).ToString("yyyy-MM-dd hh:mm:ss")
                                    orderLineItemList = New List(Of OrderSearchLineItem)
                                    Dim lineItems = JsonConvert.DeserializeObject(Of Object)(item("line_items").ToString())
                                    For Each lineItem In lineItems
                                        If lineItem("fulfillment_status").Value = Nothing Or lineItem("fulfillment_status").Value = "partial" Then
                                            orderLineItem = New OrderSearchLineItem()
                                            orderLineItem.id = lineItem("id")
                                            orderLineItem.name = lineItem("name")
                                            orderLineItem.title = lineItem("title")
                                            orderLineItem.quantity = lineItem("quantity")

                                            orderLineItemList.Add(orderLineItem)
                                        End If
                                    Next
                                End If

                                orderUnfufilled.lineItem = orderLineItemList
                                orderSearchResultsList.Add(orderUnfufilled)

                            End If
                        Next
                    End If
                End If

            Catch ex As Exception

            End Try
            Return orderSearchResultsList
        End Function

        Public Function CheckShipmentAmendableOrderWithInput(ByVal OrderId As String, ByVal access_token As String) As List(Of OrderUnfulfilledResults)

            Dim orderSearchResultsList As List(Of OrderUnfulfilledResults) = New List(Of OrderUnfulfilledResults)
            Dim restClient = New RestClient(StoreURL)

            restClient.AddDefaultHeader("X-Shopify-Access-Token", access_token)

            Dim customerId = Dummy_CustomerID
RetryAPI:
            Dim filter = String.Empty
            'filter = "?status=unshipped&limit=250&fields=id,name,fulfillment_status,line_items"        
            Dim webRequest = New RestRequest(String.Format(customerOrdersJSON, customerId) & filter, Method.GET, DataFormat.Json)
            Dim webResponse = restClient.Execute(webRequest)
            Dim orderNoFromResult As String = String.Empty
            Dim orderLineItemList As List(Of OrderSearchLineItem) = New List(Of OrderSearchLineItem)
            Dim orderLineItem As OrderSearchLineItem = New OrderSearchLineItem()
            Dim orderUnfufilled As New OrderUnfulfilledResults
            Dim orderStatus As String = String.Empty
            Dim trackingNo As String = String.Empty
            Dim proceed As Boolean = False
            Dim objCtr As Integer = 0


            Try
                Dim callLimit = webResponse.Headers.Where(Function(h) h.Name = API_Call_Limit_Header).Select(Function(p) p.Value).FirstOrDefault()

                If callLimit = API_Call_Limit Then
                    Dim retryAfter = webResponse.Headers.Where(Function(h) h.Name = API_Retry_Header).Select(Function(p) p.Value).FirstOrDefault()
                    Thread.Sleep(Int(Integer.Parse(retryAfter) * 1000))
                    GoTo RetryAPI
                Else
                    If webResponse.IsSuccessful Then
                        Dim responseObj = JsonConvert.DeserializeObject(Of Object)(webResponse.Content.ToString())


                        For Each item In responseObj("orders")
                            orderNoFromResult = item("name").Value
                            If orderNoFromResult = "#" & OrderId Then
                                orderStatus = If(item("fulfillment_status").Value Is Nothing, "unfulfilled", item("fulfillment_status"))
                                orderUnfufilled = New OrderUnfulfilledResults()
                                If orderStatus = "unfulfilled" Or orderStatus = "partial" Then
                                    orderUnfufilled.orderId = orderNoFromResult
                                    orderUnfufilled.status = orderStatus
                                    If Convert.ToDateTime(item("created_at").Value) >= DateTime.UtcNow.AddDays(-40) Then
                                        orderUnfufilled.createdDate = Convert.ToDateTime(item("created_at").Value).ToString("yyyy-MM-dd hh:mm:ss")
                                        orderLineItemList = New List(Of OrderSearchLineItem)
                                        Dim lineItems = JsonConvert.DeserializeObject(Of Object)(item("line_items").ToString())
                                        For Each lineItem In lineItems
                                            If lineItem("fulfillment_status").Value = Nothing Or lineItem("fulfillment_status").Value = "partial" Then
                                                orderLineItem = New OrderSearchLineItem()
                                                orderLineItem.id = lineItem("id")
                                                orderLineItem.name = lineItem("name")
                                                orderLineItem.title = lineItem("title")
                                                orderLineItem.quantity = lineItem("quantity")

                                                orderLineItemList.Add(orderLineItem)
                                            End If
                                        Next
                                    End If

                                    orderUnfufilled.lineItem = orderLineItemList
                                    orderSearchResultsList.Add(orderUnfufilled)

                                End If
                            End If
                        Next
                    End If
                End If

            Catch ex As Exception

            End Try
            Return orderSearchResultsList
        End Function

        Public Function GetTokenInfo() As ShopifyAuthResponse
            Dim shopifyData As New ShopifyAuthResponse
            Dim dataSet As New DataSet
            Dim sqlConnection As New SqlConnection With {
                .ConnectionString = DB_Conn
            }
            Dim getTokenInfoCmd As String = String.Format("select top 1 scopes, access_token,refresh_token from shopifyAppToken order by id desc")
            'Dim logger As ILog = Nothing)
            Using cn As New SqlConnection(sqlConnection.ConnectionString)
                Try
                    Using cmd As New SqlCommand
                        With cmd
                            .Connection = cn
                            .Connection.Open()
                            .CommandText = getTokenInfoCmd
                            .CommandType = CommandType.Text
                        End With

                        dataSet.Tables.Clear()
                        Dim da = New SqlDataAdapter(cmd)
                        da.Fill(dataSet, "shopifyAppToken")

                        Dim dataRow As DataRow

                        If dataSet IsNot Nothing Then
                            For Each dataRow In dataSet.Tables(0).Rows
                                shopifyData.scopes = dataRow(0)
                                shopifyData.access_token = dataRow(1)
                                shopifyData.refresh_token = dataRow(2)
                            Next

                        End If

                    End Using
                Catch ex As Exception
                    'logger.Error("GetTokenInfo , error occurred on " & ex.Message)
                End Try
            End Using
            Return shopifyData
        End Function
    End Class
End Namespace

