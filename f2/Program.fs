open System.Linq
open System
open System.Security.Cryptography
open System.Text
open System.Net.Http
open Microsoft.FSharp.Control
open Microsoft.FSharp.Control
open System.Text.RegularExpressions
open System.IO
open RouterControl

 

[<EntryPoint>]
let main argv =
    let rt = new Router("admin", "")
    rt.reload_request()
  

   
     
    
    
    0
   
   
