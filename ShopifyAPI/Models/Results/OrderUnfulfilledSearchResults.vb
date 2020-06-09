

Public Class OrderUnfulfilledResults
    Public Property orderId As String
    Public Property status As String
    Public Property lineItem As List(Of OrderSearchLineItem)
    Public Property tracking_number As String
    Public Property createdDate As String
    'Public Property tracking_number As String()

End Class


