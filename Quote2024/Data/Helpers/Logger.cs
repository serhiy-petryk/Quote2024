using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Data.Helpers
{
    public static class Logger
    {
        public enum Application {Main, RealTime};

        public delegate void MessageAddedEventHandler(object sender, MessageAddedEventArgs e);
        public static event MessageAddedEventHandler MessageAdded;

        private static readonly ConcurrentBag<MessageAddedEventArgs> Messages = new ConcurrentBag<MessageAddedEventArgs>();

        public static void AddMessage(string message, Application application = Application.Main)
        {
            var sf = new StackFrame(1);
            var method = sf.GetMethod();
            var callMethodName = method.DeclaringType?.Name + "." + method.Name;

            var oMessage = new MessageAddedEventArgs(message, callMethodName, application);
            Messages.Add(oMessage);
            MessageAdded?.Invoke(null, oMessage);
        }

        public class MessageAddedEventArgs : EventArgs
        {
            public readonly DateTime Date = DateTime.Now;
            public readonly string MethodName;
            public readonly string Message;
            public readonly Application Application;
            public string FullMessage => $"{MethodName}. {Message}";
            public MessageAddedEventArgs(string msg, string methodName, Application application)
            {
                Message = msg;
                Application = application;
                MethodName = methodName;
                if (Message.StartsWith("!"))
                    Debug.Print($"LOGGER: {FullMessage}");
            }
        }
    }
}
