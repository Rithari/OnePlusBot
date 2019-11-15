namespace OnePlusBot.Base.Errors {
    [System.Serializable]
    public class ChannelGroupException : System.Exception
    {
        public ChannelGroupException() { }
        public ChannelGroupException(string message) : base(message) { }
        public ChannelGroupException(string message, System.Exception inner) : base(message, inner) { }
        protected ChannelGroupException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } 
}

