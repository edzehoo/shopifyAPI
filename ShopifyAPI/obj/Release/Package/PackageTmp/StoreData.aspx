<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StoreData.aspx.vb" Inherits="ShopifyAPI.StoreData"  UnobtrusiveValidationMode="None"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
      <script src="scripts/jquery.1.9.1/jquery.min.js"></script>
       <link href="bootstrap-4.0.0-dist/css/bootstrap.min.css" rel="stylesheet" />
       <script src="bootstrap-4.0.0-dist/js/bootstrap.min.js"></script>
    <script>
        $(document).ready(function() {
            isLocal  = function() {
                const LOCAL_DOMAINS = ["localhost", "127.0.0.1"];
                if (LOCAL_DOMAINS.includes(window.location.hostname)) {
                    return true;
                } else {
                    return false;
                }
            }

            $(".orderChecking").click(function() {
                let btnId = $(this).attr('id');
                let trackingNo = btnId.split('-').pop();
                let trackingURL =  'https://www.aramex.com/us/en/track/results?mode=2&PageSize=0&PageIndex=00&TotalRecords=0&ReferenceNumber='+  trackingNo + '&CountryOfOrginCode=CN&StartDate=05%2f10%2f2020+00%3a00%3a00&EndDate=05%2f21%2f2020+00%3a00%3a00';
                window.open(
                trackingURL,
                  '_blank' // <- This is what makes it open in a new window.
                );
            })

            $("<%=txtSearchKeyWord.ClientID%>").on("keyup keydown keypress change", function () {
                let searchKeyWord = $("<%=txtSearchKeyWord.ClientID%>").val();

                if (searchKeyWord < 4) {
                    $("<%=txtBoxRegularExpressionValidator.ClientID%>").show();
                } else {
                    $("<%=txtBoxRegularExpressionValidator.ClientID%>").hide();
                }
            })

            $(".btn-search-colors").click(function () {
                let btnId = $(this).attr('id');
                let productId = btnId.split('-').pop();

                let storeColorsPage = '/StoreDataColors.aspx';
                
                if (isLocal()) {
                    let colorsPageURL = 'http://' + window.location.host + storeColorsPage + "?productTitle=" + productId;
                    window.open(
                    colorsPageURL,
                      '_blank' // <- This is what makes it open in a new window.
                    )

                } else {
                    let colorsPageURL = 'http://' + window.location.hostname + storeColorsPage + "?productTitle=" + productId;
                    window.open(
                    colorsPageURL,
                      '_blank' // <- This is what makes it open in a new window.
                    )

                }

            })
           
        });
    </script>
       <style>
           li {
               list-style-type:none;
           }
       </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="row mt-1">
                <div class="col-12">
                     <asp:Button ID="btnReset" CssClass="btn btn-link" runat="server" Text="Reset" />
                </div>
            </div>
            <div class="row mt-1">
                <div class="col-12">
                    <asp:Button ID="btnGetProduct"  CssClass="btn btn-link" runat="server" Text="Predictive Search" />
                </div>
            </div>
            <div class="row mt-1 d-none">
                <div class="col-6 border border-1">
                    <div class="panel panel-info">
                        <div class="panel-heading">Flow 1-5) Search available Sizes</div>
                        <div class="panel-body">
                            <asp:TextBox ID="txtSKU" runat="server" PlaceHolder="SKU Item"></asp:TextBox>
                            <asp:ListView ID="productsListLV" runat="server">                              
                                <ItemTemplate>
                                    <li>
                                        <div class="row border border-1">
                                            <div class="col-8 text-nowrap">
                                                <strong>Product title :</strong><i><%# Eval("title") %></i>
                                            </div>
                                            <div class="col-4">
                                                 <strong>Size :</strong><i><%# Eval("size") %></i>
                                            </div>
                                        </div>
                                    </li>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                        No Products with entered SKU are found
                                </EmptyDataTemplate>
                            </asp:ListView>
                        </div>
                        <div class="panel-footer"><asp:Button ID="btnSearchSKU" runat="server" Text="Search" /></div>
                    </div>
                </div>
                <div class="col-6 border border-1">
                     <div class="panel panel-info">
                        <div class="panel-heading">Flow 6) Check Shipment Status</div>
                        <div class="panel-body">
                            <asp:TextBox ID="txtOrderNo" runat="server" TextMode="MultiLine" PlaceHolder="Enter Order Number separated by commas"></asp:TextBox>
                            <asp:ListView ID="orderListsLV" runat="server" EnableTheming="True">
                                <ItemTemplate>
                                    <li class="border border-1">
                                        <div class="row ml-1">
                                            <strong>Order Id:</strong> &nbsp;<%# Eval("orderId") %>
                                        </div>
                                        <div class="row ml-1">
                                            <strong>Fulfillment Status:</strong> &nbsp;<%# Eval("status") %>
                                        </div>
                                        <div class="row ml-1">
                                            <strong>Tracking No : </strong>&nbsp;<button class="btn btn-link orderChecking" id="btn-<%# Eval("tracking_number") %>" style="<%# if (Eval("tracking_number") Is Nothing, "display:none;","display:block;") %>"/><%# Eval("tracking_number") %></button>
                                        </div>
                                        <div class="row ml-1">
                                              <button class="btn btn-link amendOrder" id="btn-<%# Eval("orderId") %>" style="<%# if (Eval("status") = "partial" or Eval("status") = "unfulfilled" , "display:block;","display:none;") %>" >Amend this Order</button>
                                        </div>
                                    </li>                                     
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>No Order found</p>
                                </EmptyDataTemplate>                                                                
                            </asp:ListView>                            
                        </div>
                        <div class="panel-footer"><asp:Button ID="btnCheckShipmentStatus" runat="server" Text="Search" /></div>
                    </div>
                </div>
            </div>
            <div class="row mt-1 d-none">
                <div class="col-6 border border-1">
                    <div class="panel panel-info">
                        <div class="panel-heading">Flow 2) Search that matches description or category</div>
                        <div class="panel-body">
                            <asp:TextBox ID="txtSearchKeyWord" runat="server" PlaceHolder=""></asp:TextBox>
                            <asp:RegularExpressionValidator runat="server" Display="Dynamic" ControlToValidate="txtSearchKeyWord" ID="txtBoxRegularExpressionValidator" ValidationExpression="^[\s\S]{3,}$" ErrorMessage="Minimum 3 characters required."></asp:RegularExpressionValidator>
                            <asp:ListView ID="productsDescListLV" runat="server">                              
                                <ItemTemplate>
                                    <li>
                                        <div class="row border border-1">
                                            <div class="col-8 text-nowrap">
                                                <strong>Product title :</strong><i><%# Eval("title") %></i><br />
                                                <button id="btnSearchClr-<%# Eval("title") %>" class="btn btn-link btn-search-colors" type="button">Search for Colors</button>
                                            </div>
                                            <div class="col-4">
                                                <strong>Image:</strong><img src="<%#Eval("imgURL") %>" style="width:80%;height:auto;" />
                                            </div>
                                        </div>
                                    </li>
                                </ItemTemplate>                                
                                <EmptyDataTemplate>
                                        No Products with entered Words are found
                                </EmptyDataTemplate>
                            </asp:ListView>
                        </div>
                        <div class="panel-footer"><asp:Button ID="btnSearchKeyWord" runat="server" Text="Search" /></div>
                    </div>
                </div>
                <div class="col-6 border border-1">
                    <div class="panel panel-info">
                        <div class="panel-heading">Flow 9) Check Unfulfilled Status</div>
                        <div class="panel-body">
                            <asp:TextBox ID="txtUnfulfilled" runat="server" TextMode="MultiLine" PlaceHolder="Enter Order Number separated by commas"></asp:TextBox><asp:Button ID="btnCheckUnfulfilled" runat="server" Text="Search Unfulfilled" />
                            <asp:ListView ID="orderUnfulfilledListLV" runat="server" EnableTheming="True">
                                <ItemTemplate>
                                    <li class="border border-1">
                                        <div class="row ml-1">
                                            <strong>Order Id:</strong> &nbsp;<%# Eval("orderId") %>
                                        </div>
                                        <div class="row ml-1">
                                            <strong>Fulfillment Status:</strong> &nbsp;<%# Eval("status") %>
                                        </div>
                                        <div class="row ml-1">
                                           <strong>Line Item:</strong> 
                                            <asp:ListView ID="orderUnfulfilledListItems" runat="server" EnableTheming="true">
                                                    <ItemTemplate>
                                                        <li class="border border-1 pl-2"><%# Eval("name") %></li>                                                            
                                                    </ItemTemplate>
                                            </asp:ListView>                                           
                                        </div>
                                        <div class="row ml-1 d-none">
                                              <button class="btn btn-link amendOrder" id="btn-<%# Eval("orderId") %>" style="<%# if (Eval("status") = "partial" or Eval("status") = "unfulfilled" , "display:block;","display:none;") %>" >Amend this Order</button>
                                        </div>
                                    </li>                                     
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>No Order found</p>
                                </EmptyDataTemplate>                                                                
                            </asp:ListView>
                        </div>
                        <div class="panel-footer"></div>
                    </div>
                </div>
            </div>
            <div class="row mt-1 d-none">
                <div class="col-6 border border-1">
                        <div class="panel panel-info">
                        <div class="panel-heading">Flow 10) Check Order within 40 days</div>
                            <div class="panel-body">
                                <asp:TextBox ID="txtAmendedOrders" runat="server" PlaceHolder="Enter Singler Order Only" Visible="true"></asp:TextBox><br /><asp:Button ID="btnCheckAmendableOrderWithInput" runat="server" Text="Search Orders less than 40 days old" />
                                <asp:ListView ID="amendableOrdersWithInputListLV" runat="server" EnableTheming="True">
                                <ItemTemplate>
                                    <li class="border border-1">
                                        <div class="row ml-1">
                                            <strong>Order Id:</strong> &nbsp;<%# Eval("orderId") %>
                                        </div>
                                        <div class="row ml-1">
                                            <strong>Fulfillment Status:</strong> &nbsp;<%# Eval("status") %>
                                        </div>
                                        <div class="row ml-1">
                                           <strong>Line Item:</strong> 
                                            <asp:ListView ID="amendableOrdersWithInputListItems" runat="server" EnableTheming="true">
                                                    <ItemTemplate>
                                                        <li class="border border-1 pl-2"><%# Eval("name") %> , Quantitiy: <%# Eval("quantity") %></li>                                                            
                                                    </ItemTemplate>
                                            </asp:ListView>                                           
                                        </div>
                                        <div class="row ml-1 d-none">
                                              <button class="btn btn-link amendOrder" id="btn-<%# Eval("orderId") %>" style="<%# if (Eval("status") = "partial" or Eval("status") = "unfulfilled" , "display:block;","display:none;") %>" >Amend this Order</button>
                                        </div>
                                    </li>                                     
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>No Unfulfilled Orders that are within 40 days</p>
                                </EmptyDataTemplate>                                                                
                            </asp:ListView>
                            </div>
                        </div>
                        <div class="panel-footer"></div>
                    </div>  
                <div class="col-6 border border-1">
                     <div class="panel panel-info">
                        <div class="panel-heading">Flow 11) Check Orders within 40 days (No Input)</div>
                            <div class="panel-body">
                                <asp:TextBox ID="txt" runat="server" TextMode="MultiLine" PlaceHolder="Enter Order Number separated by commas" Visible="false"></asp:TextBox><asp:Button ID="btnCheckAmendableOrder" runat="server" Text="Search Orders less than 40 days old" />
                                    <asp:ListView ID="amendableOrdersListLV" runat="server" EnableTheming="True">
                                    <ItemTemplate>
                                        <li class="border border-1">
                                            <div class="row ml-1">
                                                <strong>Order Id:</strong> &nbsp;<%# Eval("orderId") %>
                                            </div>
                                            <div class="row ml-1">
                                                <strong>Fulfillment Status:</strong> &nbsp;<%# Eval("status") %>
                                            </div>
                                            <div class="row ml-1">
                                               <strong>Line Item:</strong> 
                                                <asp:ListView ID="amendableOrdersListItems" runat="server" EnableTheming="true">
                                                        <ItemTemplate>
                                                            <li class="border border-1 pl-2"><%# Eval("name") %> , Quantitiy: <%# Eval("quantity") %></li>                                                            
                                                        </ItemTemplate>
                                                </asp:ListView>                                           
                                            </div>
                                            <div class="row ml-1 d-none">
                                                  <button class="btn btn-link amendOrder" id="btn-<%# Eval("orderId") %>" style="<%# if (Eval("status") = "partial" or Eval("status") = "unfulfilled" , "display:block;","display:none;") %>" >Amend this Order</button>
                                            </div>
                                        </li>                                     
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <p>No Unfulfilled Orders that are within 40 days</p>
                                    </EmptyDataTemplate>                                                                
                                </asp:ListView>
                            </div>

                        </div>    
                        <div class="panel-footer"></div>                    
                    </div>                      
            </div>           
        </div>
    </form>
</body>
</html>
