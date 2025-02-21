﻿using System;
using MssNet.Xml;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MssNet
{
    public class MssClient
    {
        public MssClient(MssClientSettings settings)
        {
            Settings = settings;
        }

        private MssClientSettings Settings { get; set; }
        private XmlSerializer XmlSerializer { get; } = new XmlSerializer();
        private XmlDeserializer XmlDeserializer { get; } = new XmlDeserializer();
        protected virtual HttpClient HttpClient { get; } = new HttpClient();

        public async Task<Models.Response.Root> SendRequest(Func<Models.Request.Root, Models.Request.Root> requestFunc)
        {
            var request = requestFunc(MssClientHelper.CreateRequestWithDefaults(Settings));
            var xmlContent = ParseRequest(request);
            var httpContent = PrepareHttpContent(xmlContent);
            var httpResponse = await Post(httpContent);
            var responseAsString = await httpResponse.Content.ReadAsStringAsync();
            var response = ParseResponse(responseAsString);

            var header = response.Header;
            var error = header.Error;
            if (error.Code > 0)
                throw new MssException(error.Message);

            return response;
        }

        private string ParseRequest(Models.Request.Root request)
        {
            return XmlSerializer.SerializeObject(request);
        }

        private HttpContent PrepareHttpContent(string xmlContent)
        {
            return new StringContent(xmlContent, Encoding.UTF8, "application/xml");
        }

        private Task<HttpResponseMessage> Post(HttpContent content)
        {
            return HttpClient.PostAsync("https://www.easymailing.eu/mss/mss_service.php", content);
        }

        private Models.Response.Root ParseResponse(string responseAsString)
        {
            return XmlDeserializer.Deserialize<Models.Response.Root>(responseAsString);
        }
    }
}
