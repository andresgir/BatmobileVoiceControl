using System;
using Lego.Ev3.Android;

namespace BatmobileVoiceControl
{
    public class SQSMessage
    {
		public string Type { get; set; }
		public string MessageId { get; set; }
		public string TopicArn { get; set; }
		public string Subject { get; set; }
        public string Message { get; set; }
        public Ev3Command MessageObject { get; set; }
		public string Timestamp { get; set; }
		public string SignatureVersion { get; set; }
		public string Signature { get; set; }
		public string SigningCertURL { get; set; }
		public string UnsubscribeURL { get; set; }

    }
}
