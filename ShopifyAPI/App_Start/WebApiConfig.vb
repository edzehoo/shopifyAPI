Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Http

Public Module WebApiConfig
    Sub Register(ByVal config As HttpConfiguration)
        config.Routes.MapHttpRoute(name:="ApiByAction", routeTemplate:="api/{controller}/{action}/{id}", defaults:=New With {.id = RouteParameter.Optional})
        config.Routes.MapHttpRoute(name:="DefaultApi", routeTemplate:="api/{controller}/{id}", defaults:=New With {.id = RouteParameter.Optional})
    End Sub

End Module

