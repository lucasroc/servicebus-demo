using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarManager.Helpers
{
    public static class MessageConverters
    {
        public static byte[] GetJsonBytes(object data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        public static T GetJsonMessageBody<T>(byte[] messageBody) =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(messageBody));
    }
}
