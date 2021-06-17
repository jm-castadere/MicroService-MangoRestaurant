using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Web.Services
{
    /// <summary>
    /// Service base to call service 
    /// </summary>
    public class BaseService : IBaseService
    {
        public ResponseDto responseModel { get; set; }

        public IHttpClientFactory httpClient { get; set; }

     
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">clent http</param>
        public BaseService(IHttpClientFactory httpClient)
        {
            this.responseModel = new ResponseDto();
            this.httpClient = httpClient;
        }


        /// <summary>
        /// Generic Send messsage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiRequest"> api request paramater</param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                //Create http client
                var clientHttp = httpClient.CreateClient("MangoAPI");
                
                //Create and set message http
                HttpRequestMessage messageHttp = new HttpRequestMessage();
                messageHttp.Headers.Add("Accept", "application/json");
                //Set message api url
                messageHttp.RequestUri = new Uri(apiRequest.Url);
              
                //Clean http cleint header
                clientHttp.DefaultRequestHeaders.Clear();
                
                //Check if api parameter data exist
                if (apiRequest.Data != null)
                {
                    //Set and serialize messsage content 
                    messageHttp.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8,"application/json");
                }

                //Chekc if Token
                if (!string.IsNullOrEmpty(apiRequest.AccessToken))
                {
                    clientHttp.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", apiRequest.AccessToken);
                }

                //Initialize reponse
                HttpResponseMessage apiResponse = null;
              
                //Check api typ of parameter
                switch (apiRequest.ApiType)
                {
                    //Ste message type
                    case SD.ApiType.POST:
                        messageHttp.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        messageHttp.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        messageHttp.Method = HttpMethod.Delete;
                        break;
                    default :
                        messageHttp.Method = HttpMethod.Get;
                        break;
                }

                //Send message and receive response
                apiResponse = await clientHttp.SendAsync(messageHttp);

                //Read response
                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                //Deserialize Response to type of function T
                var apiResponseDto = JsonConvert.DeserializeObject<T>(apiContent);
                
                return apiResponseDto;

            }
            catch(Exception e)
            {
                //Set response error
                var dtoResponse = new ResponseDto
                {
                    DisplayMessage = "Error",
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                
                var resError = JsonConvert.SerializeObject(dtoResponse);
                
                //Set response rerurned
                var apiResponseDto = JsonConvert.DeserializeObject<T>(resError);
                
                return apiResponseDto;
            }
        }

        /// <summary>
        /// Clean
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
