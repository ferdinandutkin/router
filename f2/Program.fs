open System.Linq
open System
open System.Security.Cryptography
open System.Text
open System.Net.Http
open Microsoft.FSharp.Control
open Microsoft.FSharp.Control
open System.Text.RegularExpressions
open System.IO

let toBase64(input : string) : string =


    let tryGet (arr : _[], idx : int) =
        if arr.Length > idx && idx > -1 then Some(arr.[idx]) else None

    let keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
    let chunkproc (a: int option, b : int option, c : int option) = 

           let enc1 = if a.IsSome then Some(a.Value >>> 2) else None;
    
           let enc2 = match a, b with
                      | Some a, Some b -> Some(((a &&& 3) <<< 4) ||| (b >>> 4));
                      | _ -> None
           
           
           let enc3 = match b, c with
                      | Some b, Some c -> Some(((b &&& 15) <<< 2) ||| (c >>> 6))
                      | _ -> Some 64
           
           
           let enc4 = if c.IsSome then Some(c.Value &&& 63) else Some 64;


           

           let optCharToString(c : char option) =
               match c with
               | Some c -> c.ToString()
               | None -> ""


           let tryGetOptIndex(arr, idx: int option) =
                tryGet(arr, if idx.IsSome then idx.Value else 0)

           let stringFromEnc(enc : int option) =
                optCharToString(tryGetOptIndex(keyStr.ToCharArray(), enc))
                
           

           String.Concat(stringFromEnc(enc1),  stringFromEnc(enc2), stringFromEnc(enc3), stringFromEnc(enc4))
 

        
        
    input |> Seq.map(fun c -> (int)c) |> Seq.chunkBySize 3 |> Seq.map(fun el ->  chunkproc(tryGet(el, 0), tryGet(el, 1), tryGet(el, 2))) |> String.Concat
                                                          
                                                                        

let tb64_2(text : string) =
      let plainTextBytes = System.Text.Encoding.UTF8.GetBytes text;
      System.Convert.ToBase64String plainTextBytes;
 
                                                                        
let md5 (text : string) =
    let md5 = MD5.Create()
    md5.ComputeHash(Encoding.UTF8.GetBytes text) |> Array.map(fun x -> x.ToString "x2") |> String.Concat;
    


                    

let generateAuthCookie(login : string, password : string) = 

    let pwordHash = md5 password;
    let authHash = tb64_2($"{login}:{pwordHash}")
    let authData = $"Basic {authHash}"
    $"Authorization={Uri.EscapeDataString authData}"




let reload_request(client : HttpClient, session_id : string) =
    let baseRequestUrl = session_id + "/userRpm/SysRebootRpm.htm"

    let requestUrl =  baseRequestUrl + "?Reboot=%D0%9F%D0%B5%D1%80%D0%B5%D0%B7%D0%B0%D0%B3%D1%80%D1%83%D0%B7%D0%B8%D1%82%D1%8C"

    let msg = new HttpRequestMessage(HttpMethod.Get, requestUrl)

    msg.Headers.Add("Referer",  client.BaseAddress.ToString() + baseRequestUrl)

    let res = client.Send(msg)

    let reader = new StreamReader(res.Content.ReadAsStream())
  
    printf "%s"  (reader.ReadToEnd())

let login_request(login: string, password : string) = 
    let handler = new HttpClientHandler ( UseCookies = false )
    let client = new HttpClient (handler)
    client.BaseAddress <- new Uri "http://192.168.0.1/"
    
    let authCookie =  generateAuthCookie(login, password)


    client.DefaultRequestHeaders.Add("Cookie", authCookie)  


    

   

    async {
            
           let! response =  client.GetStringAsync("userRpm/LoginRpm.htm?Save=Save") |> Async.AwaitTask
           
    

           let sessionId =  Regex("\d\/(\w*)\/userRpm").Match(response).Groups.[1].Value

           printfn "session id: %s" sessionId

           reload_request(client, sessionId)
            
           return response
       }
     
        

[<EntryPoint>]
let main argv =
    let login = "admin"
    let password = ""

    let response = login_request (login, password)  |>  Async.RunSynchronously
    printfn "%s" response

    
    
    0
   
     // return an integer exit code
