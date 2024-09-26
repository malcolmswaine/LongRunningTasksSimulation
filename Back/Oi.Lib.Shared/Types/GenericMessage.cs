using Newtonsoft.Json;
using Oi.Lib.Shared;

namespace Oi.Lib.Shared.Types
{
    public class GenericMessage
    {
        public MessageTypeEnum MessageType { get; set; }
        public string Payload { get; set; }

        public string SigrConnId { get; set; }

        public override string ToString() 
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}