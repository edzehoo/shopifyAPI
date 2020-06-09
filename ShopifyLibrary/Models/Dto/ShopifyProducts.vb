Public Class Variants
    Public Property id As Object
    Public Property product_id As Object
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
    Public Property inventory_item_id As Object
    Public Property inventory_quantity As Integer
    Public Property old_inventory_quantity As Integer
    Public Property requires_shipping As Boolean
    Public Property admin_graphql_api_id As String
End Class

Public Class ProductOptions
    Public Property id As Object
    Public Property product_id As Object
    Public Property name As String
    Public Property position As Integer
    Public Property values As String()
End Class

Public Class Image
        Public Property id As Long
        Public Property product_id As Long
        Public Property position As Integer
        Public Property created_at As DateTime
        Public Property updated_at As DateTime
        Public Property alt As Object
        Public Property width As Integer
        Public Property height As Integer
        Public Property src As String
        Public Property variant_ids As Object()
        Public Property admin_graphql_api_id As String
    End Class

Public Class ProductImage
    Public Property id As Long
    Public Property product_id As Long
    Public Property position As Integer
    Public Property created_at As DateTime
    Public Property updated_at As DateTime
    Public Property alt As Object
    Public Property width As Integer
    Public Property height As Integer
    Public Property src As String
    Public Property variant_ids As Object()
    Public Property admin_graphql_api_id As String
End Class

Public Class Product
        Public Property id As Long
        Public Property title As String
        Public Property body_html As String
        Public Property vendor As String
        Public Property product_type As String
        Public Property created_at As DateTime
        Public Property handle As String
        Public Property updated_at As DateTime
        Public Property published_at As DateTime
        Public Property template_suffix As String
        Public Property published_scope As String
        Public Property tags As String
        Public Property admin_graphql_api_id As String
        Public Property variants As Variants()
    Public Property options As ProductOptions()
    Public Property images As ProductImage()
    Public Property image As Image
    End Class

Public Class ShopifyProducts
    Public Property products As Product()
End Class
