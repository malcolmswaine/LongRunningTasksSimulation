using Oi.Lib.Shared.Types;

namespace Oi.Lib.Shared
{
    public interface IEventProcessor
    {
        void ProcessEvent(GenericMessage message);
    }
}
