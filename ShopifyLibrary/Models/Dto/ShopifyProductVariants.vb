Public Class ShopifyProductVariants
    Public Property id As Long
    Public Property product_id As Long
    Public Property title As String
    Public Property price As String
    Public Property sku As String
    Public Property position As Integer
    Public Property inventory_policy As String
    Public Property compare_at_price As Object
    Public Property fulfillment_service As String
    Public Property inventory_management As String
    Public Property option1 As String
    Public Property option2 As String
    Public Property option3 As Object
    Public Property created_at As DateTime
    Public Property updated_at As DateTime
    Public Property taxable As Boolean
    Public Property barcode As String
    Public Property grams As Integer
    Public Property image_id As Object
    Public Property weight As Double
    Public Property weight_unit As String
    Public Property inventory_item_id As Long
    Public Property inventory_quantity As Integer
    Public Property old_inventory_quantity As Integer
    Public Property requires_shipping As Boolean
    Public Property admin_graphql_api_id As String
End Class
