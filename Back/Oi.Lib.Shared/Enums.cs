using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oi.Lib.Shared
{
    public enum JobStateEnum
    {
        Ready,
        Running,
        Complete,
        Cancelled,
        Error
    }

    public enum MessageTypeEnum
    { 
        JobCreationRequest,
        JobCreationResponse,
        JobCancelRequest,
        JobCancelResponse,
        JobProcessingStepResponse,
        JobError,
        JobComplete
    }
}
