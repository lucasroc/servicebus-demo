using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarManager.Model
{
    public class RentResponse
    {        
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Description { get; set; }

        [JsonIgnore()]
        public string MessageId { get; set; }
        [JsonIgnore()]
        public string ResponseMessage { get; set; }

        public override string ToString()
        {
            return $"MessageId: {MessageId} Nome: {Name} | Primeiro Nome: {FirstName} | " +
                $"Descrição: {Description} | Resposta do Serviço: {ResponseMessage}";
        }
    }
}
