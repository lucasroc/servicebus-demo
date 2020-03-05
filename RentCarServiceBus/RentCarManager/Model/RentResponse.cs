using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarManager.Model
{
    public class RentResponse
    {
        public string Name { get; set; }
        public string FirsName { get; set; }
        public string Description { get; set; }
        public string ResponseMessage { get; set; }

        public override string ToString()
        {
            return $"Nome: {Name} | Primeiro Nome: {FirsName} | Descrição: {Description} | Resposta do Serviço: {ResponseMessage}";
        }
    }
}
