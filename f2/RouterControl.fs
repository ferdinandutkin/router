 
module RouterControl

open System.Net.Http
open System
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions


type Router = class

    val client : HttpClient
    val login : string
    val password : string
    val ip : string
    val sid : string option
    
    new(login, password, ?ip) as self =
        { login = login;
        sid = None;
        password = password;
        ip = defaultArg ip "http://192.168.0.1/";
        client = new HttpClient( new HttpClientHandler ( UseCookies = false ))}
        then 
            self.client.BaseAddress <- new Uri(self.ip)

             


     member self.sessionId =
        
         async {
             self.setAuthCookie() |> ignore
             let! response =  self.client.GetStringAsync("userRpm/LoginRpm.htm?Save=Save") |> Async.AwaitTask


             let sessionId =  Regex("\d\/(\w*)\/userRpm").Match(response).Groups.[1].Value
              
             return sessionId
             } |> Async.RunSynchronously
       

    
      member self.reload_request() =

            async {
                       let id = self.sessionId  

                       let baseRequestUrl =  id + "/userRpm/SysRebootRpm.htm"
               
                       let requestUrl =  baseRequestUrl + "?Reboot=%D0%9F%D0%B5%D1%80%D0%B5%D0%B7%D0%B0%D0%B3%D1%80%D1%83%D0%B7%D0%B8%D1%82%D1%8C"

                       
               
                       let msg = new HttpRequestMessage(HttpMethod.Get, requestUrl)
               


                       msg.Headers.Referrer <- new Uri(self.client.BaseAddress, baseRequestUrl)

                       let! res = self.client.SendAsync(msg) |> Async.AwaitTask

                       let reader = new System.IO.StreamReader(res.Content.ReadAsStream())

                       printf "%s" (reader.ReadToEnd())

                       return ()
                          
            } |> Async.RunSynchronously
           


    static member base64 (text : string) =
       let plainTextBytes = System.Text.Encoding.UTF8.GetBytes text;
       System.Convert.ToBase64String plainTextBytes;
  

    static member md5 (text : string) =
        let md5 = MD5.Create()
        md5.ComputeHash(Encoding.UTF8.GetBytes text) |> Array.map(fun x -> x.ToString "x2") |> String.Concat;
    


    member self.setAuthCookie() =
        let cookie : string = self.generateAuthCookie() //f# ты ретард или как
        self.client.DefaultRequestHeaders.Add("Cookie", cookie)  

    member self.generateAuthCookie() = 
    
        let pwordHash = Router.md5 self.password;
        let authHash =  Router.base64($"{self.login}:{pwordHash}")
        let authData = $"Basic {authHash}"
        $"Authorization={Uri.EscapeDataString authData}"
    
    end
    
   
  
