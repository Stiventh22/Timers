using System;

namespace Timers.Common.Responses
{
    public class Response
    {
        public int IdEmployees { get; set; }
        public DateTime Work_Time { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
